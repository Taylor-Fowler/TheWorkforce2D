using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheWorkforce.World
{
    [Serializable]
    public struct Tile
    {
        public const int PX_SIZE = 32;

        public int TileSetId;
        public float Moisture;
        public float Elevation;
        public Vector2 Position;

        public Vector3 GetWorldPosition(Vector2 chunkPosition)
        {
            chunkPosition = Chunk.CalculateWorldPosition(chunkPosition);
            return new Vector3(chunkPosition.x + Position.x, chunkPosition.y + Position.y, 1f);
        }

        public Vector3 GetWorldPositionPrecedence(Vector2 chunkPosition, int paddingTileSetId)
        {
            Vector3 position = GetWorldPosition(chunkPosition);
            position.z -= TerrainTileSet.LoadedTileSets[paddingTileSetId].Precedence;

            return position;
        }
    }    
}