using GatekeeperApi.Entities;
using GatekeeperApi.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace GatekeeperApi.Services
{
    public class TokenService(IConfiguration config) : ITokenService
    {
        public string AccessTokenUret(User k)
        {
            var jwt = config.GetSection("Jwt");
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, k.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, k.KullaniciAdi),
            new(ClaimTypes.Role, k.Rol)
        };

            var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),          // kısa ömür
                signingCredentials: new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string Ham, string Hash) RefreshTokenUret()
        {
            var ham = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return (ham, Hashle(ham));
        }

        public static string Hashle(string deger) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(deger)));
    }
}
