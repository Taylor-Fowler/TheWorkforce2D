using System.Collections;
using System.Collections.Generic;
using TheWorkforce.Network;
using UnityEngine;

public class IpSelectionUI : MonoBehaviour
{
    [SerializeField] private CustomNetworkManager _networkManager;

    public void OnEdit(string ipString)
    {
        if(ipString == "localhost")
        {
            _networkManager.networkAddress = ipString;
        }
        else
        {
            var substrings = ipString.Split('.');
            if(substrings.Length == 4)
            {
                ushort first;
                if(!ushort.TryParse(substrings[0], out first))
                {
                    return;
                }
                if (first > 255) return;

                ushort second;
                if (!ushort.TryParse(substrings[1], out second))
                {
                    return;
                }
                if (second > 255) return;

                ushort third;
                if (!ushort.TryParse(substrings[2], out third))
                {
                    return;
                }
                if (third > 255) return;

                ushort fourth;
                if (!ushort.TryParse(substrings[3], out fourth))
                {
                    return;
                }
                if (fourth > 255) return;

                _networkManager.networkAddress = ipString;
            }
        }
    }
}
