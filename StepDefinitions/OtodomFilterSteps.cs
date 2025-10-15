using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using DB_29357.Helpers;
using DB_29357.Pages;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using Reqnroll;

namespace DB_29357.StepDefinitions
{
    [Binding]
    [Category("BDD")]
    [Category("Filters")]
    public class OtodomFilterSteps
    {
        private static readonly Logger Logger = Logger.Instance;
        private readonly ScenarioContext _scenarioContext;
        private readonly OtodomSearchResultsPage _resultsPage;
        private readonly ListingDataCacheService _cacheService;
        

        public OtodomFilterSteps(ScenarioContext scenarioContext,OtodomSearchResultsPage resultsPage,
            ListingDataCacheService cacheService,ResultsPageFilters resultsPageFilters)
        {
            _scenarioContext = scenarioContext;
            _resultsPage = resultsPage;
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        
        [When(@"I remove price range filters")]
        public void WhenIRemovePriceRangeFilters()
        {
            _resultsPage.State.WaitForDisplayed();
            _resultsPage.ClearPriceFilters();
            _cacheService.InvalidateCache();

            _scenarioContext["PriceFiltersClearedAt"] = DateTime.Now;
            _scenarioContext["PriceFiltersClearStatus"] = "SUCCESS";

            Logger.Info("[Step] Price filters cleared");
        }

        [When(@"I get max and min apartment surface values from the first page and apply surface filter")]
        public void WhenIGetMaxAndMinSurfaceValuesAndApplySurfaceFilter()
        {
            _resultsPage.State.WaitForDisplayed();
            var listingData = GetOrExtractListingDataOptimized();
            var (minSurface, maxSurface, validCount) = CalculateSurfaceRange(listingData);

            StoreSurfaceParameters(minSurface, maxSurface);
            ApplySurfaceFilter(minSurface, maxSurface);
            _cacheService.InvalidateCache();

            Logger.Info($"[Step] Surface filter applied: {minSurface:F0}-{maxSurface:F0} m² ({validCount} surfaces)");
        }

        [Then(@"I verify results page opened and apartment surfaces are in the selected range")]
        public void ThenIVerifyResultsPageOpenedAndSurfacesInSelectedRange()
        {
            var minSurface = _scenarioContext.Get<double>("SurfaceMin");
            var maxSurface = _scenarioContext.Get<double>("SurfaceMax");
            _resultsPage.State.WaitForDisplayed();
            _cacheService.InvalidateCache();

            var listingData = ExtractFreshListingData();
            var apartmentCount = listingData.Count;
            apartmentCount.Should().BeGreaterThanOrEqualTo(1, "Should find at least 1 listing");
            ValidateSurfacesInRange(listingData, minSurface, maxSurface, apartmentCount);
            var surfacesValidated = listingData.Count(l => l.surface.HasValue);

            Logger.Info($"[ThenIVerifyResultsPageOpenedAndSurfacesInSelectedRange] ✓ {surfacesValidated}/{apartmentCount} surfaces validated");
            Logger.Info($"[Step] Surface validation: {surfacesValidated}/{apartmentCount} in range {minSurface:F0}-{maxSurface:F0} m²");
        }

        private List<(int? price, int? rooms, double? surface)> GetOrExtractListingDataOptimized()
        {
            return _cacheService.GetOrExtract(() => _resultsPage.ExtractListingData(), "Search");
        }

        private (double minSurface, double maxSurface, int validCount) CalculateSurfaceRange(
            List<(int? price, int? rooms, double? surface)> listingData)
        {
            var surfaceValues = listingData
                .Where(l => l.surface.HasValue && l.surface.Value >= 10 && l.surface.Value <= 500)
                .Select(l => l.surface.Value)
                .ToList();

            surfaceValues.Should().NotBeEmpty("Should find at least one listing with valid surface");

            var minSurfaceRaw = surfaceValues.Min();
            var maxSurfaceRaw = surfaceValues.Max();
            var minSurface = Math.Floor(minSurfaceRaw);
            var maxSurface = Math.Ceiling(maxSurfaceRaw);

            return (minSurface, maxSurface, surfaceValues.Count);
        }

        private void StoreSurfaceParameters(double minSurface, double maxSurface)
        {
            _scenarioContext["SurfaceMin"] = minSurface;
            _scenarioContext["SurfaceMax"] = maxSurface;
            _scenarioContext["SurfaceValuesExtractedAt"] = DateTime.Now;
            _scenarioContext["SurfaceExtractionStatus"] = "SUCCESS";
        }

        private void ApplySurfaceFilter(double minSurface, double maxSurface)
        {
            _resultsPage.SetSurfaceFilters(minSurface, maxSurface);
            _resultsPage.ExecuteSearch();

            _scenarioContext["SurfaceFiltersAppliedAt"] = DateTime.Now;
            _scenarioContext["SurfaceFilterApplicationStatus"] = "SUCCESS";
        }

        /*private int ValidateListingsExist()
        {
            var apartmentCount = _resultsPage.GetApartmentListingsCount();
            apartmentCount.Should().BeGreaterThanOrEqualTo(1, "Should find at least 1 listing");
            return apartmentCount;
        }*/

        private List<(int? price, int? rooms, double? surface)> ExtractFreshListingData()
        {
            var currentUrl = AqualityServices.Browser.CurrentUrl;
            var listingData = _resultsPage.ExtractListingData();
            _cacheService.SetCache(listingData, currentUrl);
            return listingData;
        }

        private void ValidateSurfacesInRange(
            List<(int? price, int? rooms, double? surface)> listingData, double minSurface,double maxSurface,int apartmentCount)
        {
            var surfacesFound = listingData.Where(l => l.surface.HasValue).Select(l => l.surface.Value).ToList();

            surfacesFound.Should().NotBeEmpty("Should extract at least one valid surface");

            var validApartmentSurfaces = surfacesFound.Where(s => s >= 10).ToList();

            var surfacesOutOfRange = validApartmentSurfaces.Where(surface => surface < minSurface || surface > maxSurface).ToList();

            using (new AssertionScope())
            {
                surfacesOutOfRange.Should().BeEmpty($"All surfaces in range {minSurface:F0}-{maxSurface:F0} m². " +
                    $"Out of range: {string.Join(", ", surfacesOutOfRange.Select(s => $"{s:F1} m²"))}");
            }

            StoreSurfaceValidationResults(apartmentCount, surfacesFound, minSurface, maxSurface);
        }

        private void StoreSurfaceValidationResults(int apartmentCount,List<double> surfacesFound,double minSurface,double maxSurface)
        {
            var validationResults = new
            {
                TotalListings = apartmentCount,
                SurfacesValidated = surfacesFound.Count,
                MinSurfaceFound = surfacesFound.Min(),
                MaxSurfaceFound = surfacesFound.Max(),
                AllSurfacesInRange = true,
                ExpectedMinSurface = minSurface,
                ExpectedMaxSurface = maxSurface
            };

            _scenarioContext.Set(validationResults, "SurfaceValidationResults");
            _scenarioContext["SurfaceValidationCompletedAt"] = DateTime.Now;
            _scenarioContext["SurfaceValidationStatus"] = "SUCCESS";
        }
    }
}
