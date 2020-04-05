using System;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public interface IPlayer
    {
        string FeedUrl { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int FirstSeason { get; set; }
        int LastSeason { get; set; }
        string Position { get; set; }
        double HeightInInches { get; set; }
        double? WeightInPounds { get; set; }
        DateTime BirthDate { get; set; }
    }

    public static class IPlayerExtensions
    {
        public static bool Matches(this IPlayer player, IPlayer otherPlayer)
            => player.FeedUrl == otherPlayer.FeedUrl
            && player.ID == otherPlayer.ID
            && player.Name == otherPlayer.Name
            && player.FirstSeason == otherPlayer.FirstSeason
            && player.LastSeason == otherPlayer.LastSeason
            && player.Position == otherPlayer.Position
            && player.HeightInInches == otherPlayer.HeightInInches
            && player.WeightInPounds == otherPlayer.WeightInPounds
            && player.BirthDate == otherPlayer.BirthDate;

        public static void CopyTo(this IPlayer sourcePlayer, IPlayer targetPlayer)
        {
            targetPlayer.FeedUrl = sourcePlayer.FeedUrl;
            targetPlayer.ID = sourcePlayer.ID;
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
    }
}
