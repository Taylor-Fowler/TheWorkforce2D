using System.Collections.Generic;
using UnityEngine.Networking;

namespace TheWorkforce.Network
{
    public class NetworkClientControllers : List<ClientControllers>
    {
        public NetworkClientControllers() : base() { }
        public NetworkClientControllers(IEnumerable<ClientControllers> collection) : base(collection) { }
        public NetworkClientControllers(int capacity) : base(capacity) { }

        public bool Remove(NetworkConnection conn)
        {
            return RemoveAll(clientControllers => clientControllers.Connection == conn) > 0;
        }
    }

    public class ClientControllers
    {
        public readonly NetworkConnection Connection;
        public readonly PlayerController PlayerController;
        public readonly WorldController WorldController;
        public readonly FileTransfer FileTransfer;

        public ClientControllers(NetworkConnection connection, PlayerController playerController, WorldController worldController, FileTransfer fileTransfer)
        {
            this.Connection = connection;
            this.PlayerController = playerController;
            this.WorldController = worldController;
            this.FileTransfer = fileTransfer;
        }
    }
}
