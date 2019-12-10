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

        // Some old players only have birth years listed, in which case January 1st of that year is
        // assumed. One guy (Dick Lee) doesn't even have a year I can find, so I just made something
        // up. Their birth years may not be listed alongside birth dates of other players on some
        // sites like basketball-reference, so I'm hardcoding them here to avoid extra scraping.
        public static DateTime GetEstimatedBirthDate(string playerName)
        {
            switch (playerName)
            {
                case "Bill Allen": return new DateTime(1945, 1, 1);
                case "Walter Byrd": return new DateTime(1942, 1, 1);
                case "Clarence Brookins": return new DateTime(1946, 1, 1);
                case "Don Bielke": return new DateTime(1932, 1, 1);
                case "Rich Dumas": return new DateTime(1945, 1, 1);
                case "Mack Daughtry": return new DateTime(1947, 1, 1);
                case "Darrell Hardy": return new DateTime(1944, 1, 1);
                case "Wilber Kirkland": return new DateTime(1947, 1, 1);
                case "R.B. Lynam": return new DateTime(1944, 1, 1);
                case "Dick Lee": return new DateTime(1938, 1, 1);
                case "Richard Moore": return new DateTime(1945, 1, 1);
                case "Larry Moore": return new DateTime(1944, 1, 1);
                case "Howie McCarty": return new DateTime(1919, 1, 1);
                case "Charles Parks": return new DateTime(1946, 1, 1);
                case "Errol Palmer": return new DateTime(1945, 1, 1);
                case "Randy Stoll": return new DateTime(1945, 1, 1);
                case "Bruce Spraggins": return new DateTime(1940, 1, 1);
                case "Pete Smith": return new DateTime(1947, 1, 1);
                case "Gary Turner": return new DateTime(1945, 1, 1);
                case "Willis Thomas": return new DateTime(1937, 1, 1);
                case "Jim Wilson": return new DateTime(1948, 1, 1);
                case "Bobby Wilson": return new DateTime(1944, 1, 1);
                case "Dexter Westbrook": return new DateTime(1943, 1, 1);
                default: throw new ArgumentException($"{playerName} doesn't have an estimated birth date yet.");
            }
        }
    }
}
