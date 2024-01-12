using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenIddictSetUp.Helpers;

namespace OpenIddictSetUp.Entities.Maps
{
    public class AppUserRoleMap : IEntityTypeConfiguration<AppUserRoles>
    {
        public void Configure(EntityTypeBuilder<AppUserRoles> builder)
        {
            builder.ToTable(name: nameof(AppUserRoles));
            builder.HasKey(p => new { p.UserId, p.RoleId });
            SetupData(builder);
        }

        private void SetupData(EntityTypeBuilder<AppUserRoles> builder)
        {
            List<AppUserRoles> dataList = new List<AppUserRoles>()
            {
                new AppUserRoles()
                {
                    UserId = Guid.Parse("96623538-0615-4d01-9023-7352bb4bb9c6"),
                    RoleId = RoleHelpers.SYS_ADMIN_ID(),
                }
            };

            builder.HasData(dataList);
        }
    }
}
