using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileController : NetworkBehaviour
    {
        public Vector2 TilePosition { get; private set; }
        public ChunkController ChunkController { get; private set; }

        private List<GameObject> _paddingObjects;
        private SpriteRenderer _spriteRenderer;

        #region Unity API
        private void Awake()
        {
            _paddingObjects = new List<GameObject>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        #endregion

        public void SetTile(Tile tile, ChunkController chunkController, Dictionary<int, TilePadding> paddingTiles)
        {
            TilePosition = tile.Position;
            ChunkController = chunkController;

            _spriteRenderer.sprite = TerrainTileSet.LoadedTileSets[tile.TileSetId].Tiles[TerrainTileSet.CENTRAL];
            DestroyPadding();

            foreach (var padding in paddingTiles)
            {
                SpawnPadding(padding.Key, padding.Value);
            }
        }

        private void SpawnPadding(int tileSetId, TilePadding padding)
        {
            Sprite[] sprites = TerrainTileSet.LoadedTileSets[tileSetId].GetPaddingSprites(padding);

            foreach (var sprite in sprites)
            {
                GameObject paddingObject = new GameObject();
                paddingObject.transform.SetParent(transform);
                paddingObject.AddComponent<SpriteRenderer>().sprite = sprite;
                paddingObject.transform.position = transform.position -
                                                   new Vector3(0f, 0f, TerrainTileSet.LoadedTileSets[tileSetId].Precedence);

                _paddingObjects.Add(paddingObject);
            }
        }

        private void DestroyPadding()
        {
            foreach (var paddingObject in _paddingObjects)
            {
                Destroy(paddingObject);
            }
            _paddingObjects.Clear();
        }
    }   
}