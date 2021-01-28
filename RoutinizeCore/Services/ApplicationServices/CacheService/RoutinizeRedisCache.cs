using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HelperLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public sealed class RoutinizeRedisCache : IRoutinizeRedisCache {
        
        private class CacheSettings {
            public bool RedisCacheEnabled { get; set; }
            public int RedisSlidingExpiration { get; set; }
            public int RedisAbsoluteExpiration { get; set; }
        }
        
        private readonly HttpContext _httpContext;
        private readonly IDistributedCache _redisCache;
        private readonly CacheSettings _cacheSettings = new CacheSettings();

        public RoutinizeRedisCache(
            IDistributedCache redisCache,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration
        ) {
            _httpContext = httpContextAccessor.HttpContext;
            _redisCache = redisCache;

            _cacheSettings.RedisCacheEnabled = bool.Parse(configuration.GetSection("CacheSettings")["RedisCacheEnabled"]);
            _cacheSettings.RedisSlidingExpiration = int.Parse(configuration.GetSection("CacheSettings")["RedisSlidingExpiration"]);
            _cacheSettings.RedisAbsoluteExpiration = int.Parse(configuration.GetSection("CacheSettings")["RedisAbsoluteExpiration"]);
        }

        public async Task InsertRedisCacheEntry<T>([NotNull] CacheEntry entry) {
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

        public async Task<T> GetRedisCacheEntry<T>([AllowNull] string entryKey = null) {
            if (!_cacheSettings.RedisCacheEnabled) return default(T);

            if (!Helpers.IsProperString(entryKey)) {
                var accountId = _httpContext.Session.GetInt32(nameof(AuthenticatedUser.AccountId));
                entryKey = $"{ nameof(T) }_{ accountId }";
            }

            var cachedData = await _redisCache.GetAsync(entryKey);
            return cachedData.Length == 0 ? default(T) : Helpers.DecodeUtf8<T>(cachedData);
        }
    }
}