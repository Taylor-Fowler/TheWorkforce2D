using UnityEngine;
using UnityEngine.UI;

public class ServerCreateGame : MonoBehaviour
{
    public Button CreateGameButton;
    public InputField GameNameInput;

    private void Awake()
    {
        CreateGameButton.onClick.AddListener(Create);
    }

    public void Create()
    {
    }
}