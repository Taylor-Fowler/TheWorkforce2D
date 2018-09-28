using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A TerrainTileset contains all the variations of tile textures as well
/// as the precedence for determining which tile overlaps another
/// </summary>
[System.Serializable]
public class TerrainTileset
{
    public const int 
        SOUTH_EAST = 0, SOUTH = 1, SOUTH_WEST = 2, NORTH_WEST_CORNER = 3, NORTH_EAST_CORNER = 4, VERTICAL_EDGES = 5,
        EAST = 6, CENTRAL = 7, WEST = 8, SOUTH_WEST_CORNER = 9, SOUTH_EAST_CORNER = 10, HORIZONTAL_EDGES = 11,
        NORTH_EAST = 12, NORTH = 13, NORTH_WEST = 14, U_NORTH = 15, U_EAST = 16, U_SOUTH = 17,
        U_WEST = 18, ALL_EDGES = 19, ALL_CORNERS = 20, NE_NW_SW = 21, NE_NW_SE = 22, NW_SE_SW = 23,
        NE_SE_SW = 24, NE_NW = 25, NW_SE = 26, NW_SW = 27, NE_SE = 28, NE_SW = 29,
        SE_SW = 30;

    public Sprite[] Tiles;
    //public GameObject[] TilePrefabs;

    public static Dictionary<int, TerrainTileset> LoadedTilesets
    {
        get; protected set;
    }

    /// <summary>
    /// The precedence decides the z position of the tile, the higher the precedence, the closer to the screen it becomes
    /// </summary>
    public float Precedence
    {
        get; protected set;
    }

    public int ID
    {
        get; protected set;
    }

    public static void InitialiseTilesets()
    {
        LoadedTilesets = new Dictionary<int, TerrainTileset>();

        AssetProcessor.LoadTerrainTilesets();
    }

    /// <summary>
    /// A default tileset has 6 tiles in each row and 6 tiles in each column, however the final row only has ONE tile
    /// 31 tiles total. The constructor takes a 2D texture (the spritesheet) and splits it into 31 tiles, the size of
    /// each tile given as the `tileWidth` and `tileHeight` arguments.
    /// </summary>
    /// <param name="tileset">The single texture that will be sliced into a tileset</param>
    /// <param name="tileWidth">The px width of any given tile</param>
    /// <param name="tileHeight">The px height of any given tile</param>
    public TerrainTileset(Texture2D tileset, int tileWidth, int tileHeight, TerrainTilesetSettings settings)
    {
        if(tileset == null)
        {
            throw new System.ArgumentNullException("tileset", "TerrainTileset constructor argument cannot be null");
        }

        this.Precedence = settings.Precedence;
        this.ID = settings.ID;

        this.Tiles = new Sprite[31];
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            int loop = 0;

            for(int y = 0; y < 6; y++)
            {
                for(int x = 0; x < 6; x++)
                {
                    this.Tiles[loop] = Sprite.Create(tileset, new Rect(x * tileWidth, 160 - (y * tileHeight), tileWidth, tileHeight), pivot, 32, 0, SpriteMeshType.FullRect);
                    loop++;
                    
                    if(loop == this.Tiles.Length)
                        break;
                }
            }
        }

        this.RegisterInDictionary();
    }

    public Sprite[] GetPaddingSprites(TilePadding paddingInformation)
    {
        paddingInformation.ApplyFilter();

        List<Sprite> sprites = new List<Sprite>();

        int edges = paddingInformation.EdgeInformation();
        if(edges != -1)
            sprites.Add(this.Tiles[edges]);

        int corners = paddingInformation.CornerInformation();
        if(corners != -1)
            sprites.Add(this.Tiles[corners]);

        return sprites.ToArray();
    }


    private void RegisterInDictionary()
    {
        LoadedTilesets.Add(this.ID, this);
    }
}

public class TerrainTilesetSettings
{
    public int ID;
    public float Precedence;
}

public class TilePadding
{
    public const int NORTH_WEST = 0, NORTH = 1, NORTH_EAST = 2, WEST = 3, EAST = 4, SOUTH_WEST = 5, SOUTH = 6, SOUTH_EAST = 7;
    public bool NW, N, NE, W, E, SW, S, SE;

    /// <summary>
    /// Checks to see if each edge is enabled and then disables the corners attached to that edge eg. Northern Edge is connected to 
    /// the North Eastern and North Western corners.
    /// </summary>
    public void ApplyFilter()
    {
        if(this.N)
        {
            this.NW = false;
            this.NE = false;
        }

        if(this.E)
        {
            this.NE = false;
            this.SE = false;
        }

        if(this.S)
        {
            this.SE = false;
            this.SW = false;
        }

        if(this.W)
        {
            this.NW = false;
            this.SW = false;
        }
    }

    public void Enable(int xOffset, int yOffset)
    {
        switch(xOffset)
        {
            // WEST
            case -1:
                switch (yOffset)
                {
                    // SOUTH
                    case -1:
                        this.SW = true; break;
                    // CENTRAL
                    case 0:
                        this.W = true; break;
                    // NORTH
                    case 1:
                        this.NW = true; break;
                }
                break;
            // CENTRAL
            case 0:
                switch (yOffset)
                {
                    // SOUTH
                    case -1:
                        this.S = true; break;
                    // CENTRAL - NEVER
                    case 0:
                        break;
                    // NORTH
                    case 1:
                        this.N = true; break;
                }
                break;
            // EAST
            case 1:
                switch (yOffset)
                {
                    // SOUTH
                    case -1:
                        this.SE = true; break;
                    // CENTRAL
                    case 0:
                        this.E = true; break;
                    // NORTH
                    case 1:
                        this. NE = true; break;
                }
                break;
        }
    }

    public int EdgeInformation()
    {
        if (this.N && this.E && this.S && this.W) return TerrainTileset.ALL_EDGES;
        if (this.N && this.E && this.S) return TerrainTileset.U_WEST;
        if (this.N && this.E && this.W) return TerrainTileset.U_SOUTH;
        if (this.N && this.S && this.W) return TerrainTileset.U_EAST;
        if (this.N && this.E) return TerrainTileset.NORTH_EAST_CORNER;
        if (this.N && this.S) return TerrainTileset.HORIZONTAL_EDGES;
        if (this.N && this.W) return TerrainTileset.NORTH_WEST_CORNER;
        if (this.E && this.S && this.W) return TerrainTileset.U_NORTH;
        if (this.E && this.S) return TerrainTileset.SOUTH_EAST_CORNER;
        if (this.E && this.W) return TerrainTileset.VERTICAL_EDGES;
        if (this.S && this.W) return TerrainTileset.SOUTH_WEST_CORNER;
        if (this.N) return TerrainTileset.NORTH;
        if (this.E) return TerrainTileset.EAST;
        if (this.S) return TerrainTileset.SOUTH;
        if (this.W) return TerrainTileset.WEST;
        return -1;
    }

    public int CornerInformation()
    {
        if (this.NW && this.NE && this.SW && this.SE) return TerrainTileset.ALL_CORNERS;
        if (this.NW && this.NE && this.SW) return TerrainTileset.NE_NW_SW;
        if (this.NW && this.NE && this.SE) return TerrainTileset.NE_NW_SE;
        if (this.NW && this.SW && this.SE) return TerrainTileset.NW_SE_SW;
        if (this.NW && this.NE) return TerrainTileset.NE_NW;
        if (this.NW && this.SW) return TerrainTileset.NW_SW;
        if (this.NW && this.SE) return TerrainTileset.NW_SE;
        if (this.NE && this.SW && this.SE) return TerrainTileset.NE_SE_SW;
        if (this.NE && this.SW) return TerrainTileset.NE_SW;
        if (this.NE && this.SE) return TerrainTileset.NE_SE;
        if (this.SW && this.SE) return TerrainTileset.SE_SW;
        if (this.NW) return TerrainTileset.NORTH_WEST;
        if (this.NE) return TerrainTileset.NORTH_EAST;
        if (this.SW) return TerrainTileset.SOUTH_WEST;
        if (this.SE) return TerrainTileset.SOUTH_EAST;
        return -1;
    }
}