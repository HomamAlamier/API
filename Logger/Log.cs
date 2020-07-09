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
        bool enabled;
        string name;
        public Log(string name, string path, bool enabled = true)
        {
            if (enabled)
            {
                int x = 0;
                while (File.Exists(path + name + x + ".txt"))
                {
                    x++;
                }
                fs = new FileStream(path + name + x + ".txt", FileMode.OpenOrCreate);
                queue = new Queue<string>();
                io_manager = new Thread(io_manage);
                io_manager.Start();
                this.name = name;
            }
            this.enabled = enabled;
        }
        public void WriteLine(string line)
        {
            if (enabled)
                queue.Enqueue($"[{lineNum()}, {name}] : {line}\r\n");
        }
        public void WriteLineInfo(string line)
        {
            if (enabled)
                queue.Enqueue($"[{lineNum()}, {name}] INFO:: {line}\r\n");
        }
        public void WriteLineError(Exception e)
        {
            if (enabled)
                queue.Enqueue($"[{lineNum()}, {name}] ERROR:: {e.Message}\r\n");
        }
        public void WriteLineError(string e)
        {
            if (enabled)
                queue.Enqueue($"[{lineNum()}, {name}] ERROR:: {e}\r\n");
        }
        public void Dispose()
        {
            stop = true;
            io_manager.Join();
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
            lnc++;
            return str;
        }
        private void io_manage(object obj)
        {
            while (!stop)
            {
                if (queue.Count > 0)
                {
                    string str = queue.Dequeue();
                    byte[] bt = Encoding.UTF8.GetBytes(str);
                    Console.WriteLine(str);
                    fs.Write(bt, 0, bt.Length);
                    fs.Flush();
                }
                Thread.Sleep(1000);
            }
        }
    }
}
