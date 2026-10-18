using GatekeeperApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace GatekeeperApi.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Kullanicilar => Set<User>();
        public DbSet<RefreshToken> RefreshTokenlar => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Assembly'deki tüm IEntityTypeConfiguration sınıflarını otomatik uygular
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
