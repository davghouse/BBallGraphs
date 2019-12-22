namespace BBallGraphs.Scrapers.BasketballReference
{
    public class PlayerFeed : IPlayerFeed
    {
        public string Url { get; set; }

        public override string ToString()
            => Url;
    }
}
