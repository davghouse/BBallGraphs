namespace BBallGraphs.BasketballReferenceScraper
{
    public interface IPlayerFeed
    {
        string Url { get; set; }
    }

    public static class IPlayerFeedExtensions
    {
        public static bool Matches(this IPlayerFeed playerFeed, IPlayerFeed otherPlayerFeed)
            => playerFeed.Url == otherPlayerFeed.Url;

        public static void CopyTo(this IPlayerFeed sourcePlayerFeed, IPlayerFeed targetPlayerFeed)
            => targetPlayerFeed.Url = sourcePlayerFeed.Url;
    }
}
