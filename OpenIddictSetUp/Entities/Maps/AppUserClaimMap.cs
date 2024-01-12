using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace OpenIddictSetUp.Entities.Maps
{
    public class AppUserClaimMap : IEntityTypeConfiguration<AppUserClaims>
    {
        public void Configure(EntityTypeBuilder<AppUserClaims> builder)
        {
            builder.ToTable(nameof(AppUserClaims));
            builder.HasKey(c => c.Id);
        }
    }
}
