using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;
using OpenQA.Selenium;

namespace OtodomTests_29357.Pages
{
    public class OtodomDetailsPage : BasePage
    {
        private static readonly By PriceSelector = By.XPath("//strong[@data-cy='adPageHeaderPrice']");
        private static readonly By[] RoomSelectors = new[]
        {
            By.XPath("//div[@data-sentry-element='Item'][contains(text(),'Liczba pokoi')]/following-sibling::div[@data-sentry-element='Item']"),
            By.XPath("//div[@data-sentry-element='Item'][contains(text(),'Liczba pokoi')]/following-sibling::div[1]"),
            By.XPath("//dt[contains(text(),'Liczba pokoi')]/following-sibling::dd[1]")
        };
        private static readonly By[] SurfaceSelectors = new[]
        {
            By.XPath("//div[@data-sentry-element='Item'][contains(text(),'Powierzchnia')]/following-sibling::div[@data-sentry-element='Item']"),
            By.XPath("//div[@data-sentry-element='Item'][contains(text(),'Powierzchnia')]/following-sibling::div[1]"),
            By.XPath("//dt[contains(text(),'Powierzchnia')]/following-sibling::dd[1]")
        };

        private ILabel PriceValue => ElementFactory.GetLabel(PriceSelector, "Price Value");
        private ILabel RoomsValue => GetElementWithFallback(RoomSelectors, "Rooms Value");
        private ILabel SurfaceValue => GetElementWithFallback(SurfaceSelectors, "Surface Value");

        public OtodomDetailsPage() : base(PriceSelector, "Apartment Details Page")
        {
        }

        private Dictionary<string, Func<ILabel>> ElementMapping => new Dictionary<string, Func<ILabel>>
        {
            { "detail_price", () => PriceValue },
            { "detail_rooms", () => RoomsValue },
            { "detail_surface", () => SurfaceValue }
        };

        private ILabel GetElementWithFallback(By[] selectors, string name)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    var element = ElementFactory.GetLabel(selector, name);
                    if (element.State.IsExist)
                    {
                        Logger.Debug($"Found element '{name}' using selector: {selector}");
                        return element;
                    }
                }
                catch
                {
                    Logger.Debug($"Selector failed for '{name}': {selector}");
                    continue;
                }
            }
            
            Logger.Warn($"All selectors failed for '{name}', using primary selector");
            return ElementFactory.GetLabel(selectors[0], name);
        }

        private Dictionary<string, string?> ExtractParams(List<string> keys)
        {
            Logger.Info($"[ExtractParams] Starting extraction for {keys.Count} parameters");
            var results = new Dictionary<string, string?>();
            foreach (var key in keys)
            {
                var extractedValue = ExtractSingleParameter(key);
                results[key] = extractedValue;

                LogExtractionResult(key, extractedValue);
            }
            Logger.Info($"[ExtractParams] Extraction completed: {results.Count(r => r.Value != null)}/{keys.Count} parameters found");
            return results;
        }

        private string? ExtractSingleParameter(string key)
        {
            if (!ElementMapping.ContainsKey(key))
            {
                Logger.Warn($"[ExtractSingleParameter] Key '{key}' not found in element mapping");
                return null;
            }

            var element = GetElementByKey(key);
            if (element == null)
            {
                return null;
            }
            return GetElementTextSafely(element, key);
        }

        private ILabel? GetElementByKey(string key)
        {
            if (!ElementMapping.ContainsKey(key))
            {
                Logger.Warn($"[GetElementByKey] Key '{key}' not found in element mapping");
                return null;
            }
            return ElementMapping[key]();
        }

        private string? GetElementTextSafely(ILabel element, string key)
        {
            if (!element.State.WaitForDisplayed())
            {
                Logger.Warn($"[GetElementTextSafely] Element '{key}' not displayed within timeout");
                return null;
            }

            var text = element.GetText();

            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private void LogExtractionResult(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Logger.Info($"[LogExtractionResult] Parameter '{key}' extraction failed or returned empty value");
            }
            else
            {
                Logger.Info($"[LogExtractionResult] Extracted parameter '{key}': {value}");
            }
        }

        public string ExtractPriceFromDetails()
        {
            Logger.Info("[DETAILS_PAGE] [ExtractPriceFromDetails] Starting price extraction");

            var results = ExtractParams(new List<string> { "detail_price" });
            var price = results.GetValueOrDefault("detail_price", string.Empty);

            if (!string.IsNullOrWhiteSpace(price))
            {
                Logger.Info($"[DETAILS_PAGE] [ExtractPriceFromDetails] Price successfully extracted: {price}");
            }
            else
            {
                Logger.Warn("[DETAILS_PAGE] [ExtractPriceFromDetails] No price found on details page");
            }

            return price ?? string.Empty;
        }

        public string ExtractRoomsFromDetails()
        {
            Logger.Info("[DETAILS_PAGE] [ExtractRoomsFromDetails] Starting rooms extraction");

            var results = ExtractParams(new List<string> { "detail_rooms" });
            var rooms = results.GetValueOrDefault("detail_rooms", string.Empty);

            if (!string.IsNullOrWhiteSpace(rooms))
            {
                Logger.Info($"[DETAILS_PAGE] [ExtractRoomsFromDetails] Rooms successfully extracted: {rooms}");
            }
            else
            {
                Logger.Warn("[DETAILS_PAGE] [ExtractRoomsFromDetails] No rooms found on details page");
            }

            return rooms ?? string.Empty;
        }

        public Dictionary<string, string?> ExtractAllDetailsParameters()
        {
            Logger.Info("[DETAILS_PAGE] [ExtractAllDetailsParameters] Starting extraction of all parameters");

            var allKeys = ElementMapping.Keys.ToList();
            var results = ExtractParams(allKeys);

            var successCount = results.Count(r => !string.IsNullOrWhiteSpace(r.Value));
            Logger.Info($"[DETAILS_PAGE] [ExtractAllDetailsParameters] Extraction completed: {successCount}/{allKeys.Count} parameters found");

            return results;
        }
    }
}