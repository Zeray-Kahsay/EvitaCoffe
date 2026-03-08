using System;

namespace EvitaCoffee.Application.Abstractions;

public interface IOtpStore
{
    Task StoreAsync(string phone, string code);
    Task<(bool Success, string? Error)> ValidateAsync(string phone, string code);
}
