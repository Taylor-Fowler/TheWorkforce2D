using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using URandom = UnityEngine.Random;

// -> Player Moves 
// -> Inform World Controller 
// -> World Controller Requests Chunks 
// -> World returns all available chunks and modifies parameter to only contain chunks no longer available
// -> World controller requests chunk load from file save
// -> World controller requests the final chunks from generation
//
// Define an alias due to conflicting types `PlayerController` found in 
// TheWorkforce.Player and UnityEngine.Networking
// using PlayerController = TheWorkforce.PlayerController;

namespace TheWorkforce
{
    using Game_State; using Interfaces; using Entities;

    public class WorldController : NetworkBehaviour, IManager
    {
        #region Constants + Statics
        public static event Action<WorldController> OnWorldControllerStartup;
        public static WorldController Local { get; private set; }
        #endregion

        public event Action<Vector2> OnWorldPlayerPositionUpdate;

        /// <summary>
        /// A list of chunk controllers that are used to display the local area to the player
        /// </summary>
        public readonly List<ChunkController> ChunkControllers = new List<ChunkController>();

        /// <summary>
        /// A reference to the active game manager
        /// </summary>
        public GameManager GameManager { get; private set; }
        public World World { get; private set; }


        // private List<ChunkController> _availableChunkControllers;
        private bool _hasInitialised = false;
        private Dictionary<Vector2Int, List<int>> _allChunksLoadedByPlayerPositions;
        private WorldGeneration _worldGeneration;
        private GameObject _gameWorldAnchorObject;

        /// <summary>
        /// Returns the TileController that exists within the Chunk specified in the tile position specified
        /// </summary>
        /// <param name="chunkPosition">The Chunk position specified in Chunk coordinates (not world co-ordinates)</param>
        /// <param name="tilePosition">The tile position specified relative to the chunk (0 to Chunk.SIZE(-1))</param>
        /// <returns></returns>
        public TileController this[Vector2Int chunkPosition, Vector2Int tilePosition]
        {
            get
            {
                // Find the the ChunkController in the collection with the chunk position specified
                ChunkController chunkController = ChunkControllers.Find(c => c.Chunk != null && c.Chunk.Position == chunkPosition);
                if (chunkController != null)
                {
                    return chunkController[tilePosition];
                }
                return null;
            }
        }

        //#region Network Behaviour Overrides
        ///// <summary>
        ///// Initialises the local player reference and attempts to startup this controller
        ///// </summary>
        //public override void OnStartLocalPlayer()
        //{
        //    Local = this;
        //}
        //#endregion

        /// <summary>
        /// Stores the game manager reference
        /// </summary>
        /// <param name="gameManager"></param>
        public void Startup(GameManager gameManager)
        {
            // take a reference to the game file and use it for the world creation
            GameManager = gameManager;
            if (isLocalPlayer)
            {
                Local = this;
                OnWorldControllerStartup?.Invoke(this); 
            }
            Debug.Log("[WorldController] - Startup(GameManager)");
        }

        public void OnLocalClientInitialise(World world)
        {
            World = world;
            _gameWorldAnchorObject = new GameObject
            {
                name = "World"
            };

            // Set the z position to 1, this is important for ordering of tiles
            _gameWorldAnchorObject.transform.position = new Vector3(0f, 0f, 1f);
            _allChunksLoadedByPlayerPositions = new Dictionary<Vector2Int, List<int>>();

            // Initialise all the chunk controllers that we will ever need
            for (int i = 0; i < Chunk.KEEP_LOADED * Chunk.KEEP_LOADED; ++i)
            {
                GameObject chunkObject = new GameObject();
                chunkObject.transform.SetParent(_gameWorldAnchorObject.transform);
                ChunkControllers.Add(chunkObject.AddComponent<ChunkController>());
            }
        }

        public void OnServerClientInitialise(World world)
        {
            OnLocalClientInitialise(world);
            // The server controls the world generation and so it should be the only player that creates world generation
            _worldGeneration = new WorldGeneration(World.Seed, World.NegativeXSeed, World.NegativeYSeed);
        }

        public IEnumerator InitialiseConnection(Action callback)
        {
            // Server does not need to worry about hearing from the other players as it is the first player
            if(isServer)
            {
                callback();
                yield break;
            }

            // Before we can say we are safely initialised, we must request all of the loaded chunks
            CmdRequestAllLoadedChunks();

            while(!_hasInitialised)
            {
                yield return null;
            }

            // We will reuse this going further so reset it
            _hasInitialised = false;

            // We must know of all the enitities that are actively processing in the world before we can resume gameplay on all clients
            CmdRequestAllEntities();

            while(!_hasInitialised)
            {
                yield return null;
            }

            // Connection has been initialised, inform the listener/s
            callback();
        }

        #region Server Only methods - called on the server by the server
        /// <summary>
        /// Called on the server when the chunk that a player is located in changes. (I.E. when a player moves from one chunk to another).
        /// </summary>
        /// <param name="playerController"></param>
        /// <param name="position"></param>
        [Server]
        public void OnServerPlayerChunkUpdate(PlayerController playerController, Vector2 position)
        {
            // What happens when a player moves to a new chunk?
            //    // 3 players: A (host), B and C (regular clients)
            //    // Players A and B are in the same chunk
            //    // Player C moves into a new chunk:
            //    // 1. Tell the Host about the chunks that were kept loaded by Player C
            //    //    that are no longer needed to be loaded and also about the new chunks that player
            //    //    C is keeping loaded
            //    // 2. The Host checks if any other player needs the chunks that have been asked to
            //    //    be unloaded by player C. The Host notifies all clients to unload said chunks.
            //    // 3. The Host then tells all clients to load the chunks requested by player C.
            //    // 4. Finally, the Host generates the chunks requested by player C and sends the data
            //    //    to all clients

            // Find all the chunks around the current player position, these are the new player dependant chunks
            //var chunksRequired = Chunk.SurroundingChunksOfWorldPosition(position);

            //var chunksPreviouslyRequired = World.GetPlayerLoadedChunkPositions(playerController.Id);

            RequestPlayerChunkUpdate(playerController.Id, position);

            Debug.Log("[WorldController] - OnServerPlayerChunkUpdate(PlayerController, Vector2)");
        }

        [Server]
        public Vector3 GeneratePlayerPosition()
        {
            var position = URandom.insideUnitSphere * URandom.Range(0, 10000);
            position.z = 0f;
            return position;
        }
        #endregion

        // This is called when a player object on the server moves enough to request a chunk update
        // This should only be called on the server
        public void RequestPlayerChunkUpdate(int playerId, Vector2 playerPosition)
        {
            Vector2Int playerPositionInt = new Vector2Int((int)playerPosition.x, (int)playerPosition.y);
            // Find all of the chunks loaded for the player
            var chunksPreviouslyLoaded = World.GetPlayerLoadedChunkPositions(playerId);
            // Find the chunks needed for the player based on their new position
            var chunksNeededForPlayer = Chunk.ListOfSurroundingChunksOfWorldPosition(playerPositionInt);
            var chunksNeededForPlayerArray = Chunk.SurroundingChunksOfWorldPosition(playerPositionInt);

            
            // Tell the world about the chunks that the player now depends upon
            World.UpdatePlayerChunks(playerId, chunksNeededForPlayer);
            // Remove any loaded chunks from our collection, they do not need to be loaded
            World.FilterLoadedChunks(chunksNeededForPlayer);

            // Load chunks from file
            var chunksToLoad = World.FilterKnownChunks(chunksNeededForPlayer);
            Debug.Log("[WorldController] - RequestPlayerChunkUpdate(int, Vector2)\n" +
                        $"ChunksToLoad Count: {chunksToLoad.Count}");
            RpcReceiveLoadChunks(playerId, Vector2IntsToBytes(chunksToLoad));

            // Generate the new chunks
            var generatedChunks = _worldGeneration.GenerateChunks(chunksNeededForPlayer);
            RpcReceiveChunksWithDependencies(NetworkChunk.ChunkListToNetworkChunkArray(generatedChunks), playerId, Vector2IntsToBytes(chunksNeededForPlayerArray));

            Debug.Log("[WorldController] - RequestPlayerChunkUpdate(int, Vector2)");
            //UpdateChunkControllers(World.GetPlayerLoadedChunks(playerId), chunksPreviouslyLoaded.Intersect(chunksNeededForPlayer).ToList());
        }
    
        //public void UpdatePlayerPosition(Vector2 playerPosition)
        //{
        //    // What happens when a player moves to a new chunk?
        //    // 3 players: A (host), B and C (regular clients)
        //    // Players A and B are in the same chunk
        //    // Player C moves into a new chunk:
        //    // 1. Tell the Host about the chunks that were kept loaded by Player C
        //    //    that are no longer needed to be loaded and also about the new chunks that player
        //    //    C is keeping loaded
        //    // 2. The Host checks if any other player needs the chunks that have been asked to
        //    //    be unloaded by player C. The Host notifies all clients to unload said chunks.
        //    // 3. The Host then tells all clients to load the chunks requested by player C.
        //    // 4. Finally, the Host generates the chunks requested by player C and sends the data
        //    //    to all clients
    
        //    // Get all the chunks in the vicinity of the player
        //    List<Vector2> chunksToGenerate = Chunk.ListOfSurroundingChunksOfWorldPosition(playerPosition);
    
        //    // Find the chunks that need to be loaded, which are all the generated chunks,
        //    // minus all of the chunks that are already loaded
        //    List<Vector2> chunksToLoad = chunksToGenerate.Except(World.ChunksSurroundingLocalPlayer).ToList();
    
        //    // Find the chunks that are no longer in the vicinity of the player,
        //    // Previously surrounding chunks - chunks that are currently surrounding
        //    // = chunks no longer surrounding
        //    var chunksToUnload = World.ChunksSurroundingLocalPlayer.Except(chunksToGenerate).ToArray();
    
        //    // Update the chunks surrounding the player now that we know what has changed
        //    World.SetChunksSurroundingLocalPlayer(chunksToGenerate);
    
        //    // Remove all the chunks that are loaded from our list of chunks to generate
        //    World.FilterLoadedChunks(chunksToGenerate);
    
    
        //    chunksToLoad = chunksToLoad.Except(chunksToGenerate).ToList();
    
        //    CmdPlayerChunkUpdate(chunksToUnload, chunksToLoad.ToArray(), chunksToGenerate.ToArray(),
        //        playerControllerId);

        //    WorldPlayerPositionUpdate(playerPosition);
        //}

        private void UpdateChunkControllers(List<Chunk> chunksToDisplay, List<Vector2> chunksToKeepDisplayed)
        {
            // 1. Get a collection of the chunks needed displaying
            // 2. Remove chunks that are already being displayed from both the
            //    chunk controllers collection and needed to display collection
            // 3. Loop through the remaining controllers and assign a chunk from 
            //    the need to display collection        
            List<ChunkController> chunkControllersToSet = ChunkControllers;

            // if(chunksToKeepDisplayed.Count != 0)
            // {
            //     chunkControllersToSet = ChunkControllers
            //         .Where(value => chunksToKeepDisplayed.Contains(value.Chunk.Position) != true)
            //         .ToList();
            // }

            for (int i = 0; i < chunksToDisplay.Count; i++)
            {
                chunkControllersToSet[i].SetChunk(chunksToDisplay[i], World);
            }

            Debug.Log("[WorldController] - UpdateChunkControllers(List<Chunk>, List<Vector2>)");
        }
    
        private List<Vector2Int> UpdateChunksLoadedByPlayer(Vector2Int[] chunkPositions, int playerId)
        {
            List<Vector2Int> chunksNewlyLoaded = new List<Vector2Int>();
    
            foreach (var chunkPosition in chunkPositions)
                if (_allChunksLoadedByPlayerPositions.ContainsKey(chunkPosition))
                {
                    _allChunksLoadedByPlayerPositions[chunkPosition].Add(playerId);
                }
                else
                {
                    _allChunksLoadedByPlayerPositions.Add(chunkPosition, new List<int> {playerId});
                    chunksNewlyLoaded.Add(chunkPosition);
                }
    
            return chunksNewlyLoaded;
        }
    
        private List<Vector2Int> UpdateChunksUnloadedByPlayer(Vector2Int[] chunkPositions, int playerId)
        {
            List<Vector2Int> chunksSuccessfullyUnloaded = new List<Vector2Int>();
    
            foreach (var chunkPosition in chunkPositions)
            {
                if (_allChunksLoadedByPlayerPositions.ContainsKey(chunkPosition))
                {
                    if (_allChunksLoadedByPlayerPositions[chunkPosition].Count == 1)
                    {
                        _allChunksLoadedByPlayerPositions.Remove(chunkPosition);
                        chunksSuccessfullyUnloaded.Add(chunkPosition);
                    }
                    else
                    {
                        _allChunksLoadedByPlayerPositions[chunkPosition].Remove(playerId);
                    }
                }
            }
    
            return chunksSuccessfullyUnloaded;
        }

        #region Custom Event Invoking
        private void WorldPlayerPositionUpdate(Vector2 position)
        {
            OnWorldPlayerPositionUpdate?.Invoke(position);
        }
        #endregion

        #region Commands - Methods called on the server from a client
        [Command]
        private void CmdRequestAllLoadedChunks()
        {
            const int packetBufferSize = 16;

            int chunksLeft = Local.World.LoadedChunks.Count;
            int chunksProcessed = 0;
            List<Chunk> chunksToSend = new List<Chunk>(packetBufferSize);

            foreach (var chunkPair in Local.World.LoadedChunks)
            {
                chunksToSend.Add(chunkPair.Value);

                --chunksLeft;
                bool lastSend = chunksLeft == 0;
                if(++chunksProcessed == packetBufferSize || lastSend)
                {
                    chunksProcessed = 0;
                    TargetReceiveChunks(connectionToClient, NetworkChunk.ChunkListToNetworkChunkArray(chunksToSend), lastSend);
                    chunksToSend.Clear();
                }
            }

            Debug.Log("[WorldController] - CmdRequestAllLoadedChunks()");
        }

        [Command]
        private void CmdRequestAllEntities()
        {
            int bytesLeft = 1400 - sizeof(bool); // packet size - bool
            
            List<uint> entityIds = new List<uint>();
            List<ushort> dataIds = new List<ushort>();
            List<byte> payload = new List<byte>();

            int numberOfEntities = EntityCollection.Instance().ActiveEntities.Count;
            for(int i = 0; i < numberOfEntities; ++i)
            {
                EntityInstance entityInstance = EntityCollection.Instance().ActiveEntities[i];
                EntityData entityData = entityInstance.GetData();
                bytesLeft -= sizeof(uint); // The EntityInstanceId
                bytesLeft -= sizeof(ushort); // The EntityDataId
                bytesLeft -= entityData.PacketSize(); // Payload

                // Cannot accomodate this entity within the packet
                if (bytesLeft < 0)
                {
                    TargetReceiveEntities(connectionToClient, entityIds.ToArray(), dataIds.ToArray(), payload.ToArray(), false);
                    entityIds.Clear();
                    dataIds.Clear();
                    payload.Clear();
                    bytesLeft = 1400 - sizeof(bool);
                }

                entityIds.Add(entityInstance.Id);
                dataIds.Add(entityData.Id);
                payload.AddRange(entityInstance.GetPacket());

                // Last Entity to add
                if(i == numberOfEntities - 1)
                {
                    TargetReceiveEntities(connectionToClient, entityIds.ToArray(), dataIds.ToArray(), payload.ToArray(), true);
                    entityIds.Clear();
                    dataIds.Clear();
                    payload.Clear();
                }
            }
        }
        #endregion


        #region Target RPC's - Message sent to a single client
        [TargetRpc]
        private void TargetReceiveChunks(NetworkConnection conn, NetworkChunk[] networkChunks, bool finished)
        {
            Chunk[] unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks).ToArray();
            World.AddChunks(unpackedChunks);
            _hasInitialised = finished;
            Debug.Log("[WorldController] - TargetReceiveChunks(NetworkConnection, NetworkChunk[])");
        }


        /// <summary>
        /// Called on a specified client, entities along with their data are received.
        /// </summary>
        /// <param name="conn">The connection of the targeted client</param>
        /// <param name="entityIds">The unique entity Ids contained</param>
        /// <param name="dataIds">The data Id of the entity, used to identify what data type the entity requires</param>
        /// <param name="entityPayload">The data required to populate every entity in the packet</param>
        /// <param name="finished">Identifies whether this is the final packet of entity data from the request</param>
        [TargetRpc]
        private void TargetReceiveEntities(NetworkConnection conn, uint[] entityIds, ushort[] dataIds, byte[] entityPayload, bool finished)
        {
            int offset = 0;

            for(int i = 0; i < entityIds.Length; ++i)
            {
                EntityCollection.Instance().CreateEntity(dataIds[i], entityIds[i], entityPayload, ref offset);
            }
            _hasInitialised = finished;
        }
        #endregion

        #region Client RPC's - Message sent to all clients
        ///// <summary>
        ///// Rpc informing all clients to remove the chunks provided from their list of loaded chunks
        ///// </summary>
        ///// <param name="chunksOrderedToUnload">A collection of chunk positions to unload</param>
        //[ClientRpc]
        //private void RpcUnloadChunks(Vector2Int[] chunksOrderedToUnload)
        //{
        //    foreach (var chunk in chunksOrderedToUnload)
        //    {
        //        World.LoadedChunks.Remove(chunk);
        //    }
        //}
    
        //[ClientRpc]
        //private void RpcReceiveChunkDependencies(int playerId, Vector2Int[] chunkPositions)
        //{
        //    if(!isServer)
        //    {
        //        Local.World.UpdatePlayerChunks(playerId, chunkPositions);
        //    }
        //}

        //[ClientRpc]
        //private void RpcReceiveChunks(NetworkChunk[] networkChunks, int playerId)
        //{
        //    var unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks);
        //    Local.World.AddChunks(unpackedChunks);

        //    if (!isServer)
        //    {
        //        List<Vector2Int> chunkPositions = new List<Vector2Int>();
        //        foreach(var chunk in unpackedChunks)
        //        {
        //            chunkPositions.Add(chunk.Position);
        //        }
        //        Local.World.UpdatePlayerChunks(playerId, chunkPositions);
        //    }
        //    if (Local.GameManager.PlayerController.Id == playerId)
        //    {
        //        Local.UpdateChunkControllers(Local.World.GetPlayerLoadedChunks(playerId), null);
        //    }
        //}

        [ClientRpc]
        private void RpcReceiveLoadChunks(int playerId, byte[] chunkPositionBytes)
        {
            Vector2Int[] chunkPositions = BytesToVector2Ints(chunkPositionBytes);

            var chunks = GameFile.Instance.LoadChunks(chunkPositions);
            World.AddChunks(chunks);
        }

        [ClientRpc]
        private void RpcReceiveChunksWithDependencies(NetworkChunk[] networkChunks, int playerId, byte[] chunkPositionBytes)
        {
            Vector2Int[] chunkPositions = BytesToVector2Ints(chunkPositionBytes);

            Chunk[] unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks).ToArray();
            Local.World.AddChunks(unpackedChunks);
            if(!isServer)
            {
                //Local.World.UpdatePlayerChunks(playerId, chunkPositions);
            }
            if(Local.GameManager.PlayerController.Id == playerId)
            {
                Local.UpdateChunkControllers(Local.World.GetPlayerLoadedChunks(playerId), null);
            }
        }
        #endregion

        #region Deprecated

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ///
        ///     REASON:
        ///             Initially, each client was in charge of informing the server that they needed new
        ///             data and no longer needed world data.
        ///            
        ///             However, now the server processes each clients player movement and determines what
        ///             that player needs and what they no longer need and then tells all clients about the
        ///             data that has been generated, loaded and unloaded.                 
        ///
        ///
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        ///// <summary>
        ///// A command sent from a client informing the server which chunks to load, unload and to generate. The server processes this information
        ///// and informs all other clients of the chunks to unload, load and sends the newly generated chunk information
        ///// </summary>
        ///// <param name="chunksNoLongerNeeded">The chunks no longer needed by the player due to their location</param>
        ///// <param name="chunksLoaded">The chunks that should be loaded directly due to the players position</param>
        ///// <param name="chunksToGenerate">The chunks to generated directly due to the players position</param>
        ///// <param name="playerId">The unique id of the player who sent the update</param>
        //[Command]
        //private void CmdPlayerChunkUpdate(Vector2Int[] chunksNoLongerNeeded, Vector2Int[] chunksLoaded, Vector2Int[] chunksToGenerate, int playerId)
        //{
        //    Vector2Int[] allChunksToLoad = chunksLoaded.Concat(chunksToGenerate).Distinct().ToArray();
        //    var loaded = Local.UpdateChunksLoadedByPlayer(allChunksToLoad, playerId);
        //    var unloaded = Local.UpdateChunksUnloadedByPlayer(chunksNoLongerNeeded, playerId);
        //    var generated = Local.World.GetChunks(new List<Vector2Int>(chunksToGenerate));

        //    if (unloaded.Count != 0)
        //    {
        //        RpcUnloadChunks(unloaded.ToArray());
        //    }

        //    if (generated.Count != 0)
        //    {
        //        //RpcReceiveChunks(NetworkChunk.ChunkListToNetworkChunkArray(generated));
        //    }

        //    Debug.Log("[WorldController] - CmdPlayerChunkUpdate(Vector2[], Vector2[], Vector2[], int) \n"
        //            + "playerId: " + playerId.ToString() + "\n"
        //            + "Server playerId: " + playerControllerId);
        //}
        #endregion

        private byte[] Vector2IntsToBytes(IEnumerable<Vector2Int> vec2s)
        {
            //Debug.Log($"vec2s.Length: {vec2s.Length}");

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
                {
                    foreach(var vec2 in vec2s)
                    {
                        bw.Write(vec2.x);
                        bw.Write(vec2.y);
                    }
                }
                return ms.ToArray();
            }
        }
        private Vector2Int[] BytesToVector2Ints(byte[] bytes)
        {
            Vector2Int[] vec2s;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
                {
                    var count = br.BaseStream.Length / (sizeof(int) * 2);

                    //Debug.Log($"Bytes.Length: {bytes.Length}.\nBr.BaseStream.Length: {br.BaseStream.Length}.\nCount: {count}.");
                    vec2s = new Vector2Int[count];
                    for (int i = 0; i < count; ++i)
                    {
                        vec2s[i] = new Vector2Int(br.ReadInt32(), br.ReadInt32());
                    }
                }
            }
            return vec2s;
        }
    }
}