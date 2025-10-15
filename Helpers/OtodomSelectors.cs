using OpenQA.Selenium;

namespace DB_29357.Helpers
{
    public static class OtodomSelectors
    {
        public static class DetailsPage
        {
            // Price selectors
            public static readonly By Price = By.XPath("//strong[@data-cy='adPageHeaderPrice']");
            public static readonly By PriceAlternative = By.XPath("//strong[@data-cy='adPageHeaderPrice' or @aria-label='Cena']");

            // Room selectors (ordered by priority)
            public static readonly By RoomsPrimary = By.XPath("//div[contains(@class, 'css-1okys8k') and contains(text(), 'Liczba pokoi')]/following-sibling::div[contains(@class, 'css-1okys8k')]");
            public static readonly By RoomsSecondary = By.XPath("//div[contains(text(), 'Liczba pokoi')]/following-sibling::div");
            public static readonly By RoomsTertiary = By.XPath("//div[contains(@class, 'e1mm5aqc2')][contains(text(), 'Liczba pokoi')]/following-sibling::div[contains(@class, 'e1mm5aqc2')]");

            // Surface selectors (ordered by priority)
            public static readonly By SurfacePrimary = By.XPath("//div[contains(@class, 'css-1okys8k') and contains(text(), 'Powierzchnia')]/following-sibling::div[contains(@class, 'css-1okys8k')]");
            public static readonly By SurfaceSecondary = By.XPath("//div[contains(text(), 'Powierzchnia')]/following-sibling::div");
            public static readonly By SurfaceTertiary = By.XPath("//div[contains(@class, 'e1mm5aqc2')][contains(text(), 'Powierzchnia')]/following-sibling::div[contains(@class, 'e1mm5aqc2')]");
            public static readonly By SurfaceFallback = By.CssSelector("div.css-1okys8k.e1mm5aqc2");

            // Page identifier
            public static readonly By PageIndicator = By.CssSelector("strong[data-cy='adPageHeaderPrice']");

            public static By[] GetRoomSelectors() => new[] { RoomsPrimary, RoomsSecondary, RoomsTertiary };//,  

            public static By[] GetSurfaceSelectors() => new[] { SurfacePrimary, SurfaceSecondary, SurfaceTertiary, SurfaceFallback };//, SurfaceSecondary, SurfaceTertiary, SurfaceFallback };
        }

        public static class ResultsPage
        {
            // Listing card selectors
            public static readonly By ListingCards = By.CssSelector("article[data-sentry-component='AdvertCard']");
            public static readonly By ListingCardsFallback = By.XPath("//article[contains(@class, 'offer')] | //div[contains(@class, 'listing')] | //div[contains(@data-cy, 'listing')] | //div[contains(@class, 'es-offer')]");

            // Price element selectors within listing card (UPDATED)
            public static readonly By PriceElementPrimary = By.CssSelector("span[data-cy='listing.ad.price']");
            public static readonly By PriceElementSecondary = By.XPath(".//span[contains(@class, 'css-') and contains(text(), 'zł')]");
            public static readonly By PriceElementTertiary = By.XPath(".//p[contains(@class, 'css-')]//span[contains(text(), 'zł')] | .//div[@data-cy='ad.price']//span");

            // Room element selectors within listing card
            public static readonly By RoomElementPrimary = By.XPath(".//dd[contains(@class, 'css-17je0kd') or contains(@class, 'e1am572w')]//span[contains(text(), 'pokój') or contains(text(), 'pokoje') or contains(text(), 'pok')]");
            public static readonly By RoomElementSecondary = By.XPath(".//span[contains(text(), 'pokój') or contains(text(), 'pokoje') or contains(text(), 'pok.')]");

            // Surface element selectors within listing card
            public static readonly By SurfaceElementPrimary = By.XPath(".//dd[contains(@class, 'css-17je0kd') or contains(@class, 'e1am572w')]//span[contains(text(), 'm²') or contains(text(), 'm2') or contains(text(), 'mkw')]");
            public static readonly By SurfaceElementSecondary = By.XPath(".//span[contains(text(), 'm²') or contains(text(), 'm2') or contains(text(), 'mkw')]");

            // Filter selectors
            public static readonly By PriceMinFilter = By.XPath("//input[@data-cy='search-form--field--priceMin']");
            public static readonly By PriceMaxFilter = By.XPath("//input[@data-cy='search-form--field--priceMax']");
            public static readonly By SurfaceMinFilter = By.XPath("//input[@data-cy='search-form--field--areaMin']");
            public static readonly By SurfaceMaxFilter = By.XPath("//input[@data-cy='search-form--field--areaMax']");
            public static readonly By SearchButton = By.XPath("//span[contains(text(), 'Wyniki')]/parent::button");
            public static readonly By ClearPriceButton = By.XPath("//button[contains(@class, 'clear') and contains(@data-cy, 'price')]");

            // Page identifier
            public static readonly By PageIndicator = By.XPath("//div[contains(@class, 'results') or contains(@class, 'listing')]");

            public static By[] GetPriceElementSelectors() => new[] { PriceElementPrimary, PriceElementSecondary, PriceElementTertiary };
            public static By[] GetRoomElementSelectors() => new[] { RoomElementPrimary, RoomElementSecondary };
            public static By[] GetSurfaceElementSelectors() => new[] { SurfaceElementPrimary, SurfaceElementSecondary };
        }
    }
}
