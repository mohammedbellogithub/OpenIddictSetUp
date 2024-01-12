using OpenIddictSetUp.Helpers;
using System.ComponentModel;

namespace OpenIddictSetUp.Enums
{
    public enum Permission
    {
        [Category(RoleHelpers.SYS_ADMIN), Description(@"Access All Modules")]
        FULL_CONTROL = 1001,
    }
}
