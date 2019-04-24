using System;
using UnityEngine;

namespace TheWorkforce
{
    public class PlayerMovement : AnimatedMovement
    {
        private readonly int _id;
        private readonly Action<Vector2> _chunkChange;
        private Vector2Int _chunkPositionWhenRequestedGeneration;
        private Vector2Int _worldPositionWhenRequestedGeneration;

        public PlayerMovement(int id, float speed, Animator animator, Action<Vector2> chunkChange, Transform transform) : base(speed, animator)
        {
            _id = id;
            _chunkChange = chunkChange;
            CapturePosition(transform);
            _chunkChange(transform.position.Vec2Int());
        }

        public override void Move(int horizontal, int vertical, Transform transform)
        {
            base.Move(horizontal, vertical, transform);

            Vector2Int worldPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            Vector2Int currentChunk = Chunk.CalculateResidingChunk(worldPosition);

            // Check whether the player has moved to a new chunk
            if(_chunkPositionWhenRequestedGeneration != currentChunk)
            {
                Vector2 difference = _worldPositionWhenRequestedGeneration - worldPosition;

                if (Mathf.Abs(difference.x) >= Chunk.SIZE || Mathf.Abs(difference.y) >= Chunk.SIZE)
                {
                    _chunkChange(worldPosition);
                    CapturePosition(worldPosition, currentChunk);
                }
            }
        }

        private void CapturePosition(Transform transform)
        {
            Vector2Int position = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            _chunkPositionWhenRequestedGeneration = Chunk.CalculateResidingChunk(position);
            _worldPositionWhenRequestedGeneration = position;
        }

        private void CapturePosition(Vector2Int worldPosition, Vector2Int chunkPosition)
        {
            _chunkPositionWhenRequestedGeneration = chunkPosition;
            _worldPositionWhenRequestedGeneration = worldPosition;
        }
    }
}
