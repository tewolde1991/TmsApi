namespace TmsApi.Application.Common;

public class Result<TSuccess, TError>
{
    public TSuccess? Value { get; }
    public TError? Error { get; }
    public bool IsSuccess { get; }

    private Result(TSuccess value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(TError error)
    {
        Error = error;
        IsSuccess = false;
    }

    public static Result<TSuccess, TError> Success(TSuccess value)
        => new(value);

    public static Result<TSuccess, TError> Failure(TError error)
        => new(error);

    public TResult Match<TResult>(
        Func<TSuccess, TResult> onSuccess,
        Func<TError, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(Error!);
    }
}