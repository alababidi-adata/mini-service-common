#pragma warning disable 8618
namespace Common.Service.Options
{
    public class MassTransitOptions
    {
        public const string SectionName = "MassTransit";
        public bool Enable { get; init; } = true;
        public RabbitMqOptions RabbitMq { get; init; }
    }
}
