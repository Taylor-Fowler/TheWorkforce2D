﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct NetworkChunk
{
    public Vector2 Position;
    public Tile[] Tiles;

    public NetworkChunk(Chunk chunk)
    {
        Position = chunk.Position;
        Tiles = new Tile[Chunk.AREA];

        for (int x = 0; x < Chunk.SIZE; x++)
        for (int y = 0; y < Chunk.SIZE; y++)
            Tiles[x * Chunk.SIZE + y] = chunk.Tiles[x, y];
    }

    public static NetworkChunk[] ChunkListToNetworkChunkArray(List<Chunk> chunks)
    {
        NetworkChunk[] networkChunks = new NetworkChunk[chunks.Count];
        for (int i = 0; i < chunks.Count; i++) networkChunks[i] = new NetworkChunk(chunks[i]);

        return networkChunks;
    }
}

public class ChunkMessage : MessageBase
{
    public NetworkChunk[] Chunks;
}