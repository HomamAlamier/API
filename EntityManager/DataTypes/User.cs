using EntityManager.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EntityManager.DataTypes
{
    public class UserPrivacy
    {
        public Perm Perm_CanGetInfo { get; set; }
        public Perm Perm_CanSeePicture { get; set; }
        public Perm Perm_CanSeeBio { get; set; }
    }
    public class User
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public ulong ID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ProfilePictureID { get; set; }
        public UserPrivacy Privacy { get; set; }
        public byte[] Serialize()
        {
            List<byte> bts = new List<byte>();
            bts.AddRange(BitConverter.GetBytes(ID));
            if (Privacy == null)
            {
                byte[] tmp = BitConverter.GetBytes(int.MaxValue);
                bts.AddRange(tmp);
                bts.AddRange(tmp);
                bts.AddRange(tmp);
            }
            else
            {
                bts.AddRange(BitConverter.GetBytes((int)Privacy.Perm_CanGetInfo));
                bts.AddRange(BitConverter.GetBytes((int)Privacy.Perm_CanSeePicture));
                bts.AddRange(BitConverter.GetBytes((int)Privacy.Perm_CanSeeBio));
            }
            byte[] emailBytes = Email == null ? new byte[] { 0 } : Encoding.UTF8.GetBytes(Email);
            byte[] passBytes = Password == null ? new byte[] { 0 } : Encoding.UTF8.GetBytes(Password);
            byte[] nameBytes = Name == null ? new byte[] { 0 } : Encoding.UTF8.GetBytes(Name);
            byte[] ppBytes = ProfilePictureID == null ? new byte[] { 0 } : Encoding.UTF8.GetBytes(ProfilePictureID);
            byte[] tagBytes = Tag == null ? new byte[] { 0 } : Encoding.UTF8.GetBytes(Tag);
            bts.AddRange(BitConverter.GetBytes(emailBytes.Length));
            bts.AddRange(emailBytes);
            bts.AddRange(BitConverter.GetBytes(passBytes.Length));
            bts.AddRange(passBytes);
            bts.AddRange(BitConverter.GetBytes(nameBytes.Length));
            bts.AddRange(nameBytes);
            bts.AddRange(BitConverter.GetBytes(ppBytes.Length));
            bts.AddRange(ppBytes);
            bts.AddRange(BitConverter.GetBytes(tagBytes.Length));
            bts.AddRange(tagBytes);
            return bts.ToArray();
        }
        public static User Parse(byte[] data)
        {
            User usr = new User();
            
            using (MemoryStream ms = new MemoryStream(data))
            {
                ms.Seek(0, SeekOrigin.Begin);
                byte[] id = new byte[8];
                ms.Read(id, 0, 8);
                usr.ID = BitConverter.ToUInt64(id);
                byte[] p = new byte[4];
                ms.Read(p, 0, 4);
                if (BitConverter.ToInt32(p) == int.MaxValue)
                {
                    ms.Seek(8, SeekOrigin.Current);
                }
                else
                {
                    usr.Privacy = new UserPrivacy();
                    usr.Privacy.Perm_CanGetInfo = (Perm)BitConverter.ToInt32(p);
                    p = new byte[4];
                    ms.Read(p, 0, 4);
                    usr.Privacy.Perm_CanSeePicture = (Perm)BitConverter.ToInt32(p);
                    p = new byte[4];
                    ms.Read(p, 0, 4);
                    usr.Privacy.Perm_CanSeeBio = (Perm)BitConverter.ToInt32(p);
                }

                //Email
                byte[] lenb = new byte[4];
                ms.Read(lenb, 0, 4);
                int len = BitConverter.ToInt32(lenb);
                byte[] tmp = new byte[len];
                ms.Read(tmp, 0, len);
                usr.Email = Encoding.UTF8.GetString(tmp);

                //Password
                lenb = new byte[4];
                ms.Read(lenb, 0, 4);
                len = BitConverter.ToInt32(lenb);
                tmp = new byte[len];
                ms.Read(tmp, 0, len);
                usr.Password = Encoding.UTF8.GetString(tmp);

                //Name
                lenb = new byte[4];
                ms.Read(lenb, 0, 4);
                len = BitConverter.ToInt32(lenb);
                tmp = new byte[len];
                ms.Read(tmp, 0, len);
                usr.Name = Encoding.UTF8.GetString(tmp);

                //ProfilePictureID
                lenb = new byte[4];
                ms.Read(lenb, 0, 4);
                len = BitConverter.ToInt32(lenb);
                tmp = new byte[len];
                ms.Read(tmp, 0, len);
                usr.ProfilePictureID = Encoding.UTF8.GetString(tmp);

                //Tag
                lenb = new byte[4];
                ms.Read(lenb, 0, 4);
                len = BitConverter.ToInt32(lenb);
                tmp = new byte[len];
                ms.Read(tmp, 0, len);
                usr.Tag = Encoding.UTF8.GetString(tmp);
            }
            return usr;
        }
        public override string ToString()
        {
            return $"{'{'}\r\n\tName: {Name}\r\n\tEmail: {Email}\r\n\tPassword: {Password}\r\n\tTag: {Tag}\r\n\tProfilePictureID: {ProfilePictureID}\r\n\tID: {ID}\r\n\t" +
                (Privacy != null ? ($"Privacy: \r\n\t{'{'}\r\n\t\tPerm_CanGetInfo: {Privacy.Perm_CanGetInfo.ToString()}\r\n\t\tPerm_CanSeePicture: {Privacy.Perm_CanSeePicture.ToString()}" +
                $"\r\n\t\tPerm_CanSeeBio: {Privacy.Perm_CanSeeBio.ToString()}\r\n\t{'}'}") : "") +
                $"\r\n{'}'}";
        }
    }
}
