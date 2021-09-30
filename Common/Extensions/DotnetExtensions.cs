using System;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class IDisposableExtensions
    {
        public static ValueTask DisposeAsync(this IDisposable disposable)
        {
            if (disposable is IAsyncDisposable asyncDisposable)
                return asyncDisposable.DisposeAsync();

            disposable.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
