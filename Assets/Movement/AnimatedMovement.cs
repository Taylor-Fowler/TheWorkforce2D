using UnityEngine;

namespace TheWorkforce
{
    public class AnimatedMovement : Movement
    {
        public Animator Animator { get; private set; }

        public AnimatedMovement(float speed, Animator animator) : base(speed)
        {
            Animator = animator;
        }

        public override void Move(int horizontal, int vertical, Transform transform)
        {
            base.Move(horizontal, vertical, transform);

            bool moving = horizontal != 0 || vertical != 0;

            if (moving)
            {
                Animator.SetInteger("Horizontal", horizontal);
                Animator.SetInteger("Vertical", vertical);
            }

            Animator.SetBool("Moving", moving);
        }
    }
}
