using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce.Network
{
    using Game_State; using Interfaces;

    /*
     * IMPORTANT REFERENCES:
     * 
     * Callback order: https://docs.unity3d.com/Manual/NetworkManagerCallbacks.html
     * UNet NetworkManager: https://docs.unity3d.com/Manual/UNetManager.html
     * 
     */
    
    /// <summary>
    /// The Custom Network Manager handles incoming player connections as well as server configuration.
    /// </summary>
    public class CustomNetworkManager : NetworkManager, IManager
    {
        public GameManager GameManager { get; private set; }

        public const short MsgGameStateChange = MsgType.Highest + 1;

        private List<NetworkConnection> _playerConnections;
        private Dictionary<NetworkConnection, PlayerController> _playerControllers;

        private Action _onBeginConnection;
        private Action _onBeginLoading;
        private Action _onPause;
        private Action _onResume;

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            _playerConnections = new List<NetworkConnection>();
            _playerControllers = new Dictionary<NetworkConnection, PlayerController>();
        }

        public void Initialise(Action beginConnection, Action beginLoading, Action pause, Action resume)
        {
            _onBeginConnection = beginConnection;
            _onBeginLoading = beginLoading;
            _onPause = pause;
            _onResume = resume;
        }

        #region Network Manager Overrides - Server
        /// <summary>
        /// Called on the server when it is started - including a host started server.
        /// </summary>
        public override void OnStartServer()
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStartServer()");
            base.OnStartServer();

            if(GameSave.CreateGame("Testing"))
            {

            }
        }

        /// <summary>
        /// Called on the server when a new client connects. This includes when the host starts the server and proceeds to connect.
        /// - When the incoming connection is the host client, proceed to run as usual.
        /// - When the incoming connection is a non-host client, server must inform all connected clients to pause gameplay
        /// 
        /// TODO: Assign an ID to the player on the server here. Once the player controller is initialised, they must ask for their ID from here.
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerConnect(NetworkConnection conn)
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerConnect(NetworkConnection) \n" +
                        "NetworkConnection conn.connectionId = " + conn.connectionId);

            // connectionId of 0 is the host connection, the host does not need to ask the game to pause.
            if(conn.connectionId != 0)
            {
                Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerConnect(NetworkConnection) \n" +
                            "Non-host client connected.");

                // The new client should start the connection pr
                _onBeginConnection?.Invoke();
                OnServerPauseGame();
            }

            _playerConnections.Add(conn);

            // AFAIK, No need to call base method as it does not affect the player connection
            // base.OnServerConnect(conn);
        }
        
        /// <summary>
        /// Called on the server when a client is ready. This includes the host client.
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerAddPlayer(NetworkConnection, short)");

            if(conn.isReady)
            {
                Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerAddPlayer(NetworkConnection, short) \n" + 
                            "Connection with ID-" + conn.connectionId + " is ready.");
            }

            var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            _playerConnections.Remove(conn);
            base.OnServerDisconnect(conn);
        }
        #endregion


        #region Network Manager Overrides - Client
        /// <summary>
        /// Called on the client when it is started. This includes the host client.
        /// </summary>
        /// <param name="client"></param>
        public override void OnStartClient(NetworkClient client)
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStartClient(NetworkClient)");
            base.OnStartClient(client);
        }

        /// <summary>
        /// Called on the client when it connects to the server. This includes the host client.
        /// </summary>
        /// <param name="conn"></param>
        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnClientConnect(NetworkConnection)");

            client.RegisterHandler(MsgGameStateChange, OnClientUpdateGameState);
            base.OnClientConnect(conn);
        }
        #endregion

        #region Network Messages
        /// <summary>
        /// Called on the server when a game pause needs to be scheduled. The server informs all other clients that they
        /// must pause the game at the specified time.
        /// </summary>
        private void OnServerPauseGame()
        {
            NetworkGameState networkGameState = new NetworkGameState
            {
                Previous = GameManager.GameState,
                Current = EGameState.Paused,

                Time = GameTime.Time + 5
            };

            foreach (var connection in _playerConnections)
            {
                NetworkServer.SendToClient(connection.connectionId, MsgGameStateChange, networkGameState);
            }
        }

        /// <summary>
        /// Called on each client when the game state changes. The client schedules a change of game state at the designated time.
        /// 
        /// E.G. The server will call this method informing the clients to pause the game at a specific time.
        /// </summary>
        /// <param name="networkMessage"></param>
        private void OnClientUpdateGameState(NetworkMessage networkMessage)
        {
            NetworkGameState networkGameState = networkMessage.ReadMessage<NetworkGameState>();
            
            if(networkGameState.Previous == EGameState.Paused)
            {
                if(networkGameState.Current == EGameState.Active)
                {
                    GameTime.ListenForSpecificPostTick(networkGameState.Time, _onResume);
                }
            }
            else if (networkGameState.Previous == EGameState.Active)
            {
                if (networkGameState.Current == EGameState.Paused)
                {
                    GameTime.ListenForSpecificPostTick(networkGameState.Time, _onPause);
                }
            }
        }
        #endregion
    }
}