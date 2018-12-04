using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        public Animator Animator;
        public float Speed = 3f;

        private void Start()
        {
            if (!isLocalPlayer) return;

            Animator.SetInteger("Vertical", -1);
            Animator.SetInteger("Horizontal", 0);
            Animator.SetBool("Moving", false);
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            // Solution to make the animation play at least for a foot step will be to add a coroutine that plays for at least x amount of time before allowing to be
            // stopped
            int horizontal = 0;
            int vertical = 0;
            bool moving = false;

            if (Input.GetKey(KeyCode.A)) horizontal -= 1;
            if (Input.GetKey(KeyCode.D)) horizontal += 1;

            if (Input.GetKey(KeyCode.S)) vertical -= 1;
            if (Input.GetKey(KeyCode.W)) vertical += 1;

            moving = horizontal != 0 || vertical != 0;
            CmdMove(horizontal, vertical, moving);

            if (moving)
            {
                Animator.SetInteger("Horizontal", horizontal);
                Animator.SetInteger("Vertical", vertical);
            }

            Animator.SetBool("Moving", moving);

            transform.Translate(horizontal * Speed * Time.deltaTime, vertical * Speed * Time.deltaTime, 0);
        }

        [Command]
        private void CmdMove(int horizontal, int vertical, bool move)
        {
            RpcMove(horizontal, vertical, move);
        }

        [ClientRpc]
        private void RpcMove(int horizontal, int vertical, bool move)
        {
            if (isLocalPlayer) return;

            if (move)
            {
                Animator.SetInteger("Horizontal", horizontal);
                Animator.SetInteger("Vertical", vertical);
            }

            Animator.SetBool("Moving", move);


            transform.Translate(horizontal * Time.deltaTime, vertical * Time.deltaTime, 0);
        }
    }    
}
