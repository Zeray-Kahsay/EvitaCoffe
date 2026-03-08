using System.Text.Json;
using EvitaCoffee.Application.Abstractions;
using StackExchange.Redis;

namespace EvitaCoffee.Infrastructure.Redis;

public class RedisOptStore (IConnectionMultiplexer redis) : IOtpStore
{
    private readonly IConnectionMultiplexer _redis = redis;
    public async Task StoreAsync(string phone, string code)
    {
        var db = _redis.GetDatabase();
        var key = $"otp:{phone}";

        var data = new
        {
            Code = code,
            Attempts = 0
        };

        var json = JsonSerializer.Serialize(data);

        await db.StringSetAsync(key, json, TimeSpan.FromMinutes(5));
    }

    public async Task<(bool Success, string? Error)> ValidateAsync(string phone, string code)
    {
        var db = _redis.GetDatabase();
        var key = $"otp:{phone}";

        var json = await db.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return (false, "Code expired");
        
        var data = JsonSerializer.Deserialize<OtpData>((string)json!);

        if (data!.Attempts >= 5)
            return (false, "Too many attempts");
        
        if (data.Code != code)
        {
            data.Attempts++;
            await db.StringSetAsync(key, JsonSerializer.Serialize(data));
            return (false, "Invalid code");
        }

        await db.KeyDeleteAsync(key);
        return (true, null);
    }

    private class OtpData
    {
        public string  Code  { get; set; } = string.Empty;
        public int  Attempts { get; set; }
    }
}
