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
            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));
            var packet = new byte[body.Length + 1];
            packet[0] = (byte)MessageType.Snapshot;
            
            for (int i = 0; i < body.Length; i++)
                packet[i + 1] = body[i];
            
            return packet;
        }

        public static MessageType ReadType(byte[] bytes)
        {
            return (MessageType)bytes[0];
        }

        public static SnapshotMessage ReadSnapshot(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes, 1, bytes.Length - 1);
            
            return JsonUtility.FromJson<SnapshotMessage>(json);
        }
    }
}
