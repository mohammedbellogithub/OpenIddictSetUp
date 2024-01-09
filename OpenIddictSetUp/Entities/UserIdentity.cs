using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OpenIddictSetUp.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public AppUser() 
        {
            Id = Guid.NewGuid();
        }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }

    }

    public class AppUserClaims : IdentityUserClaim<Guid>
    {
    }

    public class AppUserLogins : IdentityUserLogin<Guid>
    {
        [Key]
        [Required]
        public int Id { get; set; }
    }

    public class AppRoles : IdentityRole<Guid>
    {
        public AppRoles()
        {
            Id = Guid.NewGuid();
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }
    }

    public class AppUserRoles : IdentityUserRole<Guid>
    {
    }

    public class AppRoleClaims : IdentityRoleClaim<Guid>
    {
    }

    public class AppUserToken : IdentityUserToken<Guid>
    {
    }
}
