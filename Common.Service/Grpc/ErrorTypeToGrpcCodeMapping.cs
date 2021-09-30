using System;
using Common.Errors;
using Grpc.Core;

namespace Common.Service.Grpc
{
    public static class ErrorTypeToGrpcCodeMapping
    {
        public static StatusCode ToGrpcCode(this ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Unknown => StatusCode.Unknown,
                ErrorType.NotFound => StatusCode.NotFound,
                ErrorType.Unauthorized => StatusCode.PermissionDenied,
                ErrorType.Conflict => StatusCode.FailedPrecondition,
                ErrorType.AlreadyExists => StatusCode.AlreadyExists,
                ErrorType.Validation => StatusCode.FailedPrecondition,
                ErrorType.InvalidArgument => StatusCode.InvalidArgument,
                _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
            };
        }
    }
}
