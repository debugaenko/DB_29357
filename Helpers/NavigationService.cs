using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using DB_29357.Pages;
using OpenQA.Selenium;
using Reqnroll;
using Aquality.Selenium.Elements.Interfaces;
public class NavigationService
{
    private static readonly Logger Logger = Logger.Instance;
    private readonly ScenarioContext _scenarioContext;
    private readonly OtodomSearchResultsPage _resultsPage;
    private readonly OtodomDetailsPage _detailsPage;
    private readonly IElementFactory _elementFactory;

    public NavigationService(
        ScenarioContext scenarioContext,
        OtodomSearchResultsPage resultsPage,
        OtodomDetailsPage detailsPage,
        IElementFactory elementFactory)
    {
        _scenarioContext = scenarioContext;
        _resultsPage = resultsPage;
        _detailsPage = detailsPage;
        _elementFactory = elementFactory;
    }

    public void NavigateToSelectedOffer()
    {
        var selectedIndex = _scenarioContext.Get<int>("SelectedListingIndex");
        var currentUrl = AqualityServices.Browser.CurrentUrl;

        Logger.Debug($"Current URL before click: {currentUrl}");
        _resultsPage.ClickListingByIndex(selectedIndex);
        
        WaitForDetailsPageToLoad();
        
        _scenarioContext["OfferDetailsUrl"] = AqualityServices.Browser.CurrentUrl;
        Logger.Info($"✓ Navigated to details page: {AqualityServices.Browser.CurrentUrl}");
    }

    private void WaitForDetailsPageToLoad()
    {
        Logger.Debug("Waiting for navigation to details page...");
        _detailsPage.State.WaitForDisplayed();
        
        var titleElement = _elementFactory.GetLabel(
            By.CssSelector("[data-cy='adPageAdTitle']"), 
            "Ad Title");
        titleElement.State.WaitForDisplayed();
        
        Logger.Debug($"Navigated to: {AqualityServices.Browser.CurrentUrl}");
    }
}