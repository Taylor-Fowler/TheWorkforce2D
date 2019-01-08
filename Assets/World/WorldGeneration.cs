﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TheWorkforce.Items;

namespace TheWorkforce.World
{
    public class WorldGeneration : WorldDetails
    {
        private readonly List<IGeneratable> _ascendingElevationRequirements;
        private readonly List<IGeneratable> _ascendingMoistureRequirements;

        public WorldGeneration(int seed, List<IGeneratable> generatables) : base(seed)
        {
            _ascendingElevationRequirements = new List<IGeneratable>(generatables);
            _ascendingMoistureRequirements = new List<IGeneratable>(generatables);

            _ascendingElevationRequirements.Sort((generatable1, generatable2) =>
            {
                if(generatable1.MinimumElevation < generatable2.MinimumElevation)
                {
                    return -1;
                }
                if(generatable1.MinimumElevation == generatable2.MinimumElevation && generatable1.MaximumElevation < generatable2.MaximumElevation)
                {
                    return -1;
                }
                return 1;
            });


            _ascendingMoistureRequirements.Sort((generatable1, generatable2) =>
            {
                if (generatable1.MinimumMoisture < generatable2.MinimumMoisture)
                {
                    return -1;
                }
                if (generatable1.MinimumMoisture == generatable2.MinimumMoisture && generatable1.MaximumMoisture < generatable2.MaximumMoisture)
                {
                    return -1;
                }
                return 1;
            });
        }
    
        public override List<Chunk> GetChunks(List<Vector2> chunkPositions)
        {
            List<Chunk> chunksFound = base.GetChunks(chunkPositions);
    
            if (chunkPositions.Count == 0)
            {
                return chunksFound;
            }
    
            LoadGeneratedChunks(chunkPositions, chunksFound);
            chunksFound.AddRange(GenerateChunks(chunkPositions));
            return chunksFound;
        }
    
        public List<Chunk> GenerateChunks(IEnumerable<Vector2> positionsOfChunksToGenerate)
        {
            List<Chunk> chunks = new List<Chunk>();
    
            foreach (var position in positionsOfChunksToGenerate)
            {
                Chunk chunk = new Chunk(position);
                chunks.Add(chunk);
    
                for (int x = 0; x < Chunk.SIZE; x++)
                    for (int y = 0; y < Chunk.SIZE; y++)
                    {
                        Vector2 worldChunkPosition = chunk.WorldPosition();
    
                        float noise = GetNoise((int) worldChunkPosition.x + x, (int) worldChunkPosition.y + y);
                        
                        Tile tile = new Tile
                        {
                            Position = new Vector2(x, y),
                            Elevation = noise,
                            Moisture = GetNoise((int)worldChunkPosition.x + x, (int)worldChunkPosition.y + y, noise),
                            TileSetId = (byte)(noise < 0.333f ? 0 : noise > 0.666f ? 1 : 2)
                        };

                        GenerateGeneratableItems(tile, worldChunkPosition);
                        chunk.Tiles[x, y] = tile;
                    }
            }
    
            return chunks;
        }

        private void GenerateGeneratableItems(Tile tile, Vector2 chunkWorldPosition)
        {
            float noise = GetNoise((int)(chunkWorldPosition.x + tile.Position.x), (int)(chunkWorldPosition.y + tile.Position.y), tile.Elevation + tile.Moisture);
            if(noise >= 0.666f)
            {
                List<IGeneratable> generatables = new List<IGeneratable>();

                foreach(var generatable in _ascendingElevationRequirements)
                {
                    if(generatable.CanGenerate(tile.Moisture, tile.Elevation))
                    {
                        generatables.Add(generatable);
                    }
                }

                if(generatables.Count != 0)
                {
                    // Noise is between 1f and 0.666f, find out its value between 0 and 0.333f
                    noise = 1f - noise;
                    // Get a value between 0 and 1
                    noise /= 1f - 0.666f;

                    float weightPerItem = 1f / generatables.Count;

                    int index = Mathf.FloorToInt(noise / weightPerItem);


                    //tile.ItemOnTileId = generatables[index] as IItem;
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
