using GatekeeperApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatekeeperApi.Infrastructure.Persistence;

namespace GatekeeperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(AppDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await db.Kullanicilar.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(string id)
        {
            var user = db.Kullanicilar.FirstOrDefault(u => u.Id.Equals(id));
            return user is not null ? Ok(user) : NotFound();
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterUsersAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                word = "";

            var users = await db.Kullanicilar.Where(k => k.KullaniciAdi.Contains(word)).ToListAsync();
            return Ok(users);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Ad) || string.IsNullOrEmpty(user.KullaniciAdi))
            {
                return BadRequest("First name (Ad) and Last name (Soyad) are required.");
            }

            user.SetNewGuid();
            db.Kullanicilar.Add(user);
            await db.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                int raw = await db.Kullanicilar.Where(k => k.Id.Equals(id)).ExecuteDeleteAsync();
                return Ok(raw);
            }
            return NotFound();
        }
    }
}