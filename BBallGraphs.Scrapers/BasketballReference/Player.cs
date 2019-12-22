using System;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public class Player : IPlayer
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public int FromYear { get; set; }
        public int ToYear { get; set; }
        public string Position { get; set; }
        public double HeightInches { get; set; }
        public double? WeightPounds { get; set; }
        public DateTime BirthDate { get; set; }
        public string FeedUrl { get; set; }

        public override string ToString()
            => $"{Name} ({FromYear} - {ToYear})";
    }
}
