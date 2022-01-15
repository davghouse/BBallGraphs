using System;

namespace BBallGraphs.BasketballReferenceScraper.Helpers
{
    public static class ParseHelper
    {
        public static int? ParseNullableInt(string value)
            => string.IsNullOrWhiteSpace(value) ? (int?)null : int.Parse(value);

        public static double? ParseNullableDouble(string value)
            => string.IsNullOrWhiteSpace(value) ? (double?)null : double.Parse(value);

        public static DateTime? ParseNullableDateTime(string value)
            => string.IsNullOrWhiteSpace(value) ? (DateTime?)null : DateTime.Parse(value);

        public static double? ParseNullableHeightInInches(string height)
        {
            if (string.IsNullOrWhiteSpace(height))
                return null;

            if (!height.Contains("-"))
                throw new FormatException("Height in an unrecognized format, no separating dash.");

            string[] parts = height.Split('-');

            return int.Parse(parts[0]) * 12 + double.Parse(parts[1]);
        }

        public static int? ParseNullableSecondsPlayed(string playedTime)
        {
            if (string.IsNullOrWhiteSpace(playedTime))
                return null;

            if (!playedTime.Contains(":"))
                throw new FormatException("Played time in an unrecognized format, no separating colon.");

            string[] parts = playedTime.Split(':');

            return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
        }
    }
}
