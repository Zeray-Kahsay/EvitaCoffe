using System;

namespace EvitaCoffee.Contracts.Auth;

public record RefreshRequest
{
    public string  RefreshToken  { get; set; } = string.Empty;
}
