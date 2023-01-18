using System.Threading.Tasks;
using MassTransit;

namespace VH.MiniService.Common.Service.MassTransit
{
    public class PublishRequestContextSetterFilter<T> : IFilter<PublishContext<T>> where T : class
    {
        public void Probe(ProbeContext context) => context.CreateFilterScope(nameof(PublishRequestContextSetterFilter<T>));

        public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            var c = RequestContext.Value;

            if (c.TenantIdOrDefault != null) context.Headers.Set(CommonRequestHeaders.TenantId, c.TenantIdOrDefault);
            if (c.UserIdOrDefault != null) context.Headers.Set(CommonRequestHeaders.UserId, c.UserIdOrDefault);
            if (c.Token != null) context.Headers.Set(CommonRequestHeaders.Token, c.Token);

            return next.Send(context);
        }
    }
}
