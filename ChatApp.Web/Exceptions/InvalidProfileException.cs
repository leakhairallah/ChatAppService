using System;
using System.Net;
using System.Runtime.Serialization;

namespace ChatApp.Web.Exceptions
{
    [Serializable]
    public class InvalidProfileException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public InvalidProfileException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public InvalidProfileException(string message, Exception e, HttpStatusCode statusCode) : base(message, e)
        {
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(StatusCode)}: {StatusCode}";
        }
    }
}