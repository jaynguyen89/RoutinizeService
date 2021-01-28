using System.Diagnostics.CodeAnalysis;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public interface IRoutinizeMemoryCache {

        void SetCacheEntry<T>([NotNull] CacheEntry entry);

        T GetCacheEntryFor<T>([AllowNull] string cacheKey);

        void RemoveCacheEntry([NotNull] string entryKey);
    }
}