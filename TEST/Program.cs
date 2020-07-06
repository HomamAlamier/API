using System;
using EntityManager.DataTypes;
using API.Server;
using API.Client;
namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();


            Console.ReadKey();

            Client c = new Client("127.0.0.1");
            c.ConnectedSuccessfully += delegate
            {
                c.SendCommand(new Command(Command.CommandType.Ping, new byte[] { 90 }));
            };
            c.Connect();

        }
    }
}
