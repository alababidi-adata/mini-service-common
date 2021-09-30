using System.Threading.Tasks;
using GreenPipes;
using MassTransit;

namespace Common.Service.MassTransit
{
    //public class EndpointContainerConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>> where TMessage : class
    //{
    //    private readonly IEndpointContainer _endpointContainer;

    //    public EndpointContainerConsumeFilter(IEndpointContainer endpointContainer)
    //    {
    //        _endpointContainer = endpointContainer;
    //    }

    //    public Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    //    {
    //        if (_endpointContainer is EndpointContainer container)
    //            container.SetToContext(context);

    //        return next.Send(context);
    //    }

    //    public void Probe(ProbeContext context) => context.CreateFilterScope("EndpointContainerFilter");
    //}
}
