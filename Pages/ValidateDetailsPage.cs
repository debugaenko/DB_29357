using Aquality.Selenium.Core.Logging;
using OtodomTests_29357.Helpers;
using OtodomTests_29357.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using OpenQA.Selenium;
using Reqnroll;

namespace OtodomTests_29357.Pages
{
    public class ValidateDetailsPage : BasePage
    {
        private static readonly By PageIndicatorSelector = By.CssSelector("strong[data-cy='adPageHeaderPrice']");

        private readonly ScenarioContext _scenarioContext;
        private readonly OtodomDetailsPage _detailsPage;

        private const string OfferContextKey = "SelectedOffer";

        public ValidateDetailsPage(
            ScenarioContext scenarioContext,
            OtodomDetailsPage detailsPage)
            : base(PageIndicatorSelector, "Validate Details Page")
        {
            _scenarioContext = scenarioContext;
            _detailsPage = detailsPage;
        }

        public void ValidateDetailsPageOpened()
        {
            State.WaitForDisplayed();
            _detailsPage.State.WaitForDisplayed();

            Logger.Debug("Details page validated by key elements");
        }

        public void VerifyOfferDetailsMatch()
        {
            Logger.Info("Starting offer details verification");
            ValidateDetailsPageOpened();

            var detailsData = ExtractDetailsPageData();
            var listingData = GetSavedListingData();

            Logger.Debug($"Extracted: Price={detailsData.Price}, Rooms={detailsData.Rooms}, Surface={detailsData.Surface:F1} m²");
            Logger.Debug($"Expected: Price={listingData.Price}, Rooms={listingData.Rooms}, Surface={listingData.Surface:F1} m²");

            var matchCount = ValidateDataMatches(detailsData, listingData);
            Logger.Info($"Verification complete: {matchCount} matches");
        }

        private OfferData ExtractDetailsPageData()
        {
            var allParams = _detailsPage.ExtractAllDetailsParameters();

            return new OfferData
            {
                Price = ParsingHelper.ParseSafely(allParams.GetValueOrDefault("detail_price"), ParsingHelper.ParsePriceToPln, "ExtractDetailsPageData"),
                Rooms = ParsingHelper.ParseSafely(allParams.GetValueOrDefault("detail_rooms"), ParsingHelper.ParseRooms, "ExtractDetailsPageData"),
                Surface = ParsingHelper.ParseSafely(allParams.GetValueOrDefault("detail_surface"), ParsingHelper.ParseSurfaceM2, "ExtractDetailsPageData")
            };
        }

        private OfferData GetSavedListingData()
        {
            var offerContext = _scenarioContext.Get<OfferContextData>(OfferContextKey);

            return new OfferData
            {
                Price = offerContext.Price,
                Rooms = offerContext.Rooms,
                Surface = offerContext.Surface
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
}
