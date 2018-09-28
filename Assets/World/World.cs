using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The currently loaded world on the client machine,
/// stores an array of chunks which are loaded.
/// </summary>
[System.Serializable]
public class World
{
    public Dictionary<Vector2, Chunk> LoadedChunks;

    public int Seed
    {
        get; private set;
    }

    public int NegativeXSeed
    {
        get; private set;
    }

    public int NegativeYSeed
    {
        get; private set;
    }

    public World()
    {
        this.Seed = new System.Random().Next();
        this.CreateNegativeSeeds();
    }

    public World(int seed)
    {
        this.Seed = seed;
        this.CreateNegativeSeeds();
    }

    private void CreateNegativeSeeds()
    {
        System.Random random = new System.Random(this.Seed);
        this.NegativeXSeed = random.Next();
        this.NegativeYSeed = random.Next();
    }

    public Chunk[] RequestAllChunks()
    {
        return new List<Chunk>(this.LoadedChunks.Values).ToArray();
    }

    public void UpdateChunks(Chunk[] chunks)
    {
        if(this.LoadedChunks == null)
        {
            this.LoadedChunks = new Dictionary<Vector2, Chunk>();
        }

        foreach(var chunk in chunks)
        {
            this.LoadedChunks.Add(chunk.Position, chunk);
        }
    }

    public Dictionary<int, TilePadding> GetTilePadding(Chunk chunk, Tile tile)
    {
        Dictionary<int, TilePadding> tilePadding = new Dictionary<int, TilePadding>();

        TerrainTileset current = TerrainTileset.LoadedTilesets[tile.TilesetID];

        // top to bottom
        for (int yOffset = 1; yOffset >= -1; yOffset--)
        {
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
                if(neighbourChunkDirection != Vector2.zero)
                {
                    //return new Dictionary<int, TilePadding>();
                    if (!this.GetNeighbouringChunkTile(chunk.Position, neighbourChunkDirection, tile.Position + new Vector2(xOffset, yOffset), ref neighbourTile))
                    {
                        continue;
                    }
                }
                // The tile we are interested in is located in the current chunk, get the value of it
                else
                {
                    neighbourTile = chunk.Tiles[(int)tile.Position.x + xOffset, (int)tile.Position.y + yOffset];
                }

                TerrainTileset neighbour = TerrainTileset.LoadedTilesets[neighbourTile.TilesetID];
                

                if (neighbour != current && neighbour.Precedence > current.Precedence)
                {
                    if (!tilePadding.ContainsKey(neighbour.ID))
                    {
                        tilePadding.Add(neighbour.ID, new TilePadding());
                    }
                    tilePadding[neighbour.ID].Enable(xOffset, yOffset);
                }
            }
        }

        return tilePadding;
    }

    public bool GetNeighbouringChunkTile(Vector2 chunk, Vector2 direction, Vector2 tilePosition, ref Tile tile)
    {
        if(this.LoadedChunks.ContainsKey(chunk + direction))
        {
            // 15 -> 0 (+1)
            // 0 -> 15 (-1)
            int xPosition = (int)tilePosition.x;
            int yPosition = (int)tilePosition.y;

            if(xPosition == -1) xPosition = Chunk.SIZE - 1;
            else if(xPosition == Chunk.SIZE) xPosition = 0;

            if (yPosition == -1) yPosition = Chunk.SIZE - 1;
            else if (yPosition == Chunk.SIZE) yPosition = 0;

            tile = this.LoadedChunks[chunk + direction].Tiles[xPosition, yPosition];

            return true;
        }
        return false;
    }

    public Vector2[] NotLoaded(Vector2[] chunksToLoad)
    {
        if(this.LoadedChunks == null)
        {
            this.LoadedChunks = new Dictionary<Vector2, Chunk>();
            
            return chunksToLoad;
        }
        List<Vector2> notLoaded = new List<Vector2>();

        foreach(var chunk in chunksToLoad)
        {
            if(!this.LoadedChunks.ContainsKey(chunk))
                notLoaded.Add(chunk);
        }

        return notLoaded.ToArray();
    }

    public void SetChunks()
    {

    }

    public void Load()
    {

    }

    public void Save()
    {

    }
}
