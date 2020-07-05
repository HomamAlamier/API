using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
namespace Logger
{
    public class Log
    {
        Queue<string> queue;
        Thread io_manager;
        FileStream fs;
        public Log(string name, string path)
        {
            if (File.Exists(path + name + ".txt")) File.Delete(path + name + ".txt");
            fs = new FileStream(path + name + ".txt", FileMode.OpenOrCreate);
            queue = new Queue<string>();
            io_manager = new Thread(io_manage);
            io_manager.Start();
        }
        public void WriteLine(string line)
        {
            queue.Enqueue(line);
        }
        private void io_manage(object obj)
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    string str = queue.Dequeue();
                    byte[] bt = Encoding.UTF8.GetBytes(str + "\r\n");
                    fs.Write(bt, 0, bt.Length);
                    fs.Flush();
                }
                Thread.Sleep(1000);
            }
        }
    }
}
