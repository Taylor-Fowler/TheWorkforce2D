using System;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce
{
    using Game_State; using Inventory; using Interfaces; using Crafting; using Entities; using SOs.References;

    /*
     * IMPORTANT REFERENCES:
     *                      - https://docs.unity3d.com/Manual/NetworkBehaviourCallbacks.html
     */

    /// <summary>
    /// The PlayerController is the bridge between user input and player interaction in the world.
    /// </summary>
    public class PlayerController : NetworkBehaviour, IManager
    {
        #region Constants + Statics
        private const int PlayerInventorySize = 45;
        private const float PlayerMovementSpeed = 5f;

        /// <summary>
        /// An event that is invoked when a Player Controller starts
        /// </summary>
        public static event Action<PlayerController> OnPlayerControllerStartup;

        /// <summary>
        /// The local PlayerController for the current client
        /// </summary>
        public static PlayerController Local { get; private set; }
        #endregion

        public event Action<Vector2> OnPlayerChunkUpdate;

        public int Id { get; private set; }
        public Player Player { get; private set; }
        public GameManager GameManager { get; private set; }

        [SerializeField] private PlayerSetup _playerSetup;

        private MouseController _mouseController;

        /// <summary>
        /// Should only be called on the local player, from the game manager, this is because the method requires a valid reference to the game manager
        /// and because it relies on the game manager to be in a certain state (i.e. with public members initialised)
        /// 
        /// Creates the local player and initialises UI components that rely on the player's attributes (inventory, crafting etc)
        /// </summary>
        /// <param name="gameManager"></param>
        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
                
            if(isLocalPlayer)
            {
                Local = this;
                OnPlayerControllerStartup?.Invoke(this);
                GameTime.SubscribeToPostUpdate(UpdateController);

                GameManager.OnGameStateChange += GameManager_OnGameStateChange;
            }
        }

        private void GameManager_OnGameStateChange(GameStateChangeArgs gameStateChangeArgs)
        {
            if(gameStateChangeArgs.Current == EGameState.Disconnecting)
            {
                Local = null;
            }
        }

        /// <summary>
        /// Called on the server to initialise the player.
        /// </summary>
        /// <param name="playerData"></param>
        public void OnServerInitialise(PlayerData playerData)
        {
            // As soon as the player is created, the world area is also created
            // Because PlayerChunkUpdate is called which requests up-to-date world information
            CreateLocalPlayer(playerData);

            if(isLocalPlayer)
            {
                InitialiseMouseController();
            }
        }
        private void OnLocalClientInitialise(PlayerData playerData)
        {
            CreateLocalPlayer(playerData);
            InitialiseMouseController();
        }

        public PlayerData GetPlayerData()
        {
            return new PlayerData(Id, transform.position.x, transform.position.y)
            {
                InventoryItems = Player.Inventory.GetSaveData()
            };
        }


        #region Keyboard and Mouse controls
        private void InitialiseMouseController()
        {
            _mouseController = _playerSetup.AddComponents(this, GameManager.WorldController);
        }

        /// <summary>
        /// Update method that does not rely on Unity callbacks. 
        /// Processes keyboard and mouse actions.
        /// </summary>
        private void UpdateController()
        {
            HandleKeyboard();
            _mouseController.UpdateController();
        }

        private void HandleKeyboard()
        {
            int vertical = 0;
            int horizontal = 0;

            if (Input.GetKey(KeyCode.W))
            {
                vertical += 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                vertical -= 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                horizontal += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                horizontal -= 1;
            }

            Player.Movement.Move(horizontal, vertical, transform);

            CmdMove(horizontal, vertical, transform.position);
        }
        #endregion

        private void CreateLocalPlayer(PlayerData playerData)
        {
            Id = playerData.Id;
            transform.position = new Vector3(playerData.X, playerData.Y, transform.position.z);

            SlotCollection inventorySlots;
            if (playerData.InventoryItems == null)
            {
                inventorySlots = new SlotCollection(PlayerInventorySize);
            }
            else
            {
                inventorySlots = new SlotCollection(PlayerInventorySize, playerData.InventoryItems);
            }

            Player = new Player(this, inventorySlots, new PlayerMovement(
                        Id,
                        PlayerMovementSpeed,
                        GetComponent<Animator>(),
                        PlayerChunkUpdate,
                        transform
                    )
                );
        }

        private void CreateRemotePlayer(PlayerData playerData)
        {
            transform.position = new Vector3(playerData.X, playerData.Y, transform.position.z);


            SlotCollection inventorySlots;
            if (playerData.InventoryItems == null)
            {
                inventorySlots = new SlotCollection(PlayerInventorySize);
            }
            else
            {
                inventorySlots = new SlotCollection(PlayerInventorySize, playerData.InventoryItems);
            }

            Player = new Player(this, inventorySlots, new AnimatedMovement(PlayerMovementSpeed, GetComponent<Animator>()));
        }

        private void PlayerChunkUpdate(Vector2 position)
        {
            OnPlayerChunkUpdate?.Invoke(position);

            if(isServer)
            {
                Local.GameManager.WorldController.OnServerPlayerChunkUpdate(this, position);
            }
        }

        #region Custom Event Invoking
        #endregion

        #region Custom Network Messages
        [Command]
        public void CmdUpdatePlayerData(byte[] playerBytes)
        {
            GameFile.Instance.Save(new PlayerData(playerBytes));
            Debug.Log("[CmdUpdatePlayerDate(byte[])");
        }


        [Command]
        private void CmdMove(int horizontal, int vertical, Vector2 position) => RpcMove(horizontal, vertical, position);

        [ClientRpc]
        private void RpcMove(int horizontal, int vertical, Vector2 position)
        {
            if(Player != null && !isLocalPlayer)
            {
                Player.Movement.Move(horizontal, vertical, transform);
                transform.position = new Vector3(position.x, position.y, transform.position.z); // Fix?
            }
        }

        [ClientRpc]
        public void RpcInitialiseNewPlayer(byte[] playerBytes)
        {
            if (!isServer)
            {
                if (isLocalPlayer)
                {
                    OnLocalClientInitialise(new PlayerData(playerBytes));
                }
                else
                {
                    CreateRemotePlayer(new PlayerData(playerBytes));
                }
            }
        }

        [TargetRpc]
        public void TargetInitialisePlayer(NetworkConnection conn, byte[] playerBytes)
        {
            CreateRemotePlayer(new PlayerData(playerBytes));
        }
        #endregion
    }
}