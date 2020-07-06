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

namespace API.Server
{
    public class Server
    {
        Socket _sock;
        SslStream _stream;
        Log _log;
        X509Certificate2 _cert;
        public Server()
        {
            _log = new Log("serverlog", Directories.Logs_Path);
            try
            {
                _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sock.Bind(new IPEndPoint(IPAddress.Any, ALL.MainPort));
                _sock.Listen(5);
                _log.WriteLine($"Listening on port {ALL.MainPort}");
                _sock.BeginAccept(BeginAccept, null);
                _cert = new X509Certificate2(@"server.pfx", "password");
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
                _log.Dispose();
            }
        }

        private void BeginAccept(IAsyncResult ar)
        {
            try
            {
                Socket client = _sock.EndAccept(ar);
                _log.WriteLine($"Accepting connection ({(client.RemoteEndPoint as IPEndPoint).ToString()})");
                _log.WriteLine("Creating SSL Stream..");
                _stream = new SslStream(new NetworkStream(client, true));
                _stream.AuthenticateAsServer(_cert, false, SslProtocols.Tls12, false);
                if (_stream.IsAuthenticated)
                {
                    _log.WriteLine($"SSL Stream : \r\n{'{'}\r\n\tIsAuthenticated: {_stream.IsAuthenticated.ToString()}\r\n\tIsEncrypted: {_stream.IsEncrypted.ToString()}\r\n{'}'}");
                    byte[] buffer = new byte[1024];
                    _stream.BeginRead(buffer, 0, buffer.Length, StreamRead, buffer);
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
                byte[] buffer = (byte[])ar.AsyncState;
                int result = _stream.EndRead(ar);
                _log.WriteLine($"Reading {result} bytes !");
                if (result != 0)
                {
                    var cmd = Command.Parse(buffer, 0, result);
                    _log.WriteLine(cmd.ToString());
                }
                _stream.BeginRead(buffer, 0, buffer.Length, StreamRead, buffer);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.ToString());
            }
        }
    }
}
