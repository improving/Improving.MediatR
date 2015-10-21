namespace Improving.MediatR.Environment
{
    public class Env<T>
        where T : class
    {
        public Env()
        {
        }

        public Env(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;
            var other = obj as Env<T>;
            return other != null && Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static implicit operator T (Env<T> env)
        {
            return env.Value;
        }

        public static implicit operator Env<T>(T value)
        {
            return new Env<T>(value);
        }
    }
}
