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
    public class LoginUserEventArgs : EventArgs
    {
        public LoginUserError Error { get; private set; }
        public LoginUserEventArgs(LoginUserError err)
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
    public class Client : IDisposable
    {

        public event EventHandler ConnectedSuccessfully;
        public event EventHandler<CreateUserEventArgs> CreateUserCallBack;
        public event EventHandler<MessageReceiveEventArgs> MessageReceive;
        public event EventHandler<UserInfoReceiveEventArgs> UserInfoReceive;
        public event EventHandler<LoginUserEventArgs> LoginUserCallBack;
        public User CurrentUser => usr;
        public bool Connected => _sock.Connected;

        Socket _sock;
        SslStream _stream;
        Log _log;
        byte[] _buffer;
        string _ip;
        User usr;
        bool logEn;
        public Client(string ip, bool log = true)
        {
            _ip = ip;
            logEn = log;
        }
        public void Dispose()
        {
            if (_sock != null && _sock.Connected)
            {
                _stream.Close();
                _sock.Close();
            }
            _log.Dispose();
        }
        public void Connect()
        {
            try
            {
                _log = new Log("client", Directories.Logs_Path, logEn);
                _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = _sock.BeginConnect(new IPEndPoint(IPAddress.Parse(_ip), ALL.MainPort), null, null);
                bool result = ar.AsyncWaitHandle.WaitOne(3000);
                if (result)
                {
                    _log.WriteLineInfo($"Connected to {_ip} !");
                    _stream = new SslStream(new NetworkStream(_sock, true), false, (e1, e2, e3, e4) => { return true; });
                    _stream.AuthenticateAsClient("localhost");
                    if (_stream.IsAuthenticated)
                    {
                        _log.WriteLineInfo($"SSL Stream : \r\n{'{'}\r\n\tIsAuthenticated: {_stream.IsAuthenticated.ToString()}\r\n\tIsEncrypted: {_stream.IsEncrypted.ToString()}\r\n{'}'}");
                        _buffer = new byte[1024];
                        _stream.BeginRead(_buffer, 0, _buffer.Length, StreamRead, null);
                        ConnectedSuccessfully?.Invoke(this, new EventArgs());
                    }
                }
                else
                {
                    _log.WriteLineError("Connection failed after 3 seconds !");
                }
            }
            catch (Exception ex)
            {
                _log.WriteLineError(ex);
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
        public void Login()
        {
            Login(usr);
        }
        public void Login(User usr)
        {
            User tmp = new User()
            {
                Email = usr.Email,
                Password = usr.Password
            };
            SendCommand(new Command(Command.CommandType.LoginUser, tmp.Serialize()));
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
                _log.WriteLineError(ex);
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
                        case Command.CommandType.LoginUser:
                            {
                                int errcode = BitConverter.ToInt32(cmd.Data, 0);
                                if (errcode == (int)LoginUserError.Success)
                                {
                                    byte[] tmp = new byte[cmd.Data.Length - 4];
                                    Array.Copy(cmd.Data, 4, tmp, 0, tmp.Length);
                                    User usr = User.Parse(tmp);
                                    this.usr = usr;
                                }
                                LoginUserCallBack?.Invoke(this, new LoginUserEventArgs((LoginUserError)errcode));
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteLineError(ex);
            }
            try
            {
                if (!ar.CompletedSynchronously)
                    _stream.BeginRead(_buffer, 0, _buffer.Length, StreamRead, null);
            }
            catch (Exception ex)
            {
                _log.WriteLineError(ex);
            }
        }
    }
}
