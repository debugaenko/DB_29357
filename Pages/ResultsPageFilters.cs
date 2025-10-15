using Aquality.Selenium.Browsers;
using Aquality.Selenium.Core.Localization;
using Aquality.Selenium.Elements.Interfaces;
using OpenQA.Selenium;

namespace DB_29357.Pages
{
    public class ResultsPageFilters : BasePage
    {
        private ILocalizedLogger _logger => AqualityServices.Get<ILocalizedLogger>();
        private readonly ResultsPageExtractor _dataExtractor;
        private IElementFactory ElementFactory => AqualityServices.Get<IElementFactory>();

        private IList<ILabel> OfferCards => ElementFactory.FindElements<ILabel>(By.CssSelector("article[data-sentry-component='AdvertCard']"), "Offer cards");
        private IButton OfferCard => ElementFactory.GetButton(By.CssSelector("article[data-sentry-component='AdvertCard']"), "Offer Card");

        public ResultsPageFilters(ResultsPageExtractor dataExtractor) : base(By.XPath("//main | //body"), "Results Page")
        {
            _dataExtractor = dataExtractor ?? throw new ArgumentNullException(nameof(dataExtractor));
        }

        public IEnumerable<(string Price, string Rooms, string Surface)> GetOffersSnapshot(int maxOffers = 36)
        {
            if (!WaitForOffersToLoad())
            {
                _logger.Warn("[GetOffersSnapshot] No offers loaded - returning empty list");
                return Enumerable.Empty<(string, string, string)>();
            }

            var cards = OfferCards.Where(card => card.State.WaitForDisplayed(TimeSpan.FromSeconds(1))).Take(maxOffers).ToList();
            if (cards.Count == 0)
            {
                _logger.Warn("[GetOffersSnapshot] No visible offer cards - returning empty list");
                return Enumerable.Empty<(string, string, string)>();
            }

            _logger.Info($"[GetOffersSnapshot] Processing {cards.Count} visible cards");

            var results = new List<(string, string, string)>();
            int successCount = 0;
            int failCount = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                var offer = ExtractOfferData(cards[i], i + 1);

                if (!string.IsNullOrWhiteSpace(offer.Price))
                {
                    results.Add(offer);
                    successCount++;
                }
                else
                {
                    failCount++;
                    _logger.Warn($"[GetOffersSnapshot] Card #{i + 1}: Failed to extract price");
                }
            }

            _logger.Info($"[GetOffersSnapshot] Extracted {successCount} offers successfully, {failCount} failed from {cards.Count} cards");

            if (results.Count == 0)
            {
                _logger.Error($"[GetOffersSnapshot] CRITICAL: All {cards.Count} cards failed price extraction - check selectors!");
            }

            return results;
        }

        public bool WaitForOffersToLoad()
        {
            if (OfferCard.State.WaitForDisplayed())
            {
                _logger.Info("[WaitForOffersToLoad] Offers loaded successfully");
                return true;
            }

            _logger.Warn("[WaitForOffersToLoad] No offers loaded within timeout");
            return false;
        }

        private (string Price, string Rooms, string Surface) ExtractOfferData(ILabel card, int cardNumber)
        {
            try
            {
                var price = _dataExtractor.ExtractPriceFromCard(card);
                var surface = _dataExtractor.ExtractSurfaceFromCard(card);
                var rooms = _dataExtractor.ExtractRoomsFromCard(card);

                if (string.IsNullOrWhiteSpace(price))
                {
                    _logger.Warn($"[ExtractOfferData] Card #{cardNumber}: Price extraction returned empty/null");
                }

                return (price, rooms ?? string.Empty, surface ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.Error($"[ExtractOfferData] Card #{cardNumber}: Exception during extraction - {ex.Message}");
                return (string.Empty, string.Empty, string.Empty);
            }
        }

        public void ClearPriceFilters(IButton clearPriceButton, ITextBox priceMinFilter, ITextBox priceMaxFilter)
        {
            _logger.Info("[ClearPriceFilters] Clearing price filters");

            if (TryClearWithButton(clearPriceButton, priceMinFilter, priceMaxFilter))
            {
                _logger.Info("[ClearPriceFilters] Cleared via button");
                return;
            }

            ClearFieldsManually(priceMinFilter, priceMaxFilter);
            _logger.Info("[ClearPriceFilters] Cleared manually");
        }

        private bool TryClearWithButton(IButton clearButton, ITextBox minField, ITextBox maxField)
        {
            if (!clearButton.State.IsDisplayed)
                return false;

            clearButton.Click();

            return AqualityServices.ConditionalWait.WaitFor(
                () => string.IsNullOrEmpty(minField.Value) && string.IsNullOrEmpty(maxField.Value));
        }

        private void ClearFieldsManually(ITextBox minField, ITextBox maxField)
        {
            if (!string.IsNullOrEmpty(minField.Value))
                minField.Clear();

            if (!string.IsNullOrEmpty(maxField.Value))
                maxField.Clear();
        }

        public void SetSurfaceFilters(ITextBox surfaceMinFilter, ITextBox surfaceMaxFilter, double minSurface, double maxSurface)
        {
            _logger.Info($"[SetSurfaceFilters] Setting surface filters: {minSurface:F0} - {maxSurface:F0} m²");

            surfaceMinFilter.ClearAndType(minSurface.ToString("F0"));
            surfaceMaxFilter.ClearAndType(maxSurface.ToString("F0"));

            _logger.Info("[SetSurfaceFilters] Surface filters set successfully");
        }
    }
}