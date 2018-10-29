using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class WorldController : NetworkBehaviour
{
    private static WorldController LocalWorldController;

    // The Client-Local player controller
    public PlayerController LocalPlayerController;


    private readonly List<ChunkController> _chunkControllers = new List<ChunkController>();
    private Dictionary<Vector2, List<int>> _allChunksLoadedByPlayerPositions;

    private GameObject _gameWorldAnchorObject;
    private World _world;


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        TerrainTileSet.InitialiseTileSets();
        LocalWorldController = this;

        // If the current script is the server client
        if (isServer)
        {
            // 1. Create a brand new world with a completely random seed
            // 2. Pass that world to the world generation class
            // 3. Update the world generation with the current player (server client) position
            //      - If any of the surrounding chunks required in relation to the position of the player have not been loaded,
            //      - use the world generation to return a list of chunks that have been added to the world and must be spawned
            //      - in the local game world.

            _world = new WorldGeneration(784893570);
            _allChunksLoadedByPlayerPositions = new Dictionary<Vector2, List<int>>();

            Debug.Log("Server Seed: " + _world.Seed);
        }
    }

    public IEnumerator SetInitialPlayerPosition(Vector2 playerPosition)
    {
        SpawnChunkControllers();

        yield return new WaitForSeconds(0.1f);
        _world.UpdateChunksSurroundingPlayer(Chunk.ListOfSurroundingChunksOfWorldPosition(playerPosition));
        Vector2[] surroundingChunks = _world.ChunksSurroundingPlayer.ToArray();
        CmdPlayerChunkUpdate(new Vector2[0], surroundingChunks, surroundingChunks, LocalPlayerController.ID);
    }

    public void UpdatePlayerPosition(Vector2 playerPosition)
    {
        // What happens when a player moves to a new chunk?
        // 3 players: A (host), B and C (regular clients)
        // Players A and B are in the same chunk
        // Player C moves into a new chunk:
        // 1. Tell the Host about the chunks that were kept loaded by Player C
        //    that are no longer needed to be loaded and also about the new chunks that player
        //    C is keeping loaded
        // 2. The Host checks if any other player needs the chunks that have been asked to
        //    be unloaded by player C. The Host notifies all clients to unload said chunks.
        // 3. The Host then tells all clients to load the chunks requested by player C.
        // 4. Finally, the Host generates the chunks requested by player C and sends the data
        //    to all clients

        Vector2 playerChunk = Chunk.CalculateResidingChunk(playerPosition);
        // Get all the chunks in the vicinity of the player
        List<Vector2> chunksToGenerate = Chunk.ListOfSurroundingChunksOfWorldPosition(playerPosition);

        // Find the chunks that need to be loaded, which are all the generated chunks,
        // minus all of the chunks that are already loaded
        List<Vector2> chunksToLoad = chunksToGenerate.Except(_world.ChunksSurroundingPlayer).ToList();

        // Find the chunks that are no longer in the vicinity of the player,
        // Previously surrounding chunks - chunks that are currently surrounding
        // = chunks no longer surrounding
        var chunksToUnload = _world.ChunksSurroundingPlayer.Except(chunksToGenerate).ToArray();

        // Update the chunks surrounding the player now that we know what has changed
        _world.UpdateChunksSurroundingPlayer(chunksToGenerate);

        // Remove all the chunks that are loaded from our list of chunks to generate
        _world.FilterChunkPositionsThatAreLoaded(chunksToGenerate);


        chunksToLoad = chunksToLoad.Except(chunksToGenerate).ToList();

        CmdPlayerChunkUpdate(chunksToUnload, chunksToLoad.ToArray(), chunksToGenerate.ToArray(),
            LocalPlayerController.ID);
    }

    private void SpawnChunkControllers()
    {
        _gameWorldAnchorObject = new GameObject
        {
            name = "World"
        };
        _gameWorldAnchorObject.transform.position = new Vector3(0f, 0f, 1f);

        for (int x = 0; x < Chunk.KEEP_LOADED; x++)
        for (int y = 0; y < Chunk.KEEP_LOADED; y++)
        {
            GameObject chunkObject = new GameObject();
            chunkObject.transform.SetParent(_gameWorldAnchorObject.transform);

            _chunkControllers.Add(chunkObject.AddComponent<ChunkController>());
        }
    }

    private void UpdateChunkControllers(List<Chunk> chunksToDisplay, List<Vector2> chunksToKeepDisplayed)
    {
        // 1. Get a collection of the chunks needed displaying
        // 2. Remove chunks that are already being displayed from both the
        //    chunk controllers collection and needed to display collection
        // 3. Loop through the remaining controllers and assign a chunk from 
        //    the need to display collection        


        List<ChunkController> chunkControllersToSet = _chunkControllers
            .Where(value => chunksToKeepDisplayed.Contains(value.Chunk.Position) != true)
            .ToList();

        for (int i = 0; i < chunksToDisplay.Count; i++)
            chunkControllersToSet[i].SetChunk(chunksToDisplay[i], _world);
    }

    private List<Vector2> UpdateChunksLoadedByPlayer(Vector2[] chunkPositions, int playerID)
    {
        List<Vector2> chunksNewlyLoaded = new List<Vector2>();

        foreach (var chunkPosition in chunkPositions)
            if (_allChunksLoadedByPlayerPositions.ContainsKey(chunkPosition))
            {
                _allChunksLoadedByPlayerPositions[chunkPosition].Add(playerID);
            }
            else
            {
                _allChunksLoadedByPlayerPositions.Add(chunkPosition, new List<int> {playerID});
                chunksNewlyLoaded.Add(chunkPosition);
            }

        return chunksNewlyLoaded;
    }

    private List<Vector2> UpdateChunksUnloadedByPlayer(Vector2[] chunkPositions, int playerID)
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
                    _allChunksLoadedByPlayerPositions[chunkPosition].Remove(playerID);
                }
            }
        }

        return chunksSuccessfullyUnloaded;
    }

    [Command]
    private void CmdPlayerChunkUpdate(Vector2[] chunksNoLongerNeeded, Vector2[] chunksLoaded, Vector2[] chunksToGenerate, int playerID)
    {
        Vector2[] allChunksToLoad = chunksLoaded.Concat(chunksToGenerate).Distinct().ToArray();
        var loaded = LocalWorldController.UpdateChunksLoadedByPlayer(allChunksToLoad, playerID);
        var unloaded = LocalWorldController.UpdateChunksUnloadedByPlayer(chunksNoLongerNeeded, playerID);
        var generated = LocalWorldController._world.GetChunks(new List<Vector2>(chunksToGenerate));

        if (unloaded.Count != 0)
        {
            RpcUnloadChunks(unloaded.ToArray());
        }

        if (generated.Count != 0)
        {
            RpcReceiveChunks(NetworkChunk.ChunkListToNetworkChunkArray(generated));
        }
    }

    [ClientRpc]
    private void RpcUnloadChunks(Vector2[] chunksOrderedToUnload)
    {
        foreach (var chunk in chunksOrderedToUnload)
        {
            _world.LoadedChunks.Remove(chunk);
        }
    }

    [ClientRpc]
    private void RpcReceiveChunks(NetworkChunk[] networkChunks)
    {
        Chunk[] unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks).ToArray();
        LocalWorldController._world.AddLoadedChunks(unpackedChunks);

        List<ChunkController> chunkControllersToSet = _chunkControllers
            .Where(value =>
                value.Chunk == null || _world.ChunksSurroundingPlayer.Contains(value.Chunk.Position) != true)
            .ToList();

        List<Chunk> chunksToDisplay = unpackedChunks
            .Where(value => _world.ChunksSurroundingPlayer.Contains(value.Position))
            .ToList();

        for (int i = 0; i < chunksToDisplay.Count; i++)
        {
            chunkControllersToSet[i].SetChunk(chunksToDisplay[i], _world);
        }
    }

    //[Command]
    //private void CmdRequestWorld()
    //{
    //    Chunk[] chunks = ServerWorldController._world.GetLoadedChunksArray();
    //    NetworkChunk[] networkChunks = new NetworkChunk[chunks.Length];

    //    for(int i = 0; i < chunks.Length; i++)
    //    {
    //        networkChunks[i] = new NetworkChunk(chunks[i]);
    //    }

    //    this.TargetReceiveWorld(this.connectionToClient, ServerWorldController._world.Seed, networkChunks);
    //}

    //[TargetRpc]
    //private void TargetReceiveWorld(NetworkConnection connection, int seed, NetworkChunk[] networkChunks)
    //{
    //    this._world = new World(seed);
    //    this._worldGeneration = new WorldGeneration(this._world);
    //    this.CmdMessage("Client Received Seed: " + this._world.Seed);

    //    List<Chunk> unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks);
    //    this._world.AddChunksToLoadedChunks(unpackedChunks.ToArray());
    //    //this.SpawnTiles(unpackedChunks);
    //}

    [Command]
    private void CmdMessage(string message)
    {
        Debug.Log(message);
    }
}