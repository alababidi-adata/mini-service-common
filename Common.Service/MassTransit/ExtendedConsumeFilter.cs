using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VH.MiniService.Common.Application.Abstractions;
using VH.MiniService.Messaging.Common;
using GreenPipes;
using IdentityModel;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace VH.MiniService.Common.Service.MassTransit
{
    public class ExtendedConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>> where TMessage : class
    {
        private readonly IUserContext _userContext;
        private readonly ILogger<ExtendedConsumeFilter<TMessage>> _logger;

        public ExtendedConsumeFilter(IUserContext userContext, ILogger<ExtendedConsumeFilter<TMessage>> logger)
        {
            _userContext = userContext;
            _logger = logger;
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            LogInfo(context);

            // TODO: Add this activity to open telemetry with additional data from LogInfo

            await next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("ExtendedConsumeFilter");
        }

        private void LogInfo(ConsumeContext<TMessage> context)
        {
            var userId = context.Headers.TryGetHeader(CommonMessageHeaders.UserId, out var idHeader) ? idHeader as string : null;
            _logger.LogInformation($"{{MassTransit.ConversationId}}," +
                                   $"{{MassTransit.MessageId}}," +
                                   $"{{MassTransit.RequestId}}," +
                                   $"{{MassTransit.SourceAddress}}," +
                                   $"{{MassTransit.DestinationAddress}}," +
                                   $"{{MassTransit.ResponseAddress}}," +
                                   $"{{MassTransit.FaultAddress}}," +
                                   $"{{MassTransit.Host}}," +
                                   $"{{MassTransit.Message}}",
                context.ConversationId,
                context.MessageId,
                context.RequestId,
                context.SourceAddress,
                context.DestinationAddress,
                context.ResponseAddress,
                context.FaultAddress,
                $"{context.Host.MachineName}:{context.Host.ProcessId}/{context.Host.ProcessName}",
                context.Message,
                userId
                );

            if (_userContext is ImplicitUserContext userContext)
            {
                userContext.SetClaims(userId != null ? new[] { KeyValuePair.Create(JwtClaimTypes.Subject, userId) } : Array.Empty<KeyValuePair<string, string>>());
            }
        }
    }
}
