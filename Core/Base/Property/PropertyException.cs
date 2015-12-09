
using System;

namespace Pi.Property
{
    public class PropertyException : Exception
    {
        public PropertyException(string message)
            : base(message)
        {
        }

        public PropertyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PropertyException() { }
    }
}
