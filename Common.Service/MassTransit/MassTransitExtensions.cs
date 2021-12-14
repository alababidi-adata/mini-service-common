using System;
using System.Linq;
using System.Threading.Tasks;
using VH.MiniService.Common.Errors;
using FluentResults;
using MassTransit;
using MediatR;

namespace VH.MiniService.Common.Service.MassTransit
{
    public static class MassTransitExtensions
    {
        public static async Task Handle<T>(this ISender mediator,
            ConsumeContext<object> context,
            Func<object, IRequest<Result<T>>> asMediatorRequest,
            Func<T, object>? asMessageResult = null,
            Action<SendContext>? callback = null)
        {
            var result = await mediator
                .Send(asMediatorRequest(context.Message), context.CancellationToken)
                .RespondWithErrorsIfAny(context);

            if (result == null) return;

            var r = asMessageResult == null ? result : asMessageResult(result);

            if (callback == null)
                await context.RespondAsync(r);
            else
                await context.RespondAsync(r, callback);
        }

        public static async Task SendAndRespond<T>(this ISender mediator, ConsumeContext<IRequest<Result<T>>> context)
        {
            var result = await mediator
                .SendAndRespondWithErrorsIfAny(context);

            if (result != null)
            {
                await context.RespondAsync(result);
            }
        }

        public static async Task<T?> SendAndRespondWithErrorsIfAny<T>(this ISender mediator, ConsumeContext<IRequest<Result<T>>> context) =>
            await mediator
                .Send(context.Message, context.CancellationToken)
                .RespondWithErrorsIfAny(context);

        public static async Task<T?> RespondWithErrorsIfAny<T>(this Task<Result<T>> resultTask, ConsumeContext context)
        {
            var result = await resultTask;
            return await context.RespondWithErrorsIfAny(result);
        }

        public static async Task<T?> RespondWithErrorsIfAny<T>(this ConsumeContext context, Result<T> result)
        {
            if (result.IsSuccess) return result.Value;

            if (context.ResponseAddress is not null)
            {
                await context.RespondAsync(result.GetErrorMessage(context));
            }

            return default;
        }

        public static Task SendAndRespond(this ISender mediator, ConsumeContext<IRequest<Result>> context)
            => mediator.SendAndRespondWithErrorsIfAny(context);

        public static async Task SendAndRespondWithErrorsIfAny(this ISender mediator, ConsumeContext<IRequest<Result>> context) =>
            await mediator
                .Send(context.Message, context.CancellationToken)
                .RespondWithErrorsIfAny(context);

        public static async Task RespondWithErrorsIfAny(this Task<Result> resultTask, ConsumeContext context)
        {
            var result = await resultTask;
            await context.RespondWithErrorsIfAny(result);
        }

        public static async Task RespondWithErrorsIfAny(this ConsumeContext context, Result result)
        {
            if (result.IsSuccess) return;

            if (context.ResponseAddress is not null)
            {
                await context.RespondAsync(result.GetErrorMessage(context));
            }
        }

        private static ErrorBase GetErrorMessage(this ResultBase result, ConsumeContext context)
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
