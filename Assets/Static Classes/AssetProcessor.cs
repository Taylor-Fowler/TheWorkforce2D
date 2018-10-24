using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class AssetProcessor
{
    public static TerrainTileSet[] LoadTerrainTileSets()
    {
        const string path = "Assets/Resources/Terrain/TileSets";

        DirectoryInfo[] tileSetDirectories = new DirectoryInfo(path).GetDirectories();
        List<TerrainTileSet> tileSets = new List<TerrainTileSet>();

        foreach (var directory in tileSetDirectories)
        {
            FileInfo settingsFile = directory.GetFiles("settings.json")[0];
            string settings;

            using (var reader = new StreamReader(settingsFile.OpenRead()))
            {
                settings = reader.ReadToEnd();
            }

            TerrainTileSetSettings tileSetSettings = JsonConvert.DeserializeObject<TerrainTileSetSettings>(settings);
            Debug.Log(tileSetSettings.ID + " Precedence: " + tileSetSettings.Precedence);

            FileInfo textureFile = directory.GetFiles("*.png")[0];
            Texture2D texture = new Texture2D(Tile.PX_SIZE * 6, Tile.PX_SIZE * 6);

            texture.LoadImage(File.ReadAllBytes(textureFile.FullName));
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            tileSets.Add(new TerrainTileSet(texture, Tile.PX_SIZE, Tile.PX_SIZE, tileSetSettings));
        }

        return tileSets.ToArray();
    }
}