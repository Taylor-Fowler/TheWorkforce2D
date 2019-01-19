using UnityEngine;

namespace TheWorkforce
{
    public struct NetworkTile
    {
        #region Public Members
        public byte TileSetId;
        public uint StaticEntityInstanceId;

        public float Moisture;
        public float Elevation;
        public Vector2 Position;
        #endregion

        public NetworkTile(Tile tile)
        {
            TileSetId = tile.TileSetId;
            StaticEntityInstanceId = tile.StaticEntityInstanceId;

            Moisture = tile.Moisture;
            Elevation = tile.Elevation;
            Position = tile.Position;
        }
    }
}
