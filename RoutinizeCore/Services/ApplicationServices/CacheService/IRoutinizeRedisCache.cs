using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public interface IRoutinizeRedisCache {

        Task InsertRedisCacheEntry([NotNull] CacheEntry entry);

        Task<T> GetRedisCacheEntry<T>([NotNull] string entryKey);
    }
}