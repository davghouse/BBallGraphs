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
    public class Scraper
    {
        protected static readonly Regex _profileUrlIDRegex
            = new Regex("^https://www.basketball-reference.com/players/./(.+)\\.html$", RegexOptions.Compiled);

        public Scraper(string userAgent)
            => UserAgent = userAgent;

        public string UserAgent { get; set; }

        protected virtual IBrowsingContext GetBrowsingContext()
        {
            var requester = new DefaultHttpRequester();
            requester.Headers["User-Agent"] = UserAgent;
            var config = Configuration.Default.With(requester).WithDefaultLoader();

            return BrowsingContext.New(config);
        }

        protected static IElement GetStatCell(IEnumerable<IElement> rowCells, string statAttributeValue)
            => rowCells.SingleOrDefault(c => c.GetAttribute("data-stat") == statAttributeValue);

        public virtual async Task<IReadOnlyList<PlayerFeed>> GetPlayerFeeds()
        {
            string playerFeedsUrl = "https://www.basketball-reference.com/players";

            using (var browsingContext = GetBrowsingContext())
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

        public virtual async Task<IReadOnlyList<Player>> GetPlayers(IPlayerFeed playerFeed)
        {
            using (var browsingContext = GetBrowsingContext())
            using (var playerFeedDocument = await browsingContext.OpenAsync(playerFeed.Url))
            {
                var players = new List<Player>();

                foreach (var playerRow in playerFeedDocument
                    .QuerySelectorAll("table#players tbody tr:not(.thead)"))
                {
                    var playerRowCells = playerRow.Children;
                    var playerCell = GetStatCell(playerRowCells, "player");
                    // <strong> tag wraps anchors for active players.
                    var playerCellAnchor = (playerCell.FirstChild as IHtmlAnchorElement)
                        ?? (IHtmlAnchorElement)playerCell.FirstChild.FirstChild;
                    string profileUrl = playerCellAnchor.Href.Trim();
                    if (!_profileUrlIDRegex.IsMatch(profileUrl))
                        throw new ScrapeException("Failed to parse player ID.", playerFeed.Url, playerFeedDocument.Source.Text);

                    var player = new Player
                    {
                        ID = _profileUrlIDRegex.Match(profileUrl).Groups[1].Value,
                        FeedUrl = playerFeed.Url,
                        Name = playerCellAnchor.TextContent.Trim(),
                        FirstSeason = int.Parse(GetStatCell(playerRowCells, "year_min").TextContent),
                        LastSeason = int.Parse(GetStatCell(playerRowCells, "year_max").TextContent),
                        Position = GetStatCell(playerRowCells, "pos").TextContent.Trim(),
                        HeightInInches = ScrapeHelper.ParseHeightInInches(GetStatCell(playerRowCells, "height").TextContent),
                        WeightInPounds = NullableHelper.TryParseDouble(GetStatCell(playerRowCells, "weight").TextContent),
                    };
                    player.BirthDate = DateTime.TryParse(GetStatCell(playerRowCells, "birth_date").TextContent, out DateTime birthDate)
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

        public virtual async Task<IReadOnlyList<Game>> GetGames(IPlayer player, int season)
        {
            string gameLogUrl = player.GetGameLogUrl(season);

            using (var browsingContext = GetBrowsingContext())
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
                        Date =  DateTime.Parse(GetStatCell(gameRowCells, "date_game").TextContent.Trim()).AsUtc(),
                        Team = GetStatCell(gameRowCells, "team_id").TextContent.Trim(),
                        OpponentTeam = GetStatCell(gameRowCells, "opp_id").TextContent.Trim(),
                        IsHomeGame = !(GetStatCell(gameRowCells, "game_location").TextContent ?? "").Contains("@"),
                        IsPlayoffGame = gameRow.Id.Contains("pgl_basic_playoffs"),
                        BoxScoreUrl = (GetStatCell(gameRowCells, "date_game").FirstChild as IHtmlAnchorElement).Href.Trim()
                            // Necessary for the commented playoffs game log data mentioned above.
                            .Replace("about://", "https://www.basketball-reference.com"),
                        Won =  GetStatCell(gameRowCells, "game_result").TextContent.Contains("W"),
                        Started = string.IsNullOrWhiteSpace(GetStatCell(gameRowCells, "gs").TextContent) ? null
                            : (bool?)GetStatCell(gameRowCells, "gs").TextContent.Contains("1"),
                        SecondsPlayed = ScrapeHelper.ParseSecondsPlayed(GetStatCell(gameRowCells, "mp").TextContent),
                        FieldGoalsMade = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "fg").TextContent),
                        FieldGoalsAttempted = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "fga").TextContent),
                        ThreePointersMade = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "fg3")?.TextContent),
                        ThreePointersAttempted = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "fg3a")?.TextContent),
                        FreeThrowsMade = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "ft").TextContent),
                        FreeThrowsAttempted = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "fta").TextContent),
                        OffensiveRebounds = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "orb")?.TextContent),
                        DefensiveRebounds = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "drb")?.TextContent),
                        TotalRebounds = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "trb").TextContent),
                        Assists = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "ast").TextContent),
                        Steals = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "stl")?.TextContent),
                        Blocks = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "blk")?.TextContent),
                        Turnovers = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "tov")?.TextContent),
                        PersonalFouls = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "pf").TextContent),
                        // Points is almost always filled in with 0 if the player didn't score any points in a game. Some random
                        // exceptions where it's not filled in are just meaningless inconsistencies in how bball ref displays data.
                        Points = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "pts").TextContent) ?? 0,
                        GameScore = NullableHelper.TryParseDouble(GetStatCell(gameRowCells, "game_score")?.TextContent),
                        PlusMinus = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "plus_minus")?.TextContent)
                    };
                    game.ID = $"{player.ID} {game.Date:d}";
                    game.AgeInDays = (int)(game.Date - player.BirthDate).TotalDays;

                        // This box score is the second game of the only doubleheader in NBA history.
                    if (game.BoxScoreUrl == "https://www.basketball-reference.com/boxscores/195403082MLH.html"
                        // This guy was playing concurrently on two different teams, and played a game for each on the same day.
                        || player.ID == "fitzgbo01" && game.BoxScoreUrl == "https://www.basketball-reference.com/boxscores/194702210TRH.html")
                    {
                        game.ID = $"{game.ID} 2";
                        game.Date = game.Date.AddHours(3);
                    }

                    // There are a few instances of players playing for both teams in a game. In that case the game data
                    // is split into two rows on basketball-reference. We're not concerned about distinguishing which part
                    // of their performance came for which team, so this merges the data together.
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
                    || games.Count(g => g.IsHomeGame) > 70
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
