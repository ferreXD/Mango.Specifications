// ReSharper disable once CheckNamespace

namespace Mango.Specifications
{
    public class ConcurrentSelectorsException : Exception
    {
        private const string _message = "Concurrent specification selector transforms defined. Ensure only one of the Select() or SelectMany() transforms is used in the same specification!";

        public ConcurrentSelectorsException()
            : base(_message)
        {
        }

        public ConcurrentSelectorsException(Exception innerException)
            : base(_message, innerException)
        {
        }
    }
}