using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct Chunk 
{
    public const int SIZE = 16;
    public const int AREA = SIZE * SIZE;
    public const int KEEP_LOADED = 5;

    public Vector2 Position;
    public Tile[,] Tiles;

    public Chunk(Vector2 position)
    {
        this.Position = position;
        this.Tiles = new Tile[SIZE, SIZE];
    }

    public Chunk(NetworkChunk networkChunk) : this(networkChunk.Position)
    {
        for(int x = 0; x < SIZE; x++)
            for(int y = 0; y < SIZE; y++)
                this.Tiles[x, y] = networkChunk.Tiles[x * SIZE + y];
    }

    public Vector2 WorldPositon()
    {
        return this.Position * SIZE;
    }

    public static Vector2 CalculateCurrentChunk(Vector2 position)
    {
        Vector2 current = position / SIZE;
        current.x = Mathf.Floor(current.x);
        current.y = Mathf.Floor(current.y);

        if(position.x < 0) current.x -= 1f;
        if(position.y < 0) current.y -= 1f;

        return current;
    }

    public static Vector2 CalculateWorldPosition(Vector2 chunkPosition)
    {
        return chunkPosition * Chunk.SIZE;
    }

    public static List<Chunk> UnpackNetworkChunks(NetworkChunk[] networkChunks)
    {
        List<Chunk> unloadedChunks = new List<Chunk>();

        foreach(var netChunk in networkChunks)
        {
            unloadedChunks.Add(new Chunk(netChunk));
        }

        return unloadedChunks;
    }


    public static Vector2[] ChunksToLoad(Vector2 position)
    {
        Vector2 currentChunk = CalculateCurrentChunk(position);
        Vector2[] chunksToLoad = new Vector2[KEEP_LOADED * KEEP_LOADED];

        int halfLoaded = Mathf.FloorToInt(KEEP_LOADED * 0.5f);

        for (int x = 0; x < KEEP_LOADED; x++)
            for (int y = 0; y < KEEP_LOADED; y++)
            {
                chunksToLoad[x * KEEP_LOADED + y] = currentChunk + new Vector2(x - halfLoaded, y - halfLoaded);
            }

        return chunksToLoad;
    }
}

public struct NetworkChunk
{
    public Vector2 Position;
    public Tile[] Tiles;

    public NetworkChunk(Chunk chunk)
    {
        this.Position = chunk.Position;
        this.Tiles = new Tile[Chunk.AREA];

        for(int x = 0; x < Chunk.SIZE; x++)
            for(int y = 0; y < Chunk.SIZE; y++)
                this.Tiles[x * Chunk.SIZE + y] = chunk.Tiles[x, y];
    }
}

public class ChunkMessage : MessageBase
{
    public NetworkChunk[] Chunks;
}
