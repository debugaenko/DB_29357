using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
using OtodomTests_29357.Helpers;
using OpenQA.Selenium;
using Aquality.Selenium.Browsers;

namespace OtodomTests_29357.Pages
{
    public class OtodomSearchResultsPage : BasePage
    {
        private static readonly By ListingCardsSelector = By.CssSelector("article[data-sentry-component='AdvertCard']");
        private static readonly By ListingCardsFallbackSelector = By.XPath("//article[contains(@class, 'offer')] | //div[contains(@class, 'listing')] | //div[contains(@data-cy, 'listing')]");
        private static readonly By PriceMinFilterSelector = By.XPath("//input[@data-cy='search-form--field--priceMin']");
        private static readonly By PriceMaxFilterSelector = By.XPath("//input[@data-cy='search-form--field--priceMax']");
        private static readonly By SurfaceMinFilterSelector = By.XPath("//input[@data-cy='search-form--field--areaMin']");
        private static readonly By SurfaceMaxFilterSelector = By.XPath("//input[@data-cy='search-form--field--areaMax']");
        private static readonly By SearchButtonSelector = By.XPath("//span[contains(text(), 'Wyniki')]/parent::button");
        private static readonly By ClearPriceButtonSelector = By.XPath("//button[contains(@class, 'clear') and contains(@data-cy, 'price')]");
        private static readonly By PageIndicatorSelector = By.XPath("//div[contains(@class, 'results') or contains(@class, 'listing')]");

        private static readonly By[] SurfaceElementSelectors = new[]
        {
            By.XPath(".//dt[contains(text(),'Powierzchnia')]/following-sibling::dd[1]"),
            By.XPath(".//div[@data-sentry-element='SpecsListWrapper']//span[contains(text(),'m²') or contains(text(),'m2')]"),
            By.XPath(".//span[contains(text(), 'm²') or contains(text(), 'm2') and not(contains(text(), '/'))]")
        };

        private readonly ListingDataExtractor _dataExtractor;
        private readonly ResultsPageFilters _filters;
        private DateTime? _lastValidationTime;
        private bool? _lastValidationResult;
        private readonly Logger Logger = AqualityServices.Logger;

        private IList<ILabel> ApartmentListings => ElementFactory.FindElements<ILabel>(ListingCardsSelector);
        private IList<ILabel> EnhancedApartmentListings => ElementFactory.FindElements<ILabel>(ListingCardsFallbackSelector);
        private ITextBox PriceMinFilter => ElementFactory.GetTextBox(PriceMinFilterSelector, "Price Min Filter");
        private ITextBox PriceMaxFilter => ElementFactory.GetTextBox(PriceMaxFilterSelector, "Price Max Filter");
        private ITextBox SurfaceMinFilter => ElementFactory.GetTextBox(SurfaceMinFilterSelector, "Surface Min Filter");
        private ITextBox SurfaceMaxFilter => ElementFactory.GetTextBox(SurfaceMaxFilterSelector, "Surface Max Filter");
        private IButton SearchButton => ElementFactory.GetButton(SearchButtonSelector, "Search Button");
        private IButton ClearPriceButton => ElementFactory.GetButton(ClearPriceButtonSelector, "Clear Price Button");

        public OtodomSearchResultsPage(ListingDataExtractor dataExtractor, ResultsPageFilters filters) 
            : base(PageIndicatorSelector, "Search Results Page")
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
            InvalidateState();
            _filters.ClearPriceFilters(ClearPriceButton, PriceMinFilter, PriceMaxFilter);
        }

        public void SetSurfaceFilters(double minSurface, double maxSurface)
        {
            Logger.Info($"[SetSurfaceFilters] Setting surface filters: {minSurface:F0} - {maxSurface:F0} m²");
            InvalidateState();
            _filters.SetSurfaceFilters(SurfaceMinFilter, SurfaceMaxFilter, minSurface, maxSurface);
        }

        public void ExecuteSearch()
        {
            InvalidateState();
            SearchButton.State.WaitForClickable();
            SearchButton.Click();
            Logger.Info("[ExecuteSearch] Search executed from results page");
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
            return _dataExtractor.ExtractBatchAsTuples(listings, GetSurfaceElementFromListing, maxListings: 36);
        }

        public ILabel? GetSurfaceElementFromListing(ILabel listing)
        {
            if (!IsValidListing(listing))
            {
                return null;
            }

            Logger.Debug("[GetSurfaceElementFromListing] Searching for surface element in listing card");
            return FindElementBySelectors(listing, SurfaceElementSelectors, "Surface", IsSurfaceElement);
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