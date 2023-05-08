using System;
using System.Net;
using System.Runtime.Serialization;

namespace ChatApp.Web.Exceptions
{
    [Serializable]
    public class ConflictException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ConflictException(string message, string conflictItem, HttpStatusCode statusCode) : base(conflictItem + " " + message)
        {
            StatusCode = statusCode;
        }

        public ConflictException(string message, string conflictItem, Exception e, HttpStatusCode statusCode) : base(conflictItem + " " + message, e)
        {
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(StatusCode)}: {StatusCode}";
        }
    }
}