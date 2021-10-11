#pragma warning disable 8618
namespace Common.Service.Options
{
    public class RabbitMqOptions
    {
        public string Host { get; init; }
        public string Port { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }

        public HealthChecksOptions HealthChecks { get; init; } = new();
    }
}
