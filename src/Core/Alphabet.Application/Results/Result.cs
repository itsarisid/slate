namespace Alphabet.Application.Results;

/// <summary>
/// Represents the outcome of an operation.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }
    /// <summary>
    /// Success.
    /// </summary>

    public static Result Success() => new(true, null);
    /// <summary>
    /// Failure.
    /// </summary>

    public static Result Failure(string error) => new(false, error);

    public static implicit operator Result(string error) => Failure(error);

    public static Result Failure(Error error) => new(false, error.Message);
}

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
public class Result<T> : Result
{
    protected Result(bool isSuccess, T? value, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }
    /// <summary>
    /// Success.
    /// </summary>

    public static Result<T> Success(T value) => new(true, value, null);
    /// <summary>
    /// Failure.
    /// </summary>

    public new static Result<T> Failure(string error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(string error) => Failure(error);

    public new static Result<T> Failure(Error error) => new(false, default, error.Message);
}
