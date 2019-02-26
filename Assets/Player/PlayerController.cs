using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce
{
    using Game_State;
    using Inventory;
    using Crafting;
    using Entities;
    using SOs.References;

    public delegate void PlayerControllerStartup(object source, PlayerController playerController);

    /*
     * https://docs.unity3d.com/Manual/NetworkBehaviourCallbacks.html
     * 
     * 
     */

    /// <summary>
    /// The PlayerController is the bridge between user input and player interaction in the world.
    /// </summary>
    public class PlayerController : NetworkBehaviour, IManager
    {
        /// <summary>
        /// An event that is invoked when the local player starts
        /// </summary>
        public static event PlayerControllerStartup OnLocalPlayerControllerStartup;
        private static PlayerController Local;

        public int Id;
        public Player Player { get; protected set; }
        public GameManager GameManager { get; private set; }
        public Vector2 MouseWorldPosition { get; protected set; }
        // TODO: Move inventory prefab, toolbelt prefab, item inspector prefab to one prefab and get components off of it
        // The local player controller, the local controller has a reference to the game manager
        private bool _hasStarted = false;

        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private EntityViewLink _entityViewLink;
        [SerializeField] private PlayerCraftingDisplayRef _craftingDisplayRef;
        [SerializeField] private PlayerInventoryDisplayRef _inventoryDisplayRef;

        private MouseController _mouseController;
        //private ToolbeltDisplay _toolbeltDisplay;

        #region NetworkBehaviour Overrides
        public override void OnStartLocalPlayer()
        {
            Local = this;
            PlayerControllerStartup();
        }
        #endregion

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            CreateLocalPlayer();

            _inventoryDisplayRef.Get().SetInventory(Player.Inventory);
            _inventoryDisplayRef.Get().Hide();
            gameObject.AddComponent<PlayerCrafting>().Initialise(_craftingDisplayRef.Get(), new RecipeProcessorQueue(), Player.Inventory);

            _mouseController = gameObject.AddComponent<MouseController>();
            _mouseController.SetPlayer(Player);
            _mouseController.SetEntityView(_entityViewLink.View);
            _mouseController.SetCamera(Instantiate(_cameraPrefab, transform).GetComponent<Camera>());
            _mouseController.SetEntityCollection(GameManager.EntityCollection);
            _mouseController.SetWorldController(GameManager.WorldController);
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

        private void Update()
        {
            // Dont update if...
            //  - Not the local player
            //  - The game manager hasnt been initialised yet
            //  - The game hasnt started
            if (!isLocalPlayer || GameManager == null || !_hasStarted)
            {
                return;
            }
            HandleKeyboard();
            HandleMouse();
        }
        #endregion

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
            _mouseController.UpdateMouseOver();
        }

        private void CreateLocalPlayer()
        {
            Player = new Player(this, new SlotCollection(45), new PlayerMovement(
                        Id,
                        3f,
                        GetComponent<Animator>(),
                        Local.GameManager.WorldController.RequestPlayerChunkUpdate,
                        transform
                    )
                );
        }

        private void CreateRemotePlayer()
        {
            Player = new Player(this, new SlotCollection(45), new AnimatedMovement(3f, GetComponent<Animator>()));
        }

        #region Custom Event Invoking
        private void PlayerControllerStartup()
        {
            OnLocalPlayerControllerStartup?.Invoke(this, this);
        }
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
            _hasStarted = true;
        }
        #endregion
    }
}