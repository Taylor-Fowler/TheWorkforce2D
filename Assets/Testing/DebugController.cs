using UnityEngine;

namespace TheWorkforce.Testing
{
    using Game_State; using Interfaces;

    public class DebugController : MonoBehaviour, IManager
    {
        public GameManager GameManager { get; private set; }
        public GameObject DebugCanvas;
        public DebugItemsLoaded DebugItemsLoaded;

        [SerializeField] private DebugPlayerDetails _debugPlayerDetails;

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

        public void Startup(GameManager gameManager)
        {
            GameManager = gameManager;
            GameManager.OnApplicationStateChange += GameManager_OnApplicationStateChange;
        }

        #region Custom Event Responses
        private void GameManager_OnApplicationStateChange(ApplicationStateChangeArgs applicationStateArgs)
        {
            if(applicationStateArgs.Current == EApplicationState.Ingame)
            {
                GameManager.WorldController.OnWorldPlayerPositionUpdate += _debugPlayerDetails.DrawLines;
                _debugPlayerDetails.Initialise(GameManager.PlayerController);
                _debugPlayerDetails.DrawLines(GameManager.PlayerController.transform.position);

                // NOTE: Added before the resource generation was added
                // StartCoroutine(WaitToSpawn());
            }
            Debug.Log("[DebugController] - GameManager_OnApplicationStateChange(object, ApplicationStateArgs) \n " 
                    + "applicationStateArgs.Current: " + applicationStateArgs.Current.ToString());
        }
        #endregion
    }
}