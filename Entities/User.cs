
using System.ComponentModel.DataAnnotations.Schema;

namespace GatekeeperApi.Entities
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; private set; }
        public string Ad { get; set; } = default!;
        public string KullaniciAdi { get; set; } = default!;
        public string SifreHash { get; set; } = default!;
        public string Rol { get; set; } = "User";
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshToken> RefreshTokenlar { get; set; } = new List<RefreshToken>();

        public User() { Id = Guid.NewGuid().ToString(); }
        public User(string ad, string kullanici, string role = "User") { 
            Id = Guid.NewGuid().ToString();
            Ad = ad;
            KullaniciAdi = kullanici;
            Rol = role;
        }

        public string SetNewGuid() { Id = Guid.NewGuid().ToString(); return Id; }
    }
}
