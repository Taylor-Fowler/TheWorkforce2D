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
        /// The entity collection that stores all entity data and instances for the game
        /// </summary>
        public EntityCollection EntityCollection => _entityCollection;

        /// <summary>
        /// The current game state
        /// </summary>
        public EGameState GameState => _currentGameState;
        [SerializeField, ReadOnly] private EGameState _currentGameState;
        [SerializeField, ReadOnly] private EApplicationState _currentApplicationState;

        [SerializeField] private EntityCollection _entityCollection;

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
            PlayerController.OnPlayerControllerStartup += PlayerController_OnLocalPlayerControllerStartup;

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

        public void Quit()
        {
            Application.Quit();
        }

        private IEnumerator InitialiseAssets()
        {
            TerrainTileSet.InitialiseTileSets();
            _entityCollection.Startup(this);
            _recipes.Initialise();

            _networkManager.Startup(this);
            _debugController.Startup(this);

            _networkManager.Initialise(StartConnecting, StartLoading, StartGame, Pause, Resume, Disconnect);
            
            yield return new WaitForSeconds(1f);
            ApplicationStateChange(EApplicationState.Menu);
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

        private IEnumerator IncrementBackgroundTime()
        {
            while(_currentApplicationState == EApplicationState.Ingame || _currentApplicationState == EApplicationState.Connecting
                    || _currentApplicationState == EApplicationState.Loading)
            {
                GameTime.UpdateBackgroundTimer();
                yield return new WaitForFixedUpdate();
            }
        }

        #region State Changes
        private void StartConnecting()
        {
            ApplicationStateChange(EApplicationState.Connecting);
            StartCoroutine(IncrementBackgroundTime());
        }

        private void StartLoading()
        {
            ApplicationStateChange(EApplicationState.Loading);
            GameStateChange(EGameState.Waking);
        }

        private void StartGame()
        {
            ApplicationStateChange(EApplicationState.Ingame);
            GameStateChange(EGameState.Active);
            StartCoroutine(IncrementTickTime());
            Debug.Log("<color=brown>[GameManager]</color> - StartGame()");
        }

        private void Pause()
        {
            GameStateChange(EGameState.Paused);
            Debug.Log("<color=brown>[GameManager]</color> - Pause()");
        }

        private void Resume()
        {
            GameStateChange(EGameState.Active);
            Debug.Log("<color=brown>[GameManager]</color> - Resume()");
        }

        private void Disconnect()
        {
            GameStateChange(EGameState.Disconnecting);
            ApplicationStateChange(EApplicationState.ReturningToMenu);
            GameTime.Reset(); // Clear game timer events
            ApplicationStateChange(EApplicationState.Menu);
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
        private void WorldController_OnWorldControllerStartup(WorldController worldController) => WorldController = worldController;

        private void PlayerController_OnLocalPlayerControllerStartup(PlayerController playerController) => PlayerController = playerController;
        #endregion
    }
}