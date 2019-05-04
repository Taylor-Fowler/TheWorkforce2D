using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TheWorkforce.Network
{
    using Game_State;

    public class FileTransfer : NetworkBehaviour
    {
        #region Constants and Statics
        /// <summary>
        /// The file transfer messages use a dedicated channel to allow full use of the channel buffer
        /// </summary>
        public const int DEDICATED_CHANNEL = 2;

        /// <summary>
        /// The amount of data that can be sent within one network message in the channel
        /// </summary>
        public const short PACKET_SIZE = 1400;

        /// <summary>
        /// The number of packets that can be buffered before requiring the network channel to be flushed
        /// </summary>
        public const short BUFFERED_PACKET_SIZE = 16;

        /// <summary>
        /// A method to call when the server has received a file confirmation message from a client
        /// </summary>
        public static Action<NetworkConnection> SuccessfulTransfer;
        #endregion

        /// <summary>
        /// Player data received from the server.
        ///             Item 1 of the tuple represents the player Id
        ///             Item 2 of the tuple represents the pure player data
        /// </summary>
        private List<Tuple<int, List<byte>>> _playerFiles;

        /// <summary>
        /// Region data received from the server.
        ///             The key represents the region file name
        ///             The value represents the region file payload
        /// </summary>
        private Dictionary<string, List<byte>> _regionFiles;

        
        /// <summary>
        /// Called on the server when a client has received the final packet of the game file
        /// </summary>
        [Command]
        private void CmdConfirmFileTransfer()
        {
            SuccessfulTransfer?.Invoke(connectionToClient);
            Debug.Log("[FileTransfer] - CmdConfirmFileTransfer()");
        }

        /// <summary>
        /// A server-only method that transfers the game file to a specified client.
        /// 
        /// Scenarios that require this functionality:
        ///     1. A non-host client connects to the server
        ///     2. Somehow the client game has been corrupted or desynced
        /// </summary>
        /// <param name="targetConnection">The client connection to send the game file to</param>
        public IEnumerator ServerSendGameFile(NetworkConnection targetConnection)
        {
            short packetBuffer = 1;
            int byteOffset = 0;
            {
                Debug.Log("[FileTransfer] - ServerSendGameFile(NetworkConnection)");
                // Tell the client that a file transfer is starting for the current game
                TargetClientInitiateGameFileTransfer(
                    targetConnection,
                    System.Text.Encoding.UTF8.GetBytes(GameFile.Instance.Name),
                    GameFile.Instance.SerializeWorldData()
                );

                // Wait a small amount of time before sending the main data
                yield return new WaitForSeconds(0.1f);

                var playerData = GameFile.Instance.SerializePlayerData();

                int currentPlayer = 0;

                List<int> playerIds = new List<int>();
                List<short> playerDataCounters = new List<short>();
                List<byte> playerBytes = new List<byte>();

                byte previousFlag = 0;

                while (true)
                {
                    int bytesLeft = PACKET_SIZE;
                    byte flags = previousFlag;
                    previousFlag = 0; // we have captured the previous flag, safe to reset it
                    --bytesLeft; // flags data


                    while (currentPlayer < playerData.Count)
                    {
                        bytesLeft -= sizeof(short); // bytes required to tell us how many bytes of player data are associated with current player
                        bytesLeft -= sizeof(int); // bytes required for player Id

                        // Cannot add the current players data to the current packet
                        if (bytesLeft <= 0)
                        {
                            break;
                        }

                        playerIds.Add(playerData[currentPlayer].Item1);

                        // TODO: Cover scenario of - player data is continued from previous packet and larger than next packet
                        //          i.e. PlayerData is split across 3 packets

                        // Append to previous packet data
                        if (flags != 0)
                        {
                            var bytesToAdd = playerData[currentPlayer].Item2.Length - byteOffset;
                            playerDataCounters.Add((short)bytesToAdd);

                            var bytes = new byte[bytesToAdd];
                            Array.Copy(playerData[currentPlayer].Item2, byteOffset, bytes, 0, bytesToAdd);
                            playerBytes.AddRange(bytes);

                            byteOffset = 0;
                            ++currentPlayer;
                        }

                        // if the amount of bytes for the player is less than those left then we will split their data across
                        // multiple packets
                        // we also need to log that this is what we are doing:
                        //      the list needs to be made smaller for the next iteration
                        //      the flag also needs to be adjusted
                        else if (playerData[currentPlayer].Item2.Length > bytesLeft)
                        {
                            previousFlag = 1;
                            playerDataCounters.Add((short)playerData[currentPlayer].Item2.Length);

                            // Get the smaller subset of bytes to send in the first packet
                            var bytes = new byte[bytesLeft];
                            Array.Copy(playerData[currentPlayer].Item2, bytes, bytes.Length);

                            playerBytes.AddRange(bytes);
                            byteOffset = playerData[currentPlayer].Item2.Length - bytesLeft;
                            bytesLeft = 0;

                            break;
                        }
                        // we have enough space for all of the player data in the current packet
                        else
                        {
                            playerDataCounters.Add((short)playerData[currentPlayer].Item2.Length);
                            playerBytes.AddRange(playerData[currentPlayer].Item2);
                            bytesLeft -= playerData[currentPlayer].Item2.Length;

                            ++currentPlayer;
                        }
                    }

                    TargetClientReceivePlayerFile(targetConnection, playerIds.ToArray(), playerDataCounters.ToArray(), playerBytes.ToArray(), flags);

                    // Reset the flag and data for the next packet
                    playerIds.Clear();
                    playerDataCounters.Clear();
                    playerBytes.Clear();
                    packetBuffer++;

                    if (packetBuffer == BUFFERED_PACKET_SIZE)
                    {
                        targetConnection.FlushChannels();
                        yield return new WaitForSeconds(0.1f);
                        packetBuffer = 1;
                    }

                    if (currentPlayer == playerData.Count)
                    {
                        break;
                    }
                }
            }
            var regionData = GameFile.Instance.SerializeRegionFiles();

            while(regionData.Count > 0)
            {
                byteOffset = 0;
                var currentRegionData = regionData[regionData.Count - 1];

                byte[] fileName = System.Text.Encoding.UTF8.GetBytes(currentRegionData.Item1);
                int totalBytesLeft = currentRegionData.Item2.Length;

                while(totalBytesLeft > 0)
                {
                    int numberOfBytesToSend = Mathf.Min(totalBytesLeft, PACKET_SIZE - fileName.Length);

                    totalBytesLeft -= numberOfBytesToSend;
                    byte[] bytesToSend = new byte[numberOfBytesToSend];
                    Array.Copy(currentRegionData.Item2, byteOffset, bytesToSend, 0, numberOfBytesToSend);

                    TargetClientReceiveRegionFile(targetConnection, fileName, bytesToSend);
                    byteOffset += numberOfBytesToSend;

                    if (++packetBuffer == BUFFERED_PACKET_SIZE)
                    {
                        targetConnection.FlushChannels();
                        yield return new WaitForSeconds(0.2f);
                        packetBuffer = 1;
                    }
                }
                regionData.Remove(currentRegionData);
            }

            TargetClientFinishGameFileTransfer(targetConnection); // Tell the client that the file transfer is over
        }

        [TargetRpc(channel = DEDICATED_CHANNEL)]
        private void TargetClientInitiateGameFileTransfer(NetworkConnection conn, byte[] gameName, byte[] worldData)
        {
            Debug.Log("<color=#FF0000><b>[FileTransfer]</b></color> - TargetClientInitiateGameFileTransfer(NetworkConnection, string)\n" +
                        $"Connection ID: {conn.connectionId}");
            string name = System.Text.Encoding.UTF8.GetString(gameName);
            GameFile.CreateTemporaryGame(name, conn.connectionId, worldData);

            _playerFiles = new List<Tuple<int, List<byte>>>();
            _regionFiles = new Dictionary<string, List<byte>>();
        }

        [TargetRpc(channel = DEDICATED_CHANNEL)]
        private void TargetClientFinishGameFileTransfer(NetworkConnection conn)
        {
            foreach(var pair in _playerFiles)
            {
                GameFile.Instance.Save(pair.Item1, pair.Item2.ToArray());
            }
            _playerFiles = null;

            foreach(var pair in _regionFiles)
            {
                GameFile.Instance.Save(pair.Key, pair.Value.ToArray());
            }
            _regionFiles = null;

            // tell the network manager that we are ready to play...
            CmdConfirmFileTransfer();
        }

        [TargetRpc(channel = DEDICATED_CHANNEL)]
        private void TargetClientReceiveRegionFile(NetworkConnection conn, byte[] regionNameBytes, byte[] bytes)
        {
            string regionName = System.Text.Encoding.UTF8.GetString(regionNameBytes);

            List<byte> regionBytes;
            if(!_regionFiles.TryGetValue(regionName, out regionBytes))
            {
                regionBytes = new List<byte>();
                _regionFiles.Add(regionName, regionBytes);
            }

            regionBytes.AddRange(bytes);
        }

        [TargetRpc(channel = DEDICATED_CHANNEL)]
        private void TargetClientReceivePlayerFile(NetworkConnection conn, int[] playerIds, short[] dataLength, byte[] bytes, byte flags)
        {
            int i = 0;
            int currentBytePosition = 0; // track which byte to read from, this is our array offset

            // bit 0 of flags determines if the first file in the packet is a concatenation of the previous packet
            // concatenate with the previous player file received
            if((flags & (1 << 0)) != 0)
            {
                // process the previous    
                var dataList = _playerFiles[_playerFiles.Count - 1].Item2;

                int targetPosition = dataLength[i];

                for(; currentBytePosition < targetPosition; ++currentBytePosition)
                {
                    dataList.Add(bytes[currentBytePosition]);
                }

                ++i;
            }

            for(; i < playerIds.Length; ++i)
            {
                int bytesToRead = dataLength[i];

                // data length should be for the full file size, even when the file has to be split across multiple packets...
                // this is different for the continuation of the file in the next packet...this will contain the actual number of bytes
                // contained in the byte
                // technically i could go back to an array with this...but for the time being...
                var dataList = new List<byte>(bytesToRead);
                _playerFiles.Add(new Tuple<int, List<byte>>(playerIds[i], dataList));

                int targetPosition = currentBytePosition + bytesToRead;
                // check if we are at the end of the packet and are expecting the rest of the file in the next packet
                if(targetPosition >= bytes.Length)
                {
                    targetPosition = bytes.Length - currentBytePosition; // 1024 - 10 = 1014...reads from 10 -> 1013 (1004)
                }

                for(; currentBytePosition < targetPosition; ++currentBytePosition)
                {
                    dataList.Add(bytes[currentBytePosition]);
                }
            }           
        }
    }
}