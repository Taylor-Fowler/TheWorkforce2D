using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce.World
{
    public struct NetworkTile
    {
        #region Public Members
        public byte TileSetId;
        public int ItemOnTile;

        public float Moisture;
        public float Elevation;
        public Vector2 Position;
        #endregion

        public NetworkTile(Tile tile)
        {
            TileSetId = tile.TileSetId;
            ItemOnTile = tile.ItemOnTileId;

            Moisture = tile.Moisture;
            Elevation = tile.Elevation;
            Position = tile.Position;
        }
    }
}
