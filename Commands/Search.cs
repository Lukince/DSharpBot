using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Abot2.Crawler;
using Abot2.Poco;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using DSharpPlus.Entities;
using DiscordBot.Attributes;

namespace DiscordBot.Commands
{
    [CheckAdmin]
    class Search : BaseCommandModule
    {
        [Command("msdc")]
        public async Task<string> MSDCSearch(CommandContext ctx, string type, string version, string content)
        {
            Dictionary<string, string[]> versions = new Dictionary<string, string[]>()
            {
                { "netcore", new string[] { "1.0", "1.1", "2.0", "2.1", "2.2", "3.0", "3.1" } },
                { "netframework", new string[] { "1.1", "2.0", "3.0", "3.5", "4.0", "4.5.1", "4.5.2",
                                                 "4.6", "4.6.1", "4.6.2", "4.7", "4.7.1", "4.7.2", "4.8"} },
                { "netstandard", new string[] { "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "2.0", "2.1" } }
            };

            if (!versions.ContainsKey(type))
                throw new ArgumentException($"Type Error\n```Type : {string.Join(", ", versions.Keys)}```");

            if (!versions[type].Contains(version))
                throw new ArgumentException($"Version Error\n```Versions : {string.Join(", ", versions[type])}");

            string url = $"https://docs.microsoft.com/ko-kr/dotnet/api/{content}?view={type}-{version}";

            IWebCrawler crawler = new PoliteWebCrawler(new CrawlConfiguration()
            {
                MaxConcurrentThreads = 1,
                MaxPagesToCrawl = 10,
                MaxPagesToCrawlPerDomain = 0,
                MaxPageSizeInBytes = 0,
                UserAgentString = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko",
                CrawlTimeoutSeconds = 0,
                DownloadableContentTypes = "text/html, text/plain",
                IsUriRecrawlingEnabled = false,
                IsExternalPageCrawlingEnabled = false,
                IsExternalPageLinksCrawlingEnabled = false,
                HttpServicePointConnectionLimit = 200,
                HttpRequestTimeoutInSeconds = 15,
                HttpRequestMaxAutoRedirects = 7,
                IsHttpRequestAutoRedirectsEnabled = true,
                IsHttpRequestAutomaticDecompressionEnabled = false,
                IsSendingCookiesEnabled = false,
                IsSslCertificateValidationEnabled = false,
                IsRespectUrlNamedAnchorOrHashbangEnabled = false,
                MinAvailableMemoryRequiredInMb = 0,
                MaxMemoryUsageInMb = 0,
                MaxMemoryUsageCacheTimeInSeconds = 0,
                MaxCrawlDepth = 100,
                MaxLinksPerPage = 1000,
                IsForcedLinkParsingEnabled = false,
                MaxRetryCount = 0,
                MinRetryDelayInMilliseconds = 0,
                IsAlwaysLogin = false,
                LoginUser = "",
                LoginPassword = "",
                IsRespectRobotsDotTextEnabled = false,
                IsRespectMetaRobotsNoFollowEnabled = false,
                IsRespectAnchorRelNoFollowEnabled = false,
                IsIgnoreRobotsDotTextIfRootDisallowedEnabled = false,
                RobotsDotTextUserAgentString = "abot",
                MaxRobotsDotTextCrawlDelayInSeconds = 5,
                MinCrawlDelayPerDomainMilliSeconds = 1000
            });

            crawler.PageCrawlStarting += (s, e) =>
                Console.WriteLine($"Start : {e.PageToCrawl}");

            crawler.PageCrawlCompleted += (s, e) =>
            {
                CrawledPage pg = e.CrawledPage;

                string fn = pg.Uri.Segments[pg.Uri.Segments.Length - 1];
                File.WriteAllText($"CrawlingData/{fn}.html", pg.Content.Text);

                Console.WriteLine("Completed : {0}", pg.Uri.AbsoluteUri);
            };

            Uri uri = new Uri(url);

            await crawler.CrawlAsync(uri);
            await ctx.RespondWithFileAsync(fileName: $"CrawlingData/{content}.html", new FileStream($"CrawlingData/{content}.html", FileMode.Open), content: url);
            return $"CrawlingData/{content}.html";
        }

        [Command("ReadXml")]
        public async Task ReadXml(CommandContext ctx, string uri)
        {
            XDocument doc = XDocument.Load(uri);
            var allElements = doc.Descendants().ToArray();

            await File.WriteAllTextAsync("Data/Test.txt", allElements.ToString());
        }

        [Command("ReadHtml")]
        public async Task ReadHtml(CommandContext ctx, string uri)
        {
            HtmlDocument html = new HtmlDocument();
            var web = new HtmlWeb();
            html = web.Load(uri);
            var Elements = html.DocumentNode.Descendants();

            var toftitle = Elements.Where(l => l.Name == "div" && l.Attributes["class"] != null
            && l.Attributes["class"].Value.Contains("block_content")).ToArray();

            //await File.WriteAllTextAsync("Data/Test.txt");
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }
    }
}
