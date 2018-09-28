using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Text))]
public class ServerStatusMessage : MonoBehaviour
{
    public UnityEngine.UI.Text Text;
    public Color SuccessColour;
    public Color ErrorColour;

    //public override void OnConnectedToMaster()
    //{
    //    this.Text.text = "Connected to Server";
    //    this.Text.color = this.SuccessColour;
    //}

    //public override void OnDisconnected(DisconnectCause cause)
    //{
    //    base.OnDisconnected(cause);
    //    this.Text.text = "Disconnected from server. Cause: " + cause;
    //    this.Text.color = this.ErrorColour;
    //}
}
