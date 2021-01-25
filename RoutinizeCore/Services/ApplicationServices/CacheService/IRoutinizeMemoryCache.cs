namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public interface IRoutinizeMemoryCache {

        void SetCacheEntry(CacheEntry entry);

        T GetCacheEntryFor<T>(string cacheKey);
    }
}