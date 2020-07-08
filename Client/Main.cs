using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using Logger;
using EntityManager.Consts;
using EntityManager.DataTypes;
using EntityManager.Enums;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace API.Client
{
    public class CreateUserEventArgs : EventArgs
    {
        public CreateUserError Error { get; private set; }
        public CreateUserEventArgs(CreateUserError err)
        {
            Error = err;
        }
    }
    public class UserInfoReceiveEventArgs : EventArgs
    {
        public User UserInfo { get; private set; }
        public UserInfoReceiveEventArgs(User usr)
        {
            UserInfo = usr;
        }
    }
    public class MessageReceiveEventArgs : EventArgs
    {
        public Message Message { get; private set; }
        public MessageReceiveEventArgs(Message msg)
        {
            Message = msg;
        }
    }
    public class Client
    {

        public event EventHandler ConnectedSuccessfully;
        public event EventHandler<CreateUserEventArgs> CreateUserCallBack;
        public event EventHandler<MessageReceiveEventArgs> MessageReceive;
        public event EventHandler<UserInfoReceiveEventArgs> UserInfoReceive;
        public User CurrentUser => usr;
        public bool Connected => _sock.Connected;

        Socket _sock;
        SslStream _stream;
        Log _log;
        byte[] _buffer;
        string _ip;
        User usr;
        public Client(string ip)
        {
            _ip = ip;
        }
        public void Connect()
        {
            try
            {
                _log = new Log("client", Directories.Logs_Path);
                _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = _sock.BeginConnect(new IPEndPoint(IPAddress.Parse(_ip), ALL.MainPort), null, null);
                bool result = ar.AsyncWaitHandle.WaitOne(3000);
                if (result)
                {
                    _log.WriteLine($"Connected to {_ip} !");
                    _stream = new SslStream(new NetworkStream(_sock, true), false, (e1, e2, e3, e4) => { return true; });
                    _stream.AuthenticateAsClient("localhost");
                    if (_stream.IsAuthenticated)
                    {
                        _log.WriteLine($"SSL Stream : \r\n{'{'}\r\n\tIsAuthenticated: {_stream.IsAuthenticated.ToString()}\r\n\tIsEncrypted: {_stream.IsEncrypted.ToString()}\r\n{'}'}");
                        _buffer = new byte[1024];
                        _stream.BeginRead(_buffer, 0, _buffer.Length, StreamRead, null);
                        ConnectedSuccessfully?.Invoke(this, new EventArgs());
                    }
                }
                else
                {
                    _log.WriteLine("Connection failed after 3 seconds !");
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
            }
        }
        public void CreateUser(User user)
        {
            this.usr = user;
            SendCommand(new Command(Command.CommandType.CreateUser, user.Serialize()));
        }
        public void SendMessage(Message message)
        {
            SendCommand(new Command(Command.CommandType.Message, message.Serialize()));
        }
        public void ChangePrivacySetting(PrivacySetting setting, Perm value)
        {
            List<byte> bts = new List<byte>();
            bts.AddRange(BitConverter.GetBytes((int)setting));
            bts.AddRange(BitConverter.GetBytes((int)value));
            SendCommand(new Command(Command.CommandType.ChangeUserPrivacy, bts.ToArray()));
        }
        public void RequestUserInfo(string tag)
        {
            SendCommand(new Command(Command.CommandType.GetUserInfo, Encoding.UTF8.GetBytes(tag)));
        }
        public void SendCommand(Command cmd)
        {
            try
            {
                if (_sock.Connected && _stream.IsAuthenticated)
                {
                    _stream.Write(cmd.GetData());
                    _stream.Flush();
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
            }
        }
        private void StreamRead(IAsyncResult ar)
        {
            try
            {
                int result = _stream.EndRead(ar);
                if (result > 0)
                {
                    //_log.WriteLine($"Received {result} bytes !");
                    var cmd = Command.Parse(_buffer, 0, result);
                    switch (cmd.CmdType)
                    {
                        case Command.CommandType.Ping:
                            {
                                SendCommand(new Command(Command.CommandType.Ping, new byte[] { 1 }));
                            }
                            break;
                        case Command.CommandType.GetVersion:
                            break;
                        case Command.CommandType.CreateUser:
                            {
                                int errcode = BitConverter.ToInt32(cmd.Data);
                                if (errcode == (int)CreateUserError.Success)
                                {
                                    SendCommand(new Command(Command.CommandType.GetUserInfo, Encoding.UTF8.GetBytes(CurrentUser.Tag)));
                                }
                                CreateUserCallBack?.Invoke(this, new CreateUserEventArgs((CreateUserError)errcode));
                            }
                            break;
                        case Command.CommandType.Message:
                            {
                                Message msg = Message.Parse(cmd.Data);
                                MessageReceive?.Invoke(this, new MessageReceiveEventArgs(msg));
                            }
                            break;
                        case Command.CommandType.GetUserInfo:
                            {
                                User usr = User.Parse(cmd.Data);
                                if (usr.Tag == CurrentUser.Tag)
                                {
                                    this.usr = usr;
                                }
                                UserInfoReceive?.Invoke(this, new UserInfoReceiveEventArgs(usr));
                            }
                            break;
                        case Command.CommandType.ChangeUserPrivacy:
                            {
                                SendCommand(new Command(Command.CommandType.GetUserInfo, Encoding.UTF8.GetBytes(CurrentUser.Tag)));
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
            }
            _stream.BeginRead(_buffer, 0, _buffer.Length, StreamRead, ar.AsyncState);
        }
    }
}
