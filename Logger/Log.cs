using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
namespace Logger
{
    public class Log : IDisposable
    {
        Queue<string> queue;
        Thread io_manager;
        FileStream fs;
        bool stop = false;
        int lnc = 1;
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
        public void Dispose()
        {
            stop = true;
            io_manager.Abort();
        }
        string lineNum()
        {
            string str = "";
            int max = int.MaxValue.ToString().Length;
            int val = lnc.ToString().Length;
            for (int i = 0; i < max - val; i++)
            {
                str += "0";
            }
            str += lnc.ToString();
            return str;
        }
        private void io_manage(object obj)
        {
            while (!stop)
            {
                if (queue.Count > 0)
                {
                    string str = queue.Dequeue();
                    str = $"[{lineNum()}] in {DateTime.Now.ToString()} : {str}\r\n";
                    byte[] bt = Encoding.UTF8.GetBytes(str);
                    Console.WriteLine(str);
                    lnc++;
                    fs.Write(bt, 0, bt.Length);
                    fs.Flush();
                }
                Thread.Sleep(1000);
            }
        }
    }
}
