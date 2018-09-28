using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class NetworkPrefabRegister
{
    public static void RegisterTerrainTilesets(NetworkManager networkManager)
    {
        //foreach(var tileset in TerrainTileset.LoadedTilesets)
        //{
        //    List<GameObject> tilePrefabs = new List<GameObject>();

        //    for(int i = 0; i < tileset.Value.Tiles.Length; i++)
        //    {
        //        GameObject gameObject = new GameObject("Tileset: " + tileset.Value.ID + " Tile Type: " + i);
        //        gameObject.AddComponent<SpriteRenderer>().sprite = tileset.Value.Tiles[i];
        //        gameObject.AddComponent<TileController>();
        //        gameObject.AddComponent<NetworkIdentity>();
        //        tilePrefabs.Add(gameObject);
        //    }

        //    tileset.Value.TilePrefabs = tilePrefabs.ToArray();
        //    networkManager.spawnPrefabs.AddRange(tilePrefabs);
        //}
    }

    public static void ClientRegisterTerrainTilesets()
    {
        //foreach(var tileset in TerrainTileset.LoadedTilesets)
        //    foreach(var tile in tileset.Value.TilePrefabs)
        //        ClientScene.RegisterPrefab(tile);
    }
}
