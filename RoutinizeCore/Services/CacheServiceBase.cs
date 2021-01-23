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

        protected async Task InsertRedisCacheEntry(CacheEntry entry) {
            if (!_cacheSettings.RedisCacheEnabled) return;
            
            var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));

            await _redisCache.SetAsync(
                $"{ entry.EntryKey }_{ accountId }",
                Helpers.EncodeDataUtf8(entry.Data),
                new DistributedCacheEntryOptions {
                    SlidingExpiration = TimeSpan.FromDays(_cacheSettings.RedisSlidingExpiration),
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(_cacheSettings.RedisAbsoluteExpiration)
                }
            );
        }

        protected async Task<T> GetRedisCacheEntry<T>(string entryKey) {
            var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));

            var cachedData = await _redisCache.GetAsync($"{ entryKey }_{ accountId }");
            return cachedData == null ? default : Helpers.DecodeUtf8<T>(cachedData);
        }

        protected void InsertMemoryCacheEntry(CacheEntry entry) {
            if (!_cacheSettings.MemoryCacheEnabled) return;
            
            var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
            entry.EntryKey = $"{ entry.EntryKey }_{ accountId }";
                
            _memoryCache.SetCacheEntry(entry);
        }

        protected T GetMemoryCacheEntry<T>(string entryKey) {
            if (!_cacheSettings.MemoryCacheEnabled) return default;
            
            var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
            return _memoryCache.GetCacheEntryFor<T>($"{ entryKey }_{ accountId }");
        }
    }
}