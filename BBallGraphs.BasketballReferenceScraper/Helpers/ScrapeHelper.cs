using System;

namespace BBallGraphs.BasketballReferenceScraper.Helpers
{
    public static class ScrapeHelper
    {
        // Some old players only have birth years listed, in which case January 1st of that year is
        // assumed. One guy (Dick Lee) doesn't even have a year I can find, so I just made something up.
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
                case "Wilbur Kirkland": return new DateTime(1947, 1, 1).AsUtc();
                case "R.B. Lynam": return new DateTime(1944, 1, 1).AsUtc();
                case "Dick Lee": return new DateTime(1938, 1, 1).AsUtc();
                case "Richard Moore": return new DateTime(1945, 1, 1).AsUtc();
                case "Richie Moore": return new DateTime(1945, 1, 1).AsUtc();
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
                case "Craig Sword": return new DateTime(1994, 1, 16).AsUtc();
                default: throw new NotImplementedException($"{playerName} doesn't have an estimated birth date.");
            }
        }
    }
}
