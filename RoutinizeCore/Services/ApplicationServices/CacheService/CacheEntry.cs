using Microsoft.Extensions.Caching.Memory;

namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public sealed class CacheEntry {
        
        public string EntryKey { get; set; }
        
        public object Data { get; set; }

        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

        public int Size { get; set; } = 1;

        public int AbsoluteExpiration { get; set; } = -1;
    }
}