using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    [System.Serializable]
    public class Chunk
    {
        #region Static Methods
        public static bool ValidTileOffset(Vector2 tileOffset)
        {
            return tileOffset.x < SIZE && tileOffset.x >= 0 
                && tileOffset.y < SIZE && tileOffset.y >= 0;
        }


        /// <summary>
        ///     Calculates the Chunk that the given `worldPosition` resides in.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <returns>The Chunk that the `worldPosition` belongs in.</returns>
        public static Vector2 CalculateResidingChunk(Vector2 worldPosition)
        {
            Vector2 current = worldPosition / SIZE;
            current.x = Mathf.Floor(current.x);
            current.y = Mathf.Floor(current.y);

            return current;
        }

        /// <summary>
        ///     Calculates the World Position from a given Chunk position, the returned World
        ///     Position represents where the Chunk starts in the World (i.e bottom left corner
        ///     of the Chunk.
        /// </summary>
        /// <param name="chunkPosition">The position of a Chunk.</param>
        /// <returns>The World Position of the Chunk.</returns>
        public static Vector2 CalculateWorldPosition(Vector2 chunkPosition)
        {
            return chunkPosition * SIZE;
        }

        /// <summary>
        ///     Unpacks NetworkChunks into regular Chunks.
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
        ///     Calculates the chunks that surround a position in the world (including the
        ///     Chunk occupied by the `worldPosition`).
        /// </summary>
        /// <param name="worldPosition">The world position to find local Chunks for.</param>
        /// <returns>An array of Chunk positions that surround the `worldPosition`.</returns>
        public static Vector2[] SurroundingChunksOfWorldPosition(Vector2 worldPosition)
        {
            Vector2 currentChunk = CalculateResidingChunk(worldPosition);
            Vector2[] chunksToLoad = new Vector2[KEEP_LOADED * KEEP_LOADED];

            int halfLoaded = Mathf.FloorToInt(KEEP_LOADED * 0.5f);

            for (int x = 0; x < KEEP_LOADED; x++)
                for (int y = 0; y < KEEP_LOADED; y++)
                {
                    chunksToLoad[x * KEEP_LOADED + y] = currentChunk + new Vector2(x - halfLoaded, y - halfLoaded);
                }

            return chunksToLoad;
        }

        public static List<Vector2> ListOfSurroundingChunksOfWorldPosition(Vector2 worldPosition)
        {
            Vector2 currentChunkPosition = CalculateResidingChunk(worldPosition);
            List<Vector2> chunkPositionsToLoad = new List<Vector2>(SIZE * SIZE);

            int halfLoaded = Mathf.FloorToInt(KEEP_LOADED * 0.5f);

            for (int x = 0; x < KEEP_LOADED; x++)
                for (int y = 0; y < KEEP_LOADED; y++)
                {
                    chunkPositionsToLoad.Add(currentChunkPosition + new Vector2(x - halfLoaded, y - halfLoaded));
                }

            return chunkPositionsToLoad;
        }
        #endregion

        #region Public Constants
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
        public const int KEEP_LOADED = 5;
        #endregion

        #region Public Properties    
        /// <summary>
        ///     Gets a value indicating whether the chunk should stay loaded regardless of
        ///     player vicinity.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [keep loaded]; otherwise, <c>false</c>.
        /// </value>
        public bool KeepLoaded { get; }
        #endregion

        #region Public Members
        /// <summary>
        /// The position of the Chunk in the chunk grid, not to be confused with world position.
        /// E.G. Chunk.Position(1, 1) = World.Position(1 * Chunk.SIZE, 1 * Chunk.SIZE)
        /// </summary>
        public Vector2 Position;
    
        /// <summary>
        /// The tiles stored in the chunk, the first dimension being the x axis position and the
        /// second position being the y axis position.
        /// </summary>
        public Tile[,] Tiles;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new instance of the <see cref="Chunk" /> class. Allocates memory
        /// for the `Tiles` array, sets the Chunk `Position`and initialises KeepLoaded to
        /// false.
        /// </summary>
        /// <param name="position">The position of the Chunk.</param>
        public Chunk(Vector2 position)
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
        public Chunk(NetworkChunk networkChunk) : this(networkChunk.Position)
        {
            for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
            {
                Tiles[x, y] = new Tile(networkChunk.NetworkTiles[x * SIZE + y], new Vector2(x, y));
            }
        }
        #endregion

        #region Public Methods
        public Tile GetTile(Vector2 tileOffset)
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
        public Vector2 WorldPosition()
        {
            return Position * SIZE;
        }
        #endregion
    }    
}
