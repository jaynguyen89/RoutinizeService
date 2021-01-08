namespace RoutinizeCore.Services.ApplicationServices.CacheService {

    public sealed class CacheOptions {
        
        public string Size { get; set; }
        
        public string Compaction { get; set; }
        
        public string ScanFrequency { get; set; }
        
        public string SlidingExpiration { get; set; }
    }
}