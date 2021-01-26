using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public interface IRoutinizeRedisCache {

        Task InsertRedisCacheEntry<T>([NotNull] CacheEntry entry);

        Task<T> GetRedisCacheEntry<T>([AllowNull] string entryKey = null);
    }
}