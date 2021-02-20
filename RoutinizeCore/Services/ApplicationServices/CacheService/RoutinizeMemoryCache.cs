using System;
using System.Diagnostics.CodeAnalysis;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public sealed class RoutinizeMemoryCache : IRoutinizeMemoryCache {
        
        private bool MemoryCacheEnabled { get; set; }

        private readonly HttpContext _httpContext;
        private MemoryCache MemoryCache { get; set; }
        private MemoryCacheEntryOptions EntryOptions { get; set; }

        public RoutinizeMemoryCache(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        ) {
            _httpContext = httpContextAccessor.HttpContext;
            
            MemoryCache = new MemoryCache(new MemoryCacheOptions {
                SizeLimit = long.MaxValue, //int.Parse(configuration.GetSection("CacheSettings")["Size"]),
                CompactionPercentage = double.Parse(configuration.GetSection("CacheSettings")["Compaction"]),
                ExpirationScanFrequency = TimeSpan.FromSeconds(int.Parse(configuration.GetSection("CacheSettings")["ScanFrequency"]))
            });

            EntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(int.Parse(configuration.GetSection("CacheSettings")["SlidingExpiration"])));
            MemoryCacheEnabled = bool.Parse(configuration.GetSection("CacheSettings")["MemoryCacheEnabled"]);
        }

        public void SetCacheEntry<T>([NotNull] CacheEntry entry) {
            if (!MemoryCacheEnabled) return;
            EntryOptions.SetPriority(entry.Priority).SetSize(entry.Size);

            if (!nameof(T).Equals(nameof(AuthenticatedUser)))
                try {
                    EntryOptions.SetAbsoluteExpiration(
                            entry.AbsoluteExpiration > 0 ? TimeSpan.FromSeconds(entry.AbsoluteExpiration) : EntryOptions.SlidingExpiration.Value
                        );
                }
                catch (InvalidOperationException) {
                    EntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(SharedConstants.CacheAbsoluteExpiration));
                }

            MemoryCache.Set(entry.EntryKey, entry.Data, EntryOptions);
        }

        public T GetCacheEntryFor<T>([NotNull] string cacheKey) {
            if (!MemoryCacheEnabled) return default(T);

            try {
                var cachedItem = MemoryCache.Get<CacheEntry>(cacheKey);
                return (T) cachedItem.Data;
            }
            catch (Exception) {
                return default;
            }
        }

        public void RemoveCacheEntry([NotNull] string entryKey) {
            MemoryCache.Remove(entryKey);
        }
    }
}