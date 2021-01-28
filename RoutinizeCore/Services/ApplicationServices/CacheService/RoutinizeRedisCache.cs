using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HelperLibrary;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public sealed class RoutinizeRedisCache : IRoutinizeRedisCache {
        
        private class CacheSettings {
            public bool RedisCacheEnabled { get; set; }
            public int RedisSlidingExpiration { get; set; }
            public int RedisAbsoluteExpiration { get; set; }
        }
        
        private readonly IDistributedCache _redisCache;
        private readonly CacheSettings _cacheSettings = new();

        public RoutinizeRedisCache(
            IDistributedCache redisCache,
            IConfiguration configuration
        ) {
            _redisCache = redisCache;

            _cacheSettings.RedisCacheEnabled = bool.Parse(configuration.GetSection("CacheSettings")["RedisCacheEnabled"]);
            _cacheSettings.RedisSlidingExpiration = int.Parse(configuration.GetSection("CacheSettings")["RedisSlidingExpiration"]);
            _cacheSettings.RedisAbsoluteExpiration = int.Parse(configuration.GetSection("CacheSettings")["RedisAbsoluteExpiration"]);
        }

        public async Task InsertRedisCacheEntry([NotNull] CacheEntry entry) {
            if (!_cacheSettings.RedisCacheEnabled) return;

            await _redisCache.SetAsync(
                $"{ entry.EntryKey }",
                Helpers.EncodeDataUtf8(entry.Data),
                new DistributedCacheEntryOptions {
                    SlidingExpiration = TimeSpan.FromDays(_cacheSettings.RedisSlidingExpiration),
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(_cacheSettings.RedisAbsoluteExpiration)
                }
            );
        }

        public async Task<T> GetRedisCacheEntry<T>([NotNull] string entryKey) {
            if (!_cacheSettings.RedisCacheEnabled) return default;

            try {
                var cachedData = await _redisCache.GetAsync(entryKey);
                return cachedData.Length == 0 ? default : Helpers.DecodeUtf8<T>(cachedData);
            }
            catch (Exception) {
                return default;
            }
        }
    }
}