using System;
using System.Net;
using System.Runtime.Serialization;

namespace ChatApp.Web.Exceptions
{
    [Serializable]
    public class InvalidMessageException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public InvalidMessageException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public InvalidMessageException(string message, Exception e, HttpStatusCode statusCode) : base(message, e)
        {
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(StatusCode)}: {StatusCode}";
        }
    }
}