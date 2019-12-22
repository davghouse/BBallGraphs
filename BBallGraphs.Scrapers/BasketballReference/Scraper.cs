using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using BBallGraphs.Scrapers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public static class Scraper
    {
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

            string playerFeedsUrl = "https://www.basketball-reference.com/players";
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
                    throw new ScrapeException($"Failed to scrape player feeds.",
                        playerFeedsUrl, playerFeedsDocument.Source.Text);

                return playerFeeds;
            }
        }

        public static async Task<IReadOnlyList<Player>> GetPlayers(IPlayerFeed playerFeed)
        {
            var browsingContext = GetBrowsingContext();

            using (var playerFeedDocument = await browsingContext.OpenAsync(playerFeed.Url))
            {
                (string url, string name) getPlayerUrlAndName(IElement playerCell)
                {
                    // <strong> tag wraps anchors for active players.
                    var anchorElement = (playerCell.FirstChild as IHtmlAnchorElement)
                        ?? (IHtmlAnchorElement)playerCell.FirstChild.FirstChild;
                    return (anchorElement.Href.Trim(), anchorElement.TextContent.Trim());
                }

                var players = playerFeedDocument
                    .QuerySelectorAll("table.stats_table tbody tr:not(.thead)")
                    .Select(e => e.Children)
                    .Select(c => new Player
                    {
                        Url = getPlayerUrlAndName(c[0]).url,
                        Name = getPlayerUrlAndName(c[0]).name,
                        FromYear = int.Parse(c[1].TextContent),
                        ToYear = int.Parse(c[2].TextContent),
                        Position = c[3].TextContent.Trim(),
                        HeightInches = ScrapeHelper.ParseHeight(c[4].TextContent),
                        WeightPounds = double.TryParse(c[5].TextContent, out double weightPounds)
                            ? weightPounds : (double?)null,
                        BirthDate = DateTime.TryParse(c[6].TextContent, out DateTime birthDate)
                            ? birthDate.AsUtc() : ScrapeHelper.GetEstimatedBirthDate(getPlayerUrlAndName(c[0]).name),
                        FeedUrl = playerFeed.Url
                    })
                    .ToArray();

                if (players.Length == 0
                    || players.Select(p => p.Url).Distinct().Count() != players.Length
                    || players.Any(p => p.FromYear < 1946 || p.ToYear > DateTime.UtcNow.Year + 1
                        || p.HeightInches < 12 || p.HeightInches > 120
                        || p.BirthDate < new DateTime(1900, 1, 1)))
                    throw new ScrapeException("Failed to scrape players.",
                        playerFeed.Url, playerFeedDocument.Source.Text);

                return players;
            }
        }
    }
}
