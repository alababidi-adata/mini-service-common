namespace VH.MiniService.Common.Service.Options
{
    public class RedisOptions
    {
        public bool Enable { get; init; }
        public string? InstanceName { get; init; }
        public string? Connection { get; init; }
    }
}
