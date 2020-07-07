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
            c.CreateUserCallBack += (s, e) =>
            {
                Console.WriteLine($"CreateUserCallBack : ErrorCode = {e.Error.ToString()}.");
                if (e.Error == EntityManager.Enums.CreateUserError.Success)
                {
                    Console.WriteLine(c.CurrentUser);
                }
            };
            c.Connect();
            Console.ReadKey();
            if (c.Connected)
            {
                c.CreateUser(new User()
                {
                    Email = "test@dev.com",
                    Password = "12345",
                    Name = "amr",
                    Tag = "@amr123"
                });
            }
            Console.ReadKey();
            if (c.Connected)
            {
                c.CreateUser(new User()
                {
                    Email = "test@dev.com",
                    Password = "12345",
                    Name = "amr",
                    Tag = "@amr123"
                });
            }

        }
    }
}
