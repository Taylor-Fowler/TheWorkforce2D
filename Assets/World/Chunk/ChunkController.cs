using System.Collections.Generic;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
    /// <summary>
    ///     The tile controllers that exist within the currently controlled chunk.
    /// </summary>
    private readonly List<TileController> _tileControllers = new List<TileController>();

    /// <summary>
    ///     Gets the controlled chunk.
    /// </summary>
    /// <value>
    ///     The chunk that the controller controls
    /// </value>
    public Chunk Chunk { get; protected set; }


    public void SetChunk(Chunk chunk, World world)
    {
        Chunk = chunk;
        transform.position = Chunk.Position * Chunk.SIZE;
        transform.position += transform.parent.position;
        name = "Chunk Controller: " + Chunk.Position.x + ", " + Chunk.Position.y;

        if (_tileControllers.Count == 0) SpawnTileControllers();

        int i = 0;
        foreach (var tile in Chunk.Tiles)
        {
            _tileControllers[i].SetTile(tile, world.GetTilePadding(Chunk, tile));
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
            tile.transform.position = transform.position + new Vector3(x, y, 0f);
            tile.AddComponent<SpriteRenderer>();
            tile.name = "Tile Controller: " + x + ", " + y;

            _tileControllers.Add(tile.AddComponent<TileController>());
        }
    }
}