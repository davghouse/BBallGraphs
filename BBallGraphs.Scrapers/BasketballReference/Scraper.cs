using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using BBallGraphs.Scrapers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public static class Scraper
    {
        private static readonly string _baseUrl
            = "https://www.basketball-reference.com";

        private static readonly Regex _profileUrlIDRegex
            = new Regex($"^{_baseUrl}/players/./(.+)\\.html$", RegexOptions.Compiled);

        private static IBrowsingContext GetBrowsingContext()
        {
            var requester = new DefaultHttpRequester();
            requester.Headers["User-Agent"] = ScrapeHelper.TransparentUserAgent;
            var config = Configuration.Default.With(requester).WithDefaultLoader();

            return BrowsingContext.New(config);
        }

        public static async Task<IReadOnlyList<PlayerFeed>> GetPlayerFeeds()
        {
            var browsingContext = GetBrowsingContext();
            string playerFeedsUrl = $"{_baseUrl}/players";

            using (var playerFeedsDocument = await browsingContext.OpenAsync(playerFeedsUrl))
            {
                var playerFeeds = playerFeedsDocument
                    .QuerySelectorAll("ul.page_index li > a")
                    .OfType<IHtmlAnchorElement>()
                    .Select(a => new PlayerFeed { Url = a.Href.Trim() })
                    .ToArray();

                // The /x/ feed doesn't exist at the time of writing. Any other change to the feeds
                // besides adding /x/ is unexpected and will require a code change to support.
                if (playerFeeds.Length < 25 || playerFeeds.Length > 26
                    || !playerFeeds.First().Url.EndsWith("/a/")
                    || !playerFeeds.Last().Url.EndsWith("/z/"))
                    throw new ScrapeException($"Failed to scrape player feeds.", playerFeedsUrl, playerFeedsDocument.Source.Text);

                return playerFeeds;
            }
        }

        private static IElement GetStatCell(this IEnumerable<IElement> rowCells, string statAttributeValue)
            => rowCells.SingleOrDefault(c => c.GetAttribute("data-stat") == statAttributeValue);

        public static async Task<IReadOnlyList<Player>> GetPlayers(IPlayerFeed playerFeed)
        {
            var browsingContext = GetBrowsingContext();

            using (var playerFeedDocument = await browsingContext.OpenAsync(playerFeed.Url))
            {
                var players = new List<Player>();

                foreach (var playerRow in playerFeedDocument
                    .QuerySelectorAll("table#players tbody tr:not(.thead)"))
                {
                    var playerRowCells = playerRow.Children;
                    var playerCell = playerRowCells.GetStatCell("player");
                    // <strong> tag wraps anchors for active players.
                    var playerCellAnchor = (playerCell.FirstChild as IHtmlAnchorElement)
                        ?? (IHtmlAnchorElement)playerCell.FirstChild.FirstChild;
                    string profileUrl = playerCellAnchor.Href.Trim();
                    if (!_profileUrlIDRegex.IsMatch(profileUrl))
                        throw new ScrapeException("Failed to parse player ID.", playerFeed.Url, playerFeedDocument.Source.Text);

                    var player = new Player
                    {
                        FeedUrl = playerFeed.Url,
                        ID = _profileUrlIDRegex.Match(profileUrl).Groups[1].Value,
                        Name = playerCellAnchor.TextContent.Trim(),
                        FirstSeason = int.Parse(playerRowCells.GetStatCell("year_min").TextContent),
                        LastSeason = int.Parse(playerRowCells.GetStatCell("year_max").TextContent),
                        Position = playerRowCells.GetStatCell("pos").TextContent.Trim(),
                        HeightInInches = ScrapeHelper.ParseHeightInInches(playerRowCells.GetStatCell("height").TextContent),
                        WeightInPounds = NullableHelper.TryParseDouble(playerRowCells.GetStatCell("weight").TextContent),
                    };
                    player.BirthDate = DateTime.TryParse(playerRowCells.GetStatCell("birth_date").TextContent, out DateTime birthDate)
                        ? birthDate.AsUtc() : ScrapeHelper.GetEstimatedBirthDate(player.Name);

                    players.Add(player);
                }

                if (players.Count == 0
                    || players.Count != players.Select(p => p.ID).Distinct().Count()
                    || players.Any(p => p.FirstSeason < 1947 || p.LastSeason > DateTime.UtcNow.Year + 1
                        || p.HeightInInches < 12 || p.HeightInInches > 120
                        || p.BirthDate < new DateTime(1900, 1, 1)))
                    throw new ScrapeException("Failed to scrape players.", playerFeed.Url, playerFeedDocument.Source.Text);

                return players;
            }
        }

        public static async Task<IReadOnlyList<Game>> GetGames(IPlayer player, int season)
        {
            var browsingContext = GetBrowsingContext();
            string gameLogUrl = player.GetGameLogUrl(season);

            using (var gameLogDocument = await browsingContext.OpenAsync(gameLogUrl))
            {
                var regularSeasonGameRows = gameLogDocument
                    .QuerySelectorAll("table#pgl_basic tbody tr[id^='pgl_basic.']");
                // The playoffs game log data comes embedded in an HTML comment on the page. There's some
                // JavaScript the runs to set up commented divs, but not sure if there's a way to make it run
                // for us here. Instead, just parsing the comment directly.
                string playoffsComment = gameLogDocument
                    .Descendents<IComment>()
                    .Select(c => c.TextContent)
                    .FirstOrDefault(c => c.Contains("id=\"pgl_basic_playoffs\""));
                var playoffsGameRows = browsingContext.GetService<IHtmlParser>().ParseDocument(playoffsComment)
                    .QuerySelectorAll("table#pgl_basic_playoffs tbody tr[id^='pgl_basic_playoffs.']");

                var games = new List<Game>();

                foreach (var gameRow in regularSeasonGameRows.Concat(playoffsGameRows))
                {
                    var gameRowCells = gameRow.Children;

                    var game = new Game
                    {
                        PlayerID = player.ID,
                        PlayerName = player.Name,
                        Season = season,
                        Date =  DateTime.Parse(gameRowCells.GetStatCell("date_game").TextContent.Trim()).AsUtc(),
                        IsPlayoffGame = gameRow.Id.Contains("pgl_basic_playoffs"),
                        BoxScoreUrl = (gameRowCells.GetStatCell("date_game").FirstChild as IHtmlAnchorElement).Href.Trim(),
                        Won =  gameRowCells.GetStatCell("game_result").TextContent.Contains("W"),
                        Started = string.IsNullOrWhiteSpace(gameRowCells.GetStatCell("gs")?.TextContent) ? null
                            : (bool?)gameRowCells.GetStatCell("gs").TextContent.Contains("1"),
                        SecondsPlayed = ScrapeHelper.ParseSecondsPlayed(gameRowCells.GetStatCell("mp")?.TextContent),
                        FieldGoalsMade = NullableHelper.TryParseInt(gameRowCells.GetStatCell("fg")?.TextContent),
                        FieldGoalsAttempted = NullableHelper.TryParseInt(gameRowCells.GetStatCell("fga")?.TextContent),
                        ThreePointersMade = NullableHelper.TryParseInt(gameRowCells.GetStatCell("fg3")?.TextContent),
                        ThreePointersAttempted = NullableHelper.TryParseInt(gameRowCells.GetStatCell("fg3a")?.TextContent),
                        FreeThrowsMade = NullableHelper.TryParseInt(gameRowCells.GetStatCell("ft")?.TextContent),
                        FreeThrowsAttempted = NullableHelper.TryParseInt(gameRowCells.GetStatCell("fta")?.TextContent),
                        OffensiveRebounds = NullableHelper.TryParseInt(gameRowCells.GetStatCell("orb")?.TextContent),
                        DefensiveRebounds = NullableHelper.TryParseInt(gameRowCells.GetStatCell("drb")?.TextContent),
                        TotalRebounds = NullableHelper.TryParseInt(gameRowCells.GetStatCell("trb")?.TextContent),
                        Assists = NullableHelper.TryParseInt(gameRowCells.GetStatCell("ast")?.TextContent),
                        Steals = NullableHelper.TryParseInt(gameRowCells.GetStatCell("stl")?.TextContent),
                        Blocks = NullableHelper.TryParseInt(gameRowCells.GetStatCell("blk")?.TextContent),
                        Turnovers = NullableHelper.TryParseInt(gameRowCells.GetStatCell("tov")?.TextContent),
                        PersonalFouls = NullableHelper.TryParseInt(gameRowCells.GetStatCell("pf")?.TextContent),
                        Points = int.Parse(gameRowCells.GetStatCell("pts")?.TextContent),
                        GameScore = NullableHelper.TryParseDouble(gameRowCells.GetStatCell("game_score")?.TextContent),
                        PlusMinus = NullableHelper.TryParseInt(gameRowCells.GetStatCell("plus_minus")?.TextContent)
                    };
                    game.ID = $"{player.ID} {game.Date:d}";
                    game.AgeInDays = (int)(game.Date - player.BirthDate).TotalDays;

                    if (season == 1979
                        && (player.ID == "moneyer01" || player.ID == "simpsra01" || player.ID == "catchha01")
                        && game.Date == new DateTime(1978, 11, 8).AsUtc()
                        && games.LastOrDefault()?.Date == new DateTime(1978, 11, 8).AsUtc())
                    {
                        games[games.Count - 1] = Game.MergeSplitGameData(games.Last(), game);
                    }
                    else
                    {
                        games.Add(game);
                    }
                }

                if (games.Count > 110 /* 82 + 7*4, more are technically possible if traded but let's ignore that. */
                    || games.Count(g => g.IsPlayoffGame) > 28
                    || games.Count != games.Select(g => g.Date).Distinct().Count()
                    || games.Any(g => g.Date < new DateTime(1946, 1, 1)
                        || g.Date < player.BirthDate
                        || g.Date < new DateTime(player.FirstSeason - 1, 1, 1)
                        || g.Date > new DateTime(player.LastSeason, 12, 31)
                        || g.Points > 110 || g.Assists > 40 || g.TotalRebounds > 45
                        || g.Points < 0 || g.Assists < 0 || g.TotalRebounds < 0)
                    // Bill Russell never missed an entire regular season, or the playoffs. Use him as a canary to
                    // identify if we're missing games. Hard to do something better because of all the players that
                    // missed entire seasons and the playoffs--their game logs don't even have pgl_basic tables.
                    || player.ID == "russebi01" && (!games.Any(g => !g.IsPlayoffGame) || !games.Any(g => g.IsPlayoffGame)))
                    throw new ScrapeException("Failed to scrape games.", gameLogUrl, gameLogDocument.Source.Text);

                return games;
            }
        }
    }
}
