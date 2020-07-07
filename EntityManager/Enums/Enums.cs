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
}
