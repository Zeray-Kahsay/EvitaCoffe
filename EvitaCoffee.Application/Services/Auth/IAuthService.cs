using EvitaCoffee.Application.Common;
using EvitaCoffee.Contracts.Auth;

namespace EvitaCoffee.Application.Services.Auth;

public interface IAuthService
{
    Task<Result<bool>> RegisterAsync(RegisterRequest request);
    Task<Result<bool>> VerifyPhoneAsync(VerifyPhoneRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RefreshAsync(string refreshToken);
    Task<Result<bool>> LogoutAsync(string refreshToken);
}
