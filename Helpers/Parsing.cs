using Aquality.Selenium.Core.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DB_29357.Helpers
{
    public static class Parsing
    {
        private static readonly Logger logger = Logger.Instance;
        
        public static int? ParsePriceToPln(string text, string callerContext = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (IsPricePerSquareMeter(line))
                    continue;

                var price = ExtractPrice(line);
                if (price.HasValue && IsValidPrice(price.Value))
                {
                    return price;
                }
            }
            return null;
        }

        public static double? ParseSurfaceM2(string text, string callerContext = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (ContainsCurrency(text))
            {
                return null;
            }

            var surface = ExtractSurface(text);

            if (surface.HasValue && IsValidSurface(surface.Value))
            {
                return surface;
            }

            return null;
        }

        public static int? ParseRooms(string text, string callerContext = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var rooms = ExtractRoomCount(text);

            if (rooms.HasValue && IsValidRoomCount(rooms.Value))
            {
                return rooms;
            }

            return null;
        }

        private static int? ExtractPrice(string line)
        {
            var pattern = @"(\d{1,3}(?:[\s\u00A0]\d{3})+)(?:[,.]\d+)?\s*(?:zł|PLN|zl)(?!\s*/m)";
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            var priceText = match.Groups[1].Value
                .Replace(" ", "")
                .Replace("\u00A0", "")
                .Replace(",", "");

            return int.TryParse(priceText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int price)
                ? price
                : null;
        }

        private static double? ExtractSurface(string text)
        {
            if (Regex.IsMatch(text, @"\d+\s*-\s*\d+\s*m[²2]", RegexOptions.IgnoreCase))
            {
                return null;
            }
            var pattern = @"(?<!\d-)(\d+[,.]?\d*)(?!-\d)\s*m[²2]";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            var numberText = match.Groups[1].Value.Replace(",", ".").Trim();

            return double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out double surface)
                ? surface
                : null;
        }

        private static int? ExtractRoomCount(string text)
        {
            var patterns = new[]
            {
                @"(\d+)\s*pokój", @"(\d+)\s*pokoje", @"(\d+)\s*pokoi", @"^\s*(\d+)\s*$"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int rooms))
                {
                    return rooms;
                }
            }
            return null;
        }

        private static bool IsValidPrice(int price) => price >= 10000 && price <= 50000000;
        private static bool IsValidSurface(double surface) => surface >= 5 && surface <= 1000;
        private static bool IsValidRoomCount(int rooms) => rooms >= 1 && rooms <= 10;
        private static bool IsPricePerSquareMeter(string line)
        {
            var indicators = new[] { "/m²", "/m2", "zł/m", "PLN/m" };
            return indicators.Any(ind => line.Contains(ind, StringComparison.OrdinalIgnoreCase)) ||
                   line.Contains("cena za metr", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsCurrency(string text) => text.Contains("zł") || text.Contains("PLN");

        public static T? ParseSafely<T>(string? input, Func<string, string, T?> parser, string context) where T : struct
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            return parser(input, context);
        }
    }
}