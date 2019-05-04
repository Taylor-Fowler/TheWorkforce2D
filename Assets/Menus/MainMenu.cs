using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace TheWorkforce.UI
{
    using Game_State;

    public class MainMenu : MonoBehaviour
    {

        [SerializeField] private GameManager _gameManager;
        [SerializeField] private LoadMenu _loadMenu;

        #region UI Panel Members
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _createGamePanel;
        [SerializeField] private GameObject _loadGamePanel;
        [SerializeField] private GameObject _joinGamePanel;
        private GameObject _activePanel = null;
        #endregion

        #region Create Game Panel Objects
        [SerializeField] private Button _createGameButton;
        [SerializeField] private TextMeshProUGUI _worldNameInput;
        [SerializeField] private TextMeshProUGUI _worldSeedInput;
        #endregion

        #region Join Game Panel Objects
        [SerializeField] private Button _joinGameButton;
        #endregion

        [SerializeField] private Button _backButton;

        #region Unity API
        private void Awake()
        {
            _backButton.gameObject.SetActive(false);
            _gameManager.OnApplicationStateChange += GameManager_OnApplicationStateChange;

            _createGameButton.onClick.AddListener(delegate
            {
                CreateGame();
            });

            _joinGameButton.onClick.AddListener(delegate
            {
                _gameManager.NetworkManager.StartClient();
            });
        }
        #endregion

        /// <summary>
        /// Displays and hides canvas elements for the main menu when the application state changes
        /// 
        /// Any state -> Menu = Display the main menu canvas
        /// </summary>
        /// <param name="applicationStateArgs"></param>
        private void GameManager_OnApplicationStateChange(ApplicationStateChangeArgs applicationStateArgs)
        {    
            // Transition to menu state, this could be due to just launching the application or due to just closing the game
            if(applicationStateArgs.Current == EApplicationState.Menu)
            {
                gameObject.SetActive(true);
                if(applicationStateArgs.Previous == EApplicationState.ReturningToMenu)
                {
                    _backButton.gameObject.SetActive(false);
                    _activePanel.SetActive(false);
                    _activePanel = _mainPanel;
                    _mainPanel.SetActive(true);
                }
            }
            // Transition from menu state, this could be due to closing the application or due to loading a game
            else if(applicationStateArgs.Previous == EApplicationState.Menu)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Creates a game with the user input game-name then proceeds to start the network host
        /// </summary>
        private void CreateGame()
        {
            string worldName = _worldNameInput.text;
            // Validate world name
            if(worldName == null || worldName == string.Empty)
            {
                // error message, text cannot be null
                return;
            }

            string worldSeed = _worldSeedInput.text;
            int seed = 784893570;

            if(worldSeed != null && worldSeed != string.Empty)
            {
                // turn the string into an int
                seed = worldSeed.GetHashCode();
            }

            if(!GameFile.CreateGame(worldName, seed))
            {
                // error message, cannot create file (save exists, disk full etc)
                return;
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
            _loadMenu.InitialiseButtons();
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
