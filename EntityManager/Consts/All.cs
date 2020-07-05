using System;
using System.Collections.Generic;
using System.Text;

namespace EntityManager.Consts
{
    public class ALL
    {
        public const int MainPort = 4444;
        public static string ApplicationData_Path => System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}
