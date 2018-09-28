using UnityEngine;
using UnityEngine.UI;

public class ServerCreateGame : MonoBehaviour
{
    public Button CreateGameButton;
    public InputField GameNameInput;

    private void Awake()
    {
        this.CreateGameButton.onClick.AddListener(this.Create);
    }

    public void Create()
    {
    }
}
