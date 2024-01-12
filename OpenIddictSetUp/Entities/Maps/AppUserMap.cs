using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace OpenIddictSetUp.Entities.Maps
{
    public class AppUserMap : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable(name: nameof(AppUser));
            SetupAdmin(builder);
        }

        public PasswordHasher<AppUser> Hasher { get; set; } = new PasswordHasher<AppUser>();

        private void SetupAdmin(EntityTypeBuilder<AppUser> builder)
        {
            var adminUser = new AppUser
            {
                FirstName = "JOHN",
                LastName = "DOE",
                Id = Guid.Parse("96623538-0615-4d01-9023-7352bb4bb9c6"),
                Email = "johndoe@gmail.com",
                EmailConfirmed = true,
                NormalizedEmail = "JOHNDOE@GMAIL.COM",
                PhoneNumber = "2349058920",
                UserName = "johndoe",
                NormalizedUserName = "JOHNDOE",
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = true,
                PasswordHash = Hasher.HashPassword(null, "grantAccess"),
                SecurityStamp = "d2db0156-280e-4867-9795-8303362024dd",
            };

            builder.HasData(adminUser);
        }

    }
}
