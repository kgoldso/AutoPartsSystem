namespace AutoPartsSystem.Models;

/// <summary>
/// Представляет результат операции без возвращаемого значения.
/// </summary>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static implicit operator Result(bool success) => success ? Success() : Failure("Operation failed.");
}

/// <summary>
/// Представляет результат операции с возвращаемым значением.
/// </summary>
/// <typeparam name="T">Тип возвращаемого значения.</typeparam>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);
}
