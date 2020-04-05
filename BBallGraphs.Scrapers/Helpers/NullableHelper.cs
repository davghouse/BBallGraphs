namespace BBallGraphs.Scrapers.Helpers
{
    public static class NullableHelper
    {
        public static int? TryParseInt(string value)
            => int.TryParse(value, out int result) ? result : (int?)null;

        public static double? TryParseDouble(string value)
            => double.TryParse(value, out double result) ? result : (double?)null;
    }
}
