using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using OtodomTests_29357.Pages;
using OpenQA.Selenium;
using Reqnroll;
using Aquality.Selenium.Elements.Interfaces;
public class NavigationService
{
    private static readonly Logger Logger = AqualityServices.Logger;
    private readonly OtodomDetailsPage _detailsPage;
    private readonly IElementFactory _elementFactory;

    public NavigationService(
        ScenarioContext scenarioContext,
        OtodomSearchResultsPage resultsPage,
        OtodomDetailsPage detailsPage,
        IElementFactory elementFactory)
    {
        _detailsPage = detailsPage;
        _elementFactory = elementFactory;
    }

    public void WaitForDetailsPageToLoad()
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