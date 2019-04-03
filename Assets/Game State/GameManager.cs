using System;
using System.Collections;
using UnityEngine;

namespace TheWorkforce.Game_State
{    
    using Crafting; using Testing; using Network; using Entities;

    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Event that notifies subscribers a change in the state of the game
        /// and contains the old and new game states.
        /// </summary>
        public event Action<GameStateChangeArgs> OnGameStateChange;

        /// <summary>
        /// Event that notifies subscribers a change in the state of the application
        /// and contains the old and new application states.
        /// </summary>
        public event Action<ApplicationStateChangeArgs> OnApplicationStateChange;

        /// <summary>
        /// The application's local player controller, only set whilst in a game
        /// </summary>
        public PlayerController PlayerController { get; private set; }

        /// <summary>
        /// The application's local world controller, only set whilst in a game
        /// </summary>
        public WorldController WorldController { get; private set; }

        /// <summary>
        /// The application's network manager
        /// </summary>
        public CustomNetworkManager NetworkManager => _networkManager;

        /// <summary>
        /// 
        /// </summary>
        public EntityCollection EntityCollection => _entityCollection;

        /// <summary>
        /// The current game state
        /// </summary>
        public EGameState GameState => _currentGameState;

        [SerializeField] private EntityCollection _entityCollection;
        [SerializeField] private EApplicationState _currentApplicationState;
        [SerializeField] private EGameState _currentGameState;

        [SerializeField] private CustomNetworkManager _networkManager;
        [SerializeField] private Recipes _recipes;
        [SerializeField] private DebugController _debugController;
        
        #region Unity API
        /// <summary>
        /// Initialises the application, game state and assets. 
        /// Also starts listening for startup messages from the world and player.
        /// </summary>
        private void Awake()
        {
            _currentApplicationState = EApplicationState.Launching;
            _currentGameState = EGameState.NotLoaded;

            // GameManager needs to listen for responses from both the world controller and local player controller
            WorldController.OnWorldControllerStartup += WorldController_OnWorldControllerStartup;
            PlayerController.OnLocalPlayerControllerStartup += PlayerController_OnLocalPlayerControllerStartup;

            StartCoroutine(InitialiseAssets());
        }

        /// <summary>
        /// Clean up
        /// </summary>
        private void OnDestroy()
        {
            _recipes.Clear();
        }
        #endregion

        private IEnumerator InitialiseAssets()
        {
            TerrainTileSet.InitialiseTileSets();
            _entityCollection.Initialise();
            _recipes.Initialise();

            _networkManager.Startup(this);
            _debugController.Startup(this);

            _networkManager.Initialise(OpenConnection, BeginLoading, PauseGame, ResumeGame);
            
            yield return new WaitForSeconds(1f);
            ApplicationStateChange(EApplicationState.Menu);
        }

        private void StartupIngameControllers()
        {
            // If both local controllers are initialised, start the world controller and try to initialise the connection
            if(WorldController != null && PlayerController != null)
            {
                WorldController.Startup(this);
                StartCoroutine(WorldController.InitialiseConnection(delegate 
                {
                    PlayerController.Startup(this);
                    ApplicationStateChange(EApplicationState.Ingame);
                    GameStateChange(EGameState.Active);
                    StartCoroutine(IncrementTickTime());
                }));
            }
        }

        private IEnumerator IncrementTickTime()
        {
            while(_currentApplicationState == EApplicationState.Ingame)
            {
                if(_currentGameState == EGameState.Active)
                {
                    GameTime.Update();
                    GameTime.PostUpdate();
                }
                yield return new WaitForFixedUpdate();
            }
        }

        #region State Changes
        private void OpenConnection()
        {
            ApplicationStateChange(EApplicationState.Connecting);
        }

        private void BeginLoading()
        {
            ApplicationStateChange(EApplicationState.Loading);
            GameStateChange(EGameState.Waking);
        }

        private void PauseGame()
        {
            GameStateChange(EGameState.Paused);
        }

        private void ResumeGame()
        {
            GameStateChange(EGameState.Active);
        }
        #endregion

        #region Custom Event Invoking
        private void ApplicationStateChange(EApplicationState newState)
        {
            EApplicationState previous = _currentApplicationState;
            _currentApplicationState = newState;

            OnApplicationStateChange?.Invoke(new ApplicationStateChangeArgs(previous, _currentApplicationState));
        }

        private void GameStateChange(EGameState newState)
        {
            EGameState previous = _currentGameState;
            _currentGameState = newState;
            
            OnGameStateChange?.Invoke(new GameStateChangeArgs(previous, _currentGameState));
        }
        #endregion

        #region Custom Event Response
        private void WorldController_OnWorldControllerStartup(WorldController controller)
        {
            WorldController = controller;
            StartupIngameControllers();
        }

        private void PlayerController_OnLocalPlayerControllerStartup(PlayerController playerController)
        {
            PlayerController = playerController;
            StartupIngameControllers();
        }
        #endregion
    }
}