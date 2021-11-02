using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VH.MiniService.Common.Data.Extensions
{
    public static class DbSetExtensions
    {
        public static ValueTask<T?> FindByKeyAsync<T>(this DbSet<T> dbSet, object key,
            CancellationToken cancellationToken) where T : class
        {
            return dbSet.FindAsync(new[] { key }, cancellationToken)!;
        }
    }
}
