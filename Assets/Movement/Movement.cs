using UnityEngine;

namespace TheWorkforce
{
    // TODO: Create an event that allows this entity to notify others that they moved chunk?
    public class Movement
    {
        public float Speed { get; private set; }

        public Movement(float speed)
        {
            Speed = speed;
        }

        public virtual void Move(int horizontal, int vertical, Transform transform)
        {
            transform.Translate(horizontal * Speed * Time.deltaTime, vertical * Speed * Time.deltaTime, 0);
        }
    }
}
