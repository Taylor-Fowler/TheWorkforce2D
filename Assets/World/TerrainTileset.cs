using System;
using System.Collections.Generic;
using UnityEngine;
using TheWorkforce.Static_Classes;

namespace TheWorkforce
{
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
        /// <param name="settings"></param>
        public TerrainTileSet(Texture2D tileSet, int tileWidth, int tileHeight, TerrainTileSetSettings settings)
        {
            if (tileSet == null)
                throw new ArgumentNullException(nameof(Texture2D), "TerrainTileSet constructor argument cannot be null");
    
            Precedence = settings.Precedence;
            Id = settings.Id;
    
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
    
    
        public int Id { get; }
    
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
            LoadedTileSets.Add(Id, this);
        }
    }
}