﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A TerrainTileSet contains all the variations of tile textures as well
///     as the precedence for determining which tile overlaps another
/// </summary>
[Serializable]
public class TerrainTileSet
{
    public const int
        SOUTH_EAST = 0,
        SOUTH = 1,
        SOUTH_WEST = 2,
        NORTH_WEST_CORNER = 3,
        NORTH_EAST_CORNER = 4,
        VERTICAL_EDGES = 5,
        EAST = 6,
        CENTRAL = 7,
        WEST = 8,
        SOUTH_WEST_CORNER = 9,
        SOUTH_EAST_CORNER = 10,
        HORIZONTAL_EDGES = 11,
        NORTH_EAST = 12,
        NORTH = 13,
        NORTH_WEST = 14,
        U_NORTH = 15,
        U_EAST = 16,
        U_SOUTH = 17,
        U_WEST = 18,
        ALL_EDGES = 19,
        ALL_CORNERS = 20,
        NE_NW_SW = 21,
        NE_NW_SE = 22,
        NW_SE_SW = 23,
        NE_SE_SW = 24,
        NE_NW = 25,
        NW_SE = 26,
        NW_SW = 27,
        NE_SE = 28,
        NE_SW = 29,
        SE_SW = 30;

    public Sprite[] Tiles;


    /// <summary>
    ///     A default TileSet has 6 tiles in each row and 6 tiles in each column, however the final row only has ONE tile
    ///     31 tiles total. The constructor takes a 2D texture (the sprite sheet) and splits it into 31 tiles, the size of
    ///     each tile given as the `tileWidth` and `tileHeight` arguments.
    /// </summary>
    /// <param name="tileSet">The single texture that will be sliced into a TileSet</param>
    /// <param name="tileWidth">The px width of any given tile</param>
    /// <param name="tileHeight">The px height of any given tile</param>
    public TerrainTileSet(Texture2D tileSet, int tileWidth, int tileHeight, TerrainTileSetSettings settings)
    {
        if (tileSet == null)
            throw new ArgumentNullException(nameof(Texture2D), "TerrainTileSet constructor argument cannot be null");

        Precedence = settings.Precedence;
        ID = settings.ID;

        Tiles = new Sprite[31];
        {
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            int loop = 0;

            for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
            {
                Tiles[loop] = Sprite.Create(tileSet,
                    new Rect(x * tileWidth, 160 - y * tileHeight, tileWidth, tileHeight), pivot, 32, 0,
                    SpriteMeshType.FullRect);
                loop++;

                if (loop == Tiles.Length)
                    break;
            }
        }

        RegisterInDictionary();
    }

    public static Dictionary<int, TerrainTileSet> LoadedTileSets { get; private set; }


    public int ID { get; }

    /// <summary>
    ///     The precedence decides the z position of the tile, the higher the precedence, the closer to the screen it becomes
    /// </summary>
    public float Precedence { get; }

    public static void InitialiseTileSets()
    {
        LoadedTileSets = new Dictionary<int, TerrainTileSet>();

        AssetProcessor.LoadTerrainTileSets();
    }

    public Sprite[] GetPaddingSprites(TilePadding paddingInformation)
    {
        paddingInformation.ApplyFilter();

        List<Sprite> sprites = new List<Sprite>();

        int edges = paddingInformation.EdgeInformation();
        if (edges != -1)
            sprites.Add(Tiles[edges]);

        int corners = paddingInformation.CornerInformation();
        if (corners != -1)
            sprites.Add(Tiles[corners]);

        return sprites.ToArray();
    }


    private void RegisterInDictionary()
    {
        LoadedTileSets.Add(ID, this);
    }
}

public class TerrainTileSetSettings
{
    public int ID;
    public float Precedence;
}

public class TilePadding
{
    public const int 
        NORTH_WEST = 0,
        NORTH = 1,
        NORTH_EAST = 2,
        WEST = 3,
        EAST = 4,
        SOUTH_WEST = 5,
        SOUTH = 6,
        SOUTH_EAST = 7;

    public bool NW, N, NE, W, E, SW, S, SE;

    /// <summary>
    ///     Checks to see if each edge is enabled and then disables the corners attached to that edge eg. Northern Edge is
    ///     connected to
    ///     the North Eastern and North Western corners.
    /// </summary>
    public void ApplyFilter()
    {
        if (N)
        {
            NW = false;
            NE = false;
        }

        if (E)
        {
            NE = false;
            SE = false;
        }

        if (S)
        {
            SE = false;
            SW = false;
        }

        if (W)
        {
            NW = false;
            SW = false;
        }
    }

    public void Enable(int xOffset, int yOffset)
    {
        switch (xOffset)
        {
            // WEST
            case -1:
                switch (yOffset)
                {
                    // SOUTH
                    case -1:
                        SW = true;
                        break;
                    // CENTRAL
                    case 0:
                        W = true;
                        break;
                    // NORTH
                    case 1:
                        NW = true;
                        break;
                }

                break;
            // CENTRAL
            case 0:
                switch (yOffset)
                {
                    // SOUTH
                    case -1:
                        S = true;
                        break;
                    // CENTRAL - NEVER
                    case 0:
                        break;
                    // NORTH
                    case 1:
                        N = true;
                        break;
                }

                break;
            // EAST
            case 1:
                switch (yOffset)
                {
                    // SOUTH
                    case -1:
                        SE = true;
                        break;
                    // CENTRAL
                    case 0:
                        E = true;
                        break;
                    // NORTH
                    case 1:
                        NE = true;
                        break;
                }

                break;
        }
    }

    public int EdgeInformation()
    {
        if (N && E && S && W) return TerrainTileSet.ALL_EDGES;
        if (N && E && S) return TerrainTileSet.U_WEST;
        if (N && E && W) return TerrainTileSet.U_SOUTH;
        if (N && S && W) return TerrainTileSet.U_EAST;
        if (N && E) return TerrainTileSet.NORTH_EAST_CORNER;
        if (N && S) return TerrainTileSet.HORIZONTAL_EDGES;
        if (N && W) return TerrainTileSet.NORTH_WEST_CORNER;
        if (E && S && W) return TerrainTileSet.U_NORTH;
        if (E && S) return TerrainTileSet.SOUTH_EAST_CORNER;
        if (E && W) return TerrainTileSet.VERTICAL_EDGES;
        if (S && W) return TerrainTileSet.SOUTH_WEST_CORNER;
        if (N) return TerrainTileSet.NORTH;
        if (E) return TerrainTileSet.EAST;
        if (S) return TerrainTileSet.SOUTH;
        if (W) return TerrainTileSet.WEST;
        return -1;
    }

    public int CornerInformation()
    {
        if (NW && NE && SW && SE) return TerrainTileSet.ALL_CORNERS;
        if (NW && NE && SW) return TerrainTileSet.NE_NW_SW;
        if (NW && NE && SE) return TerrainTileSet.NE_NW_SE;
        if (NW && SW && SE) return TerrainTileSet.NW_SE_SW;
        if (NW && NE) return TerrainTileSet.NE_NW;
        if (NW && SW) return TerrainTileSet.NW_SW;
        if (NW && SE) return TerrainTileSet.NW_SE;
        if (NE && SW && SE) return TerrainTileSet.NE_SE_SW;
        if (NE && SW) return TerrainTileSet.NE_SW;
        if (NE && SE) return TerrainTileSet.NE_SE;
        if (SW && SE) return TerrainTileSet.SE_SW;
        if (NW) return TerrainTileSet.NORTH_WEST;
        if (NE) return TerrainTileSet.NORTH_EAST;
        if (SW) return TerrainTileSet.SOUTH_WEST;
        if (SE) return TerrainTileSet.SOUTH_EAST;
        return -1;
    }
}