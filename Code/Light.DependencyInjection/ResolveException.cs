using System;

namespace Light.DependencyInjection
{
    public class ResolveException : Exception
    {
        public ResolveException(string message, Exception innerException = null) : base(message, innerException) { }
    }
}