// ReSharper disable once CheckNamespace
namespace Mango.Http.Authorization
{
    using System;

    public class MangoAuthenticationException : Exception
    {
        public MangoAuthenticationException(string message) : base(message) { }
        public MangoAuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
