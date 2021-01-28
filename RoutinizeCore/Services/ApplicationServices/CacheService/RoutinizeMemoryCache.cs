using System;
using System.Diagnostics.CodeAnalysis;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
                SizeLimit = int.Parse(configuration.GetSection("CacheSettings")["Size"]),
                CompactionPercentage = double.Parse(configuration.GetSection("CacheSettings")["Compaction"]),
                ExpirationScanFrequency = TimeSpan.FromSeconds(int.Parse(configuration.GetSection("CacheSettings")["ScanFrequency"]))
            });

            EntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(int.Parse(configuration.GetSection("CacheSettings")["SlidingExpiration"])));
            MemoryCacheEnabled = bool.Parse(configuration.GetSection("CacheSettings")["MemoryCacheEnabled"]);
        }

        public void SetCacheEntry<T>([NotNull] CacheEntry entry) {
            if (!MemoryCacheEnabled) return;
            EntryOptions.SetPriority(entry.Priority).SetSize(entry.Size);
            
            if (!Helpers.IsProperString(entry.EntryKey)) {
                var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
                entry.EntryKey = $"{ nameof(T) }_{ accountId }";
            }

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

        public T GetCacheEntryFor<T>([AllowNull] string cacheKey = null) {
            if (!MemoryCacheEnabled) return default(T);
            
            if (Helpers.IsProperString(cacheKey))
                return MemoryCache.Get<T>(cacheKey);
            
            var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
            cacheKey = $"{ nameof(T) }_{ accountId }";
            
            return MemoryCache.Get<T>(cacheKey);
        }
    }
}