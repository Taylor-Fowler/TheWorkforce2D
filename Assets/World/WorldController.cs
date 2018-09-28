using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WorldController : NetworkBehaviour
{
    public GameObject TilePrefab;

    private static WorldController _serverWorldController;

    private World _world;
    private WorldGeneration _worldGeneration;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        TerrainTileset.InitialiseTilesets();

        // If the current script is the server client
        if(this.isServer)
        {
            // 1. Create a brand new world with a completely random seed
            // 2. Pass that world to the world generation class
            // 3. Update the world generation with the current player (server client) position
            //      - If any of the surrounding chunks required in relation to the position of the player have not been loaded,
            //      - use the world generation to return a list of chunks that have been added to the world and must be spawned
            //      - in the local game world.

            this._world = new World(784893570);
            this._worldGeneration = new WorldGeneration(this._world);
            this.SpawnTiles(this._worldGeneration.UpdateWorld(new Vector2(0f, 0f)));
            _serverWorldController = this;

            Debug.Log("Server Seed: " + this._world.Seed);
        }
        else
        {
            this.CmdRequestWorld();
        }
    }

    private void SpawnTiles(List<Chunk> chunks)
    {
        foreach(var chunk in chunks)
            foreach(var tile in chunk.Tiles)
            {
                GameObject go = Instantiate(this.TilePrefab);
                go.GetComponent<SpriteRenderer>().sprite = TerrainTileset.LoadedTilesets[tile.TilesetID].Tiles[TerrainTileset.CENTRAL];
                go.transform.position = tile.GetWorldPosition(chunk.Position);
                this.SpawnPadding(chunk, tile, go);
            }
    }

    private void SpawnPadding(Chunk chunk, Tile tile, GameObject parentTile)
    {
        var padding = this._world.GetTilePadding(chunk, tile);
        foreach (var paddingType in padding)
        {
            Sprite[] sprites = TerrainTileset.LoadedTilesets[paddingType.Key].GetPaddingSprites(paddingType.Value);

            foreach (var sprite in sprites)
            {
                GameObject go = Instantiate(this.TilePrefab, parentTile.transform);
                go.transform.position = tile.GetWorldPositionPrecedence(chunk.Position, paddingType.Key);
                go.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }

    private void SpawnPadding(List<Chunk> chunks)
    {
        foreach(var chunk in chunks)
            foreach(var tile in chunk.Tiles)
            {
                var padding = this._world.GetTilePadding(chunk, tile);
                foreach(var paddingType in padding)
                {
                    Sprite[] sprites = TerrainTileset.LoadedTilesets[paddingType.Key].GetPaddingSprites(paddingType.Value);

                    foreach (var sprite in sprites)
                    {
                        GameObject go = Instantiate(this.TilePrefab);
                        go.transform.position = tile.GetWorldPositionPrecedence(chunk.Position, paddingType.Key);
                        go.GetComponent<SpriteRenderer>().sprite = sprite;
                    }
                }
            }
    }

    [Command]
    private void CmdRequestWorld()
    {
        Chunk[] chunks = _serverWorldController._world.RequestAllChunks();
        NetworkChunk[] networkChunks = new NetworkChunk[chunks.Length];

        for(int i = 0; i < chunks.Length; i++)
        {
            networkChunks[i] = new NetworkChunk(chunks[i]);
        }

        this.TargetReceiveWorld(this.connectionToClient, _serverWorldController._world.Seed, networkChunks);
    }

    [TargetRpc]
    private void TargetReceiveWorld(NetworkConnection connection, int seed, NetworkChunk[] networkChunks)
    {
        this._world = new World(seed);
        this._worldGeneration = new WorldGeneration(this._world);
        this.CmdMessage("Client Received Seed: " + this._world.Seed);

        List<Chunk> unpackedChunks = Chunk.UnpackNetworkChunks(networkChunks);
        this._world.UpdateChunks(unpackedChunks.ToArray());
        this.SpawnTiles(unpackedChunks);
    }

    [Command]
    private void CmdMessage(string message)
    {
        Debug.Log(message);
    }
}
