using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheWorkforce.Game_State
{
    using Entities;

    public class RegionFile
    {
        #region Constants and Statics
        public const int CHUNKS_PER_AXIS = 32;
        public const int CHUNK_TOTAL = CHUNKS_PER_AXIS * CHUNKS_PER_AXIS;

        public const int BODY_BYTES_PER_TILE = 256;
        public const int BODY_BYTES_PER_CHUNK = BODY_BYTES_PER_TILE * Chunk.AREA;

        /// <summary>
        /// Header bytes per chunk:
        ///     - First 2 bytes - ushort defining the offset
        ///     - Last byte - byte defining flags (such as keep loaded)
        /// </summary>
        public const int HEADER_BYTES_PER_CHUNK = 3;
        public const int HEADER_SIZE = HEADER_BYTES_PER_CHUNK * CHUNK_TOTAL;

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


        public int X { get; }
        public int Z { get; }
        public FileInfo File { get; }
        public int[] Chunks { get; }

        private int _appendChunkLocation { get; set; }

        private int NextChunkOffset { get; set; }

        // Loading the region file into memory on startup
        public RegionFile(FileInfo file)
        {
            File = file;
            var splitName = File.Name.Split('.');
            X = int.Parse(splitName[1]);
            Z = int.Parse(splitName[2]);
            Chunks = new int[CHUNKS_PER_AXIS];
            ReadFile();
        }


        public RegionFile(int x, int z, FileInfo file)
        {
            X = x;
            Z = z;
            File = file;
            Chunks = new int[CHUNKS_PER_AXIS];
            ReadFile();
        }

        public RegionFile(DirectoryInfo containingDirectory, int x, int z, IEnumerable<Chunk> chunks)
        {
            X = x;
            Z = z;
            Chunks = new int[CHUNKS_PER_AXIS];
            _appendChunkLocation = HEADER_SIZE;

            File = CreateFile(containingDirectory, (fileStream) =>
            {
                Save(fileStream, chunks);
            });
        }

        public RegionFile(DirectoryInfo containingDirectory, int x, int z, Chunk chunk)
        {
            X = x;
            Z = z;
            Chunks = new int[CHUNKS_PER_AXIS];
            _appendChunkLocation = HEADER_SIZE;

            File = CreateFile(containingDirectory, (fileStream) =>
            {
                Save(fileStream, chunk);
            });
        }

        public IEnumerable<Vector2Int> GetGeneratedChunks()
        {
            List<Vector2Int> generatedChunks = new List<Vector2Int>();

            for(int x = 0; x < Chunks.Length; ++x)
            {
                for(int z = 0; z < Chunks.Length; ++z)
                {
                    if(IsChunkGeneratedExact(x, z))
                    {
                        generatedChunks.Add(new Vector2Int(x + (X * CHUNKS_PER_AXIS), z + (Z * CHUNKS_PER_AXIS)));
                    }
                }
            }

            return generatedChunks;
        }

        public byte[][][] GetChunkData(int worldPositionX, int worldPositionZ)
        {
            int x = Mathf.Abs(worldPositionX) % CHUNKS_PER_AXIS;
            int z = Mathf.Abs(worldPositionZ) % CHUNKS_PER_AXIS;

            byte[][][] chunkBytes = new byte[Chunk.SIZE][][];

            using (FileStream fileStream = File.OpenRead())
            {
                SeekChunkHeader(fileStream, x, z);
                var chunkLocation = ReadChunkHeader(fileStream);

                fileStream.Seek(chunkLocation, SeekOrigin.Begin);

                for(int ix = 0; ix < Chunk.SIZE; ++ix)
                {
                    chunkBytes[ix] = new byte[Chunk.SIZE][];

                    for(int iz = 0; iz < Chunk.SIZE; ++iz)
                    {
                        chunkBytes[ix][iz] = new byte[BODY_BYTES_PER_TILE];

                        fileStream.Read(chunkBytes[ix][iz], 0, BODY_BYTES_PER_TILE);
                    }
                }
            }

            return chunkBytes;
        }

        /// <summary>
        /// Checks whether the chunk at the specified position has been generated
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns>True if generated, false otherwise</returns>
        public bool IsChunkGenerated(int x, int z)
        {
            int xPositionWithinRegion = x % CHUNKS_PER_AXIS;
            int zPositionWithinRegion = z % CHUNKS_PER_AXIS;

            return IsChunkGeneratedExact(xPositionWithinRegion, zPositionWithinRegion);
        }

        private bool IsChunkGeneratedExact(int x, int z)
        {
            // Check if the bit at zPos of Chunks[xPos] is 1 (active)
            return (Chunks[x] & (1 << z + 1)) != 0;
        }



        public void Save(Chunk chunkToSave)
        {
            // Check if the file is not open
            // Write to file

            using (FileStream fileStream = File.Open(FileMode.Open))
            {
                Save(fileStream, chunkToSave);
            }
        }

        private FileInfo CreateFile(DirectoryInfo containingDirectory, Action<FileStream> writeAction)
        {
            string filePath = $"{containingDirectory.FullName} \\r.{X.ToString()}.{Z.ToString()}{GameFile.FileFormat}";
            FileInfo file = new FileInfo(filePath);

            using (FileStream fileStream = file.Create())
            {
                // Create the empty header
                fileStream.Write(new byte[HEADER_SIZE], 0, HEADER_SIZE);

                writeAction(fileStream);
            }

            return file;
        }

        private void ReadFile()
        {
            using (FileStream fileStream = File.OpenRead())
            {
                int biggestOffset = -1;

                // Read the header
                for (int x = 0; x < CHUNKS_PER_AXIS; ++x)
                {
                    for (int z = 0; z < CHUNKS_PER_AXIS; ++z)
                    {
                        byte[] chunkHeader = new byte[HEADER_BYTES_PER_CHUNK];
                        fileStream.Read(chunkHeader, 0, chunkHeader.Length);

                        ushort chunkOffset = BitConverter.ToUInt16(chunkHeader, 0);

                        if(chunkOffset != 0)
                        {
                            GeneratedChunk(x, z);

                            // Determine if it needs to be loaded...
                            //chunkOffset;
                            if(chunkOffset > biggestOffset)
                            {
                                biggestOffset = chunkOffset;
                                NextChunkOffset = chunkOffset;
                                Debug.Log($"Biggest offset changed {biggestOffset}");
                            }
                        }
                    }
                }

                // The next chunk to be saved (which hasnt been saved before) should be appended to the end of the file
                // The offset is 1 more than the largest detected
                _appendChunkLocation = HEADER_SIZE + ((biggestOffset + 1) * BODY_BYTES_PER_CHUNK);
            }
        }

        private void Save(FileStream fileStream, Chunk chunkToSave)
        {
            int x = Mathf.Abs(chunkToSave.Position.x) % CHUNKS_PER_AXIS;
            int z = Mathf.Abs(chunkToSave.Position.y) % CHUNKS_PER_AXIS;

            int location = _appendChunkLocation;

            if(IsChunkGeneratedExact(x, z))
            {
                Debug.Log("<color=red>Chunk is generated exact</color>");
                SeekChunkHeader(fileStream, x, z);
                //location = (ReadChunkHeader(fileStream) * BODY_BYTES_PER_CHUNK) + HEADER_SIZE;
                location = ReadChunkHeader(fileStream);
            }
            else
            {
                Debug.Log("<color=green>Chunk is not generated yet</color>");
                GeneratedChunk(x, z);
                _appendChunkLocation += BODY_BYTES_PER_CHUNK;
                SeekChunkHeader(fileStream, x, z);

                location = NextChunkOffset;// * BODY_BYTES_PER_CHUNK
                SaveChunkHeader2(fileStream);

                //SaveChunkHeader(fileStream, location);
            }

            //fileStream.Seek(location, SeekOrigin.Begin);
            fileStream.Seek(HEADER_SIZE + (location * BODY_BYTES_PER_CHUNK), SeekOrigin.Begin);

            // 64 tiles per chunk
            // each tile has 2 floats and a byte (9 bytes)
            // an entity needs...ushort for id (2 bytes)
            // lets say an entity might need 50 inventory slots (200 bytes) 211 per tile...
            // lets go to 256 bytes per tile
            // 64 * 256 = 16,384...bytes per chunk, each region file has 32x32 (1024) chunks...results in 16,384 kbytes

            foreach (var tile in chunkToSave.Tiles)
            {
                byte[] tileBytes = tile.GetByteArray();
                fileStream.Write(tileBytes, 0, tileBytes.Length);

                int bytesLeft = BODY_BYTES_PER_TILE - tileBytes.Length;
                if (tile.StaticEntityInstanceId != 0)
                {
                    var entity = EntityCollection.Instance().GetEntity(tile.StaticEntityInstanceId);
                    byte[] entityBytes = entity.GetSaveData();

                    fileStream.Write(entityBytes, 0, entityBytes.Length);
                    bytesLeft -= entityBytes.Length;
                }
                fileStream.Write(new byte[bytesLeft], 0, bytesLeft);
            }

            //Debug.Log("[RegionFile] - Save(FileStream, Chunk)");
        }

        private void Save(FileStream fileStream, IEnumerable<Chunk> chunksToSave)
        {
            foreach (var chunk in chunksToSave)
            {
                Save(fileStream, chunk);
                //int xPositionWithinRegion = Mathf.Abs(chunk.Position.x) % CHUNKS_PER_AXIS;
                //int zPositionWithinRegion = Mathf.Abs(chunk.Position.y) % CHUNKS_PER_AXIS;

                //GeneratedChunk(xPositionWithinRegion, zPositionWithinRegion);

                //SeekChunk(fileStream, xPositionWithinRegion, zPositionWithinRegion);

                //foreach (var tile in chunk.Tiles)
                //{
                //    byte[] tileBytes = tile.GetByteArray();
                //    fileStream.Write(tileBytes, 0, tileBytes.Length);

                //    int bytesLeft = 256 - tileBytes.Length;
                //    if (tile.StaticEntityInstanceId != 0)
                //    {
                //        var entity = EntityCollection.Instance().GetEntity(tile.StaticEntityInstanceId);
                //        byte[] entityBytes = entity.GetSaveData();

                //        fileStream.Write(entityBytes, 0, entityBytes.Length);
                //        bytesLeft -= entityBytes.Length;
                //    }
                //    fileStream.Write(new byte[bytesLeft], 0, bytesLeft);
                //}
            }

            Debug.Log($"[RegionFile] - Save(FileStream, IEnumerable<Chunk>)");
        }

        private void SeekChunkHeader(FileStream fileStream, int x, int z)
        {
            fileStream.Seek((x + (z * CHUNKS_PER_AXIS)) * HEADER_BYTES_PER_CHUNK, SeekOrigin.Begin);
        }

        private void SaveChunkHeader(FileStream fileStream, int location)
        {
            ushort offset = (ushort)((location - HEADER_SIZE) / BODY_BYTES_PER_CHUNK);
            fileStream.Write(BitConverter.GetBytes(offset), 0, sizeof(ushort));
            Debug.Log($"[RegionFile] - SaveChunkHeader(FileStream, int)\nChunk Header Offset: {offset}");
        }

        private void SaveChunkHeader2(FileStream fileStream)
        {
            fileStream.Write(BitConverter.GetBytes(++NextChunkOffset), 0, sizeof(ushort));
            Debug.Log($"[RegionFile] - SaveChunkHeader(FileStream, int)\nChunk Header Offset: {NextChunkOffset}");
        }

        private ushort ReadChunkHeader(FileStream fileStream)
        {
            byte[] chunkHeader = new byte[HEADER_BYTES_PER_CHUNK];
            fileStream.Read(chunkHeader, 0, chunkHeader.Length);

            return BitConverter.ToUInt16(chunkHeader, 0);
        }

        /// <summary>
        /// Updates the chunks status to enable the chunk at the given position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        private void GeneratedChunk(int x, int z)
        {
            //int xPositionWithinRegion = x % SIZE;
            //int zPositionWithinRegion = z % SIZE;

            // Inverts the bit representing the chunk in the region (enabling it)
            // NOTE: This will break if the generated chunk command gets called twice for the same chunk...it shouldnt be called twice!
            Chunks[x] ^= 1 << (z + 1);
        }
    } 
}

// make a chunk header struct