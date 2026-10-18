using GatekeeperApi.Entities;

namespace GatekeeperApi.Interfaces
{
    public interface ITokenService
    {
        string AccessTokenUret(User k);
        (string Ham, string Hash) RefreshTokenUret();
    }
}
