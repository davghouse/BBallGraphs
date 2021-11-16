using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using BBallGraphs.BasketballReferenceScraper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BBallGraphs.BasketballReferenceScraper
{
    public class Scraper
    {
        protected static readonly Regex _profileUrlIDRegex
            = new Regex("^https://www.basketball-reference.com/players/./(.+)\\.html$", RegexOptions.Compiled);

        public Scraper(string userAgent)
            => UserAgent = userAgent;

        public string UserAgent { get; set; }

        protected IBrowsingContext GetBrowsingContext()
        {
            var requester = new DefaultHttpRequester();
            requester.Headers["User-Agent"] = UserAgent;
            var config = Configuration.Default.With(requester).WithDefaultLoader();

            return BrowsingContext.New(config);
        }

        protected static IElement GetStatCell(IEnumerable<IElement> rowCells, string statAttributeValue)
            => rowCells.SingleOrDefault(c => c.GetAttribute("data-stat") == statAttributeValue);

        public async Task<IReadOnlyList<PlayerFeed>> GetPlayerFeeds()
        {
            string playerFeedsUrl = "https://www.basketball-reference.com/players/";

            using (var browsingContext = GetBrowsingContext())
            using (var playerFeedsDocument = await browsingContext.OpenAsync(playerFeedsUrl))
            {
                if (playerFeedsDocument.StatusCode != HttpStatusCode.OK)
                    throw new ScrapeException($"Failed to scrape player feeds: status code {playerFeedsDocument.StatusCode}.", playerFeedsUrl);
                if (playerFeedsDocument.Url != playerFeedsUrl)
                    throw new ScrapeException($"Failed to scrape player feeds: redirected to {playerFeedsDocument.Url}.", playerFeedsUrl);

                var playerFeeds = playerFeedsDocument
                    .QuerySelectorAll("ul.page_index li > a")
                    .OfType<IHtmlAnchorElement>()
                    .Select(a => new PlayerFeed { Url = a.Href.Trim() })
                    .ToArray();

                // The /x/ feed doesn't exist at the time of writing. Any other change to the feeds
                // besides adding /x/ is unexpected and will require a code change to support.
                string errors = (playerFeeds.Length < 25 || playerFeeds.Length > 26 ? "Found an invalid number of feeds. " : null)
                        + (!playerFeeds.First().Url.EndsWith("/a/") ? "Found a feed before the /a/ feed. " : null)
                        + (!playerFeeds.Last().Url.EndsWith("/z/") ? "Found a feed after the /z/ feed. " : null);

                if (!string.IsNullOrEmpty(errors))
                    throw new ScrapeException($"Failed to scrape player feeds: {errors}", playerFeedsUrl);

                return playerFeeds;
            }
        }

        public async Task<IReadOnlyList<Player>> GetPlayers(IPlayerFeed playerFeed)
        {
            using (var browsingContext = GetBrowsingContext())
            using (var playerFeedDocument = await browsingContext.OpenAsync(playerFeed.Url))
            {
                if (playerFeedDocument.StatusCode != HttpStatusCode.OK)
                    throw new ScrapeException($"Failed to scrape players: status code {playerFeedDocument.StatusCode}.", playerFeed.Url);
                if (playerFeedDocument.Url != playerFeed.Url)
                    throw new ScrapeException($"Failed to scrape players: redirected to {playerFeedDocument.Url}.", playerFeed.Url);

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
                        throw new ScrapeException("Failed to parse player ID.", playerFeed.Url);

                    // Work around a bugged repeated row for Tony Smith in the /s/ feed.
                    if (profileUrl == "https://www.basketball-reference.com/players/s/smithto02.html"
                        && string.IsNullOrEmpty(GetStatCell(playerRowCells, "height").TextContent))
                        continue;

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

                string errors = (players.Count == 0 ? "Found no players. " : null)
                    + (players.Count != players.Select(p => p.ID).Distinct().Count() ? "Found multiple players with the same ID. " : null)
                    + (players.Any(p => p.FirstSeason < 1947 || p.LastSeason > DateTime.UtcNow.Year + 1) ? "Found a player with an invalid season. " : null)
                    + (players.Any(p => p.HeightInInches < 12 || p.HeightInInches > 120) ? "Found a player with an invalid height. " : null)
                    + (players.Any(p => p.BirthDate < new DateTime(1900, 1, 1)) ? "Found a player with an invalid birth date. " : null);

                if (!string.IsNullOrEmpty(errors))
                    throw new ScrapeException($"Failed to scrape players: {errors}", playerFeed.Url);

                return players;
            }
        }

        public async Task<IReadOnlyList<Game>> GetGames(IPlayer player, int season)
        {
            string gameLogUrl = player.GetGameLogUrl(season);

            using (var browsingContext = GetBrowsingContext())
            using (var gameLogDocument = await browsingContext.OpenAsync(gameLogUrl))
            {
                if (gameLogDocument.StatusCode != HttpStatusCode.OK)
                    throw new ScrapeException($"Failed to scrape games: status code {gameLogDocument.StatusCode}.", gameLogUrl);
                if (gameLogDocument.Url != gameLogUrl)
                    throw new ScrapeException($"Failed to scrape games: redirected to {gameLogDocument.Url}.", gameLogUrl);

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
                        // Points is almost always filled in with 0 if the player didn't score in a game. Going to assume
                        // the rare exceptions where points is left blank are just meaningless inconsistencies in how bball
                        // ref displays 0, rather than true nulls that indicate missing data (like for steals & blocks).
                        Points = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "pts").TextContent) ?? 0,
                        GameScore = NullableHelper.TryParseDouble(GetStatCell(gameRowCells, "game_score")?.TextContent),
                        PlusMinus = NullableHelper.TryParseInt(GetStatCell(gameRowCells, "plus_minus")?.TextContent)
                    };
                    game.ID = $"{player.ID} {game.Date:d}";
                    game.AgeInDays = (int)(game.Date - player.BirthDate).TotalDays;

                        // This box score is the second game of the only doubleheader in NBA history.
                    if (game.BoxScoreUrl == "https://www.basketball-reference.com/boxscores/195403082MLH.html"
                        // This guy was playing concurrently on two different teams, and played a game for each on the same day.
                        || player.ID == "fitzgbo01" && game.BoxScoreUrl == "https://www.basketball-reference.com/boxscores/194702210TRH.html"
                        // This guy randomly played one game on a different team, not really sure what's going on here.
                        || player.ID == "johnsne01" && game.BoxScoreUrl == "https://www.basketball-reference.com/boxscores/195111220NYK.html"
                        // This guy switched teams but then randomly played a game for his old team on the same day as playing for his new team.
                        || player.ID == "rotheir01" && game.BoxScoreUrl == "https://www.basketball-reference.com/boxscores/194801080STB.html"
                        // Game protested and completed at a later date once Marion had joined the team.
                        || player.ID == "mariosh01" && game.BoxScoreUrl == "https://www.basketball-reference.com/boxscores/200712190ATL.html")
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

                string errors = (games.Count > 110 ? "Found more than 110 games. " : null)
                    + (games.Count(g => g.IsHomeGame) > 70 ? "Found more than 70 home games. " : null)
                    + (games.Count(g => g.IsPlayoffGame) > 28 ? "Found more than 28 playoff games. " : null)
                    + (games.Count != games.Select(g => g.Date).Distinct().Count() ? "Found multiple games on the same date. " : null)
                    + (games.Any(g => g.Date < new DateTime(1946, 1, 1)) ? "Found a game before 1946. " : null)
                    + (games.Any(g => g.Date > DateTime.UtcNow.AddDays(1)) ? "Found a game from the future. " : null)
                    + (games.Any(g => g.Date < player.BirthDate) ? "Found a game before the player's birth date. " : null)
                    + (games.Any(g => g.Date < new DateTime(player.FirstSeason - 1, 1, 1)) ? "Found a game before the player's first season. " : null)
                    + (games.Any(g => g.Date > new DateTime(player.LastSeason, 12, 31)) ? "Found a game after the player's last season. " : null)
                    + (games.Any(g => g.Points > 100) ? "Found a game where the player had more than 100 points. " : null)
                    + (games.Any(g => g.TotalRebounds > 55) ? "Found a game where the player had more than 55 rebounds. " : null)
                    + (games.Any(g => g.Assists > 30) ? "Found a game where the player had more than 30 assists. " : null)
                    + (games.Any(g => g.Points < 0 || g.TotalRebounds < 0 || g.Assists < 0) ? "Found a game where the player had negative points, rebounds, or assists. " : null)
                    // Bill Russell never missed an entire regular season, or the playoffs. Use him as a canary to
                    // identify if we're missing games. Hard to do something better because of all the players that
                    // missed entire seasons and the playoffs--their game logs don't even have pgl_basic tables.
                    + (player.ID == "russebi01" && (!games.Any(g => !g.IsPlayoffGame) || !games.Any(g => g.IsPlayoffGame)) ? "Missing games for Bill Russell. " : null);

                if (!string.IsNullOrEmpty(errors))
                    throw new ScrapeException($"Failed to scrape games: {errors}", gameLogUrl);

                return games;
            }
        }
    }
}
