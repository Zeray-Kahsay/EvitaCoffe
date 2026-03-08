using System;

namespace EvitaCoffee.Contracts.Auth;

public record RegisterRequest
{
    public string  PhoneNumber  { get; set; } = string.Empty;
    public string?  Email  { get; set; } = string.Empty;
    public string  Password  { get; set; } = string.Empty;
    public string  FullName  { get; set; } = string.Empty;
}
