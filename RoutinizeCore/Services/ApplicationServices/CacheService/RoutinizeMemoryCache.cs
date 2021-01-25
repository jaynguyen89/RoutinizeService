using System;
using HelperLibrary.Shared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public sealed class RoutinizeMemoryCache {
        
        private MemoryCache MemoryCache { get; set; }
        
        private MemoryCacheEntryOptions EntryOptions { get; set; }

        public RoutinizeMemoryCache(IConfiguration configuration) {
            MemoryCache = new MemoryCache(new MemoryCacheOptions {
                SizeLimit = int.Parse(configuration.GetSection("CacheSettings")["Size"]),
                CompactionPercentage = double.Parse(configuration.GetSection("CacheSettings")["Compaction"]),
                ExpirationScanFrequency = TimeSpan.FromSeconds(int.Parse(configuration.GetSection("CacheSettings")["ScanFrequency"]))
            });

            EntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(int.Parse(configuration.GetSection("CacheSettings")["SlidingExpiration"])));
        }

        public void SetCacheEntry(CacheEntry entry) {
            EntryOptions.SetPriority(entry.Priority).SetSize(entry.Size);

            try {
                EntryOptions.SetAbsoluteExpiration(
                    entry.AbsoluteExpiration > 0 ? TimeSpan.FromSeconds(entry.AbsoluteExpiration) : EntryOptions.SlidingExpiration.Value
                );
            }
            catch (InvalidOperationException) {
                EntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(SharedConstants.CACHE_ABSOLUTE_EXPIRATION));
            }

            MemoryCache.Set(entry.EntryKey, entry.Data, EntryOptions);
        }

        public T GetCacheEntryFor<T>(string cacheKey) {
            return MemoryCache.Get<T>(cacheKey);
        }
    }
}