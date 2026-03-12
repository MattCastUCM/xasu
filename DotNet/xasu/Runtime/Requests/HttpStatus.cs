namespace Xasu.Requests
{
    // xAPI Recommended: https://github.com/adlnet/xAPI-Spec/blob/master/xAPI-Communication.md#details-12
    public enum HttpStatus : int
    {
        BadRequest = 400, 
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,
        PreconditionFailed = 412,
        RequestEntityTooLarge = 413,
        TooManyRequests = 429,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
    }
}
