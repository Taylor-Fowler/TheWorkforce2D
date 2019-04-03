using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheWorkforce.Game_State
{
    public class GameSave
    {
        public static GameSave Instance => _instance;
        private static GameSave _instance;

        private const string SavesDirectoryName = "Saves";
        private const string RegionsFileName = "regions";
        private const string ChunksDirectoryName = "Chunks";
        private const string PlayersDirectoryName = "Players";
        private const string FileFormat = ".dat";
        private const string TemporaryCopyDirectoryName = "Temp";

        private readonly DirectoryInfo _saveDirectory;

        /// <summary>
        /// Finds all valid save directories within the main save directory and then returns them.
        /// 
        /// Required for the UI so that a player can choose to load a game based on the directory information
        /// </summary>
        /// <returns>A collection of all valid save directories</returns>
        public static DirectoryInfo[] GetSaveDirectoriess()
        {
            return null;
        }

        public static bool LoadGame()
        {
            return false;
        }

        public static bool CreateGame(string gameName)
        {
            // The activate game must be unloaded before a new game can be created
            if(_instance != null || gameName == null || gameName == "")
            {
                return false;
            }
            string savesDirectoryPath = Path.Combine(Application.persistentDataPath, SavesDirectoryName);

            // Check that the saves folder path exists, if not, create the directory
            if(!Directory.Exists(savesDirectoryPath))
            {
                Directory.CreateDirectory(savesDirectoryPath);
            }

            string newSaveDirectoryPath = Path.Combine(savesDirectoryPath, gameName);

            // If the new save game directory path exists, exit immediately, cannot have two games with the same name
            if(Directory.Exists(newSaveDirectoryPath))
            {
                return false;
            }

            DirectoryInfo saveDirectory = Directory.CreateDirectory(newSaveDirectoryPath);
            DirectoryInfo playersDirectory = Directory.CreateDirectory(Path.Combine(newSaveDirectoryPath, PlayersDirectoryName));
            DirectoryInfo chunksDirectory = Directory.CreateDirectory(Path.Combine(newSaveDirectoryPath, ChunksDirectoryName));

            // Initialise the instance of game save
            _instance = new GameSave(saveDirectory);

            return true;
        }

        /// <summary>
        /// Private constructor, only the static methods of the class are able to create the game
        /// </summary>
        /// <param name="directoryInfo"></param>
        private GameSave(DirectoryInfo directoryInfo)
        {
            _saveDirectory = directoryInfo;
        }

        public void SaveWorld(World world)
        {
            using (FileStream regionStream = File.Open(Path.Combine(_saveDirectory.FullName, RegionsFileName + FileFormat), FileMode.OpenOrCreate))
            {

            }
        }

        public void ExitGame()
        {
            // unloaded
            _instance = null;
        }
    }
}
