using Aquality.Selenium.Elements.Interfaces;
using Aquality.Selenium.Core.Localization;
using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace DB_29357.Pages
{
    public class ResultsPageExtractor
    {
        private readonly ILocalizedLogger _logger;

        private static readonly Regex PriceRegex = new(@"(\d[\d\s\u00A0\.,]*)\s*(z.??|zl|pln)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex RoomsRegex = new(@"\b(\d+)\s*(pokoi|pokoje|pok\.?)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex SurfaceRegex = new(@"(\d{1,3}(?:[.,]\d{1,2})?)\s*m[²2]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public ResultsPageExtractor(ILocalizedLogger logger)
        {
            _logger = logger;
        }

        public string ExtractPriceFromCard(IElement card)
        {
            var priceSelectors = new[]
                {
                    By.CssSelector("span[data-cy='listing.ad.price']"),
                    By.XPath(".//span[contains(@class, 'css-') and contains(text(), 'zł')]"),
                    By.XPath(".//p[contains(@class, 'css-')]//span[contains(text(), 'zł')] | .//div[@data-cy='ad.price']//span")
                };

            foreach (var selector in priceSelectors)
            {

                var priceElements = GetVisibleElements(card, selector);
                foreach (var element in priceElements)
                {
                    var text = Clean(element.Text);
                    if (!string.IsNullOrWhiteSpace(text) && PriceRegex.IsMatch(text))
                    {
                        return text;
                    }
                }
            }

            return string.Empty;
        }

        public string ExtractRoomsFromCard(IElement card)
        {
            const string xpath = ".//dt[contains(.,'Liczba pokoi')]/following-sibling::dd[1]//span | " +
                                 ".//span[contains(.,'pokoi') or contains(.,'pokoje')]";

            var roomElements = GetVisibleElements(card, By.XPath(xpath));

            foreach (var element in roomElements)
            {
                var match = RoomsRegex.Match(Clean(element.Text));
                if (match.Success)
                {
                    return Clean(match.Value);
                }
            }

            return string.Empty;
        }

        public string ExtractSurfaceFromCard(IElement card)
        {
            var surface = ExtractSurfaceFromPricePerM2Section(card);
            if (!string.IsNullOrEmpty(surface))
            {
                return surface;
            }

            surface = ExtractSurfaceFromSpecsList(card);
            return surface ?? string.Empty;
        }

        private string ExtractSurfaceFromPricePerM2Section(IElement card)
        {
            const string xpath = ".//dt[contains(text(),'Cena za metr kwadratowy')]/following-sibling::dd//span";
            var elements = GetVisibleElements(card, By.XPath(xpath));

            foreach (var element in elements)
            {
                var text = Clean(element.Text);
                if (!ContainsSurfaceIndicator(text)) continue;

                var match = SurfaceRegex.Match(text);
                if (match.Success)
                {
                    return match.Value;
                }
            }

            return string.Empty;
        }

        private string ExtractSurfaceFromSpecsList(IElement card)
        {
            const string xpath = ".//div[@data-sentry-element='SpecsListWrapper']//span[contains(text(),'m²') or contains(text(),'m2')]";
            var elements = GetVisibleElements(card, By.XPath(xpath));

            foreach (var element in elements)
            {
                var text = Clean(element.Text);
                if (ContainsPricePerMeter(text)) continue;

                var match = SurfaceRegex.Match(text);
                if (match.Success)
                {
                    return match.Value;
                }
            }

            return string.Empty;
        }

        private static bool IsInvalidPriceText(string text)
        {
            return string.IsNullOrEmpty(text)
                || text.Contains("/m²")
                || text.Contains("/m2")
                || !Regex.IsMatch(text, @"\d");
        }

        private static bool ContainsSurfaceIndicator(string text)
        {
            return text.Contains("m²") || text.Contains("m2");
        }

        private static bool ContainsPricePerMeter(string text)
        {
            return text.Contains("/");
        }

        private IEnumerable<ILabel> GetVisibleElements(IElement parent, By by)
        {
            return parent
                .FindChildElements<ILabel>(by)
                .Where(e => e.State.IsDisplayed)
                .ToList();
        }

        private static string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var cleaned = input.Replace('\u00A0', ' ').Trim();
            return Regex.Replace(cleaned, @"\s+", " ");
        }
    }
}