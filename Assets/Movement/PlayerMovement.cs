using System;
using UnityEngine;
using TheWorkforce.World;

namespace TheWorkforce
{
    public class PlayerMovement : AnimatedMovement
    {
        private readonly Action<Vector2> _chunkChange;
        private Vector2 _chunkPositionWhenRequestedGeneration;
        private Vector2 _worldPositionWhenRequestedGeneration;

        public PlayerMovement(float speed, Animator animator, Action<Vector2> chunkChange, Transform transform) : base(speed, animator)
        {
            _chunkChange = chunkChange;
            CapturePosition(transform);
        }

        public override void Move(int horizontal, int vertical, Transform transform)
        {
            base.Move(horizontal, vertical, transform);

            Vector2 worldPosition = transform.position;
            Vector2 currentChunk = Chunk.CalculateResidingChunk(worldPosition);

            // Check whether the player has moved to a new chunk
            if(_chunkPositionWhenRequestedGeneration != currentChunk)
            {
                Vector2 difference = _worldPositionWhenRequestedGeneration - worldPosition;

                if (Mathf.Abs(difference.x) >= Chunk.SIZE || Mathf.Abs(difference.y) >= Chunk.SIZE)
                {
                    _chunkChange(worldPosition);
                    CapturePosition(transform.position, currentChunk);
                }
            }
        }

        private void CapturePosition(Transform transform)
        {
            _chunkPositionWhenRequestedGeneration = Chunk.CalculateResidingChunk(transform.position);
            _worldPositionWhenRequestedGeneration = transform.position;
        }

        private void CapturePosition(Vector2 worldPosition, Vector2 chunkPosition)
        {
            _chunkPositionWhenRequestedGeneration = chunkPosition;
            _worldPositionWhenRequestedGeneration = worldPosition;
        }
    }
}
