﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    [System.Serializable]
    public class Chunk
    {
        #region Constants + Statics
        /// <summary>
        /// The width and height of each Chunk (in number of tiles).
        /// </summary>
        public const int SIZE = 8;

        /// <summary>
        /// The area of each Chunk (number of tiles per chunk).
        /// </summary>
        public const int AREA = SIZE * SIZE;

        /// <summary>
        /// The default number of Chunks to keep loaded in both the x and y direction,
        /// so the actual default number of chunks to keep loaded is KEEP_LOADED^2.
        /// </summary>
        public const int KEEP_LOADED = 7;

        /// <summary>
        /// Calculates whether the given tile offset is valid.
        /// </summary>
        /// <param name="tileOffset">The tile offset to validate</param>
        /// <returns>True if both axes are positive and less than the defined Chunk.SIZE</returns>
        public static bool ValidTileOffset(Vector2Int tileOffset)
        {
            return tileOffset.x < SIZE && tileOffset.x >= 0 
                && tileOffset.y < SIZE && tileOffset.y >= 0;
        }


        /// <summary>
        /// Calculates the Chunk that the given `worldPosition` resides in.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <returns>The Chunk that the `worldPosition` belongs in.</returns>
        public static Vector2Int CalculateResidingChunk(Vector2Int worldPosition)
        {
            
            int x = (int)Mathf.Floor((float)worldPosition.x / SIZE);
            int y = (int)Mathf.Floor((float)worldPosition.y / SIZE);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Calculates the World Position from a given Chunk position, the returned World
        /// Position represents where the Chunk starts in the World (i.e bottom left corner
        /// of the Chunk.
        /// </summary>
        /// <param name="chunkPosition">The position of a Chunk.</param>
        /// <returns>The World Position of the Chunk.</returns>
        public static Vector2Int CalculateWorldPosition(Vector2Int chunkPosition) => chunkPosition * SIZE;

        /// <summary>
        /// Unpacks NetworkChunks into regular Chunks.
        /// </summary>
        /// <param name="networkChunks">The NetworkChunks to unpack.</param>
        /// <returns></returns>
        public static List<Chunk> UnpackNetworkChunks(NetworkChunk[] networkChunks)
        {
            List<Chunk> unloadedChunks = new List<Chunk>();

            foreach (var netChunk in networkChunks)
            {
                unloadedChunks.Add(new Chunk(netChunk));
            }

            return unloadedChunks;
        }

        /// <summary>
        /// Calculates the chunks that surround a position in the world (including the
        /// Chunk occupied by the `worldPosition`).
        /// </summary>
        /// <param name="worldPosition">The world position to find local Chunks for.</param>
        /// <returns>An array of Chunk positions that surround the `worldPosition`.</returns>
        public static Vector2Int[] SurroundingChunksOfWorldPosition(Vector2Int worldPosition)
        {
            Vector2Int currentChunk = CalculateResidingChunk(worldPosition);
            Vector2Int[] chunksToLoad = new Vector2Int[KEEP_LOADED * KEEP_LOADED];

            int halfLoaded = Mathf.FloorToInt(KEEP_LOADED * 0.5f);

            for (int x = 0; x < KEEP_LOADED; x++)
                for (int y = 0; y < KEEP_LOADED; y++)
                {
                    chunksToLoad[x * KEEP_LOADED + y] = currentChunk + new Vector2Int(x - halfLoaded, y - halfLoaded);
                }

            return chunksToLoad;
        }

        public static List<Vector2Int> ListOfSurroundingChunksOfWorldPosition(Vector2Int worldPosition)
        {
            Vector2Int currentChunkPosition = CalculateResidingChunk(worldPosition);
            List<Vector2Int> chunkPositionsToLoad = new List<Vector2Int>(SIZE * SIZE);

            int halfLoaded = Mathf.FloorToInt(KEEP_LOADED * 0.5f);

            for (int x = 0; x < KEEP_LOADED; x++)
                for (int y = 0; y < KEEP_LOADED; y++)
                {
                    chunkPositionsToLoad.Add(currentChunkPosition + new Vector2Int(x - halfLoaded, y - halfLoaded));
                }

            return chunkPositionsToLoad;
        }
        #endregion

        public event Action<Chunk> OnInitialise;

        public bool IsInitialised { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the chunk should stay loaded regardless of
        /// player vicinity.
        /// </summary>
        /// <returns>True if the chunk needs to be kept loaded</returns>
        public bool KeepLoaded { get; }

        /// <summary>
        /// The position of the Chunk in the chunk grid, not to be confused with world position.
        /// E.G. Chunk.Position(1, 1) = World.Position(1 * Chunk.SIZE, 1 * Chunk.SIZE)
        /// </summary>
        public Vector2Int Position { get; }
    
        /// <summary>
        /// The tiles stored in the chunk, the first dimension being the x axis position and the
        /// second position being the y axis position.
        /// </summary>
        public Tile[,] Tiles { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Chunk" /> class. Allocates memory
        /// for the `Tiles` array, sets the Chunk `Position`and initialises KeepLoaded to
        /// false.
        /// </summary>
        /// <param name="position">The position of the Chunk.</param>
        public Chunk(Vector2Int position)
        {
            Position = position;
            Tiles = new Tile[SIZE, SIZE];
            KeepLoaded = false;
        }
    
        /// <summary>
        /// Reconstructs Chunk from a NetworkChunk object, this constructor is used for
        /// initialising Chunks that have been sent across the Network.
        /// </summary>
        /// <param name="networkChunk">The network chunk.</param>
        public Chunk(NetworkChunk networkChunk) : this(new Vector2Int(networkChunk.X, networkChunk.Y))
        {
            for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
            {
                Tiles[x, y] = new Tile(networkChunk.NetworkTiles[x * SIZE + y], new Vector2Int(x, y));
            }
        }

        public Tile GetTile(Vector2Int tileOffset)
        {
            Tile tile = null;
            if(ValidTileOffset(tileOffset))
            {
                tile = Tiles[(int)tileOffset.x, (int)tileOffset.y];
            }

            return tile;
        }


        /// <summary>
        /// Calculates the World Position of the Chunk, the returned World Position
        /// represents where the Chunk starts in the World (i.e bottom left corner of the
        /// Chunk.
        /// </summary>
        /// <returns>The World Position of the Chunk.</returns>
        public Vector2Int WorldPosition() => Position * SIZE;

        public void Unload()
        {
            foreach(var tile in Tiles)
            {
                tile.Unload();
            }
        }

        public void Initialise()
        {
            IsInitialised = true;
            OnInitialise?.Invoke(this);
            OnInitialise = null;
        }
    }    
}
