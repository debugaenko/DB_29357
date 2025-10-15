using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
using OpenQA.Selenium;

namespace DB_29357.Pages
{
    public class ResultsPageCache
    {
        private static readonly Logger Logger = Logger.Instance;
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(25);

        private IList<ILabel>? _cachedListings;
        private DateTime _cacheTimestamp = DateTime.MinValue;
        private string? _cachedUrl;
        private readonly Func<IList<ILabel>> _listingsFetcher;


        public ResultsPageCache(Func<IList<ILabel>> listingsFetcher)
        {
            _listingsFetcher = listingsFetcher ?? throw new ArgumentNullException(nameof(listingsFetcher));
        }

        public void InvalidateCache()
        {
            _cachedListings = null;
            _cacheTimestamp = DateTime.MinValue;
        }
        
        public IList<ILabel> GetListings(bool forceRefresh = false)
        {
            var currentUrl = Aquality.Selenium.Browsers.AqualityServices.Browser.Driver.Url;
            var urlChanged = _cachedUrl != null && _cachedUrl != currentUrl;

            if (urlChanged)
            {
                Logger.Info($"[ResultsPageCache] URL changed - cache invalidated");
                InvalidateCache();
            }

            if (!forceRefresh && _cachedListings != null && DateTime.Now - _cacheTimestamp < CacheLifetime)
            {
                return _cachedListings;
            }

            var elements = _listingsFetcher();

            _cachedListings = elements;
            _cacheTimestamp = DateTime.Now;
            _cachedUrl = currentUrl;

            return _cachedListings;
        }
    }
}
