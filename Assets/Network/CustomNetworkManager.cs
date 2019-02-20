using System;
using UnityEngine;
using UnityEngine.Networking;
using TheWorkforce.Game_State;

namespace TheWorkforce.Network
{
    /*
     * IMPORTANT REFERENCES:
     * 
     * Callback order: https://docs.unity3d.com/Manual/NetworkManagerCallbacks.html
     * https://docs.unity3d.com/Manual/UNetManager.html
     * 
     * 
     */
    public class CustomNetworkManager : NetworkManager, IManager
    {
        public GameManager GameManager { get; private set; }
        private Action _loadGame;

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
        }

        public void SetLoadGameAction(Action loadGame)
        {
            _loadGame = loadGame;
        }

        #region Network Manager Overrides
        public override void OnStartServer()
        {
            base.OnStartServer();

            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStartServer()");
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);

            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerConnect(NetworkConnection)");
        }

        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);
            
            if(_loadGame != null)
            {
                _loadGame.Invoke();
            }

            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStartClient(NetworkClient)");
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerAddPlayer(NetworkConnection, short)");
        }
        #endregion
    }
}