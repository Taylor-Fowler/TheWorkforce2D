using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TheWorkforce.World;

namespace TheWorkforce.Static_Classes
{
    public static class GameFileIO
    {
        public static string FilePath { get; private set; }

        private const string FORMAT = ".json";
        private static string BackupFilePath;


        public static void SetFilePath(string filePath)
        {
            FilePath = filePath;
            BackupFilePath = FilePath + ".backup";
        }

        public static bool SaveChunk(Chunk chunkToSave)
        {
            return false;
        }

        public static bool SaveChunks(ICollection chunksToSave)
        {
            return false;
        }

        public static List<Chunk> LoadChunks(List<Vector2> chunksToLoad)
        {
            List<Chunk> loadedChunks = new List<Chunk>();
            for (int i = chunksToLoad.Count - 1; i >= 0; i--)
            {
            }

            return loadedChunks;
        }

        public static ICollection LoadChunks(Vector2[] chunksToLoad)
        {
            ICollection loadedChunks = new List<Chunk>();
            for (int i = chunksToLoad.Length - 1; i >= 0; i--)
            {
            }

            return loadedChunks;
        }

        public static void CreateGame()
        {
            File.Create(FilePath + FORMAT);
            File.Create(BackupFilePath + FORMAT);
        }

        public static bool SaveGame()
        {
            if (File.Exists(FilePath + FORMAT))
                if (File.Exists(BackupFilePath + FORMAT))
                {
                    using (FileStream stream = File.OpenRead(BackupFilePath + FORMAT))
                    using (FileStream writeStream = File.OpenWrite(FilePath + FORMAT))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        BinaryWriter writer = new BinaryWriter(writeStream);

                        byte[] buffer = new byte[1024];
                        int bytesRead;

                        while ((bytesRead = stream.Read(buffer, 0, 1024)) > 0) writeStream.Write(buffer, 0, bytesRead);
                    }

                    return true;
                }

            return false;
        }
    }    
}
