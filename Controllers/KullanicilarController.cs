using GatekeeperApi.Entities;
using GatekeeperApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatekeeperApi.Controllers
{
    [ApiController]
    [Route("api/kullanicilar")]
    [Authorize]
    public class KullanicilarController(AppDbContext db, IPasswordHasher<User> hasher) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<KullaniciDto>>> Getir(
            [FromQuery] string? arama, CancellationToken ct) //istemci isteği iptal edince (örneğin Angular’da switchMap ile arama iptali) sorgu da durur.
        {
            //sadece okuduğun sorgularda EF’in değişiklik takibini kapatır, daha hızlıdır.
            var sorgu = db.Kullanicilar.AsNoTracking();

            //Contains SQL Server’da LIKE olarak çevrilir, büyük/küçük harf duyarlılığı veritabanı collation’ına bağlıdır.
            if (!string.IsNullOrWhiteSpace(arama))
                sorgu = sorgu.Where(k => k.Ad.Contains(arama));

            return await sorgu
                .OrderBy(k => k.Ad)
                .Select(k => new KullaniciDto(k.Id, k.Ad))   // projeksiyon gereken kolonlar
                .ToListAsync(ct);
        }

        [HttpPost]
        [Authorize(Policy = "YonetimYetkisi")]
        public async Task<ActionResult<KullaniciDto>> Ekle(KullaniciEkleIstegi istek, CancellationToken ct)
        {
            if (await db.Kullanicilar.AnyAsync(k => k.KullaniciAdi == istek.KullaniciAdi, ct))
                return Conflict(new ProblemDetails { Title = "Bu kullanıcı adı zaten kayıtlı." });

            var k = new User { Ad = istek.Ad, KullaniciAdi = istek.KullaniciAdi, Rol = "User" };
            k.SifreHash = hasher.HashPassword(k, istek.Sifre);

            db.Kullanicilar.Add(k);
            await db.SaveChangesAsync(ct);

            return Created($"/api/kullanicilar/{k.Id}", new KullaniciDto(k.Id, k.Ad));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "YonetimYetkisi")]
        public async Task<IActionResult> Sil(int id, CancellationToken ct)
        {
            // nce yükleyip sonra silmek yerine tek DELETE komutu çalıştırır. Bağlı refresh token’ları veritabanındaki ON DELETE CASCADE temizler.
            var silinen = await db.Kullanicilar.Where(k => k.Id.Equals(id)).ExecuteDeleteAsync(ct);
            return silinen == 0 ? NotFound() : NoContent();
        }
    }
}