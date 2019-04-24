using UnityEngine;
using TMPro;

namespace TheWorkforce.Testing
{
    [System.Serializable]
    public class DebugPlayerDetails
    {
        #region Private Members
        [SerializeField] private TextMeshProUGUI _playerChunkPosition;
        [SerializeField] private TextMeshProUGUI _playerWorldPosition;
        [SerializeField] private TextMeshProUGUI _playerTilePosition;
        private PlayerController _playerController;
        #endregion

        public void Initialise(PlayerController playerController)
        {
            _playerController = playerController;
        }

        public void OnUpdate()
        {
            if(_playerController != null)
            {
                Vector2Int position = new Vector2Int(Mathf.FloorToInt(_playerController.transform.position.x), Mathf.FloorToInt(_playerController.transform.position.y));
                Vector2 mousePosition = MouseController.Instance.WorldPosition;
                Vector2Int mousePositionInt = new Vector2Int((int)mousePosition.x, (int)mousePosition.y);

                //_playerChunkPosition.text = "Player Chunk \n" + Chunk.CalculateResidingChunk(_playerController.transform.position).ToString();
                _playerChunkPosition.text = 
                    position.ToString() + "\n"
                    + Chunk.CalculateResidingChunk(position).ToString() + "\n"
                    + Tile.TilePositionInRelationToChunk(position).ToString() + "\n"
                    + mousePosition.ToString() + "\n"
                    + Chunk.CalculateResidingChunk(mousePositionInt).ToString() + "\n"
                    + Tile.TilePositionInRelationToChunk(mousePositionInt).ToString() + "\n";
            }
        }

        public void DrawLines(Vector2 position)
        {
            const int numberOfChunks = Chunk.KEEP_LOADED;
            const int halfNumberOfChunks = numberOfChunks / 2;

            Vector2 chunk = Chunk.CalculateResidingChunk(position.Vec2Int());
            int chunkXInt = (int)chunk.x;
            int chunkYInt = (int)chunk.y;
            int left = (chunkXInt - halfNumberOfChunks) * Chunk.SIZE;
            int right = (chunkXInt + halfNumberOfChunks + 1) * Chunk.SIZE;
            int top = (chunkYInt + halfNumberOfChunks + 1) * Chunk.SIZE;
            int bottom = (chunkYInt - halfNumberOfChunks) * Chunk.SIZE;

            GizmoManager.lines.Clear();

            // Draw vertical lines where x is constant
            {
                int x = 0;
                int xPosition = left;

                for (; x < numberOfChunks; x++)
                {
                    GizmoManager.lines.Add(new GizmoManager.GizmoLine(new Vector3(xPosition, bottom, 0f), new Vector3(xPosition, top, 0f), Color.red));

                    for(int tileX = 0; tileX < Chunk.SIZE - 1; tileX++)
                    {
                        GizmoManager.lines.Add(new GizmoManager.GizmoLine(new Vector3(xPosition + tileX + 1, bottom, 0f), new Vector3(xPosition + tileX + 1, top, 0f), Color.cyan));
                    }
                    xPosition += Chunk.SIZE;
                }

                GizmoManager.lines.Add(new GizmoManager.GizmoLine(new Vector3(xPosition, bottom, 0f), new Vector3(xPosition, top, 0f), Color.red));
            }

            {
                int y = 0;
                int yPosition = bottom;

                for (; y < numberOfChunks; y++)
                {
                    GizmoManager.lines.Add(new GizmoManager.GizmoLine(new Vector3(left, yPosition, 0f), new Vector3(right, yPosition, 0f), Color.red));

                    for (int tileY= 0; tileY < Chunk.SIZE - 1; tileY++)
                    {
                        GizmoManager.lines.Add(new GizmoManager.GizmoLine(new Vector3(left, yPosition + tileY + 1, 0f), new Vector3(right, yPosition + tileY + 1, 0f), Color.cyan));
                    }

                    yPosition += Chunk.SIZE;
                }

                GizmoManager.lines.Add(new GizmoManager.GizmoLine(new Vector3(left, yPosition, 0f), new Vector3(right, yPosition, 0f), Color.red));
            }

            Debug.Log("[DebugPlayerDetails] - DrawLines(object, Vector2)");
        }
    }
}
