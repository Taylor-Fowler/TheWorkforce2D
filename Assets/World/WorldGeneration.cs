using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    public class WorldGeneration
    {
        #region Public Properties
        public readonly int Seed;
        public readonly int NegativeXSeed;
        public readonly int NegativeYSeed;
        #endregion

        public WorldGeneration(int seed, int negativeXSeed, int negativeYSeed)
        {
            Seed = seed;
            NegativeXSeed = negativeXSeed;
            NegativeYSeed = negativeYSeed;
        }
    
        public List<Chunk> GenerateChunks(IEnumerable<Vector2> positionsOfChunksToGenerate)
        {
            List<Chunk> chunks = new List<Chunk>();
    
            foreach (var position in positionsOfChunksToGenerate)
            {
                Chunk chunk = new Chunk(position);
                chunks.Add(chunk);
    
                for (int x = 0; x < Chunk.SIZE; x++)
                { 
                    for (int y = 0; y < Chunk.SIZE; y++)
                    {
                        Vector2 worldChunkPosition = chunk.WorldPosition();
    
                        float noise = GetNoise((int)worldChunkPosition.x + x, (int)worldChunkPosition.y + y);
                        
                        Tile tile = new Tile(
                            (byte)(noise < 0.333f ? 0 : noise > 0.666f ? 1 : 2),
                            GetNoise((int)worldChunkPosition.x + x, (int)worldChunkPosition.y + y, noise),
                            noise,
                            new Vector2(x, y));

                        GenerateGeneratableItems(tile, worldChunkPosition);
                        chunk.Tiles[x, y] = tile;
                    }
                }
            }

            return chunks;
        }

        private void GenerateGeneratableItems(Tile tile, Vector2 chunkWorldPosition)
        {
            float noise = GetNoise((int)(chunkWorldPosition.x + tile.Position.x), (int)(chunkWorldPosition.y + tile.Position.y), tile.Elevation + tile.Moisture);
            if(tile.Elevation + tile.Moisture >= 0.133f)
            {
                List<Generatable> generatables = Generation.GetGeneratables(tile.Moisture, tile.Elevation);

                if(generatables.Count != 0)
                {
                    // Noise is between 1f and 0.666f, find out its value between 0 and 0.333f
                    //noise = 1f - noise;
                    // Get a value between 0 and 1
                    //noise /= 1f - 0.666f;

                    float weightPerItem = 1f / generatables.Count;
                    int index = Mathf.FloorToInt(noise / weightPerItem);
                    Vector2 worldPosition = tile.Position + chunkWorldPosition;
                    tile.PlaceEntity(Entities.EntityCollection.Instance().CreateEntity(generatables[index].ItemId, (int)worldPosition.x, (int)worldPosition.y));
                }
            }
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

        private float GetNoise(int x, int y, float modifier)
        {
            float xModifier = (float)Seed / int.MaxValue;
            float yModifier = (float)Seed / int.MaxValue;
            if (x < 0)
            {
                xModifier += (float)NegativeXSeed / int.MaxValue;
                xModifier *= 0.5f;
            }
            xModifier *= 0.5f * modifier * 0.48917f;

            if (y < 0)
            {
                yModifier += (float)NegativeYSeed / int.MaxValue;
                yModifier *= 0.5f;
            }
            yModifier *= 0.5f * modifier * (modifier * 0.98163f);
            return Mathf.PerlinNoise(x * xModifier, y * yModifier);
        }
    }
}
