using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using EntityManager.Consts;
using Logger;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace API.Server
{
    public class Server
    {
        Socket _sock;
        SslStream _stream;
        Log log;
        X509Certificate _cert;
        public Server()
        {
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.Bind(new IPEndPoint(IPAddress.Any, ALL.MainPort));
            _sock.Listen(5);
            _sock.BeginAccept(BeginAccept, null);
            log = new Log("serverlog", ALL.ApplicationData_Path);
        }

        private void BeginAccept(IAsyncResult ar)
        {
            try
            {
                Socket client = _sock.EndAccept(ar);
                log.WriteLine($"Accepting connection ({(client.RemoteEndPoint as IPEndPoint).ToString()})");
                log.WriteLine("Creating SSL Stream..");
                _stream = new SslStream(new NetworkStream(_sock, true));
                _stream.AuthenticateAsServer(_cert, false, false);
                if (_stream.IsAuthenticated)
                {
                    byte[] buffer = new byte[1024];
                    _stream.BeginRead(buffer, 0, buffer.Length, StreamRead, buffer);
                }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex.ToString());
            }
        }

        private void StreamRead(IAsyncResult ar)
        {
            try
            {

            }
            catch (Exception ex)
            {
                log.WriteLine(ex.ToString());
            }
        }
    }
}
