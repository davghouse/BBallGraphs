using System;

namespace BBallGraphs.BasketballReferenceScraper
{
    public interface IPlayer
    {
        string ID { get; set; }
        string FeedUrl { get; set; }
        string Name { get; set; }
        int FirstSeason { get; set; }
        int LastSeason { get; set; }
        string Position { get; set; }
        double? HeightInInches { get; set; }
        double? WeightInPounds { get; set; }
        DateTime BirthDate { get; set; }
    }

    public static class IPlayerExtensions
    {
        public static bool Matches(this IPlayer player, IPlayer otherPlayer)
            => player.ID == otherPlayer.ID
            && player.FeedUrl == otherPlayer.FeedUrl
            && player.Name == otherPlayer.Name
            && player.FirstSeason == otherPlayer.FirstSeason
            && player.LastSeason == otherPlayer.LastSeason
            && player.Position == otherPlayer.Position
            && player.HeightInInches == otherPlayer.HeightInInches
            && player.WeightInPounds == otherPlayer.WeightInPounds
            && player.BirthDate == otherPlayer.BirthDate;

        public static void CopyTo(this IPlayer sourcePlayer, IPlayer targetPlayer)
        {
            targetPlayer.ID = sourcePlayer.ID;
            targetPlayer.FeedUrl = sourcePlayer.FeedUrl;
            targetPlayer.Name = sourcePlayer.Name;
            targetPlayer.FirstSeason = sourcePlayer.FirstSeason;
            targetPlayer.LastSeason = sourcePlayer.LastSeason;
            targetPlayer.Position = sourcePlayer.Position;
            targetPlayer.HeightInInches = sourcePlayer.HeightInInches;
            targetPlayer.WeightInPounds = sourcePlayer.WeightInPounds;
            targetPlayer.BirthDate = sourcePlayer.BirthDate;
        }

        public static string GetProfileUrl(this IPlayer player)
            => $"{player.FeedUrl}{player.ID}.html";

        public static string GetGameLogUrl(this IPlayer player, int season)
            => $"{player.FeedUrl}{player.ID}/gamelog/{season}/";

        // Not sure best duration to pick, but a year or two isn't enough (big injury, taking a break, playing overseas).
        public static bool IsProbablyRetired(this IPlayer player)
            => player.LastSeason + 5 < DateTime.UtcNow.Year;
    }
}
