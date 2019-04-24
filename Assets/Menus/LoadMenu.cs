using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheWorkforce.UI
{
    using Game_State; using Network;

    public class LoadMenu : MonoBehaviour, IDisplay
    {
        public const string NoFileSelectedMessage = "No File Selected";

        public TextMeshProUGUI SelectedFileNameText;
        public Button SelectFileButtonPrefab;

        [SerializeField] private CustomNetworkManager _customNetworkManager;
        [SerializeField] private Transform _selectFileScrollView;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _deleteButton;

        private List<Button> _selectFileButtons;
        private Button _selectedFileButton;
        private DirectoryInfo _selectedFileDirectory;

        private void Awake()
        {
            var saveDirectories = GameFile.GetSaveDirectories();
            _selectFileButtons = new List<Button>();

            // Create a new load save button for each save game on the client
            for (int i = 0; i < saveDirectories.Length; ++i)
            {
                Button selectFileButton = Instantiate(SelectFileButtonPrefab, _selectFileScrollView);

                int e = i;
                selectFileButton.onClick.AddListener(delegate
                {
                    SelectFileButton(selectFileButton, saveDirectories[e]);
                });

                selectFileButton.GetComponentInChildren<TextMeshProUGUI>().text = saveDirectories[i].Name;

                _selectFileButtons.Add(selectFileButton);
            }

            _loadButton.enabled = false;
            _deleteButton.enabled = false;

            _loadButton.gameObject.SetActive(false);
            _deleteButton.gameObject.SetActive(false);

            _loadButton.onClick.AddListener(LoadFile);
            _deleteButton.onClick.AddListener(DeleteFile);
        }

        /// <summary>
        /// Displays the selected save file's details and makes the game delete and load buttons process the newly
        /// selected file when clicked
        /// </summary>
        /// <param name="button"></param>
        /// <param name="directory"></param>
        private void SelectFileButton(Button button, DirectoryInfo directory)
        {
            if (_selectedFileButton != null)
            {
                DeselectFileButton(_selectedFileButton);
            }
            _selectedFileButton = button;
            _selectedFileDirectory = directory;

            SelectedFileNameText.text = directory.Name;

            _loadButton.enabled = true;
            _deleteButton.enabled = true;

            _loadButton.gameObject.SetActive(true);
            _deleteButton.gameObject.SetActive(true);
        }

        private void DeselectFileButton(Button button)
        {

        }

        private void LoadFile()
        {
            if(_selectedFileDirectory != null)
            {
                GameFile.LoadGame(_selectedFileDirectory);
                _customNetworkManager.StartHost();
            }
        }

        private void DeleteFile()
        {
            _loadButton.enabled = false;
            _deleteButton.enabled = false;

            _selectedFileDirectory?.Delete(true);
            _selectedFileDirectory = null;
            
            SelectedFileNameText.text = NoFileSelectedMessage;

            _loadButton.gameObject.SetActive(false);
            _deleteButton.gameObject.SetActive(false);

            if(_selectedFileButton != null)
            {
                _selectFileButtons.Remove(_selectedFileButton);
                Destroy(_selectedFileButton.gameObject);
                _selectedFileButton = null;
            }
        }

        #region IDisplay Implementation
        public void Display()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion
    } 
}
