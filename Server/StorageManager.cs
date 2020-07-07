using EntityManager.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;
namespace API.Server
{
    public class StorageManager
    {
        List<User> users;
        public StorageManager()
        {
            users = new List<User>();
        }
        public void StoreUser(User usr)
        {
            users.Add(usr);
        }
        public int IsValidUser(User usr)
        {
            foreach (var item in users)
            {
                if (item.Email == usr.Email)
                {
                    return -1;
                }
                else if (item.Tag == usr.Tag)
                {
                    return -2;
                }
            }
            return 0;
        }
    }
}
