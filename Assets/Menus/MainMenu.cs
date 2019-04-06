using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace TheWorkforce.UI
{
    using Game_State;

    public class MainMenu : MonoBehaviour
    {
        public Button LoadFileButtonPrefab;

        [SerializeField] private GameManager _gameManager;

        #region Display Panel Buttons
        [SerializeField] private Button _displayCreateGamePanelButton;
        [SerializeField] private Button _displayLoadGamePanelButton;
        [SerializeField] private Button _displayJoinGamePanelButton;
        [SerializeField] private Button _quitGameButton;
        #endregion

        #region UI Panel Members
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _createGamePanel;
        [SerializeField] private GameObject _loadGamePanel;
        [SerializeField] private GameObject _joinGamePanel;
        private GameObject _activePanel = null;
        #endregion

        #region Load Panel Objects
        [SerializeField] private Transform _saveFilesScrollView;
        [SerializeField] private Button _loadFileButton;
        private Button[] _loadFileButtons;
        private Button _selectedButton;

        #endregion
        [SerializeField] private Button _hostPlayButton;
        [SerializeField] private Button _clientPlayButton;
        [SerializeField] private Button _backButton;

        #region Unity API
        private void Start()
        {
            gameObject.SetActive(false);
            _backButton.gameObject.SetActive(false);
            _gameManager.OnApplicationStateChange += GameManager_OnApplicationStateChange;

            var saveDirectories = GameFile.GetSaveDirectories();
            _loadFileButtons = new Button[saveDirectories.Length];

            // Create a new load save button for each save game on the client
            for (int i = 0; i < saveDirectories.Length; i++)
            {
                Button loadFileButton = Instantiate(LoadFileButtonPrefab, _saveFilesScrollView);
                loadFileButton.onClick.AddListener(delegate
                {
                    SelectFileButton(loadFileButton, saveDirectories[i]);
                });

                loadFileButton.GetComponentInChildren<TextMeshProUGUI>().text = saveDirectories[i].Name;
                RectTransform rect = (RectTransform)loadFileButton.transform;
                rect.anchoredPosition += new Vector2(0, -i * 64.0f);

                _loadFileButtons[i] = loadFileButton;
            }
            

            _hostPlayButton.onClick.AddListener(delegate 
            {
                _gameManager.NetworkManager.StartHost();
            });
            _clientPlayButton.onClick.AddListener(delegate
            {
                _gameManager.NetworkManager.StartClient();
            });
        }
        #endregion

        private void GameManager_OnApplicationStateChange(ApplicationStateChangeArgs applicationStateArgs)
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

        private void SelectFileButton(Button button, DirectoryInfo directory)
        {
            if(_selectedButton)
            {
                DeselectFileButton(_selectedButton);

            }
            _selectedButton = button;
            // Show some text etc for the button
        }

        private void DeselectFileButton(Button button)
        {

        }

        #region Public Methods for Button Events
        public void DisplayCreateGamePanel()
        {
            _mainPanel.SetActive(false);
            _createGamePanel.SetActive(true);
            _activePanel = _createGamePanel;
            _backButton.gameObject.SetActive(true);
        }

        public void DisplayLoadGamePanel()
        {
            _mainPanel.SetActive(false);
            _loadGamePanel.SetActive(true);
            _activePanel = _loadGamePanel;
            _backButton.gameObject.SetActive(true);
        }

        public void DisplayJoinGamePanel()
        {
            _mainPanel.SetActive(false);
            _joinGamePanel.SetActive(true);
            _activePanel = _joinGamePanel;
            _backButton.gameObject.SetActive(true);
        }

        public void DisplayMainPanel()
        {
            _activePanel.SetActive(false);
            _mainPanel.SetActive(true);
            _backButton.gameObject.SetActive(false);
            _activePanel = null;
        }
        #endregion
    }
}
