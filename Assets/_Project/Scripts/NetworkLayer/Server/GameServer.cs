using _Project.Scripts.NetworkLayer.Protocol;
using _Project.Scripts.NetworkLayer.Transport;

namespace _Project.Scripts.NetworkLayer.Server
{
    public class GameServer
    {
        private readonly IServerTransport _transport;
        private readonly InProcessHub _hub;
        private readonly Match _match;
        
        private readonly bool[] _seated = new bool[2];


        public GameServer(IServerTransport transport, InProcessHub hub, Match match)
        {
            _transport = transport;
            _hub = hub;
            _match = match;
            
            _transport.Received += OnReceived;
            _transport.Disconnected += OnDisconnected;
        }

        public void Tick(long nowMs)
        {
            if (_match.ExpireAbsence(nowMs) || _match.ExpireTurn(nowMs))
            {
                SendSnapshot(0);
                SendSnapshot(1);
            }
        }

        private void OnReceived(int connectionId, byte[] bytes)
        {
            MessageType type = MessageCodec.ReadType(bytes);
            
            if (type == MessageType.Hello)
            {
                OnHello(connectionId);
                return;
            }

            if (type != MessageType.Shoot)
                return;

            int cell = MessageCodec.ReadShoot(bytes);
            
            if (!_match.TryShoot(connectionId, cell))
                return;

            _match.StartTurn(_hub.NowMs);
            SendSnapshot(0);
            SendSnapshot(1);
        }

        private void OnDisconnected(int connectionId)
        {
            _match.MarkAbsent(connectionId, _hub.NowMs);
        }

        private void OnHello(int connectionId)
        {
            _match.MarkPresent(connectionId);
            bool wasReady = _seated[0] && _seated[1];
            _seated[connectionId] = true;

            if (!_seated[0] || !_seated[1])
                return;

            _match.Place();

            if (!wasReady)
            {
                _match.StartTurn(_hub.NowMs);
                SendSnapshot(0);
                SendSnapshot(1);
                
                return;
            }

            SendSnapshot(connectionId);
        }

        private void SendSnapshot(int connectionId)
        {
            _transport.Send(connectionId, MessageCodec.EncodeSnapshot(_match.SnapshotFor(connectionId)));
        }
    }
}
