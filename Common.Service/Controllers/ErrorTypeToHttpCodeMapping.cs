using System;
using System.Net;
using Common.Errors;

namespace Common.Service.Controllers
{
    public static class ErrorTypeToHttpCodeMapping
    {
        public static HttpStatusCode ToHttpCode(this ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Unknown => HttpStatusCode.InternalServerError,
                ErrorType.NotFound => HttpStatusCode.NotFound,
                ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorType.Conflict => HttpStatusCode.Conflict,
                ErrorType.AlreadyExists => HttpStatusCode.Conflict,
                ErrorType.Validation => HttpStatusCode.BadRequest,
                ErrorType.InvalidArgument => HttpStatusCode.BadRequest,
                _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
            };
        }
    }
}
