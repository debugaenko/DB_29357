using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Localization;
using Aquality.Selenium.Elements.Interfaces;
using DB_29357.Helpers;
using DB_29357.Pages;
using Reqnroll;
using Reqnroll.BoDi;

namespace DB_29357.Hooks
{
    [Binding]
    public class DependencyInjectionConfig
    {
        private readonly IObjectContainer _objectContainer;

        public DependencyInjectionConfig(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario(Order = 1)]
        public void RegisterDependencies()
        {
            _objectContainer.RegisterFactoryAs<IElementFactory>(() => AqualityServices.Get<IElementFactory>());
            _objectContainer.RegisterFactoryAs<ILocalizedLogger>(() => AqualityServices.Get<ILocalizedLogger>());

            _objectContainer.RegisterTypeAs<ListingDataCacheService, ListingDataCacheService>();

            _objectContainer.RegisterTypeAs<ListingDataExtractor, ListingDataExtractor>();
            _objectContainer.RegisterTypeAs<MainPageLoginActions, MainPageLoginActions>();
            _objectContainer.RegisterTypeAs<ResultsPageExtractor, ResultsPageExtractor>();

            _objectContainer.RegisterTypeAs<ListingDataExtractor, ListingDataExtractor>();
            _objectContainer.RegisterTypeAs<ResultsPageFilters, ResultsPageFilters>();
            _objectContainer.RegisterTypeAs<MainPageSearchActions, MainPageSearchActions>();
            _objectContainer.RegisterTypeAs<MainPageLoginActions, MainPageLoginActions>();
            _objectContainer.RegisterTypeAs<ResultsPageExtractor, ResultsPageExtractor>();

            _objectContainer.RegisterTypeAs<OtodomMainPage, OtodomMainPage>();
            _objectContainer.RegisterTypeAs<OtodomSearchResultsPage, OtodomSearchResultsPage>();
            _objectContainer.RegisterTypeAs<OtodomDetailsPage, OtodomDetailsPage>();
        }

        [AfterScenario(Order = 100)]
        public void CleanupDependencies()
        {
            // BoDi automatically handles cleanup of registered objects
            // Additional cleanup can be added here if needed
        }
    }
}