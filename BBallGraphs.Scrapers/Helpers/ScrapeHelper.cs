using System;

namespace BBallGraphs.Scrapers.Helpers
{
    public static class ScrapeHelper
    {
        public static readonly string TransparentUserAgent
            = "BBallGraphs Scraper (https://github.com/davghouse/BBallGraphs)";

        public static double ParseHeight(string height)
        {
            string[] parts = height.Split('-');

            return int.Parse(parts[0]) * 12 + double.Parse(parts[1]);
        }

        public static DateTime AsUtc(this DateTime dateTime)
            => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        // Some old players only have birth years listed, in which case January 1st of that year is
        // assumed. One guy (Dick Lee) doesn't even have a year I can find, so I just made something
        // up. Their birth years may not be listed alongside birth dates of other players on some
        // sites like basketball-reference, so I'm hardcoding them here to avoid extra scraping.
        public static DateTime GetEstimatedBirthDate(string playerName)
        {
            switch (playerName)
            {
                case "Bill Allen": return new DateTime(1945, 1, 1).AsUtc();
                case "Walter Byrd": return new DateTime(1942, 1, 1).AsUtc();
                case "Clarence Brookins": return new DateTime(1946, 1, 1).AsUtc();
                case "Don Bielke": return new DateTime(1932, 1, 1).AsUtc();
                case "Rich Dumas": return new DateTime(1945, 1, 1).AsUtc();
                case "Mack Daughtry": return new DateTime(1947, 1, 1).AsUtc();
                case "Darrell Hardy": return new DateTime(1944, 1, 1).AsUtc();
                case "Wilber Kirkland": return new DateTime(1947, 1, 1).AsUtc();
                case "R.B. Lynam": return new DateTime(1944, 1, 1).AsUtc();
                case "Dick Lee": return new DateTime(1938, 1, 1).AsUtc();
                case "Richard Moore": return new DateTime(1945, 1, 1).AsUtc();
                case "Larry Moore": return new DateTime(1944, 1, 1).AsUtc();
                case "Howie McCarty": return new DateTime(1919, 1, 1).AsUtc();
                case "Charles Parks": return new DateTime(1946, 1, 1).AsUtc();
                case "Errol Palmer": return new DateTime(1945, 1, 1).AsUtc();
                case "Randy Stoll": return new DateTime(1945, 1, 1).AsUtc();
                case "Bruce Spraggins": return new DateTime(1940, 1, 1).AsUtc();
                case "Pete Smith": return new DateTime(1947, 1, 1).AsUtc();
                case "Gary Turner": return new DateTime(1945, 1, 1).AsUtc();
                case "Willis Thomas": return new DateTime(1937, 1, 1).AsUtc();
                case "Jim Wilson": return new DateTime(1948, 1, 1).AsUtc();
                case "Bobby Wilson": return new DateTime(1944, 1, 1).AsUtc();
                case "Dexter Westbrook": return new DateTime(1943, 1, 1).AsUtc();
                default: throw new NotSupportedException($"{playerName} doesn't have an estimated birth date.");
            }
        }
    }
}
