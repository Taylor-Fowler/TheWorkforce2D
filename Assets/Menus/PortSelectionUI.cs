using UnityEngine;
using TheWorkforce.Network;

public class PortSelectionUI : MonoBehaviour
{
    [SerializeField] private CustomNetworkManager _networkManager;

    public void OnEdit(string portString)
    {
        ushort port;
        if(ushort.TryParse(portString, out port))
        {
            if(port < 10000)
            {
                _networkManager.networkPort = port;
            }
        }
    }
}
