namespace socket4net.tests
{
    public class REnum<T>
    {
        public T Value { get; private set; }
        public REnum(T value)
        {
            Value = value;
        }

        public static implicit operator T(REnum<T> e)
        {
            return e.Value;
        }

        public static implicit operator REnum<T>(T value)
        {
            return new REnum<T>(value);
        }
    }
}