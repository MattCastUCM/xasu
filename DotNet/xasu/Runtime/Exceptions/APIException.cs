using System;
using Xasu.Requests;

namespace Xasu.Exceptions
{
    [Serializable]
    public class APIException : Exception
    {
        public int HttpCode { get; private set; }
        public HttpResponse Response { get; private set; }

        public APIException(int httpCode, string message, HttpResponse response) : base(message)
        {
            this.HttpCode = httpCode;
            this.Response = response;
            this.Response.ex = this;
        }

        public APIException(int httpCode, string message, HttpResponse response, Exception innerException) : base(message, innerException)
        {
            this.HttpCode = httpCode;
            this.Response = response;
            this.Response.ex = this;
        }
    }
}
