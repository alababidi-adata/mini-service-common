#pragma warning disable 8618
namespace VH.MiniService.Common.Service.Options
{
    public class MassTransitOptions
    {
        public bool Enable { get; init; }
        public RabbitMqOptions RabbitMq { get; init; }
    }
}
