using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VH.MiniService.Common.Errors;
using FluentResults;
using Grpc.Core;

namespace VH.MiniService.Common.Service.Grpc
{
    public static class GrpcExtensions
    {
        public static async Task<T?> ToGrpcResult<T>(this Task<Result> resultTask, ServerCallContext context) where T : class, new()
        {
            var result = await resultTask;
            return result.ToGrpcResult<T>(context);
        }

        public static async Task<T?> ToGrpcResult<T>(this Task<Result<T>> resultTask, ServerCallContext context) where T : class
        {
            var result = await resultTask;
            return result.ToGrpcResult(context);
        }

        public static T? ToGrpcResult<T>(this Result result, ServerCallContext context) where T : class, new()
        {
            if (result.IsSuccess)
                return new T();

            result.SetFailStatus(context);
            return null;
        }

        public static T? ToGrpcResult<T>(this Result<T> result, ServerCallContext context) where T : class
        {
            if (result.IsSuccess)
                return result.Value;

            result.SetFailStatus(context);
            return null;
        }

        public static void SetFailStatus(this ResultBase result, ServerCallContext context)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("Cannot call on successful result.");

            var error = result.Reasons
                .OfType<ErrorBase>()
                .FirstOrDefault();

            if (error is null)
            {
                context.Status = new Status(StatusCode.Internal, "Something went wrong.");
                return;
            }

            context.Status = new Status(error.Type.ToGrpcCode(), error.InternalMessage);
            context.ResponseTrailers.Add("MINISERVICE_ERROR", JsonSerializer.Serialize(error));
        }
    }
}
