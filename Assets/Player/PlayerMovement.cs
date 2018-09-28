using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    public Animator Animator;

    private void Start()
    {
        if(!this.isLocalPlayer) return;

	    this.Animator.SetInteger("Vertical", -1);
	    this.Animator.SetInteger("Horizontal", 0);
	    this.Animator.SetBool("Moving", false);
    }
    
    private void Update()
    {
        if(!this.isLocalPlayer) return;
	    // Solution to make the animation play at least for a foot step will be to add a coroutine that plays for at least x amount of time before allowing to be
	    // stopped
	    int horizontal = 0;
	    int vertical = 0;
	    bool moving = false;
	
	    if(Input.GetKey(KeyCode.A)) horizontal -= 1;
	    if(Input.GetKey(KeyCode.D)) horizontal += 1;

	    if(Input.GetKey(KeyCode.S)) vertical -= 1;
	    if(Input.GetKey(KeyCode.W)) vertical += 1;
	
	    moving = horizontal != 0 || vertical != 0;
        this.CmdMove(horizontal, vertical, moving);

        if (moving)
        {
            this.Animator.SetInteger("Horizontal", horizontal);
            this.Animator.SetInteger("Vertical", vertical);
        }
        this.Animator.SetBool("Moving", moving);

        this.transform.Translate(horizontal * Time.deltaTime, vertical * Time.deltaTime, 0);
    }

    [Command]
    private void CmdMove(int horizontal, int vertical, bool move)
    {
        this.RpcMove(horizontal, vertical, move);
    }

    [ClientRpc]
    private void RpcMove(int horizontal, int vertical, bool move)
    {
        if(this.isLocalPlayer) return;

        if (move)
        {
            this.Animator.SetInteger("Horizontal", horizontal);
            this.Animator.SetInteger("Vertical", vertical);
        }

        this.Animator.SetBool("Moving", move);


        this.transform.Translate(horizontal * Time.deltaTime, vertical * Time.deltaTime, 0);
    }
}
