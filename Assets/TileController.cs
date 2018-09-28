using UnityEngine;
using UnityEngine.Networking;

public class TileController : NetworkBehaviour
{
    public Tile Tile
    {
        get; protected set;
    }
}
