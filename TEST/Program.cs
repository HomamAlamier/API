using System;
using EntityManager.DataTypes;
using API.Server;
using API.Client;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using EntityManager.Consts;
using EntityManager.Enums;
namespace TEST
{
    class Program
    {
        static void Test2()
        {
            Server s = new Server(false);
            Logger.Log ll = new Logger.Log("stdout", Directories.Logs_Path);
            List<Client> cc = new List<Client>();
            int clientsCount = 100;
            for (int i = 0; i < clientsCount; i++)
            {
                Client x = new Client("127.0.0.1", false);
                Stopwatch sw = new Stopwatch();
                x.ConnectedSuccessfully += delegate
                {
                    x.CreateUser(new User()
                    {
                        Email = $"test{i}@dev.com",
                        Tag = $"test{i}",
                        Name = $"Test{i}",
                        Password = "111222333"
                    });
                };
                x.CreateUserCallBack += (s, e) =>
                {
                    sw.Stop();
                    ll.WriteLine($"Number = {i}\r\nResult = {e.Error.ToString()}\r\nTime = {sw.ElapsedMilliseconds / 1000.0}s\r\n");

                };

                sw.Start();
                x.Connect();
                cc.Add(x);
                Thread.Sleep(10);
            }
        }
        static void Test1()
        {
             Console.ReadKey();

             Client c = new Client("127.0.0.1");
             c.ConnectedSuccessfully += delegate
             {
                 c.SendCommand(new Command(Command.CommandType.Ping, new byte[] { 90 }));
             };
             c.CreateUserCallBack += (s, e) =>
             {
                 Console.WriteLine($"CreateUserCallBack : ErrorCode = {e.Error.ToString()}.");
                 if (e.Error == CreateUserError.Success)
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
             c.ChangePrivacySetting(PrivacySetting.CanGetInfo, Perm.NoBody);
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
                 if (e.Error == CreateUserError.Success)
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
        static void Test3()
        {
            Server x = new Server();
            Client c = new Client("127.0.0.1");
            User user = new User()
            {
                Email = "test@test.com",
                Password = "12345",
                Name = "test",
                Tag = "@test123"
            };
            c.ConnectedSuccessfully += (s, e) =>
            {
                c.CreateUser(user);
            };
            c.CreateUserCallBack += (s, e) =>
            {
                Console.WriteLine($"CreateUserCallBack => {e.Error.ToString()}");
            };
            c.Connect();
            Console.ReadKey();
            c.ChangePrivacySetting(PrivacySetting.CanGetInfo, Perm.NoBody);
            Console.ReadKey();
            c.Dispose();
            Console.ReadKey();
            c = new Client("127.0.0.1");
            c.ConnectedSuccessfully += delegate
            {
                c.Login(user);
            };
            c.LoginUserCallBack += (s, e) =>
            {
                Console.WriteLine($"LoginUserCallBack => {e.Error.ToString()}");
                Console.WriteLine(c.CurrentUser);
            };
            c.Connect();
            Console.ReadKey();
        }
        static void Main(string[] args)
        {
            Test3();            
        }
    }
}
