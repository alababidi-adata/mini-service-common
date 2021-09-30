using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Application.Abstractions;
using GreenPipes;
using IdentityModel;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sentry;
using Sentry.AspNetCore;
using Sentry.Extensibility;

namespace Common.Service.MassTransit
{
    public class SentryConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>> where TMessage : class
    {
        private readonly Func<IHub> _hubAccessor;
        private readonly SentryAspNetCoreOptions _options;
        private readonly IUserContext _userContext;

        public SentryConsumeFilter(Func<IHub> hubAccessor, IOptions<SentryAspNetCoreOptions> options, IUserContext userContext)
        {
            _hubAccessor = hubAccessor;
            _userContext = userContext;
            _options = options.Value;
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            var hub = _hubAccessor();
            if (!hub.IsEnabled)
            {
                await next.Send(context);
            }

            using (hub.PushAndLockScope())
            {
                hub.ConfigureScope(scope => Populate(scope, context));

                try
                {
                    await next.Send(context);
                }
                catch (Exception e) //only for real bugs
                {
                    CaptureException(hub, e);

                    // if (context.ResponseAddress != null) //if its a request/response send error back
                    //     await context.RespondAsync<>(new object());

                    throw; //should be caught by ErrorTransportFilter and sent to queue <queue name>_error
                }
            }
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("SentryConsumeFilter");
        }

        private void CaptureException(IHub hub, Exception e)
        {
            var evt = new SentryEvent(e);

            _options.DiagnosticLogger?.LogDebug("Sending event '{SentryEvent}' to Sentry.", evt);

            var id = hub.CaptureEvent(evt);

            _options.DiagnosticLogger?.LogInfo("Event '{id}' queued.", id);
        }

        private void Populate(Scope scope, ConsumeContext<TMessage> context)
        {
            KeyValuePair<string, string>[]? claims = null;

            if (context.ConversationId.HasValue)
                scope.SetTag(nameof(HttpContext.TraceIdentifier), context.ConversationId.Value.ToString());

            if (!scope.HasUser() &&
                context.Headers.TryGetHeader("MiniService.UserId", out var idHeader) &&
                idHeader is string userId)
            {
                scope.User = new User { Id = userId };
                claims = new[]
                {
                    KeyValuePair.Create(JwtClaimTypes.Subject, userId)
                };
            }

            scope.SetTag("MassTransit.MessageId", context.MessageId?.ToString() ?? string.Empty);
            scope.SetTag("MassTransit.RequestId", context.RequestId?.ToString() ?? string.Empty);
            scope.SetTag("MassTransit.Source", context.SourceAddress.ToString());
            scope.SetTag("MassTransit.Destination", context.DestinationAddress.ToString());
            scope.SetTag("MassTransit.ResponseAddress", context.ResponseAddress.ToString());
            scope.SetTag("MassTransit.FaultAddress", context.FaultAddress.ToString());
            scope.SetTag("MassTransit.Host", $"{context.Host.MachineName}:{context.Host.ProcessId}/{context.Host.ProcessName}");
            scope.SetExtra("MassTransit.Message", context.Message);

            if (_userContext is ImplicitUserContext userContext)
                userContext.SetClaims(claims ?? Array.Empty<KeyValuePair<string, string>>());
        }
    }
}
