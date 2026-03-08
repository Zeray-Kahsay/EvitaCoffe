using EvitaCoffee.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace EvitaCoffee.Infrastructure.Sms;

// ONLY DEV --> Will be replced by a real provider 
public class DevSmsSender (ILogger<DevSmsSender> logger) : ISmsSender
{
    private readonly ILogger<DevSmsSender> _logger = logger;
    public Task SendsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("SMS to {Phone}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
