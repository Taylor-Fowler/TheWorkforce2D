using UnityEngine;

namespace TheWorkforce.Testing
{
    public class DebugController : MonoBehaviour
    {
        public GameObject DebugCanvas;
        public DebugItemsLoaded DebugItemsLoaded;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugCanvas.SetActive(!DebugCanvas.activeSelf);
            }
        }
    }
}