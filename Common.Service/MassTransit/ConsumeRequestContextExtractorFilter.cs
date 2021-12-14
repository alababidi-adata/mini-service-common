using System.Collections.Generic;
using System.Threading.Tasks;
using GreenPipes;
using IdentityModel;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace VH.MiniService.Common.Service.MassTransit
{
    public class ConsumeRequestContextExtractorFilter<TMessage> : IFilter<ConsumeContext<TMessage>> where TMessage : class
    {
        private readonly ILogger<ConsumeRequestContextExtractorFilter<TMessage>> _logger;

        public ConsumeRequestContextExtractorFilter(ILogger<ConsumeRequestContextExtractorFilter<TMessage>> logger)
        {
            _logger = logger;
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            var headers = context.Headers;
            var token = headers.TryGetHeader(CommonRequestHeaders.Token, out var tokenHeader) ? tokenHeader.ToString() : null;
            var userId = headers.TryGetHeader(CommonRequestHeaders.UserId, out var idHeader) ? idHeader.ToString() : null;
            var tenant = headers.TryGetHeader(CommonRequestHeaders.TenantId, out var tenantHeader) ? int.Parse(tenantHeader.ToString()!) : (int?)null;
            var claims = userId != null
                ? new Dictionary<string, string[]>() { { JwtClaimTypes.Subject, new[] { userId } } }
                : new Dictionary<string, string[]>();

            RequestContext.SetContext(token, claims, tenant);

            // TODO: Add this activity to open telemetry with additional data from LogInfo
            LogInfo(context, userId);

            await next.Send(context);
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope(nameof(ConsumeRequestContextExtractorFilter<TMessage>));

        private void LogInfo(ConsumeContext<TMessage> context, string? userId)
        {
            _logger.LogInformation("{MassTransit.ConversationId}," +
                                   "{MassTransit.MessageId}," +
                                   "{MassTransit.RequestId}," +
                                   "{MassTransit.SourceAddress}," +
                                   "{MassTransit.DestinationAddress}," +
                                   "{MassTransit.ResponseAddress}," +
                                   "{MassTransit.FaultAddress}," +
                                   "{MassTransit.Host}," +
                                   "{MassTransit.Message}",
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
        }
    }
}
