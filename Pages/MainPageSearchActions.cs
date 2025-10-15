//using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Logging;
//using Aquality.Selenium.Core.Waitings;
using Aquality.Selenium.Elements.Interfaces;
using OpenQA.Selenium;

namespace DB_29357.Pages
{
    public class MainPageSearchActions
    {
        private readonly Logger _logger;
        private readonly IElementFactory _elementFactory;

        private ITextBox LocationButton => _elementFactory.GetTextBox(By.CssSelector("input[data-cy='search.form.location.button']"), "Location Button");
        private ITextBox ActiveLocationInput => _elementFactory.GetTextBox(By.CssSelector("input[data-cy='search.form.location.input'], input#location-search-input"), "Active Location Input");
        private ITextBox PriceMinInput => _elementFactory.GetTextBox(By.CssSelector("input[data-cy='search-form-field-priceMin'], input[id*='priceMin'], input[name*='priceMin'], input[placeholder*='Cena od']"), "Min Price Input");
        private ITextBox PriceMaxInput => _elementFactory.GetTextBox(By.CssSelector("input[data-cy='search-form-field-priceMax'], input[id*='priceMax'], input[name*='priceMax'], input[placeholder*='Cena do']"), "Max Price Input");
        private IButton SearchButton => _elementFactory.GetButton(By.CssSelector("button[data-cy='search-form-submit'], #search-form-submit, button[type='submit']"), "Search Button");
        private IList<ILabel> DropdownSuggestions => _elementFactory.FindElements<ILabel>(By.XPath("//div[@data-sentry-source-file='SuggestionRow.tsx']"), "Dropdown Suggestions");
        private ILabel FirstDropdownSuggestion => _elementFactory.GetLabel(By.XPath("//div[@data-sentry-source-file='SuggestionRow.tsx'][1]"), "First Dropdown Suggestion");
        public MainPageSearchActions(OtodomMainPage mainPage, IElementFactory elementFactory)
        {
            _logger = Logger.Instance;
            _elementFactory = elementFactory ?? throw new ArgumentNullException(nameof(elementFactory));
        }

        public void ApplyLocationAndPrice(string location, decimal minPrice, decimal maxPrice)
        {
            _logger.Info($"[ApplyLocationAndPrice] Applying search filters: '{location}', {minPrice}-{maxPrice} PLN");

            bool locationSet = SetLocationWithSuggestions(location);
            if (!locationSet)
            {
                throw new InvalidOperationException($"[ApplyLocationAndPrice] Failed to select location: '{location}'");
            }

            SetPriceFilters(minPrice, maxPrice);

            SearchButton.State.WaitForClickable();

            PerformSearch(locationSet, minPrice, maxPrice);

            _logger.Info("[ApplyLocationAndPrice] Filters applied and search executed successfully");
        }

        private bool SetLocationWithSuggestions(string location)
        {
            if (LocationButton.State.IsDisplayed && LocationButton.State.IsEnabled)
            {
                LocationButton.Click();
            }
            
            if (ActiveLocationInput.State.IsDisplayed && ActiveLocationInput.State.IsEnabled)
            {
                ActiveLocationInput.Click();
                ActiveLocationInput.SendKeys(Keys.Control + "a");
                ActiveLocationInput.SendKeys(Keys.Backspace);
                ActiveLocationInput.SendKeys(location);
            }

            FirstDropdownSuggestion.State.WaitForDisplayed();

            if (DropdownSuggestions.Count > 0)
            {
                DropdownSuggestions[0].State.WaitForDisplayed();
            }

            return SelectLocationFromSuggestions(location);
        }

        private bool SelectLocationFromSuggestions(string location)
        {
            var exactMatchElements = _elementFactory.FindElements<ILabel>(
                By.XPath($"//div[@data-sentry-source-file='SuggestionRow.tsx'][.//mark[text()='{location}']][.//p[contains(text(), 'miasto')]]//*[@data-cy='tree-item-clickable']"));

            if (exactMatchElements.Count > 0)
            {
                var firstCity = exactMatchElements.FirstOrDefault(e => e.State.IsDisplayed);
                if (firstCity != null)
                {
                    firstCity.Click();
                    _logger.Info($"[SelectLocationFromSuggestions] City selected: '{location}' (miasto)");
                    LocationButton.State.WaitForDisplayed();
                    LocationButton.State.WaitForClickable();
                    return true;
                }
            }

            _logger.Warn($"No 'miasto' match found, using keyboard navigation for '{location}'");
            ActiveLocationInput.SendKeys(Keys.ArrowDown);
            ActiveLocationInput.SendKeys(Keys.Enter);
            return true;
        }

        private void SetPriceFilters(decimal minPrice, decimal maxPrice)
        {
            if (PriceMinInput.State.WaitForDisplayed())
            { //TimeSpan.FromSeconds(3)
                PriceMinInput.State.WaitForClickable();
                PriceMinInput.Click();
                PriceMinInput.ClearAndType(((long)minPrice).ToString());
                _logger.Info($"Price Min set to {minPrice}");
            }

            if (PriceMaxInput.State.WaitForDisplayed())
            { // TimeSpan.FromSeconds(5)
                PriceMaxInput.State.WaitForClickable();
                PriceMaxInput.Click();
                PriceMaxInput.ClearAndType(((long)maxPrice).ToString());
                _logger.Info($"Price Max set to {maxPrice}");
            }
        }

        private void PerformSearch(bool locationSet, decimal minPrice, decimal maxPrice)
        {
            SearchButton.State.WaitForClickable();

            if (SearchButton.State.IsDisplayed && SearchButton.State.IsClickable)
            {
                SearchButton.ClickAndWait();
                _logger.Info($"[MAIN_PAGE] [ApplyLocationAndPrice] Search initiated: location={locationSet}, price={minPrice}-{maxPrice}");
                return;
            }

            throw new InvalidOperationException("Cannot perform search: Search button not clickable");
        }
    }
}
