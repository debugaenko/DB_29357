using Aquality.Selenium.Core.Logging;
using DB_29357.Helpers;
using DB_29357.Pages;
using FluentAssertions;
using Reqnroll;

public class OfferSelectionService
{
    private static readonly Logger Logger = Logger.Instance;
    private readonly ScenarioContext _scenarioContext;
    private readonly OtodomSearchResultsPage _resultsPage;
    private readonly ListingDataCacheService _cacheService;

    public OfferSelectionService(
        ScenarioContext scenarioContext,
        OtodomSearchResultsPage resultsPage,
        ListingDataCacheService cacheService)
    {
        _scenarioContext = scenarioContext;
        _resultsPage = resultsPage;
        _cacheService = cacheService;
    }

    public void SelectAndSaveRandomOffer()
    {
        var apartmentCount = ValidateListingsExist();
        var listingData = GetOrExtractListingData();
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

    private List<(int? price, int? rooms, double? surface)> GetOrExtractListingData()
        => _cacheService.GetOrExtract(() => _resultsPage.ExtractListingData(), "Search");

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
        _scenarioContext["SelectedListingIndex"] = index;
        _scenarioContext["ListingCardPrice"] = data.price;
        _scenarioContext["ListingCardRooms"] = data.rooms;
        _scenarioContext["ListingCardSurface"] = data.surface;
    }

    private string FormatPrice(int? price) 
        => price.HasValue ? $"{price.Value:N0} PLN" : "null";
}