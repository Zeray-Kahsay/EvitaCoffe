using System;

namespace EvitaCoffee.Application.Common;

public class JwtSettings
{
    public string  Issuer  { get; set; } = default!;
    public string  Audience  { get; set; } = default!;
    public string  SecretKey  { get; set; } = default!;
    public int  ExpiryMinutes { get; set; }
}
