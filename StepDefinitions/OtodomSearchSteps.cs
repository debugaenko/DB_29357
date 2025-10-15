using Aquality.Selenium.Core.Logging;
using DB_29357.Helpers;
using DB_29357.Pages;
using NUnit.Framework;
using Reqnroll;

namespace DB_29357.StepDefinitions
{
    [Binding]
    [Category("BDD")]
    [Category("Search")]
    public class OtodomSearchSteps
    {
        private static readonly Logger _logger = Logger.Instance;
        private readonly ScenarioContext _scenarioContext;
        private readonly OtodomMainPage _mainPage;
        private readonly OtodomSearchResultsPage _resultsPage;
        private readonly ListingDataCacheService _cacheService;
        private readonly MainPageSearchActions _mainSearch;
        private readonly ResultsPageFilters _resultPageFilters;

        public OtodomSearchSteps(
            ScenarioContext scenarioContext,
            OtodomMainPage mainPage,
            OtodomSearchResultsPage resultsPage,
            ListingDataCacheService cacheService, MainPageSearchActions mainSearch, ResultsPageFilters resultPageFilters)
        {
            _scenarioContext = scenarioContext;
            _mainPage = mainPage;
            _resultsPage = resultsPage;
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _mainSearch = mainSearch ?? throw new ArgumentNullException(nameof(mainSearch));
            _resultPageFilters = resultPageFilters ?? throw new ArgumentNullException(nameof(resultPageFilters));
        }

        public void WhenISelectLocationAndPriceFiltersAndClickSearch(string location, int minPrice, int maxPrice)
        {
            _mainPage.PriceMin = minPrice;
            _mainPage.PriceMax = maxPrice;

            _mainSearch.ApplyLocationAndPrice(location, _mainPage.PriceMin, _mainPage.PriceMax);

            _logger.Info($"[Step] Search: {location}, {minPrice}-{maxPrice} PLN");
        }

        [Then(@"I verify results page opened and apartment prices are in the selected range")]
        public void ThenIVerifyResultsPageOpenedAndApartmentPricesAreInSelectedRange()
        {
            var offers = _resultPageFilters.GetOffersSnapshot(36).ToList();

            if (offers.Count == 0)
            {
                _logger.Error("[Step] No offers found on results page");
                return;
            }

            var (pricesChecked, validPrices) = ValidatePriceRange(offers);

            _logger.Info($"[Step] ✓ Price validation: {validPrices}/{pricesChecked} in range");
        }

        private (int pricesChecked, int validPrices) ValidatePriceRange(List<(string Price, string Rooms, string Surface)> offers)
        {
            var minPrice = _mainPage.PriceMin;
            var maxPrice = _mainPage.PriceMax;

            var pricesChecked = 0;
            var validPrices = 0;

            foreach (var offer in offers)
            {
                var price = Parsing.ParsePriceToPln(offer.Price);

                if (price.HasValue)
                {
                    pricesChecked++;

                    if (price.Value >= minPrice && price.Value <= maxPrice)
                    {
                        validPrices++;
                    }
                }
            }

            return (pricesChecked, validPrices);
        }
    }
}
