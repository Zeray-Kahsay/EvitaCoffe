using EvitaCoffee.Application.Models;

namespace EvitaCoffee.Application.Abstractions;

public interface IJwtService
{
    string GenerateAccessToken(AuthUser user);
}
