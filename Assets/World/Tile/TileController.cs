using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TileController : NetworkBehaviour
{
    private readonly List<GameObject> _paddingObjects = new List<GameObject>();
    private GameObject _floor;
    private SpriteRenderer _spriteRenderer;
    public Tile Tile { get; protected set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetTile(Tile tile, Dictionary<int, TilePadding> paddingTiles)
    {
        Tile = tile;
        _spriteRenderer.sprite = TerrainTileSet.LoadedTileSets[tile.TilesetID].Tiles[TerrainTileSet.CENTRAL];
        DestroyPadding();

        foreach (var padding in paddingTiles) SpawnPadding(padding.Key, padding.Value);
    }

    public void SetFloor(GameObject floor)
    {
        _floor = floor;
    }


    private void SpawnPadding(int tilesetID, TilePadding padding)
    {
        Sprite[] sprites = TerrainTileSet.LoadedTileSets[tilesetID].GetPaddingSprites(padding);

        foreach (var sprite in sprites)
        {
            GameObject paddingObject = new GameObject();
            paddingObject.transform.SetParent(transform);
            paddingObject.AddComponent<SpriteRenderer>().sprite = sprite;
            paddingObject.transform.position = transform.position -
                                               new Vector3(0f, 0f, TerrainTileSet.LoadedTileSets[tilesetID].Precedence);

            _paddingObjects.Add(paddingObject);
        }
    }

    private void SetPadding(int tilesetID, TilePadding padding)
    {
        Sprite[] sprites = TerrainTileSet.LoadedTileSets[tilesetID].GetPaddingSprites(padding);

        foreach (var sprite in sprites)
        {
            GameObject paddingObject = new GameObject();
            paddingObject.transform.SetParent(transform);
            paddingObject.AddComponent<SpriteRenderer>().sprite = sprite;
            paddingObject.transform.position = transform.position -
                                               new Vector3(0f, 0f, TerrainTileSet.LoadedTileSets[tilesetID].Precedence);

            _paddingObjects.Add(paddingObject);
        }
    }

    private void DestroyPadding()
    {
        foreach (var paddingObject in _paddingObjects) Destroy(paddingObject);
        _paddingObjects.Clear();
    }
}