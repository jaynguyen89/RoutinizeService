using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public sealed class RoutinizeMemoryCache {
        
        private MemoryCache MemoryCache { get; set; }
        
        private MemoryCacheEntryOptions EntryOptions { get; set; }

        public RoutinizeMemoryCache(IOptions<CacheOptions> options) {
            MemoryCache = new MemoryCache(new MemoryCacheOptions {
                SizeLimit = int.Parse(options.Value.Size),
                CompactionPercentage = double.Parse(options.Value.Compaction),
                ExpirationScanFrequency = TimeSpan.FromSeconds(int.Parse(options.Value.ScanFrequency))
            });

            EntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(int.Parse(options.Value.SlidingExpiration)));
        }

        public void SetCacheEntry(CacheEntry entry) {
            EntryOptions.SetPriority(entry.Priority).SetSize(entry.Size);
            if (entry.AbsoluteExpiration > 0) EntryOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(entry.AbsoluteExpiration));

            MemoryCache.Set(entry.EntryKey, entry.Data, EntryOptions);
        }

        public T GetCacheEntryFor<T>(string cacheKey) {
            return MemoryCache.Get<T>(cacheKey);
        }
    }
}