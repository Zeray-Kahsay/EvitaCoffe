using EvitaCoffee.Application.Services.Auth;
using StackExchange.Redis;

namespace EvitaCoffee.Infrastructure.Services.Redis;

public class RedisRefreshTokenStore(IConnectionMultiplexer redis) : IRefreshTokenStore
{
    private readonly IConnectionMultiplexer _redis = redis;
    public async Task StoreAsync(Guid userId, string tokenId, string hashedToken, TimeSpan expiry)
    {
        var db = _redis.GetDatabase();
        var key = $"refresh:{userId}:{tokenId}";
        await db.StringSetAsync(key,hashedToken, expiry);
        
    }


    public async Task<bool> ValidateAndDeleteAsync(Guid userId, string tokenId, string hashedToken)
    {
        var db = _redis.GetDatabase();
        var key = $"refresh:{userId}:{tokenId}";

        var stored = await db.StringGetAsync(key);
        if (stored != hashedToken)
            return false;
        
        await db.KeyDeleteAsync(key);
        return true;
    }

    public async Task RemoveAsync(Guid userId, string tokenId)
    {
        var db = _redis.GetDatabase();
        var key = $"refresh:{userId}:{tokenId}";
        await db.KeyDeleteAsync(key);
    }
}
