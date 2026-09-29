using System;
using _Project.Scripts.Configs;
using _Project.Scripts.NetworkLayer.Protocol;
using _Project.Scripts.NetworkLayer.Transport;
using UnityEngine;

namespace _Project.Scripts.NetworkLayer.Client
{
    public class ClientSession
    {
        private readonly IClientTransport _transport;
        private readonly MatchConfig _matchConfig;
        private readonly int _playerId;
        private bool _waiting;
        private bool _connected;

        public int RetransmitMs
        {
            get => _transport.RetransmitMs;
            set => _transport.RetransmitMs = value < 0 ? 0 : value;
        }

        public void SetNetwork(int latencyMs, float dropChance, int retransmitMs)
        {
            Latency = latencyMs;
            DropChance = dropChance;
            RetransmitMs = retransmitMs;
        }

        public int Latency
        {
            get => _transport.Latency;
            set => _transport.Latency = value < 0 ? 0 : value;
        }

        public float DropChance
        {
            get => _transport.DropChance;
            set
            {
                if (value < 0f)
                    value = 0f;
                
                if (value > 1f)
                    value = 1f;

                _transport.DropChance = value;
            }
        }
        public int PlayerId => _playerId;
        public SnapshotMessage Snapshot { get; private set; }
        
        public event Action<SnapshotMessage> SnapshotReceived;
        public event Action OutOfTurn;
        public event Action Disconnected;
        

        public ClientSession(IClientTransport transport, int playerId, MatchConfig matchConfig)
        {
            _transport = transport;
            _playerId = playerId;
            _matchConfig = matchConfig;
            
            _transport.Received += OnReceived;
            _transport.Disconnected += OnDisconnected;
        }

        public void Start()
        {
            Connect();
        }

        public void Connect()
        {
            _transport.Connect();
            _connected = true;
            
            _transport.Send(MessageCodec.EncodeHello());
            
            Log("sent Hello");
        }

        public void Disconnect()
        {
            _transport.Disconnect();
        }

        public bool Shoot(int cell)
        {
            if (!_connected)
                return false;

            if (Snapshot.winner != SnapshotMessage.NoWinner)
                return false;

            if (Snapshot.turn != _playerId)
            {
                OutOfTurn?.Invoke();
                
                return false;
            }

            if (_waiting)
                return false;

            _waiting = true;
            
            _transport.Send(MessageCodec.EncodeShoot(cell));
            
            Log("sent Shoot " + cell);
            
            return true;
        }

        private void OnReceived(byte[] bytes)
        {
            if (MessageCodec.ReadType(bytes) != MessageType.Snapshot)
                return;

            Snapshot = MessageCodec.ReadSnapshot(bytes);
            
            _waiting = false;
            
            Log("received Snapshot turn " + Snapshot.turn + " winner " + Snapshot.winner);
            
            SnapshotReceived?.Invoke(Snapshot);
        }

        private void OnDisconnected()
        {
            _connected = false;
            _waiting = false;
            
            Disconnected?.Invoke();
        }

        private void Log(string message)
        {
            if (!_matchConfig.logEnabled)
                return;

            Debug.Log("Player " + _playerId + " " + message);
        }
    }
}
