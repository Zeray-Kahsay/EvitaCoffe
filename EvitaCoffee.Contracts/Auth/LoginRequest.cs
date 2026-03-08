using System;

namespace EvitaCoffee.Contracts.Auth;

public record LoginRequest
{
    public string  PhoneNumber  { get; set; } = string.Empty;
    public string  Password  { get; set; } = string.Empty;
}
