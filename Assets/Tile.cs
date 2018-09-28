using UnityEngine;

[System.Serializable]
public struct Tile
{
    public const int PX_SIZE = 32;

    public int TilesetID;
    public Vector2 Position;

    public Vector3 GetWorldPosition(Vector2 chunkPosition)
    {
        chunkPosition = Chunk.CalculateWorldPosition(chunkPosition);
        return new Vector3(chunkPosition.x + this.Position.x, chunkPosition.y + this.Position.y, 1f);
    }

    public Vector3 GetWorldPositionPrecedence(Vector2 chunkPosition, int paddingTilesetID)
    {
        Vector3 position = this.GetWorldPosition(chunkPosition);
        position.z -= TerrainTileset.LoadedTilesets[paddingTilesetID].Precedence;

        return position;
    }
}
