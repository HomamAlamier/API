using System;
using System.Collections.Generic;
using System.Text;

namespace EntityManager.Enums
{
    public enum CreateUserError
    {
        EmailIsNotValid = 0x1,
        TagIsNotValid = 0x10,
        Success = 0x0
    }
    public enum Perm
    {
        NoBody = 0x0,
        FriendOnly = 0x1,
        All = 0x10
    }
    public enum PrivacySetting
    {
        CanGetInfo = 0x0,
        CanSeeProfilePicture = 0x1,
        CanSeeBio = 0x10
    }
}
