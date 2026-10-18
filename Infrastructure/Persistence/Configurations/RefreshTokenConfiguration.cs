using GatekeeperApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GatekeeperApi.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("RefreshTokenlar");
            b.HasKey(x => x.Id);

            b.Property(x => x.TokenHash).IsRequired().HasMaxLength(64);   // SHA-256 hex = 64 karakter
            b.Property(x => x.YerineGecenHash).HasMaxLength(64);

            b.HasIndex(x => x.TokenHash).IsUnique();                      // aramalar hash ile yapılır

            b.HasOne(x => x.Kullanici)
             .WithMany(k => k.RefreshTokenlar)
             .HasForeignKey(x => x.KullaniciId)
             .OnDelete(DeleteBehavior.Cascade);                           // kullanıcı silinince tokenları da silinir

            b.Ignore(x => x.Aktif);                                       // hesaplanan özellik, kolon değil
        }
    }
}
