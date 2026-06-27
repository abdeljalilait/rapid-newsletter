namespace NewsletterPlatform.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string ErrorCode { get; }

    protected Result(bool success, string? error, string errorCode)
    {
        IsSuccess = success;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new Result(true, null, ErrorCodes.None);
    public static Result Failure(string error, string errorCode = ErrorCodes.Unprocessable) =>
        new Result(false, error, errorCode);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool success, string? error, string errorCode)
        : base(success, error, errorCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(value, true, null, ErrorCodes.None);
    public static new Result<T> Failure(string error, string errorCode = ErrorCodes.Unprocessable) =>
        new Result<T>(default, false, error, errorCode);
}

public static class ErrorCodes
{
    public const string None = "none";
    public const string Unprocessable = "unprocessable";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string Validation = "validation";
}