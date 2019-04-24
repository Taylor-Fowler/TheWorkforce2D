using System;
using System.Collections;
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

        private NetworkClientControllers _networkClientControllers;
        private Tuple<uint, EGameState, List<NetworkConnection>> _awaitingConfirmation;

        private Action _onBeginConnection;
        private Action _onBeginGame;
        private Action _onBeginLoading;
        private Action _onPause;
        private Action _onResume;

        private List<KeyValuePair<NetworkConnection, short>> _awaitingProcessing;
        private bool _preparingToProcessIncomingConnections = false;

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                _onPause.Invoke();
                StopHost();
            }
        }

        #region Initialisation/Dependency Injection
        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            _networkClientControllers = new NetworkClientControllers();
        }

        public void Initialise(Action beginConnection, Action beginLoading, Action beginGame, Action pause, Action resume)
        {
            _onBeginConnection = beginConnection;
            _onBeginLoading = beginLoading;
            _onBeginGame = beginGame;
            _onPause = pause;
            _onResume = resume;
        }
        #endregion

        #region Network Manager Overrides - Server
        /// <summary>
        /// Called on the server when it is started - including a host started server.
        /// 
        /// Initialises the collection of new NetworkConnections to process
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
        /// Cleans up the server client, saves all connected clients' player data
        /// and saves the world to file.
        /// </summary>
        public override void OnStopServer()
        {
            client.UnregisterHandler(MsgClientIdentifier);

            _awaitingProcessing = null;

            foreach(var controllers in _networkClientControllers)
            {
                PlayerData playerData = controllers.PlayerController.GetPlayerData();
                GameFile.Instance.Save(playerData);
            }
            GameFile.Instance.Save(WorldController.Local.World);

            //Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnStopServer() \n"
            //            + $"_networkClientControllers.Count: {_networkClientControllers.Count}");

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
                        $"NetworkConnection conn.connectionId = {conn.connectionId}");

            // connectionId of 0 is the host connection, the host does not need to ask the game to pause.
            if(conn.connectionId != 0)
            {
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
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="playerControllerId"></param>
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("<color=#4688f2><b>[CustomNetworkManager]</b></color> - OnServerAddPlayer(NetworkConnection, short)");

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
            // TODO: Safer disconnect, release assets assigned to the current connection etc
            _networkClientControllers.Remove(conn);
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

            if(client.connection.connectionId != 0)
            {
                // Delete the temp game world
            }

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

            var networkConnections = new List<NetworkConnection>();
            foreach (var clientControllers in _networkClientControllers)
            {
                NetworkServer.SendToClient(clientControllers.Connection.connectionId, MsgGameStateChange, networkGameState);
                networkConnections.Add(clientControllers.Connection);
            }

            _awaitingConfirmation = new Tuple<uint, EGameState, List<NetworkConnection>>
            (
                GameTime.BackgroundTime,
                EGameState.Paused,
                networkConnections
            );
        }
        #endregion

        #region Sending Network Messages - from client to server
        /// <summary>
        /// Called on the client when the server requests a pause. This includes the host client.
        /// 
        /// Invokes the pause event and informs the server that it has successfully paused.
        /// </summary>
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

        /// <summary>
        /// Called on the server when a state confirmation message has been received and unpackaged.
        /// 
        /// Checks whether all other clients have paused and if so continues its queued process (AddPlayer etc)
        /// </summary>
        /// <param name="networkStateConfirmation"></param>
        /// <param name="conn"></param>
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
            else
            {
                Debug.LogError("<color=#4688f2><b>[CustomNetworkManager]</b></color> " +
                            "- OnServerProcessStateConfirmation(NetworkStateConfirmation, NetworkConnection) \n" +
                            $"Fatal Error - A client with connection Id: {conn.connectionId} is trying to change to a different game state.\n" +
                            $"Expected: {_awaitingConfirmation.Item2.ToString()} \n" +
                            $"Received: {networkStateConfirmation.NewState.ToString()}.");
            }
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
            GameObject playerObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

            if (!NetworkServer.AddPlayerForConnection(conn, playerObject, playerControllerId))
            {
                Debug.LogError("[CustomNetworkManager] - OnServerCreatePlayer(conn, playerControllerId) \n"
                                + $"Fatal Error - Network Server could not create player for connection: {conn}");
                return;
            }

            ClientControllers clientControllers = new ClientControllers
            (
                conn,
                playerObject.GetComponent<PlayerController>(),
                playerObject.GetComponent<WorldController>(),
                playerObject.GetComponent<FileTransfer>()
            );

            clientControllers.WorldController.Startup(GameManager);
            clientControllers.PlayerController.Startup(GameManager);

            // TODO: 
            //          FirstTime: Create the player data and save it
            //                      Generate the world data around the player and save it
            //          All times: Send the player and world data
            //                      tell all clients to initialise player and world from data sent

            // Scenario: The host client is being created, this is the first player on the server
            // 
            //      1. Load the world data from file
            //      2. Initialise the worldController with the loaded world
            //      3. Initialise the playerController with the player identifier
            //          3a. The player may not exist yet 
            //              - initialise after creating the player data?
            //              - OR create player data whilst initialising (need access to the worldController)
            //      4. Use the playerController position to generate/load chunks
            if (conn.connectionId == 0)
            {
                World gameWorld = GameFile.Instance.LoadWorld();

                clientControllers.WorldController.OnServerClientInitialise(gameWorld); // Set the world reference in the world controller

                byte[] playerBytes;
                if(!GameFile.Instance.PlayerExists(conn.connectionId, out playerBytes)) // If the player does not exist in the save, create and save their data
                {
                    var playerPosition = clientControllers.WorldController.GeneratePlayerPosition();
                    PlayerData playerData = new PlayerData(conn.connectionId, playerPosition.x, playerPosition.y);

                    GameFile.Instance.Save(playerData);
                    clientControllers.PlayerController.OnServerInitialise(playerData);
                }
                else
                {
                    clientControllers.PlayerController.OnServerInitialise(new PlayerData(playerBytes));
                }
                _networkClientControllers.Add(clientControllers);
                _onBeginGame?.Invoke();
            }
            // Scenario: A regular client has connected and their player is being created
            //
            //      1. Initialise the new playerController with their player identifier
            //      2. Generate the necessary chunks for the new player
            //          2a. Server saves the chunks
            //          2b. Send the newly created chunks to the other playerControllers (not the new one)
            //              2bi. The playerControllers must save the chunks to file
            //      3. Send the updated game file to the new client
            //      4. After the client confirms receiving the files, make other playerControllers initialise
            //          on the new client
            //      5.
            else
            {
                //networkClientControllers.PlayerController.OnServerInitialise(conn.connectionId);
                clientControllers.FileTransfer.ServerSendGameFile(conn);
                // TODO: 
                // once the file is transferred, onclient initialise for both playerController and
                // for worldController
            }
        }

        /// <summary>
        /// Called on the server when a client has confirmed the game file transfer.
        /// 
        /// As a file transfer is only initiated when the client joins, it is at this point
        /// the client's controllers are initialised from file.
        /// </summary>
        /// 
        /// <param name="controllers"></param>
        private void OnServerConfirmFileTransfer(NetworkClientControllers controllers)
        {

        }
    }
}