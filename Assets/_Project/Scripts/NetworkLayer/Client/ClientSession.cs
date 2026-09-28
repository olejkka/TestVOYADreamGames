using _Project.Scripts.NetworkLayer.Protocol;
using _Project.Scripts.NetworkLayer.Transport;

namespace _Project.Scripts.NetworkLayer.Client
{
    public class ClientSession
    {
        private readonly IClientTransport _transport;

        public ClientSession(IClientTransport transport)
        {
            _transport = transport;
            _transport.Received += OnReceived;
        }

        public SnapshotMessage Snapshot { get; private set; }

        public void Start()
        {
            _transport.Connect();
            _transport.Send(MessageCodec.EncodeHello());
        }

        private void OnReceived(byte[] bytes)
        {
            if (MessageCodec.ReadType(bytes) != MessageType.Snapshot)
                return;

            Snapshot = MessageCodec.ReadSnapshot(bytes);
        }
    }
}
