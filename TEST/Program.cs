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
            c.MessageReceive += (s, e) =>
            {
                Console.WriteLine(e.Message);
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
            Client c2 = new Client("127.0.0.1");
            ulong id = 0;
            c2.ConnectedSuccessfully += delegate
            {
                c2.SendCommand(new Command(Command.CommandType.Ping, new byte[] { 90 }));
            };
            c2.CreateUserCallBack += (s, e) =>
            {
                Console.WriteLine($"CreateUserCallBack : ErrorCode = {e.Error.ToString()}.");
                if (e.Error == EntityManager.Enums.CreateUserError.Success)
                {
                    Console.WriteLine(c2.CurrentUser);
                }
            };
            c2.UserInfoReceive += (s, e) =>
            {
                id = e.UserInfo.ID;
                Console.WriteLine(e.UserInfo);
            };
            c2.MessageReceive += (s, e) =>
            {
                Console.WriteLine(e.Message);
            };
            c2.Connect();
            Console.ReadKey();
            if (c2.Connected)
            {
                c2.CreateUser(new User()
                {
                    Email = "test2@dev.com",
                    Password = "12345",
                    Name = "amr",
                    Tag = "@amr222"
                });
            }
            Console.ReadKey();
            c2.RequestUserInfo("@amr123");
            Console.ReadKey();
            c2.SendMessage(new Message()
            {
                From = c2.CurrentUser.ID,
                To = id,
                MessageID = 11,
                ReplayMessageID = 0,
                Time = DateTime.Now,
                Content = "Hello World !"
            });
            Console.ReadKey();
        }
    }
}
