using System;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public interface IPlayer
    {
        string Url { get; set; }
        string Name { get; set; }
        int FromYear { get; set; }
        int ToYear { get; set; }
        string Position { get; set; }
        double HeightInches { get; set; }
        double? WeightPounds { get; set; }
        DateTime BirthDate { get; set; }
        string FeedUrl { get; set; }
    }

    public static class IPlayerExtensions
    {
        public static bool Matches(this IPlayer player, IPlayer otherPlayer)
            => player.Url == otherPlayer.Url
            && player.Name == otherPlayer.Name
            && player.FromYear == otherPlayer.FromYear
            && player.ToYear == otherPlayer.ToYear
            && player.Position == otherPlayer.Position
            && player.HeightInches == otherPlayer.HeightInches
            && player.WeightPounds == otherPlayer.WeightPounds
            && player.BirthDate == otherPlayer.BirthDate
            && player.FeedUrl == otherPlayer.FeedUrl;

        public static void CopyTo(this IPlayer sourcePlayer, IPlayer targetPlayer)
        {
            targetPlayer.Url = sourcePlayer.Url;
            targetPlayer.Name = sourcePlayer.Name;
            targetPlayer.FromYear = sourcePlayer.FromYear;
            targetPlayer.ToYear = sourcePlayer.ToYear;
            targetPlayer.Position = sourcePlayer.Position;
            targetPlayer.HeightInches = sourcePlayer.HeightInches;
            targetPlayer.WeightPounds = sourcePlayer.WeightPounds;
            targetPlayer.BirthDate = sourcePlayer.BirthDate;
            targetPlayer.FeedUrl = sourcePlayer.FeedUrl;
        }
    }
}
