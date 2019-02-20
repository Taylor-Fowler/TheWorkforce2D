using System;
using System.Collections;
using UnityEngine;
using TheWorkforce.Crafting;
using TheWorkforce.Testing;
using TheWorkforce.Network;
using TheWorkforce.Entities;

namespace TheWorkforce.Game_State
{
    public delegate void GameStateChangeHandler(object source, GameStateArgs gameStateArgs);
    public delegate void ApplicationStateChangeHandler(object source, ApplicationStateArgs applicationStateArgs);
    public delegate void DirtyHandler(object source);
    public delegate void DestroyHandler();
    
    public class GameManager : MonoBehaviour
    {
        #region Custom Event Declarations
        public event GameStateChangeHandler OnGameStateChange;
        public event ApplicationStateChangeHandler OnApplicationStateChange;
        #endregion

        #region Properties
        public PlayerController PlayerController { get; private set; }
        public WorldController WorldController { get; private set; }
        public CustomNetworkManager NetworkManager => _networkManager;
        public EntityCollection EntityCollection => _entityCollection;
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
            PlayerController.OnLocalPlayerControllerStartup += PlayerController_OnLocalPlayerControllerStartup;

            StartCoroutine(InitialiseAssets());
        }
        #endregion

        private IEnumerator InitialiseAssets()
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
            ApplicationStateChange(EApplicationState.Menu);
        }

        private void StartupIngameControllers()
        {
            if(WorldController != null && PlayerController != null)
            {
                WorldController.Startup(this);
                StartCoroutine(WorldController.InitialiseConnection(delegate 
                {
                    PlayerController.Startup(this);
                    ApplicationStateChange(EApplicationState.Ingame);
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

        private void PlayerController_OnLocalPlayerControllerStartup(object source, PlayerController playerController)
        {
            PlayerController = playerController;
            StartupIngameControllers();
        }
        #endregion
    }
}