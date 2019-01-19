using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileController : NetworkBehaviour
    {
        private Tile _tile;
        private GameObject _entityOnTile;
        private GameObject _paddingAnchor;
        private List<GameObject> _paddingObjects;
        private SpriteRenderer _spriteRenderer;

        #region Unity API
        private void Awake()
        {
            _paddingObjects = new List<GameObject>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _paddingAnchor = new GameObject();
            _paddingAnchor.transform.SetParent(transform);
        }
        #endregion

        public Tile GetTile()
        {
            return _tile;
        }

        public void SetTile(Tile tile, Dictionary<int, TilePadding> paddingTiles)
        {
            _spriteRenderer.sprite = TerrainTileSet.LoadedTileSets[tile.TileSetId].Tiles[TerrainTileSet.CENTRAL];
            DestroyPadding();

            foreach (var padding in paddingTiles)
            {
                SpawnPadding(padding.Key, padding.Value);
            }

            if(_entityOnTile != null)
            {
                Destroy(_entityOnTile);
            }

            if(tile.StaticEntityInstanceId != 0)
            {
                SetItem(tile.StaticEntityInstanceId);
            }
        }

        public void SetItem(uint entityId)
        {
            _entityOnTile = Entities.EntityCollection.Instance().GetEntity(entityId).Spawn();
            _entityOnTile.transform.SetParent(transform, false);
            _entityOnTile.transform.Translate(0f, 0f, -0.6f);
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