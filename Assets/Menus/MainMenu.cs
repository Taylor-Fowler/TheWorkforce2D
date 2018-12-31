using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TheWorkforce.Game_State;


namespace TheWorkforce.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _hostPlayButton;
        [SerializeField] private Button _clientPlayButton;

        #region Unity API
        private void Start()
        {
            // TODO: Find a nicer way of doing this.....getting a component from the scene is just messy
            GameManager gameManager = FindObjectOfType<GameManager>();

            gameManager.OnApplicationStateChange += GameManager_OnApplicationStateChange;
            gameObject.SetActive(false);
            _hostPlayButton.onClick.AddListener(delegate 
            {
                gameManager.NetworkManager.StartHost();
            });
            _clientPlayButton.onClick.AddListener(delegate
            {
                gameManager.NetworkManager.StartClient();
            });
        }
        #endregion

        private void GameManager_OnApplicationStateChange(object source, ApplicationStateArgs applicationStateArgs)
        {    
            // Transition to menu state, this could be due to just launching the application or due to just closing the game
            if(applicationStateArgs.Current == EApplicationState.Menu)
            {
                gameObject.SetActive(true);
            }
            // Transition from menu state, this could be due to closing the application or due to loading a game
            else if(applicationStateArgs.Previous == EApplicationState.Menu)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
