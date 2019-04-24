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
        //private ToolbeltDisplay _toolbeltDisplay;

        #region NetworkBehaviour Overrides
        /// <summary>
        /// Called when the local player starts, sets the local player controller reference and invokes the 
        /// local player controller startup event
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            // TODO: Move to startup
            //Local = this;
            //GameTime.SubscribeToPostUpdate(UpdateController);
        }
        #endregion

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
            }
            // CreateLocalPlayer();
            // _mouseController = _playerSetup.AddComponents(this, gameManager.WorldController);

            
            // CmdStartAll();
            // All other players on the client should listen for when the player is ready and then initialise themselves also.
            // Local Player tells other players on this client (who are already in game) that they should initialise
        }

        public void OnClientInitialise(PlayerData playerData)
        {
            // Add the local player components (new camera, mouse controller etc)
            if (isLocalPlayer)
            {
                _mouseController = _playerSetup.AddComponents(this, GameManager.WorldController);
            }
        }

        /// <summary>
        /// Called on the server to initialise the player.
        /// </summary>
        /// <param name="playerData"></param>
        public void OnServerInitialise(PlayerData playerData)
        {
            Id = playerData.Id;
            transform.position = new Vector3(playerData.X, playerData.Y, transform.position.z);

            SlotCollection inventorySlots;
            if(playerData.InventoryItems == null)
            {
                inventorySlots = new SlotCollection(PlayerInventorySize);
            }
            else
            {
                inventorySlots = new SlotCollection(PlayerInventorySize, playerData.InventoryItems);
            }
            
            // As soon as the player is created, the world area is also created
            // Because PlayerChunkUpdate is called which requests up-to-date world information
            Player = new Player(this, inventorySlots, new PlayerMovement(
                        Id,
                        15f,
                        GetComponent<Animator>(),
                        PlayerChunkUpdate,
                        transform
                    )
                );

            // Add the local player components (new camera, mouse controller etc)
            if(isLocalPlayer)
            {
                _mouseController = _playerSetup.AddComponents(this, GameManager.WorldController);
            }
        }

        public void LoadPlayer()
        {

        }

        public void CreatePlayer()
        {

        }

        public PlayerData GetPlayerData()
        {
            return new PlayerData(Id, transform.position.x, transform.position.y)
            {
                InventoryItems = Player.Inventory.GetSaveData()
            };
        }


        #region Keyboard and Mouse controls
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

            CmdMove(horizontal, vertical);
        }
        #endregion

        private void CreateLocalPlayer()
        {
            Player = new Player(this, new SlotCollection(PlayerInventorySize), new PlayerMovement(
                        Id,
                        15f,
                        GetComponent<Animator>(),
                        PlayerChunkUpdate,
                        transform
                    )
                );
        }

        private void PlayerChunkUpdate(Vector2 position)
        {
            OnPlayerChunkUpdate?.Invoke(position);

            if(isServer)
            {
                Local.GameManager.WorldController.OnServerPlayerChunkUpdate(this, position);
            }
        }

        private void CreateRemotePlayer()
        {
            Player = new Player(this, new SlotCollection(45), new AnimatedMovement(3f, GetComponent<Animator>()));
        }


        #region Custom Event Invoking
        #endregion

        #region Custom Network Messages
        [Command]
        private void CmdMove(int horizontal, int vertical) => RpcMove(horizontal, vertical);

        [ClientRpc]
        private void RpcMove(int horizontal, int vertical)
        {
            if(Player != null)
            {
                Player.Movement.Move(horizontal, vertical, transform);            
            }
        }
        #endregion
    }
}