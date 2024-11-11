namespace ITCentral.Types;

public readonly struct Result<T, E>
{
    private readonly bool success;
    public readonly T Value;
    public readonly E Error;

    private Result(T v, E e, bool s)
    {
        Value = v;
        Error = e;
        success = s;
    }

    public bool IsSuccessful => success;

    public static Result<T, E> Ok(T v)
    {
        return new(v, default!, true);
    }

    public static Result<T, E> Err(E e)
    {
        return new(default!, e, false);
    }

    public static implicit operator Result<T, E>(T v) => new(v, default!, true);
    public static implicit operator Result<T, E>(E e) => new(default!, e, false);

    public R Match<R>(
        Func<T, R> successful,
        Func<E, R> fail
    ) => success ? successful(Value) : fail(Error);
}