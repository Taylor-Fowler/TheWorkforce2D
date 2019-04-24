using System;
using UnityEngine;

namespace TheWorkforce
{
    [Serializable]
    public class Tile
    {
        public const int PIXEL_SIZE = 32;

        public byte TileSetId { get; }
        public float Moisture { get; }
        public float Elevation { get; }
        public Vector2Int Position { get; }

        public uint StaticEntityInstanceId;


        public Tile(NetworkTile networkTile, Vector2Int position)
        {
            TileSetId = networkTile.TileSetId;

            StaticEntityInstanceId = networkTile.StaticEntityInstanceId;
            Moisture = networkTile.Moisture;
            Elevation = networkTile.Elevation;
            Position = position;
        }

        public Tile(byte tileSetId, float moisture, float elevation, Vector2Int position)
        {
            TileSetId = tileSetId;
            Moisture = moisture;
            Elevation = elevation;
            Position = position;
        }

        public void PlaceEntity(uint entityInstanceId)
        {
            StaticEntityInstanceId = entityInstanceId;
        }

        public byte[] GetByteArray()
        {
            const int floatSize = sizeof(float);

            int byteOffset = 0;

            // tilesetId + (elevation, moisture)
            // NOTE: No need to save tile position, can be deduced from position in file
            // NOTE: No need to save entity Id, this will be re-initialised on each game start
            byte[] bytes = new byte[1 + (floatSize * 2)];

            bytes[byteOffset] = TileSetId;
            ++byteOffset;

            Array.Copy(BitConverter.GetBytes(Moisture), 0, bytes, byteOffset, floatSize);
            byteOffset += floatSize;

            Array.Copy(BitConverter.GetBytes(Elevation), 0, bytes, byteOffset, floatSize);

            return bytes;
        }

        public Vector3 GetWorldPosition(Vector2Int chunkPosition)
        {
            chunkPosition = Chunk.CalculateWorldPosition(chunkPosition);
            return new Vector3(chunkPosition.x + Position.x, chunkPosition.y + Position.y, 1f);
        }

        public Vector3 GetWorldPositionPrecedence(Vector2Int chunkPosition, int paddingTileSetId)
        {
            Vector3 position = GetWorldPosition(chunkPosition);
            position.z -= TerrainTileSet.LoadedTileSets[paddingTileSetId].Precedence;

            return position;
        }

        /// <summary>
        /// Calculates the relative tile position from a given world position
        /// </summary>
        /// <param name="worldPosition">The world position to calculate from</param>
        /// <returns>A tile position relative to its chunk, ranging from: 0 -> (Chunk.SIZE-1)</returns>
        public static Vector2Int TilePositionInRelationToChunk(Vector2Int worldPosition)
        {
            worldPosition.x %= Chunk.SIZE;
            worldPosition.y %= Chunk.SIZE;

            //worldPosition.x = Mathf.Floor(worldPosition.x);
            //worldPosition.y = Mathf.Floor(worldPosition.y);

            // The world position's x co-ordinate is in the negative axis therefore we remap the range
            // Original x: (-1, -2, -3...-15, 0) -> Remapped x: (15 to 0) 
            if (worldPosition.x < 0)
            {
                worldPosition.x = Chunk.SIZE + worldPosition.x;
            }
            // Same formula as above
            if (worldPosition.y < 0)
            {
                worldPosition.y = Chunk.SIZE + worldPosition.y;
            }
            return worldPosition;
        }
    }
}