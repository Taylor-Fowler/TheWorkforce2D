using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce
{
    public class ChunkController : MonoBehaviour
    {
        #region Public Static Methods
        public static IEnumerable<string> ChunkPositionStrings(IEnumerable<ChunkController> chunkControllers)
        {
            List<string> strings = new List<string>();
            foreach(var chunkController in chunkControllers)
            {
                strings.Add(chunkController.Chunk.Position.x.ToString() + ", " + chunkController.Chunk.Position.y.ToString());
            }
            return strings;
        }
        #endregion

        #region Public Indexers
        public TileController this[Vector2 tilePosition]
        {
            get
            {
                if (tilePosition.x < 0 || tilePosition.x > Chunk.SIZE - 1 || tilePosition.y < 0 || tilePosition.y > Chunk.SIZE - 1)
                {
                    return null;
                }
                return _tileControllers[(int)tilePosition.x * Chunk.SIZE + (int)tilePosition.y];
            }
        }
        #endregion
        /// <summary>
        ///     The tile controllers that exist within the currently controlled chunk.
        /// </summary>
        public readonly List<TileController> _tileControllers = new List<TileController>();

        /// <summary>
        ///     Gets the controlled chunk.
        /// </summary>
        /// <value>
        ///     The chunk that the controller controls
        /// </value>
        public Chunk Chunk { get; protected set; }

        #region Debugging Members
        #if DEBUG
        [SerializeField] private Chunk _chunk;
#endif
        #endregion

        #region Unity API
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0f, Chunk.SIZE, 0f));
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(Chunk.SIZE, 0f, 0f));
            Gizmos.DrawLine(transform.position + new Vector3(0f, Chunk.SIZE, 0f), transform.position + new Vector3(Chunk.SIZE, Chunk.SIZE, 0f));
            Gizmos.DrawLine(transform.position + new Vector3(Chunk.SIZE, 0f, 0f), transform.position + new Vector3(Chunk.SIZE, Chunk.SIZE, 0f));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 origin = transform.position;
            // TODO: Find a way of drawing the gizmos on top of unselected ones
            //       Solution 1: This didnt work + new Vector3(0f, 0f, 1.1f);

            Gizmos.DrawLine(origin, origin + new Vector3(0f, Chunk.SIZE, 0f));
            Gizmos.DrawLine(origin, origin + new Vector3(Chunk.SIZE, 0f, 0f));
            Gizmos.DrawLine(origin + new Vector3(0f, Chunk.SIZE, 0.1f), origin + new Vector3(Chunk.SIZE, Chunk.SIZE, 0f));
            Gizmos.DrawLine(origin + new Vector3(Chunk.SIZE, 0f, 0f), origin + new Vector3(Chunk.SIZE, Chunk.SIZE, 0f));
        }
        #endregion

        public void SetChunk(Chunk chunk, World worldDetails)
        {
            #if DEBUG
            _chunk = chunk;
            #endif

            Chunk = chunk;
            transform.position = Chunk.Position * Chunk.SIZE;
            transform.position += transform.parent.position;
            name = "Chunk Controller: " + Chunk.Position.x + ", " + Chunk.Position.y;

            if (_tileControllers.Count == 0)
            {
                SpawnTileControllers();
            }

            int i = 0;
            foreach (var tile in Chunk.Tiles)
            {
                _tileControllers[i].SetTile(tile, worldDetails.GetTilePadding(Chunk, tile));
                i++;
            }
        }

        private void SpawnTileControllers()
        {
            for (int x = 0; x < Chunk.SIZE; x++)
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                GameObject tile = new GameObject();
                tile.transform.SetParent(transform);
                tile.transform.position = transform.position + new Vector3(x + 0.5f, y + 0.5f, 0f);
                tile.AddComponent<SpriteRenderer>();
                tile.name = "Tile Controller: " + x + ", " + y;

                _tileControllers.Add(tile.AddComponent<TileController>());
            }
        }
    }   
}