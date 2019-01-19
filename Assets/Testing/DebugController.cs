using UnityEngine;
using TheWorkforce.Game_State;

namespace TheWorkforce.Testing
{
    public class DebugController : MonoBehaviour, IManager
    {
        #region IManager Implementation
        public GameManager GameManager { get; private set; }

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            GameManager.OnApplicationStateChange += GameManager_OnApplicationStateChange;
        }

        #endregion

        #region Public Members
        public GameObject DebugCanvas;
        public DebugItemsLoaded DebugItemsLoaded;
        #endregion

        #region Private Members
        [SerializeField] private DebugPlayerDetails _debugPlayerDetails;
        #endregion


        #region Unity API
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugCanvas.SetActive(!DebugCanvas.activeSelf);
                GizmoManager.Show = DebugCanvas.activeSelf;
            }

            if(DebugCanvas.activeInHierarchy)
            {
                _debugPlayerDetails.OnUpdate();
            }
        }
        #endregion

        #region Custom Event Responses
        private void GameManager_OnApplicationStateChange(object source, ApplicationStateArgs applicationStateArgs)
        {
            if(applicationStateArgs.Current == EApplicationState.Ingame)
            {
                GameManager.WorldController.OnWorldPlayerPositionUpdate += _debugPlayerDetails.DrawLines;
                _debugPlayerDetails.Initialise(GameManager.PlayerController);
                _debugPlayerDetails.DrawLines(this, GameManager.PlayerController.transform.position);

                // NOTE: Added before the resource generation was added
                // StartCoroutine(WaitToSpawn());
            }
            Debug.Log("[DebugController] - GameManager_OnApplicationStateChange(object, ApplicationStateArgs) \n " 
                    + "applicationStateArgs.Current: " + applicationStateArgs.Current.ToString());
        }
        #endregion

        #region Button Events

        #endregion
    }
}