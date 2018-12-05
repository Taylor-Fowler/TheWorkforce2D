using System;
using UnityEngine;
using UnityEngine.Networking;
using TheWorkforce.Game_State;
using TheWorkforce.Testing;

namespace TheWorkforce.Network
{
    public class CustomNetworkManager : NetworkManager, IManager
    {
        #region IManager Implementation
        public GameManager GameManager { get; private set; }

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
        }
        #endregion

        public override void OnClientConnect(NetworkConnection connection)
        {
            base.OnClientConnect(connection);
        }
    }
}