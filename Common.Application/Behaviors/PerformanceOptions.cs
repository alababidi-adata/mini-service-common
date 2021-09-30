using System;

namespace Common.Application.Behaviors
{
    public class PerformanceOptions
    {
        public TimeSpan WarningThreshold { get; set; } = TimeSpan.FromHours(1);
    }
}
