using System.Collections.Generic;
using UnityEngine;

public class WorldGeneration
{
    private World _world;

    public WorldGeneration(World world)
    {
        this._world = world;
    }

    public List<Chunk> UpdateWorld(Vector2 position)
    {
        Vector2[] chunksToLoad = Chunk.ChunksToLoad(position);
        List<Chunk> spawnedChunks = new List<Chunk>();

        chunksToLoad = this._world.NotLoaded(chunksToLoad);

        foreach(var chunkPosition in chunksToLoad)
        {
            Chunk chunk = new Chunk(chunkPosition);
            spawnedChunks.Add(chunk);
            for (int x = 0; x < Chunk.SIZE; x++)
                for (int y = 0; y < Chunk.SIZE; y++)
                {
                    Vector2 worldChunkPosition = chunk.WorldPositon();

                    float noise = this.GetNoise((int)worldChunkPosition.x + x, (int)worldChunkPosition.y + y);

                    chunk.Tiles[x, y] = new Tile {
                        Position = new Vector2(x, y),
                        TilesetID = noise < 0.333f ? 0 : (noise > 0.666f) ? 1 : 2
                    };
                }
        }

        this._world.UpdateChunks(spawnedChunks.ToArray());
        return spawnedChunks;
    }

    private float GetNoise(int x, int y)
    {
        float xModifier = (float)this._world.Seed / int.MaxValue;
        float yModifier = (float)this._world.Seed / int.MaxValue;
        if (x < 0)
        {
            xModifier += (float)this._world.NegativeXSeed / int.MaxValue;
            xModifier *= 0.5f;
            //xModifier *= xModifier;
            //xModifier *= 0.666f;
        }

        xModifier *= 0.5f;

        if(y < 0)
        {
            yModifier += (float)this._world.NegativeYSeed / int.MaxValue;
            yModifier *= 0.5f;
            //yModifier *= yModifier;
            //yModifier *= 0.666f;
        }

        yModifier *= 0.5f;


        return Mathf.PerlinNoise(x * xModifier, y * yModifier);
    }


}
