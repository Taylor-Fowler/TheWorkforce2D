using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using TheWorkforce.Game_State;
using System.Collections;
using TheWorkforce.Entities;

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
    public delegate void WorldPlayerPositionUpdateHandler(object source, Vector2 position);
    public delegate void WorldControllerStartup(object source, WorldController controller);

    public class WorldController : NetworkBehaviour, IManager
    {
        #region Static Members
        private static WorldController Local;
        #endregion

        #region Custom Event Declarations
        public static event WorldControllerStartup OnWorldControllerStartup;
        public event WorldPlayerPositionUpdateHandler OnWorldPlayerPositionUpdate;
        #endregion

        #region Public Indexers
        public TileController this[Vector2 chunkPosition, Vector2 tilePosition]
        {
            get
            {
                ChunkController chunkController = ChunkControllers.Find(c => c.Chunk != null && c.Chunk.Position == chunkPosition);
                if(chunkController != null)
                {
                    return chunkController[tilePosition];
                }
                return null;
            }
        }
        #endregion

        #region Public Members
        /// <summary>
        /// A list of chunk controllers that are used to display the local area to the player
        /// </summary>
        public readonly List<ChunkController> ChunkControllers = new List<ChunkController>();
        public GameManager GameManager { get; private set; }
        #endregion

        #region Private Members
        // private List<ChunkController> _availableChunkControllers;
        private bool _hasInitialised = false;
        private Dictionary<Vector2, List<int>> _allChunksLoadedByPlayerPositions;
        private World _world;
        private WorldGeneration _worldGeneration;
        private GameObject _gameWorldAnchorObject;
        #endregion

        #region NetworkBehaviour Overrides
        public override void OnStartLocalPlayer()
        {
            Local = this;
            WorldControllerStartup();
        }
        #endregion

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            _world = new World(784893570);
            _gameWorldAnchorObject = new GameObject
            {
                name = "World"
            };
            _gameWorldAnchorObject.transform.position = new Vector3(0f, 0f, 1f);
            _allChunksLoadedByPlayerPositions = new Dictionary<Vector2, List<int>>();

            for (int x = 0; x < Chunk.KEEP_LOADED; x++)
            {
                for (int y = 0; y < Chunk.KEEP_LOADED; y++)
                {
                    GameObject chunkObject = new GameObject();
                    chunkObject.transform.SetParent(_gameWorldAnchorObject.transform);

                    ChunkControllers.Add(chunkObject.AddComponent<ChunkController>());
                }
            }
            if (isServer)
            {
                _worldGeneration = new WorldGeneration(_world.Seed, _world.NegativeXSeed, _world.NegativeYSeed);
            }

            Debug.Log("[WorldController : IManager] - Startup() \n"
                    + "Server Seed: " + _world.Seed);
        }

        public IEnumerator InitialiseConnection(Action callback)
        {
            if(isServer)
            {
                callback();
                yield break;
            }

            CmdRequestAllLoadedChunks();
            while(!_hasInitialised)
            {
                yield return null;
            }

            _hasInitialised = false;

            CmdRequestAllEntities();
            while(!_hasInitialised)
            {
                yield return null;
            }
            callback();
        }

        // This is called when a plyer object on the server moves enough to request a chunk update
        // This should only be called on the server
        public void RequestPlayerChunkUpdate(int playerId, Vector2 playerPosition)
        {
            Debug.Log("[WorldController] - RequestPlayerChunkUpdate(int, Vector2)");

            // The local player should notify any listeners of their movement
            if(isLocalPlayer)
            {
                WorldPlayerPositionUpdate(playerPosition);
            }

            // Only the server should process the chunk update
            if(!isServer)
            {
                return;
            }
            // Find all of the chunks loaded for the player
            var chunksPreviouslyLoaded = _world.GetPlayerLoadedChunkPositions(playerId);
            // Find the chunks needed for the player based on their new position
            var chunksNeededForPlayer = Chunk.ListOfSurroundingChunksOfWorldPosition(playerPosition);
            var chunksNeededForPlayerArray = Chunk.SurroundingChunksOfWorldPosition(playerPosition);

            
            // Tell the world about the chunks that the player now depends upon
            _world.UpdatePlayerChunks(playerId, chunksNeededForPlayer);
            // Remove any loaded chunks from our collection, they do not need to be loaded
            _world.FilterLoadedChunks(chunksNeededForPlayer);
            // Load chunks from file

            // Generate the new chunks
            var generatedChunks = _worldGeneration.GenerateChunks(chunksNeededForPlayer);
            RpcReceiveChunksWithDependencies(NetworkChunk.ChunkListToNetworkChunkArray(generatedChunks), playerId, chunksNeededForPlayerArray);

            //UpdateChunkControllers(_world.GetPlayerLoadedChunks(playerId), chunksPreviouslyLoaded.Intersect(chunksNeededForPlayer).ToList());
        }

        public Tile RequestTile(Vector2 chunkPosition, Vector2 tilePosition)
        {
            Tile tile = null;
            Chunk chunk = _world.GetChunk(chunkPosition);

            if(chunk != null)
            {
                tile = chunk.GetTile(tilePosition);
            }

            return tile;
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
        //    List<Vector2> chunksToLoad = chunksToGenerate.Except(_world.ChunksSurroundingLocalPlayer).ToList();
    
        //    // Find the chunks that are no longer in the vicinity of the player,
        //    // Previously surrounding chunks - chunks that are currently surrounding
        //    // = chunks no longer surrounding
        //    var chunksToUnload = _world.ChunksSurroundingLocalPlayer.Except(chunksToGenerate).ToArray();
    
        //    // Update the chunks surrounding the player now that we know what has changed
        //    _world.SetChunksSurroundingLocalPlayer(chunksToGenerate);
    
        //    // Remove all the chunks that are loaded from our list of chunks to generate
        //    _world.FilterLoadedChunks(chunksToGenerate);
    
    
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
                chunkControllersToSet[i].SetChunk(chunksToDisplay[i], _world);
            }

            Debug.Log("[WorldController] - UpdateChunkControllers(List<Chunk>, List<Vector2>)");
        }
    
        private List<Vector2> UpdateChunksLoadedByPlayer(Vector2[] chunkPositions, int playerId)
        {
            List<Vector2> chunksNewlyLoaded = new List<Vector2>();
    
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
    
        private List<Vector2> UpdateChunksUnloadedByPlayer(Vector2[] chunkPositions, int playerId)
        {
            List<Vector2> chunksSuccessfullyUnloaded = new List<Vector2>();
    
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
            OnWorldPlayerPositionUpdate?.Invoke(this, position);
        }

        private void WorldControllerStartup()
        {
            OnWorldControllerStartup?.Invoke(this, this);
        }
        #endregion

        #region Custom Network Messages
        /// <summary>
        /// A command sent from a client informing the server which chunks to load, unload and to generate. The server processes this information
        /// and informs all other clients of the chunks to unload, load and sends the newly generated chunk information
        /// </summary>
        /// <param name="chunksNoLongerNeeded">The chunks no longer needed by the player due to their location</param>
        /// <param name="chunksLoaded">The chunks that should be loaded directly due to the players position</param>
        /// <param name="chunksToGenerate">The chunks to generated directly due to the players position</param>
        /// <param name="playerId">The unique id of the player who sent the update</param>
        [Command]
        private void CmdPlayerChunkUpdate(Vector2[] chunksNoLongerNeeded, Vector2[] chunksLoaded, Vector2[] chunksToGenerate, int playerId)
        {
            Vector2[] allChunksToLoad = chunksLoaded.Concat(chunksToGenerate).Distinct().ToArray();
            var loaded = Local.UpdateChunksLoadedByPlayer(allChunksToLoad, playerId);
            var unloaded = Local.UpdateChunksUnloadedByPlayer(chunksNoLongerNeeded, playerId);
            var generated = Local._world.GetChunks(new List<Vector2>(chunksToGenerate));
    
            if (unloaded.Count != 0)
            {
                RpcUnloadChunks(unloaded.ToArray());
            }
    
            if (generated.Count != 0)
            {
                RpcReceiveChunks(NetworkChunk.ChunkListToNetworkChunkArray(generated));
            }

            Debug.Log("[WorldController] - CmdPlayerChunkUpdate(Vector2[], Vector2[], Vector2[], int) \n" 
                    + "playerId: " + playerId.ToString() + "\n"
                    + "Server playerId: " + playerControllerId);
        }

        [Command]
        private void CmdRequestAllLoadedChunks()
        {
            const int max = 25;
            int chunksLeft = Local._world.ChunksLoaded.Count;
            int chunksProcessed = 0;
            List<Chunk> chunksToSend = new List<Chunk>(max);

            foreach (var chunkPair in Local._world.ChunksLoaded)
            {
                chunksToSend.Add(chunkPair.Value);

                --chunksLeft;
                bool lastSend = chunksLeft == 0;
                if(++chunksProcessed == max || lastSend)
                {
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

                entityIds.Add(entityInstance.GetId());
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

        [TargetRpc]
        private void TargetReceiveChunks(NetworkConnection conn, NetworkChunk[] networkChunks, bool finished)
        {
            Chunk[] unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks).ToArray();
            _world.AddChunks(unpackedChunks);
            _hasInitialised = finished;
            Debug.Log("[WorldController] - TargetReceiveChunks(NetworkConnection, NetworkChunk[])");
        }

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
        /// <summary>
        /// Rpc informing all clients to remove the chunks provided from their list of loaded chunks
        /// </summary>
        /// <param name="chunksOrderedToUnload">A collection of chunk positions to unload</param>
        [ClientRpc]
        private void RpcUnloadChunks(Vector2[] chunksOrderedToUnload)
        {
            foreach (var chunk in chunksOrderedToUnload)
            {
                _world.ChunksLoaded.Remove(chunk);
            }
        }
    
        [ClientRpc]
        private void RpcReceiveChunkDependencies(int playerId, Vector2[] chunkPositions)
        {
            if(!isServer)
            {
                Local._world.UpdatePlayerChunks(playerId, chunkPositions);
            }
        }

        [ClientRpc]
        private void RpcReceiveChunksWithDependencies(NetworkChunk[] networkChunks, int playerId, Vector2[] chunkPositions)
        {
            Chunk[] unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks).ToArray();
            Local._world.AddChunks(unpackedChunks);
            if(!isServer)
            {
                Local._world.UpdatePlayerChunks(playerId, chunkPositions);
            }
            if(Local.GameManager.PlayerController.Id == playerId)
            {
                Local.UpdateChunkControllers(Local._world.GetPlayerLoadedChunks(playerId), null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="networkChunks"></param>
        [ClientRpc]
        private void RpcReceiveChunks(NetworkChunk[] networkChunks)
        {
            Chunk[] unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks).ToArray();
            Local._world.AddChunks(unpackedChunks);
        }
        #endregion
    }
}
