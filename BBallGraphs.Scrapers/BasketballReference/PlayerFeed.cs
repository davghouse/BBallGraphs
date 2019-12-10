namespace BBallGraphs.Scrapers.BasketballReference
{
    public class PlayerFeed
    {
        public PlayerFeed(string url)
            => Url = url;

        public string Url { get; }

        public override string ToString()
            => Url;
    }
}
