using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace OpenIddictSetUp.Entities.Maps
{
    public class AppUserTokenMap : IEntityTypeConfiguration<AppUserToken>
    {
        public void Configure(EntityTypeBuilder<AppUserToken> builder)
        {
            builder.ToTable(nameof(AppUserToken));
            builder.HasKey(b => new { b.UserId, b.LoginProvider, b.Name });
        }
    }
}
