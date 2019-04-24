using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace TheWorkforce.Game_State
{
    using Entities;

    public class GameFile
    {
        #region Constants + Statics
        /// <summary>
        /// Get the singleton instance of GameFile
        /// </summary>
        public static GameFile Instance => _instance;

        /// <summary>
        /// Backing field for Instance property, stores reference to GameFile singleton
        /// </summary>
        private static GameFile _instance;

        #region Directory Names
        /// <summary>
        /// The directory name that stores all the save files
        /// </summary>
        private const string SavesDirectoryName = "Saves";
        private const string TemporaryCopyDirectoryName = "Temp";

        private const string RegionsDirectoryName = "Regions";
        private const string PlayersDirectoryName = "Players";
        #endregion

        #region File Names
        public const string WorldFileName = "world";        
        public const string FileFormat = ".dat";
        #endregion

        #region Methods
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
            if(_instance != null)
            {
                // Must unload previous game first
                return false;
            }
            if(saveDirectory == null || !saveDirectory.Exists)
            {
                // Save folder does not exist
                return false;
            }

            DirectoryInfo playersDirectory = new DirectoryInfo(Path.Combine(saveDirectory.FullName, PlayersDirectoryName));
            if(!playersDirectory.Exists)
            {
                // Players directory must exist
                return false;
            }
            
            DirectoryInfo chunksDirectory = new DirectoryInfo(Path.Combine(saveDirectory.FullName, RegionsDirectoryName));
            if(!chunksDirectory.Exists)
            {
                // Chunks directory must exist
                return false;
            }

            _instance = new GameFile(saveDirectory, playersDirectory, chunksDirectory);
            return true;
        }

        public static bool CreateGame(string gameName, int worldSeed)
        {
            // The activate game must be unloaded before a new game can be created
            if(_instance != null)
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
            DirectoryInfo chunksDirectory = Directory.CreateDirectory(Path.Combine(newSaveDirectoryPath, RegionsDirectoryName));

            // Initialise the instance of game save
            _instance = new GameFile(saveDirectory, playersDirectory, chunksDirectory);
            _instance.Save(new World(worldSeed));
            return true;
        }

        public static bool CreateTemporaryGame(string gameName)
        {
            // The activate game must be unloaded before a new game can be created
            if (_instance != null)
            {
                return false;
            }

            string savesDirectoryPath = Path.Combine(Application.persistentDataPath, TemporaryCopyDirectoryName);
            GetSavesDirectory(savesDirectoryPath);

            string newSaveDirectoryPath = Path.Combine(savesDirectoryPath, gameName);

            // If the new save game directory path exists, delete it as it is only temporary anyways
            // this will most likely happen with disconnects followed by reconnects OR default game names
            if (Directory.Exists(newSaveDirectoryPath))
            {
                // Delete the folder and its contents recursively
                var directoryInfo = new DirectoryInfo(newSaveDirectoryPath);
                directoryInfo.Delete(true);
            }

            // Recreate the folders
            DirectoryInfo saveDirectory = Directory.CreateDirectory(newSaveDirectoryPath);
            DirectoryInfo playersDirectory = Directory.CreateDirectory(Path.Combine(newSaveDirectoryPath, PlayersDirectoryName));
            DirectoryInfo chunksDirectory = Directory.CreateDirectory(Path.Combine(newSaveDirectoryPath, RegionsDirectoryName));

            // Initialise the instance of game save
            _instance = new GameFile(saveDirectory, playersDirectory, chunksDirectory);

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

        private static FileInfo SearchForFile(DirectoryInfo directoryInfo, string fileName)
        {
            foreach(var fileInfo in directoryInfo.GetFiles())
            {
                if(fileInfo.Name == fileName)
                {
                    return fileInfo;
                }
            }

            return null;
        }
        #endregion
        #endregion

        /// <summary>
        /// Get the game save name
        /// </summary>
        public string Name => _saveDirectory.Name;

        private Dictionary<Vector2Int, RegionFile> _regionFiles;

        private readonly DirectoryInfo _saveDirectory;
        private readonly DirectoryInfo _playersDirectory;
        private readonly DirectoryInfo _regionsDirectory;

        public int GetPlayerId(FileInfo fileInfo) => Int32.Parse(fileInfo.Name.Split('.')[0]);

        /// <summary>
        /// Private constructor, only the static methods of the class are able to create the game
        /// </summary>
        /// <param name="directoryInfo"></param>
        private GameFile(DirectoryInfo saveDirectory, DirectoryInfo playersDirectory, DirectoryInfo regionsDirectory)
        {
            _saveDirectory = saveDirectory;
            _playersDirectory = playersDirectory;
            _regionsDirectory = regionsDirectory;
            _regionFiles = new Dictionary<Vector2Int, RegionFile>();
        }

        #region Saving
        public void Save(World world)
        {
            FileInfo worldInfo = SearchForFile(_saveDirectory, WorldFileName + FileFormat);

            // First time saving world
            if (worldInfo == null)
            {
                worldInfo = new FileInfo(_saveDirectory.FullName + "\\" + WorldFileName + FileFormat);
                byte[] worldSeed = BitConverter.GetBytes(world.Seed);

                using (FileStream fs = worldInfo.OpenWrite())
                {
                    fs.Write(worldSeed, 0, worldSeed.Length);
                }
            }

            foreach(var chunk in world.LoadedChunks)
            {
                Save(chunk.Value);
            }
            Debug.Log($"[GameFile] - Save(World)\nNumber of Chunks being saved <color=green>{world.LoadedChunks.Count}</color>");
        }

        public void Save(Chunk chunk)
        {
            var regionPosition = RegionFile.CalculateRegion(chunk.Position.x, chunk.Position.y);
            RegionFile regionFile;

            if(_regionFiles.TryGetValue(regionPosition, out regionFile))
            {
                regionFile.Save(chunk);
            }
            else
            {
                regionFile = new RegionFile(_regionsDirectory, regionPosition.x, regionPosition.y, chunk);
                _regionFiles.Add(regionPosition, regionFile);
            }
        }

        public void Save(IEnumerable<Chunk> chunks)
        {
            // Organise each chunk into the correct region 
            // Then save the collection of chunks per region
        }

        public void Save(PlayerData playerData) => Save(playerData.Id, playerData.ByteArray());

        public void Save(int playerId, byte[] playerData)
        {
            string playerFileName = playerId.ToString() + FileFormat;
            FileInfo playerFile = SearchForFile(_playersDirectory, playerFileName);

            // If the player file does not exist then create it
            if (playerFile == null)
            {
                playerFile = new FileInfo(_playersDirectory.FullName + "\\" + playerFileName);
            }

            using (FileStream fs = playerFile.OpenWrite())
            {
                fs.Write(playerData, 0, playerData.Length);
            }
        }
        #endregion

        #region Loading
        public World LoadWorld()
        {
            FileInfo worldInfo = SearchForFile(_saveDirectory, WorldFileName + FileFormat);

            if (worldInfo == null)
            {
                Debug.LogError($"[GameFile] - LoadWorld() \nFatal Error - {WorldFileName}{FileFormat} file does not exist!");
                return null;
            }

            int worldSeed = 0;
            using (FileStream fs = worldInfo.OpenRead())
            {
                var seedBytes = new byte[sizeof(int)];
                fs.Read(seedBytes, 0, seedBytes.Length);
                worldSeed = BitConverter.ToInt32(seedBytes, 0);
            }

            World world = new World(worldSeed);
            LoadRegions();

            foreach(var regionFilePair in _regionFiles)
            {
                var knownChunks = regionFilePair.Value.GetGeneratedChunks();
                world.RegisterKnownChunks(knownChunks);
            }


            return world;
        }

        public List<Chunk> LoadChunks(IEnumerable<Vector2Int> chunkPositions)
        {
            List<Chunk> chunks = new List<Chunk>();

            foreach(var chunkPosition in chunkPositions)
            {
                var chunk = new Chunk(chunkPosition);
                chunks.Add(chunk);


                var regionPosition = RegionFile.CalculateRegion(chunkPosition.x, chunkPosition.y);

                var regionFile = _regionFiles[regionPosition];
                var chunkData = regionFile.GetChunkData(chunkPosition.x, chunkPosition.y);

                var chunkWorldPosition = Chunk.CalculateWorldPosition(chunkPosition);

                for(int x = 0; x < chunkData.Length; ++x)
                {
                    for(int z = 0; z < chunkData[x].Length; ++z)
                    {
                        int byteOffset = 0;

                        byte tilesetId = chunkData[x][z][byteOffset];
                        ++byteOffset;

                        float moisture = BitConverter.ToSingle(chunkData[x][z], byteOffset);
                        byteOffset += sizeof(float);

                        float elevation = BitConverter.ToSingle(chunkData[x][z], byteOffset);
                        byteOffset += sizeof(float);

                        Tile tile = new Tile(tilesetId, moisture, elevation, new Vector2Int(x, z));
                        chunk.Tiles[x, z] = tile;

                        uint entityId = BitConverter.ToUInt32(chunkData[x][z], byteOffset);

                        if(entityId == 0)
                        {
                            continue;
                        }

                        byteOffset += sizeof(uint);
                        
                        ushort entityType = BitConverter.ToUInt16(chunkData[x][z], byteOffset);
                        byteOffset += sizeof(ushort);

                        EntityCollection.Instance().LoadEntity(
                            entityType, 
                            entityId,
                            x + chunkWorldPosition.x,
                            z + chunkWorldPosition.y,
                            chunkData[x][z], 
                            byteOffset);

                        tile.PlaceEntity(entityId);
                    }

                }
            }

            return chunks;
        }

        public List<Tuple<int, byte[]>> LoadPlayerData()
        {
            var playerFiles = _saveDirectory.GetFiles();

            // If there are no player files
            // This should not happen!
            if (playerFiles.Length == 0)
            {
                return null;
            }

            List<Tuple<int, byte[]>> playerData = new List<Tuple<int, byte[]>>();

            foreach (var fileInfo in playerFiles)
            {
                byte[] fileData = File.ReadAllBytes(fileInfo.FullName);
                // TODO: Better error checking...or any!
                // Filename is in the format of playerId.dat e.g. 1.dat
                //      Split at the period, will give [ "1", "dat" ]
                int playerId = GetPlayerId(fileInfo);
                playerData.Add(new Tuple<int, byte[]>(playerId, fileData));
            }

            return playerData;
        }

        private void LoadRegions()
        {
            var regionFiles = _regionsDirectory.GetFiles();
            foreach (var fileInfo in regionFiles)
            {
                var regionFile = new RegionFile(fileInfo);
                _regionFiles.Add(new Vector2Int(regionFile.X, regionFile.Z), regionFile);
            }

            Debug.Log($"[GameFile] - LoadRegions()\nRegion Files Count: {regionFiles.Length}");
        }
        #endregion



        /// <summary>
        /// Checks if the player with the given Id exists and when true, gives the player data
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool PlayerExists(int playerId, out byte[] data)
        {
            string playerFileName = playerId.ToString() + FileFormat;
            FileInfo playerFile = SearchForFile(_playersDirectory, playerFileName);

            // Player file does not exist
            if (playerFile == null)
            {
                data = null;
                return false;
            }

            data = File.ReadAllBytes(playerFile.FullName);
            return true;
        }

        public void ExitGame()
        {
            // unloaded
            _instance = null;
        }
    }

    
}
