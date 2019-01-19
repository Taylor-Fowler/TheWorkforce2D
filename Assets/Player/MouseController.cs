using System;
using System.Collections.Generic;
using TheWorkforce.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheWorkforce
{
    public class MouseController : MonoBehaviour
    {
        private Camera _personalCamera;
        private EntityCollection _entityCollection;
        private WorldController _worldController;
        private Vector2 _screenPosition;
        private Vector2 _worldPosition;

        private Tile _activeTile;
        private EntityInstance _activeInstance;

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

        public Vector2 CalculateWorldPosition()
        {
            _screenPosition = Input.mousePosition;
            
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f;

            _worldPosition = _personalCamera.ScreenToWorldPoint(mousePosition);

            return _worldPosition;
        }

        public void UpdateMouseOver()
        {
            UpdateMouseOverTile();

            if (EventSystem.current.IsPointerOverGameObject())
            {
                _activeInstance?.Hide();
            }
            else
            {
                UpdateMouseOverEntity();
            }
        }

        private void UpdateMouseOverTile()
        {
            Vector2 chunkPosition = Chunk.CalculateResidingChunk(_worldPosition);
            Vector2 tilePosition = Tile.TilePosition(_worldPosition);

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
                    _activeInstance?.Display();
                }
            }
        }

        private void NullifyEntityReference()
        {
            _activeInstance?.Hide();
            _activeInstance = null;
        }
    }
}
