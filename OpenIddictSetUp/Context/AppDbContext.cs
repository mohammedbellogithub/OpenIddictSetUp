using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddictSetUp.Entities;
using OpenIddictSetUp.Entities.Maps;

namespace OpenIddictSetUp.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRoles , Guid, AppUserClaims, AppUserRoles, AppUserLogins, AppRoleClaims, AppUserToken>
    {

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AppRoleClaimMap());
            modelBuilder.ApplyConfiguration(new AppRoleMap());
            modelBuilder.ApplyConfiguration(new AppUserMap());
            modelBuilder.ApplyConfiguration(new AppUserRoleMap());
            modelBuilder.ApplyConfiguration(new AppUserLoginMap());
            modelBuilder.ApplyConfiguration(new AppUserTokenMap());
            modelBuilder.ApplyConfiguration(new AppUserClaimMap());
            modelBuilder.UseOpenIddict<AppOpenIddictApplication, AppOpenIddictAuthorization, AppOpenIddictScope, AppOpenIddictToken, Guid>();

        }
    }
}
