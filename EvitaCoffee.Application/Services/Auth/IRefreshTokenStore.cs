using System;

namespace EvitaCoffee.Application.Services.Auth;

public interface IRefreshTokenStore
{
    Task StoreAsync(Guid userId, string tokenId, string hashedToken, TimeSpan expiry);
    Task<bool> ValidateAndDeleteAsync(Guid userId, string tokenId, string hashedToken);
    Task RemoveAsync(Guid userId, string tokenId);
}
