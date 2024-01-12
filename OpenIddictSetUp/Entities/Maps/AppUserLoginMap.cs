using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace OpenIddictSetUp.Entities.Maps
{
    public class AppUserLoginMap : IEntityTypeConfiguration<AppUserLogins>
    {
        public void Configure(EntityTypeBuilder<AppUserLogins> builder)
        {
            builder.ToTable(nameof(AppUserLogins));
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();
            builder.HasKey(u => new { u.LoginProvider, u.ProviderKey });
        }
    }
}
