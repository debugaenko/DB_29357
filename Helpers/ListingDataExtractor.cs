using Aquality.Selenium.Core.Logging;
using Aquality.Selenium.Elements.Interfaces;

namespace DB_29357.Helpers
{
    public class ListingDataExtractor
    {
        private readonly Logger _logger = Logger.Instance;

        public List<ListingData> ExtractBatch(IList<ILabel> listings, Func<ILabel, ILabel?>? getSurfaceElement = null, int maxListings = 36)
        {
            if (listings == null || listings.Count == 0)
            {
                _logger.Warn("[ExtractBatch] No listings to extract data from");
                return new List<ListingData>();
            }

            var itemsToProcess = Math.Min(listings.Count, maxListings);
            var displayedListings = WaitForListingsBatch(listings, itemsToProcess);
            var results = displayedListings.Select(listing => ExtractFromReadyListing(listing, getSurfaceElement)).Where(data => data.IsValid).ToList();
            _logger.Info($"[ExtractBatch] Extracted {results.Count}/{itemsToProcess} valid listings");
            return results;
        }

        public List<(int? price, int? rooms, double? surface)> ExtractBatchAsTuples(
            IList<ILabel> listings,
            Func<ILabel, ILabel?>? getSurfaceElement = null,
            int maxListings = 36)
        {
            var data = ExtractBatch(listings, getSurfaceElement, maxListings);
            return data.Select(d => d.ToTuple()).ToList();
        }

        private List<ILabel> WaitForListingsBatch(IList<ILabel> listings, int count)
        {
            var targetCount = Math.Min(count, listings.Count);

            if (targetCount == 0)
                return new List<ILabel>();
            
            if (listings.Count > 0 && !listings[0].State.WaitForDisplayed())
            {
                _logger.Warn("[WaitForListingsBatch] First listing not displayed within timeout");
                return new List<ILabel>();
            }
            return listings.Take(targetCount).Where(l => l.State.IsDisplayed).ToList();
        }

        private ListingData ExtractFromReadyListing(ILabel listing, Func<ILabel, ILabel?>? getSurfaceElement)
        {
            var text = listing.GetText();

            if (string.IsNullOrWhiteSpace(text))
            {
                return ListingData.Empty;
            }
            var listingData = new ListingData
            {
                Price = Parsing.ParsePriceToPln(text),
                Rooms = Parsing.ParseRooms(text),
                Surface = ExtractSurface(listing, getSurfaceElement, text)
            };
            return listingData;
        }

        private double? ExtractSurface(ILabel listing, Func<ILabel, ILabel?>? getSurfaceElement, string fallbackText)
        {
            if (getSurfaceElement != null)
            {
                var surfaceElement = getSurfaceElement(listing);

                if (surfaceElement?.State.IsDisplayed == true)
                {
                    var surfaceText = surfaceElement.GetText();
                    var surface = Parsing.ParseSurfaceM2(surfaceText);

                    if (surface.HasValue)
                    {
                        return surface;
                    }
                }
            }
            return Parsing.ParseSurfaceM2(fallbackText);
        }

        public class ListingData
        {
            public int? Price { get; set; }
            public int? Rooms { get; set; }
            public double? Surface { get; set; }
            public bool IsValid => Price.HasValue || Rooms.HasValue || Surface.HasValue;
            public static ListingData Empty => new ListingData();
            public (int? price, int? rooms, double? surface) ToTuple() => (Price, Rooms, Surface);
        }
    }
}