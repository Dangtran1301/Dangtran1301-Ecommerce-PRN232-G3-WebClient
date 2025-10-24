namespace PRN232_WebClient_Tachonogy.Models;

public class Result
{
    public bool IsSuccess { get; init; }
    public Error Error { get; init; } = Error.None;

    protected Result(bool success, Error? error)
    {
        IsSuccess = success;
        Error = error;
    }

    public static Result Ok() => new(true, Error.None);

    public static Result Fail(Error error) => new(false, error);

    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    public static Result<T> Fail<T>(Error error) => Result<T>.Fail(error);

    public static implicit operator Result(bool success)
        => success ? Ok() : Fail(Error.None);

    public static implicit operator Result(Error error)
        => Fail(error);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool success, Error error)
        : base(success, error)
    {
        Value = value;
    }

    public static Result<T> Ok(T value) => new(value, true, Error.None);

    public new static Result<T> Fail(Error error) => new(default, false, error);

    public static implicit operator Result<T>(T value) => Ok(value);

    public static implicit operator Result<T>(Error error) => Fail(error);
}