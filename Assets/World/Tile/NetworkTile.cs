namespace TheWorkforce
{
    /// <summary>
    /// A simple data structure that is used as a container for networked messaging of the state of a tile object
    /// 
    /// Current Size: 13 bytes
    /// </summary>
    public struct NetworkTile
    {
        #region Public Members
        public readonly byte TileSetId; // 1 byte
        public readonly uint StaticEntityInstanceId; // 5 bytes

        public readonly float Moisture; // 9 bytes
        public readonly float Elevation; // 13 bytes
        #endregion

        public NetworkTile(Tile tile)
        {
            TileSetId = tile.TileSetId;
            StaticEntityInstanceId = tile.StaticEntityInstanceId;
            Moisture = tile.Moisture;
            Elevation = tile.Elevation;
        }
    }
}
