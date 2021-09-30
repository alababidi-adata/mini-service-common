using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Errors;
using FluentResults;
using MassTransit;

namespace Common.Service.MassTransit
{
    public static class MassTransitExtensions
    {
        public static async Task ToMassTransitResult(this Task<Result> resultTask, ConsumeContext context)
        {
            var result = await resultTask;
            await result.ToMassTransitResult(context);
        }

        public static async Task<T?> ToMassTransitResult<T>(this Task<Result<T>> resultTask, ConsumeContext context) where T : class
        {
            var result = await resultTask;
            return await result.ToMassTransitResult(context);
        }

        public static async Task ToMassTransitResult(this Result result, ConsumeContext context)
        {
            if (result.IsSuccess)
                return;

            if (context.ResponseAddress is not null)
                await context.RespondAsync(result.CreateErrorMessage(context));
        }

        public static async Task<T?> ToMassTransitResult<T>(this Result<T> result, ConsumeContext context) where T : class
        {
            if (result.IsSuccess)
                return result.Value;

            if (context.ResponseAddress is not null)
                await context.RespondAsync(result.CreateErrorMessage(context));

            return null;
        }

        public static ErrorBase? CreateErrorMessage(this ResultBase result, ConsumeContext context)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("Cannot call on successful result.");

            var error = result.Reasons
                .OfType<ErrorBase>()
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Expecting to find error of type {nameof(ErrorBase)}.");

            return error;
        }
    }
}
