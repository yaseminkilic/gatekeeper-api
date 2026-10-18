using System.ComponentModel.DataAnnotations;

namespace GatekeeperApi.Entities
{
    public class LoginRequest
    {
        [Required] public string KullaniciAdi { get; set; } = default!;
        [Required] public string Sifre { get; set; } = default!;
    }

    public record LoginResponse(string AuthToken, int ExpiresIn);

    public record KullaniciDto(string Id, string Ad);

    public class KullaniciEkleIstegi
    {
        [Required, StringLength(100, MinimumLength = 3)]
        public string Ad { get; set; } = default!;

        [Required, StringLength(50, MinimumLength = 3)]
        public string KullaniciAdi { get; set; } = default!;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Sifre { get; set; } = default!;
    }
}
