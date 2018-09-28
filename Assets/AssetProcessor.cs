using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public static class AssetProcessor
{
    public static TerrainTileset[] LoadTerrainTilesets()
    {
        string path = "Assets/Resources/Terrain/Tilesets";

        DirectoryInfo[] tilesetDirectories = new DirectoryInfo(path).GetDirectories();
        List<TerrainTileset> tilesets = new List<TerrainTileset>();

        foreach(var directory in tilesetDirectories)
        {
            FileInfo settingsFile = directory.GetFiles("settings.json")[0];
            string settings;

            using(var reader = new StreamReader(settingsFile.OpenRead()))
            {
                settings = reader.ReadToEnd();
            }
            TerrainTilesetSettings tilesetSettings = JsonConvert.DeserializeObject<TerrainTilesetSettings>(settings);
            Debug.Log(tilesetSettings.ID + " Precedence: " + tilesetSettings.Precedence);

            FileInfo textureFile = directory.GetFiles("*.png")[0];
            Texture2D texture = new Texture2D(Tile.PX_SIZE * 6, Tile.PX_SIZE * 6);

            texture.LoadImage(File.ReadAllBytes(textureFile.FullName));
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            tilesets.Add(new TerrainTileset(texture, Tile.PX_SIZE, Tile.PX_SIZE, tilesetSettings));
        }

        return tilesets.ToArray();
    }
}
