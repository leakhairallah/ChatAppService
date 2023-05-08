using System;
using System.Net;
using System.Runtime.Serialization;

namespace ChatApp.Web.Exceptions
{
    [Serializable]
    public class InvalidConversationException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public InvalidConversationException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public InvalidConversationException(string message, Exception e, HttpStatusCode statusCode) : base(message, e)
        {
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(StatusCode)}: {StatusCode}";
        }
    }
}