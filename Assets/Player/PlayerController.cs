using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
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
        #region Custom Event Declarations
        /// <summary>
        /// Called when the local PlayerController starts
        /// </summary>
        public static event PlayerControllerStartup OnPlayerControllerStartup;
        #endregion

        #region IManager Implementation
        public GameManager GameManager { get; private set; }

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;

            Player = new Player
                (
                    this, 
                    new SlotCollection(36),
                    //new Toolbelt((IEnumerable<EToolType>)Enum.GetValues(typeof(EToolType))),
                    new PlayerMovement(3f, GetComponent<Animator>(), GameManager.WorldController.RequestPlayerChunkUpdate, transform)
                );
            _inventoryDisplay.SetInventory(Player.Inventory);
            //_toolbeltDisplay.SetToolbelt(Player.Toolbelt);
        }
        #endregion

        #region Public Properties
        // TODO: Id will be required to identify the player across save files
        public int Id { get; private set; }
        public Player Player { get; protected set; }
        public Vector2 MouseWorldPosition { get; protected set; }
        #endregion

        #region Private Members
        // TODO: Move inventory prefab, toolbelt prefab, item inspector prefab to one prefab and get components off of it
        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private GameObject _toolbeltPrefab;
        [SerializeField] private GameObject _itemInspectorPrefab;
        [SerializeField] private GameObject _hudOptionsPrefab;

        private Camera _personalCamera;
        private PlayerInventoryDisplay _inventoryDisplay;
        private EntityInstance _mouseOverInstance;
        //private ToolbeltDisplay _toolbeltDisplay;
        #endregion

        #region NetworkBehaviour Overrides
        public override void OnStartLocalPlayer()
        {
            Id = playerControllerId;
            _personalCamera = Instantiate(_cameraPrefab, transform).GetComponent<Camera>();

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

        #region Unity API
        private void Start()
        {
            if(!isLocalPlayer)
            {
                Player = new Player
                    (
                        this,
                        new SlotCollection(36),
                        //new Toolbelt((IEnumerable<EToolType>)Enum.GetValues(typeof(EToolType))),
                        new AnimatedMovement(3f, GetComponent<Animator>())
                    );
            }
        }

        private void Update()
        {
            if (!isLocalPlayer || GameManager == null)
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
            if(!EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = 10f;

                MouseWorldPosition = _personalCamera.ScreenToWorldPoint(mousePosition);
                Vector2 chunkPosition = Chunk.CalculateResidingChunk(MouseWorldPosition);
                Vector2 tilePosition = Tile.TilePosition(MouseWorldPosition);

                var tile = GameManager.WorldController.RequestTile(chunkPosition, tilePosition);
                if(tile != null)
                {
                    uint entity = tile.StaticEntityInstanceId;
                    if(entity != 0)
                    {
                        GameManager.EntityCollection.GetEntity(entity).Display();
                    }
                }
            }
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
        #endregion
    }
}