using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ServerStatusMessage : MonoBehaviour
{
    public Color ErrorColour;
    public Color SuccessColour;
    public Text Text;

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