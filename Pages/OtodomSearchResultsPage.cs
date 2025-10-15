using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
using DB_29357.Helpers;
using OpenQA.Selenium;

namespace DB_29357.Pages
{
    public class OtodomSearchResultsPage : BasePage
    {
        private readonly ListingDataExtractor _dataExtractor;
        private readonly ResultsPageFilters _filters;
        private DateTime? _lastValidationTime;
        private bool? _lastValidationResult;

        private ResultsPageCache? _cache;
        private ResultsPageCache Cache => _cache ??= new ResultsPageCache(() => ElementFactory.FindElements<ILabel>(OtodomSelectors.ResultsPage.ListingCards));
        private IList<ILabel> ApartmentListings => Cache.GetListings();
        private IList<ILabel> EnhancedApartmentListings => ElementFactory.FindElements<ILabel>(OtodomSelectors.ResultsPage.ListingCardsFallback);
        private ITextBox PriceMinFilter => ElementFactory.GetTextBox(OtodomSelectors.ResultsPage.PriceMinFilter,"Price Min Filter");
        private ITextBox PriceMaxFilter => ElementFactory.GetTextBox(OtodomSelectors.ResultsPage.PriceMaxFilter,"Price Max Filter");
        private ITextBox SurfaceMinFilter => ElementFactory.GetTextBox(OtodomSelectors.ResultsPage.SurfaceMinFilter,"Surface Min Filter");
        private ITextBox SurfaceMaxFilter => ElementFactory.GetTextBox( OtodomSelectors.ResultsPage.SurfaceMaxFilter,"Surface Max Filter");
        private IButton SearchButton => ElementFactory.GetButton(OtodomSelectors.ResultsPage.SearchButton,"Search Button");
        private IButton ClearPriceButton => ElementFactory.GetButton(OtodomSelectors.ResultsPage.ClearPriceButton,"Clear Price Button");

        public OtodomSearchResultsPage(ListingDataExtractor dataExtractor, ResultsPageFilters filters) : base(OtodomSelectors.ResultsPage.PageIndicator, "Search Results Page")
        {
            _dataExtractor = dataExtractor ?? throw new ArgumentNullException(nameof(dataExtractor));
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        }

        public int GetApartmentListingsCount()
        {
            return GetApartmentListings()?.Count ?? 0;
        }

        public IList<ILabel> GetApartmentListings()
        {
            var listings = ApartmentListings;
            if (listings.Count == 0)
            {
                listings = GetFallbackListings();
            }
            Logger.Info($"Found {listings.Count} apartment listings");
            return listings;
        }
        private IList<ILabel> GetFallbackListings()
        {
            Logger.Debug("[GetFallbackListings] Primary selector returned 0 listings, trying fallback selectors");
            var fallbackListings = EnhancedApartmentListings;
            Logger.Debug($"[GetFallbackListings] Fallback selectors found {fallbackListings.Count} listings");
            return fallbackListings;
        }

        public void ClearPriceFilters()
        {
            InvalidateCaches();
            _filters.ClearPriceFilters(ClearPriceButton, PriceMinFilter, PriceMaxFilter);
        }

        public void SetSurfaceFilters(double minSurface, double maxSurface)
        {
            Logger.Info($"[SetSurfaceFilters] Setting surface filters: {minSurface:F0} - {maxSurface:F0} m²");
            InvalidateCaches();
            _filters.SetSurfaceFilters(SurfaceMinFilter, SurfaceMaxFilter, minSurface, maxSurface);
        }

        public void ExecuteSearch()
        {
            InvalidateCaches();
            SearchButton.State.WaitForClickable();
            SearchButton.Click();
            Logger.Info("[ExecuteSearch] Search executed from results page");
        }
        private void InvalidateCaches()
        {
            Cache.InvalidateCache();
            InvalidateState();
        }

        public void InvalidateState()
        {
            if (_lastValidationTime.HasValue)
            {
                Logger.Debug("[InvalidateState] State manually invalidated");
            }
            _lastValidationTime = null;
            _lastValidationResult = null;
        }

        public List<(int? price, int? rooms, double? surface)> ExtractListingData()
        {
            var listings = GetApartmentListings();
            if (listings.Count == 0)
            {
                Logger.Warn("[ExtractListingData] No listings found to extract data from");
                return new List<(int? price, int? rooms, double? surface)>();
            }
            return _dataExtractor.ExtractBatchAsTuples(listings,GetSurfaceElementFromListing, maxListings: 36);
        }

        public ILabel? GetSurfaceElementFromListing(ILabel listing)
        {
            if (!IsValidListing(listing))
            {
                return null;
            }

            Logger.Debug("[GetSurfaceElementFromListing] Searching for surface element in listing card");

            var surfaceSelectors = OtodomSelectors.ResultsPage.GetSurfaceElementSelectors();
            return FindElementBySelectors(listing, surfaceSelectors, "Surface", IsSurfaceElement);
        }

        private bool IsValidListing(ILabel? listing)
        {
            if (listing == null || !listing.State.IsDisplayed)
            {
                Logger.Debug("[IsValidListing] Cannot get element from null or non-displayed listing");
                return false;
            }
            return true;
        }

        private ILabel? FindElementBySelectors(ILabel listing, IEnumerable<By> selectors, string elementType, Func<string, bool> textValidator)
        {
            foreach (var selector in selectors)
            {
                var element = TryFindElement(listing, selector, elementType, textValidator);
                if (element != null)
                {
                    return element;
                }
            }

            Logger.Debug($"[FindElementBySelectors] No {elementType.ToLower()} element found in listing card");
            return null;
        }
        private ILabel? TryFindElement(ILabel listing, By selector, string elementType, Func<string, bool> textValidator)
        {
           var elements = listing.FindChildElements<ILabel>(selector, $"{elementType} Element");

            foreach (var element in elements)
            {
                if (!element.State.IsDisplayed)
                    continue;

                var text = element.GetText();

                if (textValidator(text))
                {
                    Logger.Debug($"[TryFindElement] Found {elementType.ToLower()} element: '{text}'");
                    return element;
                }
            }  
            return null;
        }
        private bool IsSurfaceElement(string text)
        {
            return !text.Contains("zł/") && !text.Contains("PLN/");
        }

        public void ClickListingByIndex(int index)
        {
            var listings = GetApartmentListings();

            if (index < 0 || index >= listings.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"[ClickListingByIndex] Index {index} out of range (0-{listings.Count - 1})");
            }
            Logger.Info($"[ClickListingByIndex] Clicking listing #{index + 1} of {listings.Count}");
            var listing = listings[index];
            if (listing == null)
            {
                Logger.Error($"[ClickListingByIndex] Listing at index {index} is null");
                throw new InvalidOperationException($"[ClickListingByIndex] Listing at index {index} is null");
            }
            listing.State.WaitForClickable();
            if (!listing.State.IsClickable)
            {
                throw new InvalidOperationException($"[ClickListingByIndex] Listing #{index + 1} is not clickable");
            }
            listing.Click();
            Logger.Info($"[ClickListingByIndex] Successfully clicked listing #{index + 1}");
        }
    }
}