using TheWorkforce.Entities;
using TheWorkforce.Entities.Interactions;
using TheWorkforce.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheWorkforce
{
    public class MouseController : MonoBehaviour
    {
        #region Static Members
        public static MouseController Instance => _instance;
        private static MouseController _instance;
        #endregion

        #region Private Members
        [SerializeField] private Sprite _outlineSprite;
        private EntityView _entityView;
        private Player _player;
        private Camera _personalCamera;
        private EntityCollection _entityCollection;
        private EntityInteractionDisplay _entityInteractionDisplay;
        private WorldController _worldController;
        private Vector2 _screenPosition;
        private Vector2 _worldPosition;

        private Tile _activeTile;
        private EntityInstance _activeInstance;
        private Interaction _activeInteraction;
        private ItemStack _itemInHand;

        private SpriteRenderer _outlinerRenderer;
        #endregion

        #region Unity API
        private void Awake()
        {
            if(_instance != null)
            {
                DestroyImmediate(this);
            }
            _instance = this;
            _entityInteractionDisplay = FindObjectOfType<EntityInteractionDisplay>();
            _outlineSprite = Resources.Load<Sprite>("UI/64x64 Outline");
            _outlinerRenderer = new GameObject("Outline Renderer").AddComponent<SpriteRenderer>();
            _outlinerRenderer.sprite = _outlineSprite;
            _outlinerRenderer.gameObject.SetActive(false);
        }
        #endregion

        #region Simple Setters
        public void SetPlayer(Player player)
        {
            _player = player;
        }

        public void SetCamera(Camera camera)
        {
            _personalCamera = camera;
        }

        public void SetEntityCollection(EntityCollection entityCollection)
        {
            _entityCollection = entityCollection;
        }

        public void SetWorldController(WorldController worldController)
        {
            _worldController = worldController;
        }

        public void SetEntityView(EntityView entityView)
        {
            _entityView = entityView;
        }
        #endregion

        public Vector2 CalculateWorldPosition()
        {
            _screenPosition = Input.mousePosition;
            
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f;

            _worldPosition = _personalCamera.ScreenToWorldPoint(mousePosition);

            return _worldPosition;
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
            
            _itemInHand.Add(itemStack);
            return itemStack;
        }

        public void UpdateMouseOver()
        {
            UpdateMouseOverTile();

            if (EventSystem.current.IsPointerOverGameObject())
            {
                NullifyEntityReference();
            }
            else
            {
                UpdateMouseOverEntity();
            }
            UpdateInteraction();
        }

        private void UpdateMouseOverTile()
        {
            Vector2 chunkPosition = Chunk.CalculateResidingChunk(_worldPosition);
            Vector2 tilePosition = Tile.TilePositionInRelationToChunk(_worldPosition);

            _activeTile = _worldController.RequestTile(chunkPosition, tilePosition);
        }

        private void UpdateMouseOverEntity()
        {
            uint currentEntityId = (_activeInstance == null) ? 0 : _activeInstance.GetId();

            // hovering over a tile
            if (_activeTile != null)
            {
                // find id of tile entity
                uint entity = _activeTile.StaticEntityInstanceId;

                // the new entity is not the current one
                if (currentEntityId != entity)
                {
                    NullifyEntityReference();
                    _activeInstance = _entityCollection.GetEntity(entity);
                    _entityView.SetEntity(_activeInstance);

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

        private void StartNewInteraction()
        {
            IInteract interact = (_activeInstance == null) ? null : _activeInstance as IInteract;
            if (interact != null)
            {
                _activeInteraction = interact.Interact(_player);
                _activeInteraction.OnDestroy += ResetInteraction;
            }
        }

        private void NullifyEntityReference()
        {
            _activeInstance = null;
            _entityView.SetEntity(_activeInstance);
        }

        private void ResetInteraction()
        {
            if(_activeInteraction != null)
            {
                _activeInteraction.Hide(_entityInteractionDisplay);
                _activeInteraction.OnDestroy -= ResetInteraction;
                _activeInteraction = null;
            }
        }

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
