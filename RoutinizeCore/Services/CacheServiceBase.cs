using System;
using System.Threading.Tasks;
using HelperLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using RoutinizeCore.Services.ApplicationServices.CacheService;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Services {

    public class CacheServiceBase {

        private readonly RoutinizeMemoryCache _memoryCache;
        private readonly IDistributedCache _redisCache;
        private readonly HttpContext _httpContext;
        private readonly CacheSettings _cacheSettings = new CacheSettings();
        
        private class CacheSettings {
            public bool MemoryCacheEnabled { get; set; }
            public bool RedisCacheEnabled { get; set; }
            public int RedisSlidingExpiration { get; set; }
            public int RedisAbsoluteExpiration { get; set; }
        }

        protected CacheServiceBase() { }

        protected CacheServiceBase(
            IDistributedCache redisCache,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration
        ) {
            _redisCache = redisCache;
            _httpContext = httpContextAccessor.HttpContext;
            SetCacheSettings(configuration);
        }

        protected CacheServiceBase(
            RoutinizeMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration
        ) {
            _memoryCache = memoryCache;
            _httpContext = httpContextAccessor.HttpContext;
            SetCacheSettings(configuration);
        }

        private void SetCacheSettings(IConfiguration configuration) {
            _cacheSettings.MemoryCacheEnabled = bool.Parse(configuration.GetSection("CacheSettings")["MemoryCacheEnabled"]);
            _cacheSettings.RedisCacheEnabled = bool.Parse(configuration.GetSection("CacheSettings")["RedisCacheEnabled"]);
            _cacheSettings.RedisSlidingExpiration = int.Parse(configuration.GetSection("CacheSettings")["RedisSlidingExpiration"]);
            _cacheSettings.RedisAbsoluteExpiration = int.Parse(configuration.GetSection("CacheSettings")["RedisAbsoluteExpiration"]);
        }

        protected async Task InsertRedisCacheEntry<T>(CacheEntry entry) {
            if (!_cacheSettings.RedisCacheEnabled) return;

            if (!Helpers.IsProperString(entry.EntryKey)) {
                var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
                entry.EntryKey = $"{ nameof(T) }_{ accountId }";
            }

            await _redisCache.SetAsync(
                $"{ entry.EntryKey }",
                Helpers.EncodeDataUtf8(entry.Data),
                new DistributedCacheEntryOptions {
                    SlidingExpiration = TimeSpan.FromDays(_cacheSettings.RedisSlidingExpiration),
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(_cacheSettings.RedisAbsoluteExpiration)
                }
            );
        }

        protected async Task<T> GetRedisCacheEntry<T>(string entryKey = null) {
            if (!_cacheSettings.RedisCacheEnabled) return default(T);

            if (!Helpers.IsProperString(entryKey)) {
                var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
                entryKey = $"{ nameof(T) }_{ accountId }";
            }

            var cachedData = await _redisCache.GetAsync(entryKey);
            return cachedData.Length == 0 ? default(T) : Helpers.DecodeUtf8<T>(cachedData);
        }

        protected void InsertMemoryCacheEntry<T>(CacheEntry entry) {
            if (!_cacheSettings.MemoryCacheEnabled) return;
            
            if (!Helpers.IsProperString(entry.EntryKey)) {
                var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
                entry.EntryKey = $"{nameof(T)}_{accountId}";
            }

            _memoryCache.SetCacheEntry(entry);
        }

        protected T GetMemoryCacheEntry<T>(string entryKey = null) {
            if (!_cacheSettings.MemoryCacheEnabled) return default(T);

            if (Helpers.IsProperString(entryKey))
                return _memoryCache.GetCacheEntryFor<T>(entryKey);
            
            var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
            entryKey = $"{ nameof(T) }_{ accountId }";

            return _memoryCache.GetCacheEntryFor<T>(entryKey);
        }
    }
}