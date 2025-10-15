using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using DB_29357.Helpers;
using DB_29357.Pages;
using FluentAssertions;
using FluentAssertions.Execution;
using Reqnroll;

public class OfferDataVerificationService
{
    private static readonly Logger Logger = Logger.Instance;
    private readonly ScenarioContext _scenarioContext;
    private readonly OtodomDetailsPage _detailsPage;

    public OfferDataVerificationService(
        ScenarioContext scenarioContext,
        OtodomDetailsPage detailsPage)
    {
        _scenarioContext = scenarioContext;
        _detailsPage = detailsPage;
    }

    public void VerifyOfferDetailsMatch()
    {
        ValidateDetailsPageOpened();

        var detailsData = ExtractDetailsPageData();
        var listingData = GetSavedListingData();

        Logger.Debug($"Extracted: Price={detailsData.Price}, Rooms={detailsData.Rooms}, Surface={detailsData.Surface:F1} m²");
        Logger.Debug($"Expected: Price={listingData.Price}, Rooms={listingData.Rooms}, Surface={listingData.Surface:F1} m²");

        var matchCount = ValidateDataMatches(detailsData, listingData);
        Logger.Info($"Verification complete: {matchCount} matches");
    }

    private void ValidateDetailsPageOpened()
    {
        var currentUrl = AqualityServices.Browser.CurrentUrl;
        var isDetailsPage = currentUrl.Contains("/oferta/") || currentUrl.Contains("/mieszkanie/");

        isDetailsPage.Should().BeTrue($"Should be on details page. URL: {currentUrl}");
        _detailsPage.State.WaitForDisplayed();
        Logger.Debug("Details page content loaded");
    }

    private OfferData ExtractDetailsPageData()
    {
        var allParams = _detailsPage.ExtractAllDetailsParameters();
        
        return new OfferData
        {
            Price = Parsing.ParseSafely(allParams.GetValueOrDefault("detail_price"), Parsing.ParsePriceToPln, "ExtractDetailsPageData"),
            Rooms = Parsing.ParseSafely(allParams.GetValueOrDefault("detail_rooms"), Parsing.ParseRooms, "ExtractDetailsPageData"),
            Surface = Parsing.ParseSafely(allParams.GetValueOrDefault("detail_surface"), Parsing.ParseSurfaceM2, "ExtractDetailsPageData")
        };
    }

    private OfferData GetSavedListingData()
    {
        return new OfferData
        {
            Price = _scenarioContext.ContainsKey("ListingCardPrice")?_scenarioContext.Get<int?>("ListingCardPrice"): null,
            Rooms = _scenarioContext.ContainsKey("ListingCardRooms")? _scenarioContext.Get<int?>("ListingCardRooms"): null,
            Surface = _scenarioContext.ContainsKey("ListingCardSurface")? _scenarioContext.Get<double?>("ListingCardSurface"): null
        };
    }

    private int ValidateDataMatches(OfferData details, OfferData listing)
    {
        var comparisons = new List<(string Field, bool Matches)>
        {
            ("Price", CompareValues(listing.Price, details.Price)),
            ("Rooms", CompareValues(listing.Rooms, details.Rooms)),
            ("Surface", CompareSurface(listing.Surface, details.Surface))
        };

        var validComparisons = comparisons.Where(c => c.Matches != default).ToList();
        var matchCount = validComparisons.Count(c => c.Matches);

        using (new AssertionScope())
        {
            validComparisons.Should().NotBeEmpty("Should have comparable data");
            matchCount.Should().Be(validComparisons.Count, "All data should match");
        }

        return validComparisons.Count;
    }

    private bool CompareValues<T>(T? expected, T? actual) where T : struct
    {
        if (!expected.HasValue || !actual.HasValue) return default;
        return expected.Value.Equals(actual.Value);
    }

    private bool CompareSurface(double? expected, double? actual)
    {
        if (!expected.HasValue || !actual.HasValue) return default;
        return Math.Abs(expected.Value - actual.Value) <= 0.1;
    }

    private class OfferData
    {
        public int? Price { get; init; }
        public int? Rooms { get; init; }
        public double? Surface { get; init; }
    }
}