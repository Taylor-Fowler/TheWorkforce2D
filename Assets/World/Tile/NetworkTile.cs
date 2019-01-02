using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce.World
{
    public struct NetworkTile
    {
        #region Public Members
        public int TileSetId;
        public object[] ItemOnTile;

        public float Moisture;
        public float Elevation;
        public Vector2 Position;
        #endregion

        public NetworkTile(Tile tile)
        {
            TileSetId = tile.TileSetId;
            if(tile.ItemOnTile != null)
            {
                ItemOnTile = tile.ItemOnTile.Pack();
            }
            else
            {
                ItemOnTile = new object[1] { -1 };
            }
            Moisture = tile.Moisture;
            Elevation = tile.Elevation;
            Position = tile.Position;
        }
    }
}
