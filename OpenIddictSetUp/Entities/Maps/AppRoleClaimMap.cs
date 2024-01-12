using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddictSetUp.Enums;
using OpenIddictSetUp.Helpers;

namespace OpenIddictSetUp.Entities.Maps
{
    public class AppRoleClaimMap : IEntityTypeConfiguration<AppRoleClaims>
    {
        private static int counter = 0;
        public void Configure(EntityTypeBuilder<AppRoleClaims> builder)
        {
            builder.ToTable(nameof(AppRoleClaims));
            SetupData(builder);
        }

        private void SetupData(EntityTypeBuilder<AppRoleClaims> builder)
        {
            var roleDictionary = new Dictionary<string, Guid>()
            {
                {RoleHelpers.SYS_ADMIN, RoleHelpers.SYS_ADMIN_ID()},
            };

            var permission = (Permission[])Enum.GetValues(typeof(Permission));

            Array.ForEach(permission, p =>
            {
                if (!string.IsNullOrWhiteSpace(p.GetPermissionCategory()) || roleDictionary.ContainsKey(p.GetPermissionCategory()))
                {
                    builder.HasData(new AppRoleClaims()
                    {
                        Id = ++counter,
                        RoleId = roleDictionary[p.GetPermissionCategory()],
                        ClaimType = nameof(Permission),
                        ClaimValue = p.ToString(),
                    });
                }
            });
        }
    }
}
