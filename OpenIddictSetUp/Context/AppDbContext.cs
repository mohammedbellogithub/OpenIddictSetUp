using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddictSetUp.Entities;
using System.Reflection.Emit;

namespace OpenIddictSetUp.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRoles , Guid, AppUserClaims, AppUserRoles, AppUserLogins, AppRoleClaims, AppUserToken>
    {

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseOpenIddict<AppOpenIddictApplication, AppOpenIddictAuthorization, AppOpenIddictScope, AppOpenIddictToken, Guid>();

        }
    }
}
