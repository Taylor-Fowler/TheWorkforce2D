using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public GameObject CameraPrefab;

    public override void OnStartLocalPlayer()
    {
        Instantiate(CameraPrefab, this.transform);
    }
}
