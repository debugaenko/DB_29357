using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Localization;
using Aquality.Selenium.Elements.Interfaces;
using Reqnroll;
using Reqnroll.BoDi;

namespace OtodomTests_29357.Hooks
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
        }

       [AfterScenario(Order = 100)]
        public void CleanupDependencies()
        { }
    }
}