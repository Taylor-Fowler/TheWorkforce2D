using System;
using System.Collections;
using TheWorkforce.Crafting;
using TheWorkforce.StaticClasses;
using TheWorkforce.Testing;
using TheWorkforce.World;
using UnityEngine;

namespace TheWorkforce.Game_State
{
    public delegate void GameStateChangeHandler(object source, GameStateArgs gameStateArgs);
    public delegate void ApplicationStateChangeHandler(object source, ApplicationStateArgs applicationStateArgs);
    

    
    public class GameManager : MonoBehaviour
    {
        #if (DEBUG)
        public DebugController DebugController;
        #endif
        
        #region Private Members
        [SerializeField] private EApplicationState _currentApplicationState;
        [SerializeField] private EGameState _currentGameState;

        [SerializeField] private CraftingManager _craftingManager;
        #endregion

        #region Custom Event Declarations
        public event GameStateChangeHandler OnGameStateChange;
        public event ApplicationStateChangeHandler OnApplicationStateChange;
        #endregion
        
        #region Unity API
        private void Awake()
        {
            _currentApplicationState = EApplicationState.Launching;
            _currentGameState = EGameState.NotLoaded;
            _craftingManager = new CraftingManager();
            
            #if (DEBUG)
            Debug.Log("[GameManager] DEBUG Active");
            DebugController.gameObject.SetActive(true);
            #endif

            StartCoroutine(InitialiseAssets(FinishedLoadingAssets));
        }
        #endregion

        private IEnumerator InitialiseAssets(Action callback)
        {
            TerrainTileSet.InitialiseTileSets();

            var items = AssetProcessor.LoadItems();
            _craftingManager.RegisterItems(items);
            _craftingManager.RegisterRecipes(AssetProcessor.LoadCraftingRecipes());
            
            #if (DEBUG)
            DebugController.DebugItemsLoaded.UpdateItems(items);
            #endif
            
            yield return new WaitForSeconds(1f);
            callback();
        }

        private void FinishedLoadingAssets()
        {
            ApplicationStateChange(EApplicationState.Menu);
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
    }
}