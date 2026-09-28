using System;

namespace _Project.Scripts.NetworkLayer.Transport
{
    public interface IServerTransport
    {
        event Action<int, byte[]> Received;
        event Action<int> Connected;
        event Action<int> Disconnected;

        
        void Send(int connectionId, byte[] bytes);
    }
}
