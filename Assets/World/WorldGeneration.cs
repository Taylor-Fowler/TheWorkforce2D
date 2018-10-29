using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration : World
{
    public WorldGeneration(int seed) : base(seed)
    {
    }

    public override List<Chunk> GetChunks(List<Vector2> chunkPositions)
    {
        List<Chunk> chunksFound = base.GetChunks(chunkPositions);

        if (chunkPositions.Count == 0) return chunksFound;

        LoadGeneratedChunks(chunkPositions, chunksFound);
        chunksFound.AddRange(GenerateChunks(chunkPositions));
        return chunksFound;
    }

    public List<Chunk> GenerateChunks(List<Vector2> positionsOfchunksToGenerate)
    {
        List<Chunk> chunks = new List<Chunk>();

        foreach (var position in positionsOfchunksToGenerate)
        {
            Chunk chunk = new Chunk(position);
            chunks.Add(chunk);

            for (int x = 0; x < Chunk.SIZE; x++)
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                Vector2 worldChunkPosition = chunk.WorldPosition();

                float noise = GetNoise((int) worldChunkPosition.x + x, (int) worldChunkPosition.y + y);

                chunk.Tiles[x, y] = new Tile
                {
                    Position = new Vector2(x, y),
                    Elevation = noise, 
                    TilesetID = noise < 0.333f ? 0 : noise > 0.666f ? 1 : 2
                };
            }
        }

        return chunks;
    }

    private float GetNoise(int x, int y)
    {
        float xModifier = (float) Seed / int.MaxValue;
        float yModifier = (float) Seed / int.MaxValue;
        if (x < 0)
        {
            xModifier += (float) NegativeXSeed / int.MaxValue;
            xModifier *= 0.5f;
            //xModifier *= xModifier;
            //xModifier *= 0.666f;
        }

        xModifier *= 0.5f;

        if (y < 0)
        {
            yModifier += (float) NegativeYSeed / int.MaxValue;
            yModifier *= 0.5f;
            //yModifier *= yModifier;
            //yModifier *= 0.666f;
        }
        yModifier *= 0.5f;

        return Mathf.PerlinNoise(x * xModifier, y * yModifier);
    }
}