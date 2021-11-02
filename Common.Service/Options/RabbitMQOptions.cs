#pragma warning disable 8618
namespace VH.MiniService.Common.Service.Options
{
    public class RabbitMqOptions
    {
        public string Host { get; init; }
        public string Port { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }
    }
}
