namespace bagsisbaku.Application.Common.Results;

public class Result
{
    protected Result(
        bool isSuccess,
        Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException(
                "Uğurlu nəticənin xətası ola bilməz.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException(
                "Uğursuz nəticənin xətası olmalıdır.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success()
    {
        return new Result(
            isSuccess: true,
            Error.None);
    }

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result(
            isSuccess: false,
            error);
    }

    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(
            value,
            isSuccess: true,
            Error.None);
    }

    public static Result<T> Failure<T>(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<T>(
            default,
            isSuccess: false,
            error);
    }
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(
        T? value,
        bool isSuccess,
        Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException(
                    "Uğursuz nəticənin Value dəyəri oxuna bilməz.");
            }

            return _value!;
        }
    }
}
