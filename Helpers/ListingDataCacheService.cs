using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Browsers;
using Reqnroll;

namespace DB_29357.Helpers
{
    public class ListingDataCacheService
    {
        private static readonly Logger Logger = Logger.Instance;
        private readonly ScenarioContext _scenarioContext;
        
        private const string CacheKey = "CachedListingData";
        private const string TimestampKey = "CacheTimestamp";
        private const string UrlKey = "CachedPageUrl";
        private const int CacheExpirationSeconds = 30;

        public ListingDataCacheService(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
        }

        public List<(int? price, int? rooms, double? surface)>? TryGetCache(string currentUrl)
        {
            if (!IsCacheValid(currentUrl))
            {
                return null;
            }
            if (_scenarioContext.TryGetValue(CacheKey, out List<(int? price, int? rooms, double? surface)> cachedData))
            {
                Logger.Info($"[TryGetCache] Cache hit: {cachedData.Count} listings reused");
                return cachedData;
            }
            return null;
        }

        public void SetCache(
            List<(int? price, int? rooms, double? surface)> data, 
            string currentUrl)
        {
            if (data == null || data.Count == 0)
            {
                return;
            }

            _scenarioContext.Set(data, CacheKey);
            _scenarioContext.Set(DateTime.UtcNow, TimestampKey);
            _scenarioContext.Set(currentUrl, UrlKey);
        }
        public void InvalidateCache()
        {
            _scenarioContext.Remove(CacheKey);
            _scenarioContext.Remove(TimestampKey);
            _scenarioContext.Remove(UrlKey);
        }

        private bool IsCacheValid(string currentUrl)
        {
            if (!_scenarioContext.ContainsKey(CacheKey))
            {
                return false;
            }

            if (_scenarioContext.TryGetValue(UrlKey, out string cachedUrl))
            {
                if (cachedUrl != currentUrl)
                {
                    Logger.Debug($"[IsCacheValid] Cache invalid: URL changed from '{cachedUrl}' to '{currentUrl}'");
                    return false;
                }
            }

            if (_scenarioContext.TryGetValue(TimestampKey, out DateTime timestamp))
            {
                var age = DateTime.UtcNow - timestamp;
                if (age.TotalSeconds > CacheExpirationSeconds)
                {
                    Logger.Debug($"[IsCacheValid] Cache expired: {age.TotalSeconds:F1}s old (max: {CacheExpirationSeconds}s)");
                    return false;
                }
            }

            return true;
        }
        /*public (bool HasCache, int Count, TimeSpan Age) GetCacheStats()
        {
            if (!_scenarioContext.TryGetValue(CacheKey, out List<(int? price, int? rooms, double? surface)> data))
            {
                return (false, 0, TimeSpan.Zero);
            }

            var age = _scenarioContext.TryGetValue(TimestampKey, out DateTime timestamp)
                ? DateTime.UtcNow - timestamp
                : TimeSpan.Zero;

            return (true, data.Count, age);
        }*/

        public List<(int? price, int? rooms, double? surface)> GetOrExtract(
        Func<List<(int? price, int? rooms, double? surface)>> extractor,
        string context = null)
        {
            var currentUrl = AqualityServices.Browser.Driver.Url;
            var cachedData = TryGetCache(currentUrl);

            if (cachedData != null)
            {
                return cachedData;
            }

            var contextMsg = string.IsNullOrEmpty(context) ? "" : $" [{context}]";
            Logger.Info($"[GetOrExtract]{contextMsg} Cache miss: extracting listings");

            var listingData = extractor();
            SetCache(listingData, currentUrl);
            return listingData;
        }
    }
}
