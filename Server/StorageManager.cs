using EntityManager.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;
namespace API.Server
{
    public class StorageManager
    {
        List<User> users;
        List<List<Message>> msgs;
        public StorageManager()
        {
            users = new List<User>();
            msgs = new List<List<Message>>();
        }
        public int StoreUser(User usr)
        {
            usr.ID = (ulong)users.Count;
            users.Add(usr);
            msgs.Add(new List<Message>());
            return users.Count - 1;
        }
        public Message[] GetUserMessage(ulong id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].ID == id)
                {
                    return msgs[i].ToArray();
                }
            }
            return null;
        }
        public void StoreMessage(Message msg)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].ID == msg.To)
                {
                    msgs[i].Add(msg);
                    break;
                }
            }
        }
        public User GetUser(string tag)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Tag == tag)
                {
                    return users[i];
                }
            }
            return null;
        }
        public User GetUser(ulong id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].ID == id)
                {
                    return users[i];
                }
            }
            return null;
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
