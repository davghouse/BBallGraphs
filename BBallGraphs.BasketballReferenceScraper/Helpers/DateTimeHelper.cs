using System;

namespace BBallGraphs.BasketballReferenceScraper.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime AsUtc(this DateTime dateTime)
            => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
