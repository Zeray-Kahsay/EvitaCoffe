using System;

namespace EvitaCoffee.Contracts.Common;

public record ApiErrorResponse
{
    public string  Code  { get; set; } = string.Empty;
    public string  Message  { get; set; } = string.Empty;
    //public Dictionary<string,string[]>? ValidationErrors  { get; set; } = validationErrors;
    public string? StackTrace  { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
