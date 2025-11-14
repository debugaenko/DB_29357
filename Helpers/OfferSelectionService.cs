using Aquality.Selenium.Core.Logging;
using OtodomTests_29357.Models;
using OtodomTests_29357.Pages;
using FluentAssertions;
using Reqnroll;
using Aquality.Selenium.Browsers;

public class OfferSelectionService
{
    private static readonly Logger Logger = AqualityServices.Logger;
    private readonly ScenarioContext _scenarioContext;
    private readonly OtodomSearchResultsPage _resultsPage;
    private readonly NavigationService _navigationService;
    
    private const string OfferContextKey = "SelectedOffer";
    
    public OfferSelectionService(
        ScenarioContext scenarioContext,
        OtodomSearchResultsPage resultsPage,
        NavigationService navigationService)
    {
        _scenarioContext = scenarioContext;
        _resultsPage = resultsPage;
        _navigationService = navigationService;
    }

    public void SelectAndSaveRandomOffer()
    {
        var apartmentCount = ValidateListingsExist();
        var listingData = _resultsPage.ExtractListingData();
        Logger.Debug($"Retrieved {listingData.Count} listing data entries");
        
        var selectedIndex = SelectRandomIndex(listingData.Count);
        var selectedData = ExtractSelectedOfferData(listingData, selectedIndex);
        
        SaveOfferToContext(selectedIndex, selectedData);
        Logger.Info($"Offer #{selectedIndex + 1} selected: Price={FormatPrice(selectedData.price)}, Rooms={selectedData.rooms}, Surface={selectedData.surface:F1} m²");
    }

    private int ValidateListingsExist()
    {
        var count = _resultsPage.GetApartmentListingsCount();
        count.Should().BeGreaterThanOrEqualTo(1, "Should find at least 1 apartment");
        return count;
    }

    private int SelectRandomIndex(int count)
    {
        var maxListings = Math.Min(count, 10);
        return Random.Shared.Next(0, maxListings);
    }

    private (int? price, int? rooms, double? surface) ExtractSelectedOfferData(
        List<(int? price, int? rooms, double? surface)> listingData, 
        int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= listingData.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedIndex),
                $"Index must be between 0 and {listingData.Count - 1}, but was {selectedIndex}");
        }
        return listingData[selectedIndex];
    }

    private void SaveOfferToContext(int index, (int? price, int? rooms, double? surface) data)
    {
        var offerData = new OfferContextData
        {
            SelectedListingIndex = index,
            Price = data.price,
            Rooms = data.rooms,
            Surface = data.surface
        };
        
        _scenarioContext[OfferContextKey] = offerData;
        Logger.Debug($"Saved offer to context: Index={index}, Price={data.price}, Rooms={data.rooms}, Surface={data.surface}");
    }

    private string FormatPrice(int? price) => price.HasValue ? $"{price.Value:N0} PLN" : "null";

    public void NavigateToSelectedOffer()
    {
        var offerData = _scenarioContext.Get<OfferContextData>(OfferContextKey);
        var currentUrl = AqualityServices.Browser.CurrentUrl;

        Logger.Debug($"Current URL before click: {currentUrl}");
        _resultsPage.ClickListingByIndex(offerData.SelectedListingIndex);

        _navigationService.WaitForDetailsPageToLoad();

        _scenarioContext["OfferDetailsUrl"] = AqualityServices.Browser.CurrentUrl;
        Logger.Info($"✓ Navigated to details page: {AqualityServices.Browser.CurrentUrl}");
    }
}