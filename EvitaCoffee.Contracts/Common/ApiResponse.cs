using System;

namespace EvitaCoffee.Contracts.Common;

public class ApiResponse<T>
{
    public bool Success  { get; set; }
    public T? Data  { get; set; }
    public ApiErrorResponse? Error  { get; set; }
    public string CorrelationId  { get; set; } = null!;
}
