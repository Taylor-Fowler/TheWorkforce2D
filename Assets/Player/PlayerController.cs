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
        /// <summary>
        /// An event that is invoked when the local player starts
        /// </summary>
        public static event Action<PlayerController> OnLocalPlayerControllerStartup;

        // The local player controller, the local controller has a reference to the game manager
        private static PlayerController Local;

        public event Action<Vector2> OnPlayerChunkUpdate;

        public int Id;
        public Player Player { get; protected set; }
        public GameManager GameManager { get; private set; }
        public Vector2 MouseWorldPosition { get; protected set; }

        // TODO: Move inventory prefab, toolbelt prefab, item inspector prefab to one prefab and get components off of it

        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private EntityViewLink _entityViewLink;
        [SerializeField] private PlayerCraftingDisplayRef _craftingDisplayRef;
        [SerializeField] private PlayerInventoryDisplayRef _inventoryDisplayRef;
        [SerializeField] private PlayerRecipeQueueDisplayRef _recipeQueueDisplayRef;

        private MouseController _mouseController;
        //private ToolbeltDisplay _toolbeltDisplay;

        #region NetworkBehaviour Overrides
        /// <summary>
        /// Called when the local player starts, sets the local player controller reference and invokes the 
        /// local player controller startup event
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            Local = this;
            OnLocalPlayerControllerStartup?.Invoke(this);
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
            CreateLocalPlayer();

            _inventoryDisplayRef.Get().SetInventory(Player.Inventory);
            _inventoryDisplayRef.Get().Hide();
            gameObject.AddComponent<PlayerCrafting>().Initialise(_craftingDisplayRef.Get(), _recipeQueueDisplayRef.Get(), Player.Inventory);

            _mouseController = gameObject.AddComponent<MouseController>();
            _mouseController.Initialise(Player, Instantiate(_cameraPrefab, transform).GetComponent<Camera>(), GameManager.WorldController, _entityViewLink.View);

            GameTime.SubscribeToPostUpdate(UpdateController);
            CmdStartAll();
            // All other players on the client should listen for when the player is ready and then initialise themselves also.
            // Local Player tells other players on this client (who are already in game) that they should initialise

            //_toolbeltDisplay.SetToolbelt(Player.Toolbelt);
        }

        #region Unity API
        private void Start()
        {
            // A player is initialised on their local client in OnStartLocalPlayer, all other clients will initialise the player
            // later during the start method
            if (!isLocalPlayer)
            {
                // The server should treat all player controllers as local players in terms of initialisation
                // This is so that when ANY player moves, the server will process the move and be able to 
                // update chunk dependencies based on the movement
                if (isServer)
                {
                    CreateLocalPlayer();
                }
                else
                {
                    CreateRemotePlayer();
                }
            }
        }
        #endregion

        /// <summary>
        /// Update method that does not rely on Unity callbacks. 
        /// Processes keyboard and mouse actions.
        /// </summary>
        private void UpdateController()
        {
            HandleKeyboard();
            HandleMouse();
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

        private void HandleMouse()
        {
            MouseWorldPosition = _mouseController.CalculateWorldPosition();
            _mouseController.UpdateController();
        }

        private void CreateLocalPlayer()
        {
            Player = new Player(this, new SlotCollection(45), new PlayerMovement(
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
        private void CmdMove(int horizontal, int vertical)
        {
            RpcMove(horizontal, vertical);
        }

        [ClientRpc]
        private void RpcMove(int horizontal, int vertical)
        {
            if(Player != null)
            {
                Player.Movement.Move(horizontal, vertical, transform);            
            }
        }

        [Command]
        private void CmdStartAll()
        {
            RpcStart();
        }

        [ClientRpc]
        private void RpcStart()
        {
            Id = playerControllerId;
            MouseWorldPosition = Vector2.zero;
        }
        #endregion
    }
}