using System;
using System.Collections;
using UnityEngine;
using TheWorkforce.Crafting;
using TheWorkforce.Static_Classes;
using TheWorkforce.Testing;
using TheWorkforce.Network;
using TheWorkforce.Items;
using TheWorkforce.Entities;

namespace TheWorkforce.Game_State
{
    public delegate void GameStateChangeHandler(object source, GameStateArgs gameStateArgs);
    public delegate void ApplicationStateChangeHandler(object source, ApplicationStateArgs applicationStateArgs);
    public delegate void DirtyHandler(object source);
    
    public class GameManager : MonoBehaviour
    {
        #region Custom Event Declarations
        public event GameStateChangeHandler OnGameStateChange;
        public event ApplicationStateChangeHandler OnApplicationStateChange;
        #endregion

        // TODO: Potentially create the main menu from a prefab and inject the reference to this game manager
        //       List of potential injections:
        //       - MainMenu
        //       - PlayerController
        #region Properties
        public PlayerController PlayerController { get; private set; }
        public WorldController WorldController { get; private set; }
        public CustomNetworkManager NetworkManager { get { return _networkManager; } }
        public EntityCollection EntityCollection { get { return _entityCollection; } }
        #endregion

        #region Private Members
        [SerializeField] private EntityCollection _entityCollection;
        [SerializeField] private EApplicationState _currentApplicationState;
        [SerializeField] private EGameState _currentGameState;
        [SerializeField] private CustomNetworkManager _networkManager;
        [SerializeField] private DebugController _debugController;
        #endregion
        
        #region Unity API
        private void Awake()
        {
            _currentApplicationState = EApplicationState.Launching;
            _currentGameState = EGameState.NotLoaded;

            WorldController.OnWorldControllerStartup += WorldController_OnWorldControllerStartup;
            PlayerController.OnPlayerControllerStartup += PlayerController_OnPlayerControllerStartup;

            StartCoroutine(InitialiseAssets(FinishedLoadingAssets));
        }
        #endregion

        #region Public Methods
        #endregion

        private IEnumerator InitialiseAssets(Action callback)
        {
            _entityCollection.Initialise();
            TerrainTileSet.InitialiseTileSets();
            Recipes.Initialise();

            _networkManager.Startup(this);
            _networkManager.SetLoadGameAction(delegate 
            {
                ApplicationStateChange(EApplicationState.Loading);
            });
            
            #if (DEBUG)
            _debugController.Startup(this);
            #endif

            yield return new WaitForSeconds(1f);
            callback();
        }

        private void FinishedLoadingAssets()
        {
            ApplicationStateChange(EApplicationState.Menu);
        }

        private void StartupIngameControllers()
        {
            if(WorldController != null && PlayerController != null)
            {
                WorldController.Startup(this);
                PlayerController.Startup(this);
                ApplicationStateChange(EApplicationState.Ingame);
            }
        }

        #region Custom Event Invoking
        private void ApplicationStateChange(EApplicationState newState)
        {
            EApplicationState previous = _currentApplicationState;
            _currentApplicationState = newState;

            OnApplicationStateChange?.Invoke(this, new ApplicationStateArgs(previous, _currentApplicationState));
        }

        private void GameStateChange(EGameState newState)
        {
            EGameState previous = _currentGameState;
            _currentGameState = newState;
            
            OnGameStateChange?.Invoke(this, new GameStateArgs(previous, _currentGameState));
        }
        #endregion

        #region Custom Event Response
        private void WorldController_OnWorldControllerStartup(object source, WorldController controller)
        {
            WorldController = controller;
            StartupIngameControllers();
        }

        private void PlayerController_OnPlayerControllerStartup(object source, PlayerController playerController)
        {
            PlayerController = playerController;
            StartupIngameControllers();
        }
        #endregion
    }
}