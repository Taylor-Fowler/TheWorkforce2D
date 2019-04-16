using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using URandom = UnityEngine.Random;

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
        #region Constants + Statics
        /// <summary>
        /// Identifier for the game state change message
        /// </summary>
        public const short MsgGameStateChange = MsgType.Highest + 1;

        /// <summary>
        /// Identifier for the client name message
        /// 
        /// NOTE: The host is concerned with this message as they need to get client data from the save file
        /// </summary>
        public const short MsgClientIdentifier = MsgGameStateChange + 1;

        /// <summary>
        /// Identifier for the client game state confirmation message
        /// 
        /// NOTE: The host needs a confirmation from all clients that their game state has been updated
        /// </summary>
        public const short MsgGameStateConfirmation = MsgClientIdentifier + 1;
        #endregion

        /// <summary>
        /// Reference to the game manager
        /// </summary>
        public GameManager GameManager { get; private set; }

        private List<NetworkConnection> _playerConnections;
        private Dictionary<NetworkConnection, PlayerController> _playerControllers;
        private Tuple<uint, EGameState, List<NetworkConnection>> _awaitingConfirmation;

        private Action _onBeginConnection;
        private Action _onBeginLoading;
        private Action _onPause;
        private Action _onResume;

        private List<KeyValuePair<NetworkConnection, short>> _awaitingProcessing;
        private bool _preparingToProcessIncomingConnections = false;

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
        /// 
        /// Registers a handler on the server client for client identifier messages
        /// </summary>
        public override void OnStartServer()
        {
            _awaitingProcessing = new List<KeyValuePair<NetworkConnection, short>>();

            base.OnStartServer();
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStartServer()");
        }

        /// <summary>
        /// Called on the server when it is stopped - including a host server.
        /// 
        /// Unregisters the handler on the server client for client identifier messages
        /// </summary>
        public override void OnStopServer()
        {
            client.UnregisterHandler(MsgClientIdentifier);

            _awaitingProcessing = null;

            base.OnStopServer();
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStopServer()");
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

                if(!_preparingToProcessIncomingConnections)
                {
                    OnServerPauseGame();
                    _preparingToProcessIncomingConnections = true;
                }
            }
            // AFAIK, No need to call base method as it does not affect the player connection
            // base.OnServerConnect(conn);
        }
        
        /// <summary>
        /// Called on the server when a client is ready. This includes the host client.
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerReady(NetworkConnection conn)
        {
            //base.OnServerReady(conn);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="playerControllerId"></param>
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerAddPlayer(NetworkConnection, short)");

            //if(conn.isReady)
            //{
            //    Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerAddPlayer(NetworkConnection, short) \n" + 
            //                "Connection with ID-" + conn.connectionId + " is ready.");
            //}

            if(conn.connectionId != 0)
            {
                // TODO: The coroutine processing the new connections may have stopped at this point
                //          maybe this fix will work...
                if(!_preparingToProcessIncomingConnections)
                {
                    OnServerPauseGame();
                }
                _awaitingProcessing.Add(new KeyValuePair<NetworkConnection, short>(conn, playerControllerId));
            }
            else
            {
                OnServerCreatePlayer(conn, playerControllerId);
            }
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            _playerConnections.Remove(conn);
            var playerController = _playerControllers[conn];

            _playerControllers.Remove(conn);
            //GameFile.Instance.SavePlayer(playerController.Player);

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
            // Only the server needs to register for the following messages
            // client.connection is only initialised on the host (server + client) at this point...I think based on observations
            // 
            // No harm in keeping the Id check incase observation is wrong
            if(client.connection != null && client.connection.connectionId == 0)
            {
                client.RegisterHandler(MsgClientIdentifier, OnServerReceiveClientIdentifier);
                client.RegisterHandler(MsgGameStateConfirmation, OnServerReceiveStateConfirmation);
            }

            client.RegisterHandler(MsgGameStateChange, OnClientUpdateGameState);

            _onBeginConnection?.Invoke();

            base.OnStartClient(client);
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStartClient(NetworkClient)");
        }

        public override void OnStopClient()
        {
            client.UnregisterHandler(MsgGameStateChange);

            base.OnStopClient();
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStopClient()");
        }

        /// <summary>
        /// Called on the client when it connects to the server. This includes the host client.
        /// </summary>
        /// <param name="conn"></param>
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnClientConnect(NetworkConnection)");
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            Debug.Log(client.connection.clientOwnedObjects.Count);

            base.OnClientSceneChanged(conn);
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnClientSceneChanged(NetworkConnection)");
        }
        #endregion

        #region Sending Network Messages - from server
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

            _awaitingConfirmation = new Tuple<uint, EGameState, List<NetworkConnection>>
            (
                GameTime.BackgroundTime,
                EGameState.Paused,
                new List<NetworkConnection>(_playerConnections)
            );
        }
        #endregion

        #region Sending Network Messages - from client to server
        private void OnClientPause()
        {
            _onPause.Invoke();

            // The host client does not need to message itself (the server)
            // Only non-host clients need to message the server
            if(client.connection.connectionId != 0)
            {
                client.Send(MsgGameStateConfirmation, new NetworkStateConfirmation { NewState = EGameState.Paused });
            }
            // Server can call the local method
            else
            {
                OnServerProcessStateConfirmation(new NetworkStateConfirmation { NewState = EGameState.Paused }, client.connection);
            }
        }
        #endregion

        #region Receiving Network Messages - to server from client
        private void OnServerReceiveStateConfirmation(NetworkMessage networkMessage)
        {
            NetworkStateConfirmation networkStateConfirmation = networkMessage.ReadMessage<NetworkStateConfirmation>();

            OnServerProcessStateConfirmation(networkStateConfirmation, networkMessage.conn);
        }

        private void OnServerReceiveClientIdentifier(NetworkMessage networkMessage)
        {
            NetworkClientIdentifier networkClientIdentifier = networkMessage.ReadMessage<NetworkClientIdentifier>();


        }
        #endregion

        #region Receiving Network Messages - to client from server
        /// <summary>
        /// Called on each client when the game state changes. The client schedules a change of game state at the designated time.
        /// 
        /// E.G. The server will call this method informing the clients to pause the game at a specific time.
        /// </summary>
        /// <param name="networkMessage"></param>
        private void OnClientUpdateGameState(NetworkMessage networkMessage)
        {
            // Find the game state change to process and at which time
            NetworkGameState networkGameState = networkMessage.ReadMessage<NetworkGameState>();
            
            // From paused to active
            if(networkGameState.Previous == EGameState.Paused)
            {
                if(networkGameState.Current == EGameState.Active)
                {
                    GameTime.ListenForSpecificPostTick(networkGameState.Time, _onResume);
                }
            }
            // From active to paused
            else if (networkGameState.Previous == EGameState.Active)
            {
                if (networkGameState.Current == EGameState.Paused)
                {
                    GameTime.ListenForSpecificPostTick(networkGameState.Time, OnClientPause);
                }
            }
        }
        #endregion

        private void OnServerProcessStateConfirmation(NetworkStateConfirmation networkStateConfirmation, NetworkConnection conn)
        {
            if (_awaitingConfirmation.Item2 == networkStateConfirmation.NewState)
            {
                // Host connection special rule for removal...could not send the confirmation regular way
                // so instead using local method invoking on the server
                if(conn.connectionId == 0)
                {
                    _awaitingConfirmation.Item3.RemoveAll(x => x.connectionId == 0);
                }
                else
                {
                    _awaitingConfirmation.Item3.Remove(conn);
                }

                // All clients have confirmed that they changed state, now the server must process the new state
                if (_awaitingConfirmation.Item3.Count == 0)
                {
                    StartCoroutine(OnServerProcessNewPlayers());
                }
            }
            // otherwise....what has gone wrong
        }

        private IEnumerator OnServerProcessNewPlayers()
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerProcessNewPlayers()");

            while (_awaitingProcessing.Count > 0)
            {
                var pair = _awaitingProcessing[_awaitingProcessing.Count - 1];
                _awaitingProcessing.RemoveAt(_awaitingProcessing.Count - 1);

                OnServerCreatePlayer(pair.Key, pair.Value);
                yield return new WaitForFixedUpdate();
            }
            _preparingToProcessIncomingConnections = false;
        }

        private void OnServerCreatePlayer(NetworkConnection conn, short playerControllerId)
        {
            // Create the player object
            var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

            byte[] playerData;
                 
            // If the player already exists in the game file, get the data and initialise locally
            if(GameFile.Instance.PlayerExists(conn.connectionId, out playerData))
            {
                // the player already exists, initialise the player object and send to the player controller
                // the player controller should tell themselves on all other clients to initialise?....
            }
            else
            {
                // TODO: Create player data
                
                // Saves the player data to file 
                GameFile.Instance.SavePlayer(conn.connectionId, new byte[] { 0, 0, 0, 0, 1, 1, 1, 1 });
            }

            

            // If the player was added successfully, pair their connection with their controller
            if (NetworkServer.AddPlayerForConnection(conn, player, playerControllerId))
            {
                var playerController = player.GetComponent<PlayerController>();
                var worldController = player.GetComponent<WorldController>();

                
                _playerConnections.Add(conn);
                _playerControllers.Add(conn, playerController);

                // The new player is a regular client, we must send them the game data
                if(conn.connectionId != 0)
                {
                    player.GetComponent<FileTransfer>().ServerSendGameFile(conn);
                }
            }
        }

        private Vector3 GeneratePlayerPosition()
        {
            var position = URandom.insideUnitSphere * URandom.Range(0, 10000);
            position.z = 0f;
            return position;
        }
    }
}