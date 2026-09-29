using System;
using _Project.Scripts.NetworkLayer.Protocol;
using _Project.Scripts.NetworkLayer.Transport;

namespace _Project.Scripts.NetworkLayer.Client
{
    public class ClientSession
    {
        private readonly IClientTransport _transport;
        private readonly int _playerId;
        private bool _waiting;

        public int PlayerId => _playerId;
        public SnapshotMessage Snapshot { get; private set; }
        public event Action<SnapshotMessage> SnapshotReceived;
        public event Action OutOfTurn;
        

        public ClientSession(IClientTransport transport, int playerId)
        {
            _transport = transport;
            _playerId = playerId;
            _transport.Received += OnReceived;
        }

        public void Start()
        {
            _transport.Connect();
            _transport.Send(MessageCodec.EncodeHello());
        }

        public void Shoot(int cell)
        {
            if (Snapshot.winner != SnapshotMessage.NoWinner)
                return;

            if (Snapshot.turn != _playerId)
            {
                OutOfTurn?.Invoke();
                
                return;
            }

            if (_waiting)
                return;

            _waiting = true;
            _transport.Send(MessageCodec.EncodeShoot(cell));
        }

        private void OnReceived(byte[] bytes)
        {
            if (MessageCodec.ReadType(bytes) != MessageType.Snapshot)
                return;

            Snapshot = MessageCodec.ReadSnapshot(bytes);
            _waiting = false;
            
            SnapshotReceived?.Invoke(Snapshot);
        }
    }
}
