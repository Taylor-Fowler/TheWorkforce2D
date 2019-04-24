using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using TheWorkforce.Static_Classes;

namespace TheWorkforce
{
    /// <summary>
    /// The World class manages the chunks and their status within the game.
    /// 
    /// Responsibilities:
    ///     -   Loading a chunk from file into memory
    ///     -   Saving a chunk to file and keeping the chunk in memory (when a new chunk is received from server)
    ///     -   Saving a chunk to file and then unloading the chunk from memory
    ///     -   Tracking which chunks are required by which player
    ///     -   Tracking which chunks are required to be loaded at all times
    /// </summary>
    [Serializable]
    public class World
    {
        public int Seed { get; }
        public int NegativeXSeed { get; }
        public int NegativeYSeed { get; }

        /// <summary>
        /// LoadedChunks contains all of the chunks that are currently in memory.
        /// </summary>
        public Dictionary<Vector2Int, Chunk> LoadedChunks { get; }

        public HashSet<Vector2Int> KnownChunks { get; }
    
        /// <summary>
        /// Stores all of the chunks that directly surround the local player.
        /// </summary>
        public List<Vector2Int> ChunksSurroundingLocalPlayer;

        /// <summary>
        /// Stores each chunk with a list of players which are keeping it loaded.
        /// </summary>
        public Dictionary<Vector2Int, List<int>> ChunkLoadedByPlayers;

        public Dictionary<int, List<Vector2Int>> PlayerLoadedChunks;

        /// <summary>
        /// Initializes a new instance of the <see cref="World" /> class. Generates a random seed.
        /// </summary>
        public World() : this(new Random().Next()) {}
    
        /// <summary>
        /// Initializes a new instance of the <see cref="World" /> class.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public World(int seed)
        {
            Seed = seed;
            Random random = new Random(Seed);
            NegativeXSeed = random.Next();
            NegativeYSeed = random.Next();

            KnownChunks = new HashSet<Vector2Int>();
            LoadedChunks = new Dictionary<Vector2Int, Chunk>();
            ChunksSurroundingLocalPlayer = new List<Vector2Int>();
            ChunkLoadedByPlayers = new Dictionary<Vector2Int, List<int>>();
            PlayerLoadedChunks = new Dictionary<int, List<Vector2Int>>();
        }

        public void RegisterKnownChunks(IEnumerable<Vector2Int> chunkPositions)
        {
            int count = 0;
            foreach(var chunkPosition in chunkPositions)
            {
                KnownChunks.Add(chunkPosition);
                ++count;
            }

            Debug.Log("[World] - RegisterKnownChunks(IEnumerable<Vector2Int>)\n" +
                        $"Chunks registered: {count}");
        }

        public void UpdatePlayerChunks(int playerId, IEnumerable<Vector2Int> chunks)
        {
            SetChunksLoadedByPlayers(playerId, chunks);
            SetPlayerLoadedChunks(playerId, chunks);
        }

        /// <summary>
        /// Finds all chunk positions that are directly loaded for the player identified by the given Id.
        /// </summary>
        /// <param name="playerId">The player Id to get chunk positions for</param>
        /// <returns>A collection of chunk positions that are loaded by `playerId`</returns>
        public List<Vector2Int> GetPlayerLoadedChunkPositions(int playerId)
        {
            List<Vector2Int> positions;
            if (PlayerLoadedChunks.TryGetValue(playerId, out positions))
            {
                return positions;
            }

            return new List<Vector2Int>();
        }

        /// <summary>
        /// Finds all chunks that are directly loaded for the player identified by the given Id.
        /// </summary>
        /// <param name="playerId">The player Id to get chunk positions for</param>
        /// <returns>A collection of chunks that are loaded by `playerId`</returns>
        public List<Chunk> GetPlayerLoadedChunks(int playerId)
        {
            List<Chunk> chunks = new List<Chunk>();

            // Get all of the chunk positions then use the position to access their mapped value in the 
            // dictionary
            var positions = GetPlayerLoadedChunkPositions(playerId);
            foreach(var position in positions)
            {
                chunks.Add(LoadedChunks[position]);
            }

            return chunks;
        }

        /// <summary>
        /// Clears the ChunksSurroundingLocalPlayer and adds the given chunk positions.
        /// </summary>
        /// <param name="chunksSurroundingPlayer"></param>
        /// 
        public void SetChunksSurroundingLocalPlayer(IEnumerable<Vector2Int> chunksSurroundingPlayer)
        {
            ChunksSurroundingLocalPlayer.Clear();
            ChunksSurroundingLocalPlayer.AddRange(chunksSurroundingPlayer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunks"></param>
        public void AddChunks(IEnumerable<Chunk> chunks)
        {
            // DEBUG: Remove for final build
            int count = 0;
            foreach (var chunk in chunks)
            {
                LoadedChunks.Add(chunk.Position, chunk);
                Game_State.GameFile.Instance.Save(chunk);
                ++count;
            }

            Debug.Log($"[World] - AddChunks(IEnumerable<Chunk>) \nChunks.Length: {count}");
        }

        public Chunk GetChunk(Vector2Int chunkPosition)
        {
            Chunk result;
            LoadedChunks.TryGetValue(chunkPosition, out result);
            return result;
        }

        public List<Chunk> GetChunks(List<Vector2Int> chunkPositions)
        {
            List<Chunk> result = new List<Chunk>();

            for(int i = chunkPositions.Count - 1; i >= 0; --i)
            {
                Chunk value;
                if(LoadedChunks.TryGetValue(chunkPositions[i], out value))
                {
                    result.Add(value);
                    chunkPositions.RemoveAt(i);
                }
            }

            return result;
        }
    
        /// <summary>
        /// Iterates over a list of chunk positions and removes any position that is loaded in the world
        /// </summary>
        /// <param name="chunkPositions">The collection of chunk positions to filter</param>
        public void FilterLoadedChunks(List<Vector2Int> chunkPositions)
        {
            for (int i = chunkPositions.Count - 1; i >= 0; i--)
            {
                if (LoadedChunks.ContainsKey(chunkPositions[i]))
                {
                    chunkPositions.RemoveAt(i);
                }
            }
        }  

        public List<Vector2Int> FilterKnownChunks(List<Vector2Int> chunkPositions)
        {
            Debug.Log("[World] - FilterKnownChunks(List<Vector2Int>)\n" +
                        $"Number of Chunks to look for: {chunkPositions.Count}\n" +
                        $"Known Chunks: {KnownChunks.Count}");
            List<Vector2Int> knownChunks = new List<Vector2Int>();
            for(int i = chunkPositions.Count -1; i >= 0; --i)
            {
                var chunkPosition = chunkPositions[i];
                if(KnownChunks.Contains(chunkPosition))
                {
                    knownChunks.Add(chunkPosition);
                    chunkPositions.RemoveAt(i);
                }
            }

            return knownChunks;
        }
    
        public bool CanUnloadChunk(Vector2Int chunkPositionToUnload)
        {
            return !LoadedChunks[chunkPositionToUnload].KeepLoaded;
        }
    
        public Dictionary<int, TilePadding> GetTilePadding(Chunk chunk, Tile tile)
        {
            Dictionary<int, TilePadding> tilePadding = new Dictionary<int, TilePadding>();
    
            TerrainTileSet current = TerrainTileSet.LoadedTileSets[tile.TileSetId];
    
            // top to bottom
            for (int yOffset = 1; yOffset >= -1; yOffset--)
            {
                // go from left to right
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    Tile neighbourTile = null;
    
                    // Check if the neighbour tile we are interested in is located in a different chunk
                    // If the tile position + offset is less than zero or equal to the size of a chunk then it means that the 
                    // tile is in a different chunk (negative when less than zero and positive when equal to chunk size in the axis that
                    // is being checked).
                    Vector2Int neighbourChunkDirection = Vector2Int.zero;
                    if (tile.Position.x + xOffset < 0)
                        neighbourChunkDirection.x = xOffset;
                    else if (tile.Position.x + xOffset == Chunk.SIZE)
                        neighbourChunkDirection.x = xOffset;
                    if (tile.Position.y + yOffset < 0)
                        neighbourChunkDirection.y = yOffset;
                    else if (tile.Position.y + yOffset == Chunk.SIZE)
                        neighbourChunkDirection.y = yOffset;
    
                    // The tile we want to inspect is in a different chunk if the direction has changed from zero, so we must check if the 
                    // chunk that we need (given by the direction) is actively loaded. If it is loaded then we get the value of that tile
                    // otherwise we continue looping through the surrounding tiles
                    if (neighbourChunkDirection != Vector2.zero)
                    {
                        //return new Dictionary<int, TilePadding>();
                        if (!GetNeighbouringChunkTile(chunk.Position, neighbourChunkDirection,
                            tile.Position + new Vector2Int(xOffset, yOffset), out neighbourTile)) continue;
                    }
                    // The tile we are interested in is located in the current chunk, get the value of it
                    else
                    {
                        neighbourTile = chunk.Tiles[(int) tile.Position.x + xOffset, (int) tile.Position.y + yOffset];
                    }

                    TerrainTileSet neighbour = TerrainTileSet.LoadedTileSets[neighbourTile.TileSetId];
    
    
                    if (neighbour != current && neighbour.Precedence > current.Precedence)
                    {
                        if (!tilePadding.ContainsKey(neighbour.Id)) tilePadding.Add(neighbour.Id, new TilePadding());
                        tilePadding[neighbour.Id].Enable(xOffset, yOffset);
                    }
                }
            }
    
            return tilePadding;
        }
    
        public bool GetNeighbouringChunkTile(Vector2Int chunk, Vector2Int direction, Vector2Int tilePosition, out Tile tile)
        {
            if (LoadedChunks.ContainsKey(chunk + direction))
            {
                // 15 -> 0 (+1)
                // 0 -> 15 (-1)
                int xPosition = (int) tilePosition.x;
                int yPosition = (int) tilePosition.y;
    
                if (xPosition == -1) xPosition = Chunk.SIZE - 1;
                else if (xPosition == Chunk.SIZE) xPosition = 0;
    
                if (yPosition == -1) yPosition = Chunk.SIZE - 1;
                else if (yPosition == Chunk.SIZE) yPosition = 0;
    
                tile = LoadedChunks[chunk + direction].Tiles[xPosition, yPosition];
    
                return true;
            }
            tile = null;
            return false;
        }

        private void SetChunksLoadedByPlayers(int playerId, IEnumerable<Vector2Int> chunks)
        {

        }


        private void SetPlayerLoadedChunks(int playerId, IEnumerable<Vector2Int> chunks)
        {
            List<Vector2Int> result = null;

            if (PlayerLoadedChunks.TryGetValue(playerId, out result))
            {
                result.Clear();
            }
            else
            {
                result = new List<Vector2Int>();
                PlayerLoadedChunks.Add(playerId, result);
            }
            result.AddRange(chunks);
        }



        /// <summary>
        /// Looks for a set of chunk positions within the currently loaded chunks and adds
        /// them to the given chunk list whilst removing the position from the positions list.
        /// </summary>
        /// <param name="chunkPositions">The chunk positions list to iterate through.</param>
        /// <param name="lookupResult">The list of chunks to add any found chunks to.</param>
        private void LookupLoadedChunks(List<Vector2Int> chunkPositions, List<Chunk> lookupResult)
        {
            // No chunks loaded so none to look up
            if (LoadedChunks.Count == 0)
            {
                return;
            }
    
            // Loop through the LoadedChunks and add any chunks that have a position specified 
            // within the chunkPositions list to the lookupResult list, remove the position also
            // as the chunk does not need to be looked for any longer
            for (int i = chunkPositions.Count - 1; i >= 0; i--)
            {
                if (LoadedChunks.ContainsKey(chunkPositions[i]))
                {
                    lookupResult.Add(LoadedChunks[chunkPositions[i]]);
                    chunkPositions.RemoveAt(i);
                }
            }
        }
    }
}