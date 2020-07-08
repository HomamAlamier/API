using System;
using System.Collections.Generic;
using System.Text;

namespace EntityManager.DataTypes
{
    public class Command
    {
        public enum CommandType
        {
            Ping              =  0x1,
            GetVersion        =  0x10,
            CreateUser        =  0x11,
            LoginUser         =  0x110,
            GetUserInfo       =  0x111,
            ChangeUserPrivacy =  0x1110,
            Message           =  0x1111
        }
        public CommandType CmdType => ctype;
        public byte[] Data => data;

        CommandType ctype;
        byte[] data;
        public Command(CommandType type)
        {
            ctype = type;
        }
        public Command(CommandType type, byte[] data)
        {
            ctype = type;
            this.data = data;
        }
        public byte[] GetData()
        {
            byte[] bt = new byte[data.Length + 8];
            byte[] bt2 = BitConverter.GetBytes((int)ctype);
            Array.Copy(bt2, 0, bt, 0, 4);
            bt2 = BitConverter.GetBytes(data.Length);
            Array.Copy(bt2, 0, bt, 4, 4);
            Array.Copy(data, 0, bt, 8, data.Length);
            return bt;
        }
        public static Command Parse(byte[] data, int startindex, int count)
        {
            byte[] bt = new byte[count];
            Array.Copy(data, startindex, bt, 0, count);
            return Parse(bt);
        }
        public static Command Parse(byte[] data)
        {
            CommandType type = (CommandType)BitConverter.ToInt32(data, 0);
            int len = BitConverter.ToInt32(data, 4);
            byte[] dd = new byte[len];
            Array.Copy(data, 8, dd, 0, len);
            return new Command(type, dd);
        }
        public override string ToString()
        {
            return $"CommandType: {ctype.ToString()}\r\nData: {Encoding.UTF8.GetString(data)}\r\n";
        }
    }
}
