using System;
using System.Collections.Generic;
using System.Text;

namespace EntityManager.Consts
{
    public class ALL
    {
        public const int MainPort = 4444;
        public static string ApplicationData_Path
        {
            get
            {
                var str = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/API_Logger/";
                if (!System.IO.Directory.Exists(str)) System.IO.Directory.CreateDirectory(str);
                return str;
            }
        }
    }
}
