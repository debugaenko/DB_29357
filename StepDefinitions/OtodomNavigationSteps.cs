using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
using DB_29357.Pages;
using FluentAssertions;
using NUnit.Framework;
using OtodomTests.Support;
using Reqnroll;

namespace DB_29357.StepDefinitions
{
    [Binding]
    [Category("BDD")]
    [Category("Navigation")]
    public class OtodomNavigationSteps
    {
        private static readonly Logger _logger = AqualityServices.Logger;
        private readonly OtodomMainPage _mainPage;
        private readonly TestConfig _config;
        private readonly OtodomSearchSteps _otodomSearchSteps;

        public OtodomNavigationSteps(ScenarioContext scenarioContext, OtodomMainPage mainPage, TestConfig config, OtodomSearchSteps otodomSearchSteps)
        {
            _mainPage = mainPage;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _otodomSearchSteps = otodomSearchSteps;
        }

        [Given(@"I open the Otodom main page and verify it loads correctly")]
        public void GivenIOpenTheOtodomMainPageAndVerifyItLoadsCorrectly()
        {
            _mainPage.GivenIOpenTheOtodomMainPageAndVerifyItLoadsCorrectly();
        }

        [When(@"I pass through the authorization process with valid credentials")]
        public void WhenIPassThroughTheAuthorizationProcessWithValidCredentials()
        {
            _mainPage.WhenIPassThroughTheAuthorizationProcessWithValidCredentials();
        }

        [Then(@"I verify the user is authorized and main page is opened")]
        public void ThenIVerifyTheUserIsAuthorizedAndMainPageIsOpened()
        {
            _mainPage.ThenIVerifyTheUserIsAuthorizedAndMainPageIsOpened();
        }

        [When(@"I select location ""(.*)"" and price filters (\d+) to (\d+) and click Search")]
        public void WhenISelectLocationAndPriceFiltersAndClickSearch(string location, int minPrice, int maxPrice)
        {
            _otodomSearchSteps.WhenISelectLocationAndPriceFiltersAndClickSearch(location, minPrice, maxPrice);
        }
    }
}
