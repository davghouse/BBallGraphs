using System;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public class Player
    {
        public Player(
            string url, string name, int fromYear, int toYear, string position,
            double heightInches, double? weightPounds, DateTime birthDate)
        {
            Url = url;
            Name = name;
            FromYear = fromYear;
            ToYear = toYear;
            Position = position;
            HeightInches = heightInches;
            WeightPounds = weightPounds;
            BirthDate = birthDate;
        }

        public string Url { get; }
        public string Name { get; }
        public int FromYear { get; }
        public int ToYear { get; }
        public string Position { get; }
        public double HeightInches { get; }
        public double? WeightPounds { get; }
        public DateTime BirthDate { get; }

        public override string ToString()
            => $"{Name} ({FromYear} - {ToYear})";
    }
}
