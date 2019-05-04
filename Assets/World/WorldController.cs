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
    using Game_State; using Interfaces; using Entities; using Network;

    public class WorldController : NetworkBehaviour, IManager
    {
        #region Constants + Statics
        public static event Action<WorldController> OnWorldControllerStartup;
        public static WorldController Local { get; private set; }
        #endregion

        /// <summary>
        /// A list of chunk controllers that are used to display the local area to the player
        /// </summary>
        public readonly List<ChunkController> ChunkControllers = new List<ChunkController>();

        /// <summary>
        /// A reference to the active game manager
        /// </summary>
        public GameManager GameManager { get; private set; }
        public World World { get; private set; }

        public bool HasInitialised { get; private set; }

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

        #region Startup and Initialisation
        /// <summary>
        /// Stores the game manager reference
        /// </summary>
        /// <param name="gameManager"></param>
        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            if (isLocalPlayer)
            {
                Local = this;
                OnWorldControllerStartup?.Invoke(this);
                GameManager.OnGameStateChange += GameManager_OnGameStateChange;
            }
        }

        private void GameManager_OnGameStateChange(GameStateChangeArgs gameStartChangeArgs)
        {
            if(gameStartChangeArgs.Current == EGameState.Disconnecting)
            {
                Local = null;
                Destroy(_gameWorldAnchorObject);
                _gameWorldAnchorObject = null;
            }
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

            // Initialise all the chunk controllers that we will ever need
            for (int i = 0; i < Chunk.KEEP_LOADED * Chunk.KEEP_LOADED; ++i)
            {
                GameObject chunkObject = new GameObject();
                chunkObject.transform.SetParent(_gameWorldAnchorObject.transform);
                ChunkControllers.Add(chunkObject.AddComponent<ChunkController>());
            }
            HasInitialised = true;
        }

        public void OnServerClientInitialise(World world)
        {
            OnLocalClientInitialise(world);
            _worldGeneration = new WorldGeneration(World.Seed, World.NegativeXSeed, World.NegativeYSeed); // The server controls the world generation and so it should be the only player that creates world generation
        }
        #endregion

        #region Server Only methods - called on the server by the server
        /// <summary>
        /// Called on the server when the chunk that a player is located in changes. (I.E. when a player moves from one chunk to another).
        /// </summary>
        /// <param name="playerController"></param>
        /// <param name="position"></param>
        public void OnServerPlayerChunkUpdate(PlayerController playerController, Vector2 position)
        {
            var playerId = playerController.Id;
            Vector2Int playerPositionInt = new Vector2Int((int)position.x, (int)position.y);

            // Find the chunks needed for the player based on their new position
            var chunksNeededForPlayer = Chunk.ListOfSurroundingChunksOfWorldPosition(playerPositionInt);

            // Find all of the chunks loaded for the player, 
            // E.G. 
            //          Previous    -> 5, 6, 7, 8, 9
            //          Needed      -> 7, 8, 9, 10, 11
            //
            //          Previous should become -> 5, 6
            var chunksLoadedForPlayer = World.GetPlayerLoadedChunkPositions(playerId);

            var chunksToUnloadForPlayer = chunksLoadedForPlayer.Except(chunksNeededForPlayer).ToList();
            var newChunksNeededForPlayer = chunksNeededForPlayer.Except(chunksLoadedForPlayer).ToList();

            var chunksToRemove = World.RemoveChunks(playerId, chunksToUnloadForPlayer);
            GameFile.Instance.Save(chunksToRemove);

            chunksToUnloadForPlayer.Clear();
            foreach(var chunk in chunksToRemove)
            {
                chunk.Unload();
                chunksToUnloadForPlayer.Add(chunk.Position);
            }

            // Tell the world about the chunks that the player now depends upon
            World.UpdatePlayerChunks(playerId, newChunksNeededForPlayer);
            // If the chunks are already loaded then we can forget about them
            World.FilterLoadedChunks(newChunksNeededForPlayer);

            // Load chunks from file
            var chunksToLoad = World.FilterKnownChunks(newChunksNeededForPlayer);
            Debug.Log("[WorldController] - OnServerPlayerChunkUpdate(PlayerController, Vector2)\n" +
                        $"ChunksToLoad Count: {chunksToLoad.Count}");

            // If there are more players than just the server client, do this call..
            var chunks = GameFile.Instance.LoadChunks(chunksToLoad);
            World.AddChunks(chunks);

            // Generate the new chunks
            var generatedChunks = _worldGeneration.GenerateChunks(newChunksNeededForPlayer);
            World.AddChunks(generatedChunks);

            foreach(var chunk in generatedChunks)
            {
                chunk.Initialise();
            }

            if (GameManager.NetworkManager.NumberOfClientControllers > 1)
            {
                var chunksToLoadBytes = Vector2IntsToBytes(chunksToLoad);
                var chunksToUnloadBytes = Vector2IntsToBytes(chunksToUnloadForPlayer);
                NetworkChunk[] generatedNetworkChunks = (generatedChunks.Count > 0) ? NetworkChunk.ChunkListToNetworkChunkArray(generatedChunks) : null;

                List<NetworkConnection> sendEntitiesTo = new List<NetworkConnection>();

                foreach(var clientController in GameManager.NetworkManager.ClientControllers)
                {
                    if (clientController.IsActive && clientController.WorldController != this)
                    {

                        TargetUpdateChunks(clientController.Connection, chunksToLoadBytes, chunksToUnloadBytes);

                        if (generatedNetworkChunks != null)
                        {
                            sendEntitiesTo.Add(clientController.Connection);
                            TargetReceiveChunks(clientController.Connection, generatedNetworkChunks);
                        }

                        if (clientController.PlayerController.Id == playerId)
                        {
                            TargetUpdateChunkControllers(clientController.Connection);
                        }
                    }
                }

                if(sendEntitiesTo.Count > 0)
                {
                    StartCoroutine(SendEntities(sendEntitiesTo, generatedChunks));
                }
            }

            if(playerId == GameManager.PlayerController.Id)
            {
                StartCoroutine(UpdateChunkControllers(World.GetPlayerLoadedChunks(playerId)));
            }
        }
        #endregion
 
        private IEnumerator UpdateChunkControllers(List<Chunk> chunksToDisplay)
        {
            // 1. Get a collection of the chunks needed displaying
            // 2. Remove chunks that are already being displayed from both the
            //    chunk controllers collection and needed to display collection
            // 3. Loop through the remaining controllers and assign a chunk from 
            //    the need to display collection        
            List<ChunkController> chunkControllersToSet = new List<ChunkController>(ChunkControllers);
            for(int i = chunkControllersToSet.Count - 1; i >= 0; --i)
            {
                if(chunkControllersToSet[i].Chunk == null || !chunksToDisplay.Contains(chunkControllersToSet[i].Chunk))
                {
                    continue;
                }

                chunksToDisplay.Remove(chunkControllersToSet[i].Chunk);
                chunkControllersToSet.RemoveAt(i);
            }

            for (int i = 0; i < chunksToDisplay.Count; i++)
            {
                if (chunksToDisplay[i].IsInitialised)
                {
                    chunkControllersToSet[i].SetChunk(chunksToDisplay[i], World);
                    yield return null;
                }
                else
                {
                    ChunkController chunkController = chunkControllersToSet[i];
                    chunksToDisplay[i].OnInitialise += (chunk) =>
                    {
                        chunkController.SetChunk(chunk, World);
                    };
                }
            }
            Debug.Log("[WorldController] - UpdateChunkControllers(List<Chunk>, List<Vector2>)");
        }

        private IEnumerator SendEntities(List<NetworkConnection> targets, List<Chunk> chunks)
        {
            int bytesLeft = 1400;
            int bufferSize = 16;

            List<uint> entityIds = new List<uint>();
            List<ushort> dataIds = new List<ushort>();
            List<byte> payload = new List<byte>();

            foreach(var chunk in chunks)
            {
                foreach(var tile in chunk.Tiles)
                {
                    if(tile.StaticEntityInstanceId == 0)
                    {
                        continue;
                    }

                    EntityInstance entityInstance = EntityCollection.Instance.GetEntity(tile.StaticEntityInstanceId);
                    EntityData entityData = entityInstance.GetData();
                    bytesLeft -= sizeof(uint); // The EntityInstanceId
                    bytesLeft -= sizeof(ushort); // The EntityDataId
                    bytesLeft -= entityData.PacketSize(); // Payload

                    // Cannot accomodate this entity within the packet
                    if (bytesLeft < 0)
                    {
                        foreach(var target in targets)
                        {
                            TargetReceiveEntities(target, entityIds.ToArray(), dataIds.ToArray(), payload.ToArray());
                        }
                        bufferSize--;

                        if(bufferSize == 0)
                        {
                            yield return new WaitForSeconds(0.15f);
                            bufferSize = 16;
                        }
                        entityIds.Clear();
                        dataIds.Clear();
                        payload.Clear();
                        bytesLeft = 1400;
                    }

                    entityIds.Add(entityInstance.Id);
                    dataIds.Add(entityData.Id);
                    payload.AddRange(entityInstance.GetPacket());
                }
            }

            if(entityIds.Count != 0)
            {
                foreach (var target in targets)
                {
                    TargetReceiveEntities(target, entityIds.ToArray(), dataIds.ToArray(), payload.ToArray());
                }
                entityIds.Clear();
                dataIds.Clear();
                payload.Clear();
            }


            List<Vector2Int> chunkPositions = new List<Vector2Int>(chunks.Count);
            foreach(var chunk in chunks)
            {
                chunkPositions.Add(chunk.Position);
            }

            var chunkPositionBytes = Vector2IntsToBytes(chunkPositions);
            
            foreach(var target in targets)
            {
                TargetInitialiseChunks(target, chunkPositionBytes);
            }
        }

        #region Target RPC's - Message sent to a single client
        [TargetRpc]
        private void TargetReceiveChunks(NetworkConnection conn, NetworkChunk[] networkChunks)
        {
            Chunk[] unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks).ToArray();
            Local.World.AddChunks(unpackedChunks);
            Debug.Log("[WorldController] - TargetReceiveChunks(NetworkConnection, NetworkChunk[])");
        }

        [TargetRpc]
        private void TargetUpdateChunkControllers(NetworkConnection conn)
        {
            var chunks = Local.World.GetChunks(Chunk.ListOfSurroundingChunksOfWorldPosition(Local.transform.position.Vec2Int()));
            StartCoroutine(Local.UpdateChunkControllers(chunks));
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
        private void TargetReceiveEntities(NetworkConnection conn, uint[] entityIds, ushort[] dataIds, byte[] entityPayload)
        {
            int offset = 0;

            for(int i = 0; i < entityIds.Length; ++i)
            {
                EntityCollection.Instance.CreateEntity(dataIds[i], entityIds[i], entityPayload, ref offset);
            }
        }

        [TargetRpc]
        private void TargetInitialiseChunks(NetworkConnection conn, byte[] chunkPositionBytes)
        {
            Vector2Int[] chunkPositions = BytesToVector2Ints(chunkPositionBytes);
            foreach(var chunkPosition in chunkPositions)
            {
                Local.World.GetChunk(chunkPosition).Initialise();
            }
        }

        [TargetRpc]
        public void TargetFinishInitialisation(NetworkConnection conn, byte[]chunkPositionBytes)
        {
            OnLocalClientInitialise(GameFile.Instance.LoadWorld());

            Vector2Int[] chunkPositions = BytesToVector2Ints(chunkPositionBytes);
            var chunks = GameFile.Instance.LoadChunks(chunkPositions); // Load chunks
            World.AddChunks(chunks);
            // Update controllers
            StartCoroutine(UpdateChunkControllers(World.GetChunks(Chunk.ListOfSurroundingChunksOfWorldPosition(transform.position.Vec2Int()))));
        }

        [TargetRpc]
        private void TargetUpdateChunks(NetworkConnection conn, byte[] loadChunks, byte[] unloadChunks)
        {
            if(!isServer && Local.HasInitialised)
            {
                Vector2Int[] chunkPositions = BytesToVector2Ints(loadChunks);
                var chunks = GameFile.Instance.LoadChunks(chunkPositions);
                Local.World.AddChunks(chunks);

                chunkPositions = BytesToVector2Ints(unloadChunks);
                chunks = Local.World.RemoveChunks(chunkPositions);
                GameFile.Instance.Save(chunks);

                foreach(var chunk in chunks)
                {
                    chunk.Unload();
                }
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

        //[Command]
        //private void CmdRequestAllEntities()
        //{
        //    int bytesLeft = 1400 - sizeof(bool); // packet size - bool

        //    List<uint> entityIds = new List<uint>();
        //    List<ushort> dataIds = new List<ushort>();
        //    List<byte> payload = new List<byte>();

        //    int numberOfEntities = EntityCollection.Instance.ActiveEntities.Count;
        //    for(int i = 0; i < numberOfEntities; ++i)
        //    {
        //        EntityInstance entityInstance = EntityCollection.Instance.ActiveEntities[i];
        //        EntityData entityData = entityInstance.GetData();
        //        bytesLeft -= sizeof(uint); // The EntityInstanceId
        //        bytesLeft -= sizeof(ushort); // The EntityDataId
        //        bytesLeft -= entityData.PacketSize(); // Payload

        //        // Cannot accomodate this entity within the packet
        //        if (bytesLeft < 0)
        //        {
        //            TargetReceiveEntities(connectionToClient, entityIds.ToArray(), dataIds.ToArray(), payload.ToArray(), false);
        //            entityIds.Clear();
        //            dataIds.Clear();
        //            payload.Clear();
        //            bytesLeft = 1400 - sizeof(bool);
        //        }

        //        entityIds.Add(entityInstance.Id);
        //        dataIds.Add(entityData.Id);
        //        payload.AddRange(entityInstance.GetPacket());

        //        // Last Entity to add
        //        if(i == numberOfEntities - 1)
        //        {
        //            TargetReceiveEntities(connectionToClient, entityIds.ToArray(), dataIds.ToArray(), payload.ToArray(), true);
        //            entityIds.Clear();
        //            dataIds.Clear();
        //            payload.Clear();
        //        }
        //    }
        //}
        //#endregion

        //#region Commands - Methods called on the server from a client
        //[Command]
        //private void CmdRequestAllLoadedChunks()
        //{
        //    const int packetBufferSize = 16;

        //    int chunksLeft = Local.World.LoadedChunks.Count;
        //    int chunksProcessed = 0;
        //    List<Chunk> chunksToSend = new List<Chunk>(packetBufferSize);

        //    foreach (var chunkPair in Local.World.LoadedChunks)
        //    {
        //        chunksToSend.Add(chunkPair.Value);

        //        --chunksLeft;
        //        bool lastSend = chunksLeft == 0;
        //        if(++chunksProcessed == packetBufferSize || lastSend)
        //        {
        //            chunksProcessed = 0;
        //            TargetReceiveChunks(connectionToClient, NetworkChunk.ChunkListToNetworkChunkArray(chunksToSend), lastSend);
        //            chunksToSend.Clear();
        //        }
        //    }

        //    Debug.Log("[WorldController] - CmdRequestAllLoadedChunks()");
        //}
        #endregion

        #region Utilities
        public Vector3 GeneratePlayerPosition()
        {
            return Vector3.zero;
            var position = URandom.insideUnitSphere * URandom.Range(0, 10000);
            position.z = 0f;
            return position;
        }
        public static byte[] Vector2IntsToBytes(IEnumerable<Vector2Int> vec2s)
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
        public static Vector2Int[] BytesToVector2Ints(byte[] bytes)
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
        #endregion
    }
}