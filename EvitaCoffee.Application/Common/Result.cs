using EvitaCoffee.Contracts.Common;

namespace EvitaCoffee.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ApiErrorResponse? Error { get; set; }
    //public Dictionary<string, string[]>? ValidationErrors { get; }

    protected Result(bool isSuccess, T? value, ApiErrorResponse? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        //ValidationErrors = validationErrors;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(ApiErrorResponse error) => new(false, default, error);


    // public static Result<T> ValidationFailure(Dictionary<string, string[]> validationErrors) => 
    //     new(false, default, new ApiErrorResponse("Validation failed", "VALIDATION_ERROR"), validationErrors);

}
