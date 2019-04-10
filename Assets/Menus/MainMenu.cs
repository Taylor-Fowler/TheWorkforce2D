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

        #region Create Game Panel Objects
        [SerializeField] private Button _createGameButton;
        [SerializeField] private TextMeshProUGUI _worldNameInput;
        [SerializeField] private TextMeshProUGUI _worldSeedInput;
        #endregion

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
            for (int i = 0; i < saveDirectories.Length; ++i)
            {
                Button loadFileButton = Instantiate(LoadFileButtonPrefab, _saveFilesScrollView);
                int e = i;
                loadFileButton.onClick.AddListener(delegate
                {
                    SelectFileButton(loadFileButton, saveDirectories[e]);
                });

                loadFileButton.GetComponentInChildren<TextMeshProUGUI>().text = saveDirectories[i].Name;
                RectTransform rect = (RectTransform)loadFileButton.transform;
                rect.anchoredPosition += new Vector2(0, -i * 128.0f);

                _loadFileButtons[i] = loadFileButton;
            }

            _createGameButton.onClick.AddListener(delegate
            {
                CreateGame();
            });
            //_clientPlayButton.onClick.AddListener(delegate
            //{
            //    _gameManager.NetworkManager.StartClient();
            //});
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

        private void CreateGame()
        {
            string worldName = _worldNameInput.text;
            // Validate world name
            if(worldName == null || worldName == string.Empty)
            {
                // error message, text cannot be null
                return;
            }

            if(!GameFile.CreateGame(worldName))
            {
                // error message, cannot create file (save exists, disk full etc)
                return;
            }

            string worldSeed = _worldSeedInput.text;
            int seed = 784893570;
            if(worldSeed != null && worldSeed != string.Empty)
            {
                // turn the string into an int
            }

            _gameManager.NetworkManager.StartHost();
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
