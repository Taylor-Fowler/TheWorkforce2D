using UnityEngine;
using UnityEngine.Networking;
using TheWorkforce.Game_State;
using TheWorkforce.Inventory;
using TheWorkforce.Entities;

namespace TheWorkforce
{
    public delegate void PlayerControllerStartup(object source, PlayerController playerController);

    /// <summary>
    /// The PlayerController is the bridge between user input and player interaction in the world.
    /// </summary>
    public class PlayerController : NetworkBehaviour, IManager
    {
        /// <summary>
        /// Called when the local PlayerController starts
        /// </summary>
        public static event PlayerControllerStartup OnPlayerControllerStartup;

        public int Id;
        public Player Player { get; protected set; }
        public GameManager GameManager { get; private set; }
        public Vector2 MouseWorldPosition { get; protected set; }

        #region Private Members
        // TODO: Move inventory prefab, toolbelt prefab, item inspector prefab to one prefab and get components off of it
        // The local player controller, the local controller has a reference to the game manager
        private static PlayerController Local;
        private bool _hasStarted = false;
        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private GameObject _toolbeltPrefab;
        [SerializeField] private GameObject _itemInspectorPrefab;
        [SerializeField] private GameObject _hudOptionsPrefab;

        private MouseController _mouseController;
        private PlayerInventoryDisplay _inventoryDisplay;
        private EntityInstance _mouseOverInstance;
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
            _mouseController.SetCamera(Instantiate(_cameraPrefab, transform).GetComponent<Camera>());
            _mouseController.SetEntityCollection(GameManager.EntityCollection);
            _mouseController.SetWorldController(GameManager.WorldController);
            CmdStartAll();
            //_toolbeltDisplay.SetToolbelt(Player.Toolbelt);
        }

        #region Unity API
        private void Update()
        {
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

        #region Custom Event Invoking
        private void PlayerControllerStartup()
        {
            OnPlayerControllerStartup?.Invoke(this, this);
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

            // All players on the server have a reference to the world controller request player chunk update
            if (isServer || isLocalPlayer)
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
            else
            {
                Player = new Player(this, new SlotCollection(45), new AnimatedMovement(3f, GetComponent<Animator>()));
            }

            if (isLocalPlayer)
            {
                _inventoryDisplay.SetInventory(Player.Inventory);
                _mouseController.SetPlayer(Player);
            }
        }
        #endregion
    }
}