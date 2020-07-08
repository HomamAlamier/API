using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using EntityManager.Consts;
using Logger;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using EntityManager.DataTypes;
using System.Security.Authentication;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EntityManager.Enums;
using System.IO;

namespace API.Server
{
    public struct Session
    {
        public ulong ID;
        public ulong uID;
        public ulong Time;
        public bool Logined;
        public SslStream Stream;
        public Socket Socket;
    }
    public class Server
    {
        Socket _sock;
        Log _log;
        X509Certificate2 _cert;
        List<Session> sessions;
        StorageManager store;
        Thread pingThread;
        public Server()
        {
            _log = new Log("serverlog", Directories.Logs_Path);
            sessions = new List<Session>();
            try
            {
                _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sock.Bind(new IPEndPoint(IPAddress.Any, ALL.MainPort));
                _sock.Listen(5);
                _log.WriteLine($"Listening on port {ALL.MainPort}");
                _sock.BeginAccept(BeginAccept, null);
                _cert = new X509Certificate2(@"server.pfx", "password");
                pingThread = new Thread(Handle_Ping);
                pingThread.Start();
                store = new StorageManager();
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
                _log.Dispose();
            }
        }

        private void Handle_Ping()
        {
            while (true)
            {
                for (int i = 0; i < sessions.Count; i++)
                {
                    if (sessions[i].Time == 0)
                    {
                        _log.WriteLine($"Connection ({(sessions[i].Socket.RemoteEndPoint as IPEndPoint).ToString()}) timed out ! disconnecting...");
                        sessions[i].Stream.Close();
                        sessions[i].Socket.Close();
                        sessions.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        sendCommand(new Command(Command.CommandType.Ping, new byte[] { 0 })
                                    , sessions[i].Stream);
                        var s = sessions[i];
                        s.Time--;
                        sessions[i] = s;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        void sendCommand(Command cmd, SslStream stream)
        {
            try
            {
                stream.Write(cmd.GetData());
                stream.Flush();
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
            }
        }
        private void BeginAccept(IAsyncResult ar)
        {
            try
            {
                Socket client = _sock.EndAccept(ar);
                _log.WriteLine($"Accepting connection ({(client.RemoteEndPoint as IPEndPoint).ToString()})");
                _log.WriteLine("Creating SSL Stream..");
                SslStream _stream = new SslStream(new NetworkStream(client, true));
                _stream.AuthenticateAsServer(_cert, false, SslProtocols.Tls12, false);
                if (_stream.IsAuthenticated)
                {
                    _log.WriteLine($"SSL Stream : \r\n{'{'}\r\n\tIsAuthenticated: {_stream.IsAuthenticated.ToString()}\r\n\tIsEncrypted: {_stream.IsEncrypted.ToString()}\r\n{'}'}");
                    byte[] buffer = new byte[1024];
                    sessions.Add(new Session()
                    {
                        ID = 0,
                        uID = 0,
                        Stream = _stream,
                        Socket = client,
                        Logined = false
                    });
                    _stream.BeginRead(buffer, 0, buffer.Length, StreamRead, new object[] { _stream, buffer, sessions.Count - 1 });
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
            }
            _sock.BeginAccept(BeginAccept, null);
        }

        private void StreamRead(IAsyncResult ar)
        {
            try
            {
                SslStream _stream = (SslStream)((object[])ar.AsyncState)[0];
                byte[] buffer = (byte[])((object[])ar.AsyncState)[1];
                int sIndex = (int)((object[])ar.AsyncState)[2];
                int result = _stream.EndRead(ar);
                //_log.WriteLine($"Reading {result} bytes !");
                if (result != 0)
                {
                    var cmd = Command.Parse(buffer, 0, result);
                    switch (cmd.CmdType)
                    {
                        case Command.CommandType.Ping:
                            {
                                var s = sessions[sIndex];
                                s.Time = 20;
                                sessions[sIndex] = s;
                            }
                            break;
                        case Command.CommandType.GetVersion:
                            break;
                        case Command.CommandType.CreateUser:
                            {
                                User usr = User.Parse(cmd.Data);
                                switch (store.IsValidUser(usr))
                                {
                                    case 0:
                                        usr.Privacy = new UserPrivacy()
                                        {
                                            Perm_CanGetInfo = Perm.All,
                                            Perm_CanSeeBio = Perm.All,
                                            Perm_CanSeePicture = Perm.All
                                        };
                                        int u = store.StoreUser(usr);
                                        var s = sessions[sIndex];
                                        s.uID = (ulong)u;
                                        s.Logined = true;
                                        sessions[sIndex] = s;
                                        sendCommand(new Command(Command.CommandType.CreateUser
                                            , BitConverter.GetBytes((int)CreateUserError.Success)), _stream);
                                        break;
                                    case -1:
                                        sendCommand(new Command(Command.CommandType.CreateUser
                                            , BitConverter.GetBytes((int)CreateUserError.EmailIsNotValid)), _stream);
                                        break;
                                    case -2:
                                        sendCommand(new Command(Command.CommandType.CreateUser
                                            , BitConverter.GetBytes((int)CreateUserError.TagIsNotValid)), _stream);
                                        break;
                                }
                            }
                            break;
                        case Command.CommandType.Message:
                            {
                                Message msg = Message.Parse(cmd.Data);
                                if (msg.From != sessions[sIndex].uID || !sessions[sIndex].Logined) break;
                                bool found = false;
                                foreach (var item in sessions)
                                {
                                    if (item.Logined && item.uID == msg.To)
                                    {
                                        sendCommand(new Command(Command.CommandType.Message, cmd.Data), item.Stream);
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    store.StoreMessage(msg);
                                }
                            }
                            break;
                        case Command.CommandType.GetUserInfo:
                            {
                                string tag = Encoding.UTF8.GetString(cmd.Data);
                                User usr = store.GetUser(tag);
                                if (usr != null)
                                {
                                    if (usr.ID == sessions[sIndex].uID)
                                    {
                                        sendCommand(new Command(Command.CommandType.GetUserInfo, usr.Serialize()), _stream);
                                    }
                                    else
                                    {
                                        User tmp = new User();
                                        if (usr.Privacy.Perm_CanGetInfo == Perm.All)
                                        {
                                            tmp.Name = usr.Name;
                                            tmp.ProfilePictureID = usr.ProfilePictureID;
                                        }
                                        tmp.ID = usr.ID;
                                        tmp.Tag = usr.Tag;
                                        sendCommand(new Command(Command.CommandType.GetUserInfo, tmp.Serialize()), _stream);
                                    }
                                }
                            }
                            break;
                        case Command.CommandType.ChangeUserPrivacy:
                            {
                                PrivacySetting setting;
                                Perm value;
                                using (MemoryStream ms = new MemoryStream(cmd.Data))
                                {
                                    ms.Seek(0, SeekOrigin.Begin);
                                    byte[] tmp = new byte[4];
                                    ms.Read(tmp, 0, 4);
                                    setting = (PrivacySetting)BitConverter.ToInt32(tmp);
                                    tmp = new byte[4];
                                    ms.Read(tmp, 0, 4);
                                    value = (Perm)BitConverter.ToInt32(tmp);
                                }
                                User usr = store.GetUser(sessions[sIndex].ID);
                                switch (setting)
                                {
                                    case PrivacySetting.CanGetInfo:
                                        usr.Privacy.Perm_CanGetInfo = value;
                                        break;
                                    case PrivacySetting.CanSeeProfilePicture:
                                        usr.Privacy.Perm_CanSeePicture = value;
                                        break;
                                    case PrivacySetting.CanSeeBio:
                                        usr.Privacy.Perm_CanSeeBio = value;
                                        break;
                                }
                                sendCommand(new Command(Command.CommandType.ChangeUserPrivacy, new byte[1] { 1 }), _stream);
                            }
                            break;
                    }
                }
                _stream.BeginRead(buffer, 0, buffer.Length, StreamRead, ar.AsyncState);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
            }
        }
    }
}
