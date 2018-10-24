using System;
using UnityEngine;

[Serializable]
public struct Tile
{
    public const int PX_SIZE = 32;

    public int TilesetID;
    public Vector2 Position;

    public Vector3 GetWorldPosition(Vector2 chunkPosition)
    {
        chunkPosition = Chunk.CalculateWorldPosition(chunkPosition);
        return new Vector3(chunkPosition.x + Position.x, chunkPosition.y + Position.y, 1f);
    }

    public Vector3 GetWorldPositionPrecedence(Vector2 chunkPosition, int paddingTilesetID)
    {
        Vector3 position = GetWorldPosition(chunkPosition);
        position.z -= TerrainTileSet.LoadedTileSets[paddingTilesetID].Precedence;

        return position;
    }
}