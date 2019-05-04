using UnityEngine;
using TMPro;

namespace TheWorkforce.Testing
{
    [System.Serializable]
    public class DebugWorldDetails
    {
        [SerializeField] private TextMeshProUGUI _chunkLoadedDetails;
        private WorldController _worldController;
        private PlayerController _playerController;

        public void Initialise(WorldController worldController, PlayerController playerController)
        {
            _worldController = worldController;
            _playerController = playerController;
        }

        public void OnUpdate()
        {
            if(_worldController != null && _playerController != null)
            {
                _chunkLoadedDetails.text =
                    $"{_worldController.World.LoadedChunks.Count} \n" +
                    $"{_worldController.World.GetPlayerLoadedChunks(_playerController.Id).Count}\n" +
                    $"{_worldController.World.KnownChunks.Count}";
            }
        }
    }

}