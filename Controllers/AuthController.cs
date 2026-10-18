using GatekeeperApi.Entities;
using GatekeeperApi.Infrastructure.Persistence;
using GatekeeperApi.Interfaces;
using GatekeeperApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatekeeperApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AppDbContext db, ITokenService tokens, IPasswordHasher<User> hasher) : ControllerBase
    {
        private const string CookieAdi = "refreshToken";
        private bool SifreDogruMu(User k, string sifre) => 
            hasher.VerifyHashedPassword(k, k.SifreHash, sifre) != PasswordVerificationResult.Failed;

        private async Task TumOturumlariIptalEt(string id)
        {
            await db.RefreshTokenlar.Where(t => t.KullaniciId.Equals(id)).ForEachAsync(t => { t.IptalTarihi = DateTime.UtcNow; });
            await db.SaveChangesAsync();
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest istek)
        {
            var k = await db.Kullanicilar.SingleOrDefaultAsync(x => x.KullaniciAdi.Equals(istek.KullaniciAdi));
            if (k is null || !SifreDogruMu(k, istek.Sifre))
                return Unauthorized(new ProblemDetails { Title = "Kullanıcı adı veya şifre hatalı." });
            // hangisinin yanlış olduğunu ayrı ayrı söyleme (kullanıcı numaralandırma riski)

            return await OturumAc(k);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> Yenile()
        {
            var ham = Request.Cookies[CookieAdi];
            if (string.IsNullOrEmpty(ham)) return Unauthorized();

            var hash = TokenService.Hashle(ham);
            var mevcut = await db.RefreshTokenlar.SingleOrDefaultAsync(x => x.TokenHash == hash);
            if (mevcut is null) return Unauthorized();

            // Daha önce iptal edilmiş token tekrar kullanıldı: çalınmış olabilir
            if (mevcut.IptalTarihi is not null)
            {
                await TumOturumlariIptalEt(mevcut.KullaniciId);
                return Unauthorized();
            }
            if (!mevcut.Aktif) return Unauthorized();

            var k = await db.Kullanicilar.FindAsync(mevcut.KullaniciId);
            if (k is null) return Unauthorized();

            mevcut.IptalTarihi = DateTime.UtcNow;                 // rotation: eskisi iptal
            return await OturumAc(k, mevcut);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Cikis()
        {
            var ham = Request.Cookies[CookieAdi];
            if (!string.IsNullOrEmpty(ham))
            {
                var hash = TokenService.Hashle(ham);
                var t = await db.RefreshTokenlar.SingleOrDefaultAsync(x => x.TokenHash == hash);
                if (t is not null) { t.IptalTarihi = DateTime.UtcNow; await db.SaveChangesAsync(); }
            }
            Response.Cookies.Delete(CookieAdi, new CookieOptions { Path = "/api/auth" });
            return NoContent();
        }

        private async Task<LoginResponse> OturumAc(User k, RefreshToken? eski = null)
        {
            var (ham, hash) = tokens.RefreshTokenUret();
            if (eski is not null) eski.YerineGecenHash = hash;

            db.RefreshTokenlar.Add(new RefreshToken
            {
                KullaniciId = k.Id,
                TokenHash = hash,
                BitisTarihi = DateTime.UtcNow.AddDays(7)
            });
            await db.SaveChangesAsync();

            Response.Cookies.Append(CookieAdi, ham, new CookieOptions
            {
                HttpOnly = true,                 // JS okuyamaz (XSS koruması)
                Secure = true,
                SameSite = SameSiteMode.Strict,  // CSRF koruması
                Path = "/api/auth",              // yalnızca bu uçlara gider
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return new LoginResponse(tokens.AccessTokenUret(k), 600);
        }
    }
}
