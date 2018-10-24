using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/// <summary>
///     The currently loaded world on the client machine,
///     stores an array of chunks which are loaded.
/// </summary>
[Serializable]
public class World
{
    public readonly HashSet<Vector2> ChunksGenerated;
    public readonly Dictionary<Vector2, Chunk> LoadedChunks;

    public List<Vector2> ChunksSurroundingPlayer;


    /// <summary>
    ///     Initializes a new instance of the <see cref="World" /> class. Generates a random seed.
    /// </summary>
    public World() : this(new Random().Next())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="World" /> class.
    /// </summary>
    /// <param name="seed">The seed.</param>
    public World(int seed)
    {
        Seed = seed;
        Random random = new Random(Seed);
        NegativeXSeed = random.Next();
        NegativeYSeed = random.Next();

        ChunksSurroundingPlayer = new List<Vector2>();
        LoadedChunks = new Dictionary<Vector2, Chunk>();
        ChunksGenerated = new HashSet<Vector2>();
    }

    public int Seed { get; }
    public int NegativeXSeed { get; }
    public int NegativeYSeed { get; }

    public void UpdateChunksSurroundingPlayer(List<Vector2> chunksSurroundingPlayer)
    {
        ChunksSurroundingPlayer.Clear();
        foreach (var chunk in chunksSurroundingPlayer)
            ChunksSurroundingPlayer.Add(chunk);
    }

    public void AddLoadedChunks(Chunk[] chunks)
    {
        foreach (var chunk in chunks) LoadedChunks.Add(chunk.Position, chunk);
    }

    public Vector2[] FilterChunksThatAreLoaded(Vector2[] chunksToLoad)
    {
        if (LoadedChunks.Count == 0) return chunksToLoad;

        List<Vector2> notLoaded = new List<Vector2>();

        foreach (var chunk in chunksToLoad)
            if (!LoadedChunks.ContainsKey(chunk))
                notLoaded.Add(chunk);
        return notLoaded.ToArray();
    }

    public void FilterChunkPositionsThatAreLoaded(List<Vector2> chunkPositions)
    {
        for (int i = chunkPositions.Count - 1; i >= 0; i--)
            if (LoadedChunks.ContainsKey(chunkPositions[i]))
                chunkPositions.RemoveAt(i);
    }

    public virtual List<Chunk> GetChunks(List<Vector2> chunkPositions)
    {
        // 1. Loop through chunk positions
        // 2. If chunk position is loaded, add directly to list
        // 3. If chunk position is not loaded but is already generated, load the chunk
        //    from the save file, add the chunk to the loaded chunks list and add to return list
        // 4. If chunk position is not loaded or generated, return the list of found chunks
        //    and the list of positions will contain chunks that need to be generated which will be
        //    handled by the server and distributed accordingly
        List<Chunk> retrievedChunks = new List<Chunk>();

        LookupLoadedChunks(chunkPositions, retrievedChunks);
        if (chunkPositions.Count == 0)
            return retrievedChunks;

        retrievedChunks.AddRange(GameFileIO.LoadChunks(chunkPositions));

        return retrievedChunks;
    }

    public virtual bool CanUnloadChunk(Vector2 chunkPositionToUnload)
    {
        return !LoadedChunks[chunkPositionToUnload].KeepLoaded;
    }

    public Dictionary<int, TilePadding> GetTilePadding(Chunk chunk, Tile tile)
    {
        Dictionary<int, TilePadding> tilePadding = new Dictionary<int, TilePadding>();

        TerrainTileSet current = TerrainTileSet.LoadedTileSets[tile.TilesetID];

        // top to bottom
        for (int yOffset = 1; yOffset >= -1; yOffset--)
            // go from left to right
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            Tile neighbourTile = new Tile();

            // Check if the neighbour tile we are interested in is located in a different chunk
            // If the tile position + offset is less than zero or equal to the size of a chunk then it means that the 
            // tile is in a different chunk (negative when less than zero and positive when equal to chunk size in the axis that
            // is being checked).
            Vector2 neighbourChunkDirection = Vector2.zero;
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
                    tile.Position + new Vector2(xOffset, yOffset), ref neighbourTile)) continue;
            }
            // The tile we are interested in is located in the current chunk, get the value of it
            else
            {
                neighbourTile = chunk.Tiles[(int) tile.Position.x + xOffset, (int) tile.Position.y + yOffset];
            }

            TerrainTileSet neighbour = TerrainTileSet.LoadedTileSets[neighbourTile.TilesetID];


            if (neighbour != current && neighbour.Precedence > current.Precedence)
            {
                if (!tilePadding.ContainsKey(neighbour.ID)) tilePadding.Add(neighbour.ID, new TilePadding());
                tilePadding[neighbour.ID].Enable(xOffset, yOffset);
            }
        }

        return tilePadding;
    }

    public bool GetNeighbouringChunkTile(Vector2 chunk, Vector2 direction, Vector2 tilePosition, ref Tile tile)
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

        return false;
    }

    /// <summary>
    ///     Looks for a set of chunk positions within the currently loaded chunks and adds
    ///     them to the given chunk list whilst removing the position from the positions list.
    /// </summary>
    /// <param name="chunkPositions">The chunk positions list to iterate through.</param>
    /// <param name="lookupResult">The list of chunks to add any found chunks to.</param>
    private void LookupLoadedChunks(List<Vector2> chunkPositions, List<Chunk> lookupResult)
    {
        // No chunks loaded so none to look up
        if (LoadedChunks.Count == 0)
            return;

        // Loop through the LoadedChunks and add any chunks that have a position specified 
        // within the chunkPositions list to the lookupResult list, remove the position also
        // as the chunk does not need to be looked for any longer
        for (int i = chunkPositions.Count - 1; i >= 0; i--)
            if (LoadedChunks.ContainsKey(chunkPositions[i]))
            {
                lookupResult.Add(LoadedChunks[chunkPositions[i]]);
                chunkPositions.RemoveAt(i);
            }
    }

    /// <summary>
    ///     Loads any chunks, from file, that exist with the corresponding position specified
    ///     in the chunkPositions list.
    /// </summary>
    /// <param name="chunkPositions">The chunk positions to try and load.</param>
    /// <param name="loadResult">The list of chunks to concatenate the load result with.</param>
    public void LoadGeneratedChunks(List<Vector2> chunkPositions, List<Chunk> loadResult)
    {
        // Loops through the requested chunk positions and adds any that are known to exist to a list
        // and adds the others that do not yet exist to a separate list. The known list is then loaded
        // from file and finally the requested chunk positions list is replaced with the non-existant chunk
        // list.
        if (ChunksGenerated.Count == 0)
            return;

        List<Vector2> existingChunks = new List<Vector2>();
        List<Vector2> nonExistantChunks = new List<Vector2>();

        foreach (var chunkPosition in chunkPositions)
            if (ChunksGenerated.Contains(chunkPosition))
                existingChunks.Add(chunkPosition);
            else
                nonExistantChunks.Add(chunkPosition);

        loadResult.AddRange(GameFileIO.LoadChunks(existingChunks));
        chunkPositions = nonExistantChunks;
    }
}