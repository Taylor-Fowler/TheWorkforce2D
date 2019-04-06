using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheWorkforce.Game_State
{
    public class GameFile
    {
        /// <summary>
        /// Get the singleton instance of GameFile
        /// </summary>
        public static GameFile Instance => _instance;

        /// <summary>
        /// Backing field for Instance property, stores reference to GameFile singleton
        /// </summary>
        private static GameFile _instance;

        /// <summary>
        /// The directory name that stores all the save files
        /// </summary>
        private const string SavesDirectoryName = "Saves";

        /// <summary>
        /// The region file name, the region file stores all regions that have been generated and links them to
        /// the appropriate file containing that regions information
        /// </summary>
        private const string RegionsFileName = "regions";

        /// <summary>
        /// 
        /// </summary>
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
        public static DirectoryInfo[] GetSaveDirectories()
        {
            var savesDirectory = GetSavesDirectory(Path.Combine(Application.persistentDataPath, SavesDirectoryName));
            // Debug.Log(Path.Combine(Application.persistentDataPath, SavesDirectoryName));
            return savesDirectory.GetDirectories();
        }

        public static bool LoadGame(DirectoryInfo saveDirectory)
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
            GetSavesDirectory(savesDirectoryPath);

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
            _instance = new GameFile(saveDirectory);

            return true;
        }

        private static DirectoryInfo GetSavesDirectory(string path)
        {
            // Check that the saves folder path exists, if not, create the directory
            if (!Directory.Exists(path))
            {
                return Directory.CreateDirectory(path);
            }
            return new DirectoryInfo(path);
        }

        /// <summary>
        /// Private constructor, only the static methods of the class are able to create the game
        /// </summary>
        /// <param name="directoryInfo"></param>
        private GameFile(DirectoryInfo directoryInfo)
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

    public class Region
    {
        #region Constants and Statics
        public const int SIZE = 32;
        public const byte HEADER_BYTES_PER_CHUNK = 4;

        /// <summary>
        /// Calculate the region that a chunk resides in
        /// </summary>
        /// <param name="x">The x position of the chunk</param>
        /// <param name="z">The z position of the chunk</param>
        /// <returns></returns>
        public static Vector2Int CalculateRegion(int x, int z)
        {
            return new Vector2Int(x >> 5, z >> 5);
        }
        #endregion


        public readonly int X;
        public readonly int Z;

        public readonly int[] Chunks;

        public Region(int x, int z)
        {
            Chunks = new int[SIZE];
        }

        /// <summary>
        /// Checks whether the chunk at the specified position has been generated
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns>True if generated, false otherwise</returns>
        public bool IsChunkGenerated(int x, int z)
        {
            int xPositionWithinRegion = x % SIZE;
            int zPositionWithinRegion = z % SIZE;

            // Check if the bit at zPos of Chunks[xPos] is 1 (active)
            return (Chunks[xPositionWithinRegion] & (1 << zPositionWithinRegion + 1 )) != 0;
        }

        /// <summary>
        /// Updates the chunks status to enable the chunk at the given position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void ChunkGenerated(int x, int z)
        {
            int xPositionWithinRegion = x % SIZE;
            int zPositionWithinRegion = z % SIZE;

            // Inverts the bit representing the chunk in the region (enabling it)
            // NOTE: This will break if the generated chunk command gets called twice for the same chunk...it shouldnt be called twice!
            Chunks[xPositionWithinRegion] ^= 1 << (zPositionWithinRegion + 1);
        }

        public void SaveChunk(Chunk chunkToSave)
        {
            int xPositionWithinRegion = (int)chunkToSave.Position.x % SIZE;
            int zPositionWithinRegion = (int)chunkToSave.Position.y % SIZE;

            // find the byte offset to save at
            // save
            // update the header
        }
    }
}
