using GatekeeperApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GatekeeperApi.Infrastructure.Persistence.Configurations
{
    public class KullaniciConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            // This method requires Microsoft.EntityFrameworkCore.Relational package
            b.ToTable("Kullanicilar");
            b.HasKey(x => x.Id);
            // Tells EF Core to generate a unique string value on insert
            b.Property(u => u.Id).ValueGeneratedOnAdd();

            b.Property(x => x.Ad).IsRequired().HasMaxLength(100);
            b.Property(x => x.KullaniciAdi).IsRequired().HasMaxLength(50);
            b.Property(x => x.SifreHash).IsRequired().HasMaxLength(500);
            b.Property(x => x.Rol).IsRequired().HasMaxLength(30);

            b.HasIndex(x => x.KullaniciAdi).IsUnique();   // aynı kullanıcı adı iki kez olamaz
        }

    }
}
