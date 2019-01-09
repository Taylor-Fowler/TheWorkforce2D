using System;
using TheWorkforce.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheWorkforce.World
{
    [Serializable]
    public class Tile
    {
        #region Public Static Methods
        public static Vector2 TilePosition(Vector2 worldPosition)
        {
            worldPosition.x %= Chunk.SIZE;
            worldPosition.y %= Chunk.SIZE;

            worldPosition.x = Mathf.Floor(worldPosition.x);
            worldPosition.y = Mathf.Floor(worldPosition.y);

            if(worldPosition.x < 0)
            {
                worldPosition.x = Chunk.SIZE + worldPosition.x;
            }

            if(worldPosition.y < 0)
            {
                worldPosition.y = Chunk.SIZE + worldPosition.y;
            }

            return worldPosition;
        }
        #endregion

        #region Public Constant Members
        public const int PX_SIZE = 32;
        #endregion

        #region Public Members
        public byte TileSetId;
        public ushort ItemOnTileId;

        public float Moisture;
        public float Elevation;
        public Vector2 Position;
        #endregion

        public Tile() {}

        public Tile(NetworkTile networkTile)
        {
            TileSetId = networkTile.TileSetId;
            
            ItemOnTileId = networkTile.ItemOnTile;
            Moisture = networkTile.Moisture;
            Elevation = networkTile.Elevation;
            Position = networkTile.Position;
        }

        #region Public Methods
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
        #endregion
    }
}







public interface IView
{
    void view(SomeMonoUI ui);
}

public class A : IView
{
    public void view(SomeMonoUI ui)
    {
        ui.DisplayText();
        ui.DisplayImage();
    }
}

public class B : IView
{
    public void view(SomeMonoUI ui)
    {
        ui.DisplayText();
    }
}

public class SomeMonoUI
{
    public void DisplayText()
    {

    }

    public void DisplayImage()
    {

    }
}


public class someplayer
{
    IView[] allTheViews;
    SomeMonoUI referenceUI;

    public void Update()
    {
        // find view where mouse is
        allTheViews[0].view(referenceUI);
    }
}