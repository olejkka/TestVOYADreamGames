using System.Text;
using UnityEngine;

namespace _Project.Scripts.NetworkLayer.Protocol
{
    public static class MessageCodec
    {
        public static byte[] EncodeHello()
        {
            return new[] { (byte)MessageType.Hello };
        }

        public static byte[] EncodeSnapshot(SnapshotMessage message)
        {
            return Encode(MessageType.Snapshot, JsonUtility.ToJson(message));
        }

        public static byte[] EncodeShoot(int cell)
        {
            return Encode(MessageType.Shoot, JsonUtility.ToJson(new ShootMessage { cell = cell }));
        }

        public static MessageType ReadType(byte[] bytes)
        {
            return (MessageType)bytes[0];
        }

        public static SnapshotMessage ReadSnapshot(byte[] bytes)
        {
            return JsonUtility.FromJson<SnapshotMessage>(ReadJson(bytes));
        }

        public static int ReadShoot(byte[] bytes)
        {
            return JsonUtility.FromJson<ShootMessage>(ReadJson(bytes)).cell;
        }

        private static byte[] Encode(MessageType type, string json)
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            
            var packet = new byte[body.Length + 1];
            packet[0] = (byte)type;

            for (int i = 0; i < body.Length; i++)
                packet[i + 1] = body[i];

            return packet;
        }

        private static string ReadJson(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 1, bytes.Length - 1);
        }
    }
}
