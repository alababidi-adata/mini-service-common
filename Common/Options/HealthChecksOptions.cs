using System;

namespace VH.MiniService.Common.Options
{
    public class HealthChecksOptions
    {
        public bool Enable { get; init; } = true;
        public string[] Tags { get; init; } = Array.Empty<string>();
    }
}
