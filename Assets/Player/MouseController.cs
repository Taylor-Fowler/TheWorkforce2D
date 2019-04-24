using UnityEngine;
using UnityEngine.EventSystems;

namespace TheWorkforce
{
    using Entities; using Entities.Interactions; using Interfaces;

    public class MouseController : MonoBehaviour
    {
        /// <summary>
        /// Get a reference to the mouse controller instance
        /// </summary>
        public static MouseController Instance => _instance;
        private static MouseController _instance;

        public Vector2 WorldPosition => _worldPosition;
        private Vector2 _worldPosition;
        private Vector2Int _worldPositionInt;

        #region Private Members
        [SerializeField] private SpriteRenderer _outlinerRenderer;
        [SerializeField] private EntityCollection _entityCollection;
        [SerializeField] private EntityInteractionDisplay _entityInteractionDisplay;

        private EntityView _entityView;
        private Player _player;
        private Camera _personalCamera;
        private WorldController _worldController;

        private Vector2 _screenPosition;

        private Tile _activeTile;
        private EntityInstance _activeInstance;
        private Interaction _activeInteraction;
        private ItemStack _itemInHand;

        #endregion

        #region Unity API
        /// <summary>
        /// Initialises the singleton reference and gets a reference to the entity interaction display
        /// </summary>
        private void Awake()
        {
            if(_instance != null)
            {
                DestroyImmediate(this);
            }

            _instance = this;

            _entityInteractionDisplay = FindObjectOfType<EntityInteractionDisplay>();
            _outlinerRenderer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Resets the mouse controller singleton instance
        /// </summary>
        private void OnDestroy()
        {
            if(_instance == this)
            {
                _instance = null;
            }
        }
        #endregion

        /// <summary>
        /// Assigns references to the dependencies
        /// </summary>
        /// <param name="player"></param>
        /// <param name="camera"></param>
        /// <param name="worldController"></param>
        /// <param name="entityView"></param>
        public void Initialise(Player player, Camera camera, WorldController worldController, EntityView entityView)
        {
            _player = player;
            _personalCamera = camera;
            _worldController = worldController;
            _entityView = entityView;
        }

        public void UpdateController()
        {
            CalculateWorldPosition(); // find the world position of the mouse
            UpdateMouseOverTile(); // get the tile at the mouse world position

            // If the mouse is over a UI object then stop displaying the entity that
            // the mouse was over previously
            if (EventSystem.current.IsPointerOverGameObject())
            {
                NullifyEntityReference();
            }
            // The mouse is not over UI so update which entity it is over
            else
            {
                UpdateMouseOverEntity();
            }
            UpdateInteraction();
        }

        public void CalculateWorldPosition()
        {
            _screenPosition = Input.mousePosition;
            
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f;

            _worldPosition = _personalCamera.ScreenToWorldPoint(mousePosition);
            _worldPositionInt.x = (int)_worldPosition.x;
            _worldPositionInt.y = (int)_worldPosition.y;
        }

        public ItemStack AddItemStackToHand(ItemStack itemStack)
        {
            // Do a simple swap if either of the items are null or the items mismatch
            if(_itemInHand == null || itemStack == null || _itemInHand.Item != itemStack.Item)
            {
                var hand = _itemInHand;
                _itemInHand = itemStack;
                return hand;
            }
            
            ushort amountAdded = _itemInHand.Add(itemStack.Count);
            if(amountAdded == itemStack.Count)
            {
                return null;
            }

            itemStack.Subtract(amountAdded);
            return itemStack;
        }

        /// <summary>
        /// Calculates the chunk and tile position of where the mouse currently is and then
        /// retrieves the tile at that location from the world controller
        /// </summary>
        private void UpdateMouseOverTile()
        {
            Vector2Int chunkPosition = Chunk.CalculateResidingChunk(_worldPositionInt);
            Vector2Int tilePosition = Tile.TilePositionInRelationToChunk(_worldPositionInt);

            TileController tileController = _worldController[chunkPosition, tilePosition];
            _activeTile = tileController?.GetTile();
        }

        private void UpdateMouseOverEntity()
        {
            // get the entity id that we were previously moused over (if any)
            uint previousEntityId = (_activeInstance == null) ? 0 : _activeInstance.Id;

            // hovering over a tile and not UI
            if (_activeTile != null)
            {
                // find id of the entity on the tile we are actively moused over
                uint currentEntityId = _activeTile.StaticEntityInstanceId;

                // we are now looking at a different entity
                if (previousEntityId != currentEntityId)
                {
                    _activeInstance = _entityCollection.GetEntity(currentEntityId);
                    _entityView.SetEntity(_activeInstance);

                    SetOutline();
                }
                // we are looking at the same entity, just need to reset the outline
                else if(currentEntityId != 0)
                {
                    SetOutline();
                }
            }
        }

        private void UpdateInteraction()
        {
            bool isInteracting = Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject();            

            // there is no current interaction
            if(_activeInteraction == null)
            {
                if (isInteracting)
                {
                    StartNewInteraction();
                }
            }
            else if(_activeInteraction.Target != _activeInstance)
            {
                //end previous interaction
                _activeInteraction.Destroy();
                //try start a new interaction
                if(isInteracting)
                {
                    StartNewInteraction();
                }
            }
            else
            {
                if(_activeInteraction.RequiresConstantInteraction && !isInteracting)
                {
                    //end previous interaction
                    _activeInteraction.Destroy();
                }
            }
            _activeInteraction?.Display(_entityInteractionDisplay);
        }

        /// <summary>
        /// Checks
        /// </summary>
        private void StartNewInteraction()
        {
            IInteract interact = (_activeInstance == null) ? null : _activeInstance as IInteract;
            if (interact != null)
            {
                _activeInteraction = interact.Interact(_player);
                _activeInteraction.OnDestroy += ResetInteraction;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void NullifyEntityReference()
        {
            _activeInstance = null;
            _entityView.SetEntity(_activeInstance);
        }

        /// <summary>
        /// Checks if there is an active interaction and if so:
        /// - Hides the interaction display
        /// - Stops listening for when the interaction is destroyed/finished
        /// - Removes the reference to the interaction
        /// </summary>
        private void ResetInteraction()
        {
            if(_activeInteraction != null)
            {
                _activeInteraction.Hide(_entityInteractionDisplay);
                _activeInteraction.OnDestroy -= ResetInteraction;
                _activeInteraction = null;
            }
        }

        /// <summary>
        /// Displays an outline shape around the entity that the mouse is currently over
        /// </summary>
        private void SetOutline()
        {
            if (_activeInstance != null)
            {
                var data = _activeInstance.GetData();
                int width = data.Width;
                int height = data.Height;

                _outlinerRenderer.transform.localScale = new Vector3(width, height, 1.0f);
                _outlinerRenderer.transform.position = new Vector3(_activeInstance.X + (0.5f * width), _activeInstance.Y + (0.5f * height), -2.0f);
                _outlinerRenderer.gameObject.SetActive(true);
            }
            else
            {
                _outlinerRenderer.gameObject.SetActive(false);
            }
        }
    }
}
