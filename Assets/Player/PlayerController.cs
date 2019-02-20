using UnityEngine;
using UnityEngine.Networking;
using TheWorkforce.Game_State;
using TheWorkforce.Inventory;
using TheWorkforce.Entities;

namespace TheWorkforce
{
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

        #region Private Members
        // TODO: Move inventory prefab, toolbelt prefab, item inspector prefab to one prefab and get components off of it
        // The local player controller, the local controller has a reference to the game manager
        private bool _hasStarted = false;
        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private GameObject _toolbeltPrefab;
        [SerializeField] private GameObject _hudOptionsPrefab;
        [SerializeField] private EntityViewLink _entityViewLink;

        private MouseController _mouseController;
        private PlayerInventoryDisplay _inventoryDisplay;
        //private ToolbeltDisplay _toolbeltDisplay;
        #endregion

        #region NetworkBehaviour Overrides
        public override void OnStartLocalPlayer()
        {
            Local = this;
            var canvas = new GameObject("Player Canvases").transform;
            canvas.SetParent(transform);

            _inventoryDisplay = Instantiate(_inventoryPrefab, canvas).GetComponent<PlayerInventoryDisplay>();
            //_toolbeltDisplay = Instantiate(_toolbeltPrefab, canvas).GetComponentInChildren<ToolbeltDisplay>();

            {
                var hud = Instantiate(_hudOptionsPrefab, canvas).GetComponent<HudMenuOptions>();
                hud.InventoryHudOption.SetDisplay(_inventoryDisplay);    
                //hud.ToolbeltHudOption.SetDisplay(_toolbeltDisplay);
            }
            
            PlayerControllerStartup();
        }
        #endregion

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            _mouseController = gameObject.AddComponent<MouseController>();
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
            if (isServer || isLocalPlayer)
            {
                CreateLocalPlayer();
            }
            else
            {
                CreateRemotePlayer();
            }

            if (isLocalPlayer)
            {
                _inventoryDisplay.SetInventory(Player.Inventory);
                _inventoryDisplay.Hide();
                _mouseController.SetPlayer(Player);
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

            // The server should treat all player controllers as local players in terms of initialisation
            // This is so that when ANY player moves, the server will process the move and be able to 
            // update chunk dependencies based on the movement
            //if (isServer || isLocalPlayer)
            //{
            //    CreateLocalPlayer();
            //}
            //else
            //{
            //    CreateRemotePlayer();
            //}

            //if (isLocalPlayer)
            //{
            //    _inventoryDisplay.SetInventory(Player.Inventory);
            //    _inventoryDisplay.Hide();
            //    _mouseController.SetPlayer(Player);
            //}
        }
        #endregion
    }
}