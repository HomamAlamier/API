using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EntityManager.DataTypes
{
    public class Message
    {
        public ulong MessageID { get; set; }
        public ulong ReplayMessageID { get; set; }
        public ulong From { get; set; }
        public ulong To { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public byte[] Serialize()
        {
            List<byte> bts = new List<byte>();
            bts.AddRange(BitConverter.GetBytes(MessageID));
            bts.AddRange(BitConverter.GetBytes(ReplayMessageID));
            bts.AddRange(BitConverter.GetBytes(From));
            bts.AddRange(BitConverter.GetBytes(To));
            byte[] tm = Encoding.UTF8.GetBytes(Time.ToString());
            byte[] c = Encoding.UTF8.GetBytes(Content);
            bts.AddRange(BitConverter.GetBytes(tm.Length));
            bts.AddRange(tm);
            bts.AddRange(BitConverter.GetBytes(c.Length));
            bts.AddRange(c);
            return bts.ToArray();
        }
        public static Message Parse(byte[] data)
        {
            Message msg = new Message();
            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Seek(0, SeekOrigin.Begin);
                byte[] tmp = new byte[8];
                ms.Read(tmp, 0, 8);
                msg.MessageID = BitConverter.ToUInt64(tmp);
                tmp = new byte[8];
                ms.Read(tmp, 0, 8);
                msg.ReplayMessageID = BitConverter.ToUInt64(tmp);
                tmp = new byte[8];
                ms.Read(tmp, 0, 8);
                msg.From = BitConverter.ToUInt64(tmp);
                tmp = new byte[8];
                ms.Read(tmp, 0, 8);
                msg.To = BitConverter.ToUInt64(tmp);
                tmp = new byte[4];
                ms.Read(tmp, 0, 4);
                int len = BitConverter.ToInt32(tmp);
                tmp = new byte[len];
                ms.Read(tmp, 0, len);
                msg.Time = DateTime.Parse(Encoding.UTF8.GetString(tmp));
                tmp = new byte[4];
                ms.Read(tmp, 0, 4);
                len = BitConverter.ToInt32(tmp);
                tmp = new byte[len];
                ms.Read(tmp, 0, len);
                msg.Content = Encoding.UTF8.GetString(tmp);
            }
            return msg;
        }
        public override string ToString()
        {
            return $"{'{'}\r\n\tMessageID: {MessageID}\r\n\tReplayMessageID: {ReplayMessageID}\r\n\tFrom: {From}\r\n\t"
                + $"To: {To}\r\n\tTime: {Time.ToString()}\r\n\tContent: {Content}\r\n{'}'}";
        }
    }
}
