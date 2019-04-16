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
        public const short PACKET_SIZE = 1400;

        private List<Tuple<int, List<byte>>> _playerFiles;
        private List<Tuple<string, List<byte>>> _regionFiles;

        /// <summary>
        /// A server-only method that transfers the game file to a specified client.
        /// 
        /// Scenarios that require this functionality:
        ///     1. A non-host client connects to the server
        ///     2. Somehow the client game has been corrupted or desynced
        /// </summary>
        /// <param name="targetConnection"></param>
        [Server]
        public IEnumerator ServerSendGameFile(NetworkConnection targetConnection)
        {
            // Tell the client that a file transfer is starting for the current game
            TargetClientInitiateGameFileTransfer(targetConnection, GameFile.Instance.Name);

            yield return new WaitForFixedUpdate();
            // Transfer files
            var playerData = GameFile.Instance.GetPlayerData();

            int currentPlayer = 0;

            List<int> playerIds = new List<int>();
            List<short> playerDataCounters = new List<short>();
            List<byte> playerBytes = new List<byte>();

            byte previousFlag = 0;
            int byteOffset = 0;

            while(true)
            {
                int bytesLeft = PACKET_SIZE;
                byte flags = previousFlag;
                previousFlag = 0; // we have captured the previous flag, safe to reset it
                --bytesLeft; // flags data


                while(currentPlayer < playerData.Count)
                {
                    bytesLeft -= sizeof(short); // bytes required to tell us how many bytes of player data are associated with current player
                    bytesLeft -= sizeof(int); // bytes required for player Id

                    // Cannot add the current players data to the current packet
                    if(bytesLeft <= 0)
                    {
                        break;
                    }

                    playerIds.Add(playerData[currentPlayer].Item1);

                    // TODO: Cover scenario of - player data is continued from previous packet and larger than next packet
                    //          i.e. PlayerData is split across 3 packets

                    // Append to previous packet data
                    if(flags != 0)
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
                    else if(playerData[currentPlayer].Item2.Length < bytesLeft)
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

                if(currentPlayer == playerData.Count)
                {
                    break;
                }
            }

            // Tell the client that the file transfer is over
            TargetClientFinishGameFileTransfer(targetConnection);
        }

        [TargetRpc(channel = (int)QosType.ReliableSequenced)]
        private void TargetClientInitiateGameFileTransfer(NetworkConnection conn, string gameName)
        {
            Debug.Log("<color=#FF0000><b>[FileTransfer]</b></color> - TargetClientInitiateGameFileTransfer(NetworkConnection, string)");
            GameFile.CreateTemporaryGame(gameName);

            _playerFiles = new List<Tuple<int, List<byte>>>();
            _regionFiles = new List<Tuple<string, List<byte>>>();
        }

        [TargetRpc(channel = (int)QosType.ReliableSequenced)]
        private void TargetClientFinishGameFileTransfer(NetworkConnection conn)
        {
            foreach(var pair in _playerFiles)
            {
                GameFile.Instance.SavePlayer(pair.Item1, pair.Item2.ToArray());
            }
            _playerFiles = null;

            // tell the network manager that we are ready to play...
        }

        [TargetRpc(channel = (int)QosType.ReliableSequenced)]
        private void TargetClientReceiveRegionFile(NetworkConnection conn, byte[] bytes)
        {
            // What needs to be sent...
            // Folder and file structure...
            // File contents...

            // method 1: send the directory structure first, followed by contents

            // method 2: send each file separately with their containing folder
            //              in the file header...
            // cons: if there are multiple small files then its kind of a waste of
            //              packets...

            // method 3: send the structure with file contents and split files based
            //              on packet size
            //              EG: A packet can contain more than one file's contents

            //  arrays of: filename, file length, if the first file is a new file or
            //              concatenation of data with previous packet...
            //              payload
            //      .... cannot remember if strings can be sent as rpc
        }

        [TargetRpc(channel = (int)QosType.ReliableSequenced)]
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