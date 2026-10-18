using System.ComponentModel.DataAnnotations.Schema;

namespace GatekeeperApi.Entities
{
    public class RefreshToken
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; init; }
        public string KullaniciId { get; set; }
        public string TokenHash { get; set; } = default!;
        public DateTime BitisTarihi { get; set; }
        public DateTime? IptalTarihi { get; set; }
        public string? YerineGecenHash { get; set; }

        public User Kullanici { get; set; } = default!;

        public bool Aktif => IptalTarihi is null && BitisTarihi > DateTime.UtcNow;
        
        public RefreshToken()
        {
            Id = Guid.NewGuid().ToString();
        }

        public RefreshToken(string kullaniciId, DateTime bitisTarihi, DateTime? iptalTarihi, string? yerineGecenHash, User? kullanici)
        { 
            Id = Guid.NewGuid().ToString();
            KullaniciId = kullaniciId;
            BitisTarihi = bitisTarihi;
            if (iptalTarihi.HasValue) IptalTarihi = iptalTarihi.Value;
            if (yerineGecenHash is not null) YerineGecenHash = yerineGecenHash;
            if (kullanici is not null) Kullanici = kullanici;
        }

    }
}
