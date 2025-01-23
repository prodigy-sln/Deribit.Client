namespace Prodigy.Solutions.Deribit.Client.Observable;

public class Exceptional
{
    public static Exceptional<T> From<T>(T value) => new Exceptional<T>(value);
    public static Exceptional<T> From<T>(Exception ex) => new Exceptional<T>(ex);
    public static Exceptional<T> From<T>(Func<T> factory) => new Exceptional<T>(factory);
}

public class Exceptional<T>
{
    public bool HasException { get; private set; }
    public Exception? Exception { get; private set; }
    public T? Value { get; private set; }

    public Exceptional(T value)
    {
        this.HasException = false;
        this.Value = value;
    }

    public Exceptional(Exception exception)
    {
        this.HasException = true;
        this.Exception = exception;
    }

    public Exceptional(Func<T> factory)
    {
        try
        {
            this.Value = factory();
            this.HasException = false;
        }
        catch (Exception ex)
        {
            this.Exception = ex;
            this.HasException = true;
        }
    }

    public override string? ToString() =>
        this.HasException
            ? this.Exception?.GetType().Name
            : this.Value != null ? this.Value.ToString() : "null";
}


public static class ExceptionalExtensions
{
    public static Exceptional<T> ToExceptional<T>(this T value) => Exceptional.From(value);

    public static Exceptional<T> ToExceptional<T>(this Func<T> factory) => Exceptional.From(factory);

    public static Exceptional<U> Select<T, U>(this Exceptional<T> value, Func<T?, U> m) =>
        value.SelectMany(t => Exceptional.From(() => m(t)));

    public static Exceptional<U> SelectMany<T, U>(this Exceptional<T> value, Func<T?, Exceptional<U>> k) =>
        value.HasException ? Exceptional.From<U>(value.Exception!) : k(value.Value);

    public static Exceptional<V> SelectMany<T, U, V>(this Exceptional<T> value, Func<T?, Exceptional<U>> k, Func<T?, U?, V> m) =>
        value.SelectMany(t => k(t).SelectMany(u => Exceptional.From(() => m(t, u))));
}
