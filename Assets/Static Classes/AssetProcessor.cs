using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TheWorkforce.Crafting;
using UnityEngine;
using TheWorkforce.World;
using TheWorkforce.Items;

namespace TheWorkforce.Static_Classes
{
    public static class AssetProcessor
    {
        // TODO: Add a method that returns the directoryInfo requested only if the directory exists
        //       More error checking and handling!

        #region Private Members
        private static readonly Vector2 _texturePivot = new Vector2(0.5f, 0.5f);
        private const string BasePath = "Assets/Resources/";
        #endregion

        #region TileSet Loading
        public static TerrainTileSet[] LoadTerrainTileSets()
        {
            const string path = BasePath + "Terrain/TileSets";

            DirectoryInfo[] tileSetDirectories = new DirectoryInfo(path).GetDirectories();
            List<TerrainTileSet> tileSets = new List<TerrainTileSet>();

            foreach (var directory in tileSetDirectories)
            {
                var settingsFileContents = GetJsonFile(directory);
                
                TerrainTileSetSettings tileSetSettings = JsonConvert.DeserializeObject<TerrainTileSetSettings>(settingsFileContents);
                FileInfo textureFile = directory.GetFiles("*.png")[0];
                Texture2D texture = LoadTexture(textureFile, Tile.PX_SIZE * 6, Tile.PX_SIZE * 6);

                tileSets.Add(new TerrainTileSet(texture, Tile.PX_SIZE, Tile.PX_SIZE, tileSetSettings));
                
                // NOTE: Commented logging of Tile Precedence
                // Debug.Log(tileSetSettings.Id + " Precedence: " + tileSetSettings.Precedence);
            }

            return tileSets.ToArray();
        }
        #endregion

        #region Sprite & Texture Utility Functions
        private static Sprite LoadSpriteFromTexture(Texture2D texture, int width = Tile.PX_SIZE, int height = Tile.PX_SIZE, int pixelsPerUnit = Tile.PX_SIZE, int x = 0, int y = 0)
        {
            return Sprite.Create(texture, new Rect(x, y, width, height), _texturePivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Texture2D LoadTexture(FileInfo textureFile, int width = Tile.PX_SIZE, int height = Tile.PX_SIZE)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(File.ReadAllBytes(textureFile.FullName));
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }
        #endregion


        private static string GetJsonFile(DirectoryInfo directory, string fileName = "settings")
        {
            FileInfo fileInfo = directory.GetFiles(fileName + ".json")[0];
            string fileContents;

            using (var reader = new StreamReader(fileInfo.OpenRead()))
            {
                fileContents = reader.ReadToEnd();
            }

            return fileContents;
        }
    }
}
