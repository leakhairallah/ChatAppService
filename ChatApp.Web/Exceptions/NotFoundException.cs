using System;
using System.Net;
using System.Runtime.Serialization;

namespace ChatApp.Web.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public NotFoundException(string message, string notFoundItem, HttpStatusCode statusCode) : base(notFoundItem + " " + message)
        {
            StatusCode = statusCode;
        }

        public NotFoundException(string message, string notFoundItem, Exception e, HttpStatusCode statusCode) : base(notFoundItem + " " + message, e)
        {
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(StatusCode)}: {StatusCode}";
        }
    }
}