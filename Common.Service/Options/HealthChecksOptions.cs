using System;

namespace Common.Service.Options
{
    public class HealthChecksOptions
    {
        public bool Enable { get; init; } = true;
        public string[] Tags { get; init; } = Array.Empty<string>();
    }
}
