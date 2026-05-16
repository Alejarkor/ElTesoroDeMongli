using System;

namespace ElTesoroDeMongli.API
{
    public class ApiException : Exception
    {
        public long StatusCode { get; }
        public int? ErrorCode { get; }
        public string ResponseBody { get; }

        public ApiException(string message, long statusCode = 0, int? errorCode = null, string responseBody = null) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            ResponseBody = responseBody;
        }
    }
}
