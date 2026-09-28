using System;

namespace _Project.Scripts.NetworkLayer.Transport
{
    public interface IClientTransport
    {
        int Latency { get; set; }

        event Action<byte[]> Received;
        event Action Disconnected;

        
        void Send(byte[] bytes);
        void Connect();
        void Disconnect();
    }
}
