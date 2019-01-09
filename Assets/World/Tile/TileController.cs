using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TheWorkforce.Items;

namespace TheWorkforce.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileController : NetworkBehaviour
    {
        public Vector2 TilePosition { get; private set; }
        public ChunkController ChunkController { get; private set; }
        public ItemController ItemController { get; private set; }

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

        public void SetTile(Tile tile, ChunkController chunkController, Dictionary<int, TilePadding> paddingTiles)
        {
            TilePosition = tile.Position;
            ChunkController = chunkController;

            _spriteRenderer.sprite = TerrainTileSet.LoadedTileSets[tile.TileSetId].Tiles[TerrainTileSet.CENTRAL];
            DestroyPadding();

            if(ItemController != null)
            {
                Destroy(ItemController.gameObject);
                ItemController = null;
            }

            foreach (var padding in paddingTiles)
            {
                SpawnPadding(padding.Key, padding.Value);
            }

            if(tile.ItemOnTileId != 0)
            {
                SetItem(tile.ItemOnTileId);
            }
        }

        public void SetItem(ushort itemId)
        {
            var go = new GameObject();

            go.AddComponent(ItemFactory.Instance.Get(itemId).MonoType);
            //Debug.Log("[TileController] - SetItem(IItem) \n"
            //        + "Item Name: " + item.Name);

            // spawn monobehaviour for the item
            // monobehaviour 
            //ItemController = item.SpawnObject(transform);
        }

        public GameObject ObjectOnTile()
        {
            if(ItemController == null)
            {
                return null;
            }

            return ItemController.gameObject;
        }

        public bool CanPlace()
        {
            return ItemController == null;
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