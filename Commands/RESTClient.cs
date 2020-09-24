using DiscordBot.Configs;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static DiscordBot.Config;
using static DiscordBot.Variable;

namespace DiscordBot.Commands
{
    class RESTClient : BaseCommandModule
    {
        protected ChromeDriverService _driverService;
        protected ChromeOptions _options;
        protected ChromeDriver _driver;

        public async Task GetCovid19(CommandContext ctx)
        {
            StringBuilder str = new StringBuilder();
            str.Append("http://openapi.data.go.kr/openapi/service/rest/Covid19/getCovid19InfStateJson");
            str.Append($"?serviceKey={DataKey}");
            str.Append("&numOfRows=1");
            WebClient web = new WebClient();
            var response = web.DownloadString(str.ToString());

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(response);
            var nodes = xml.SelectNodes("/response/body/items/item")[0];

            DiscordEmbedBuilder Covid19Embed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Blue,
                Timestamp = DateTimeOffset.Parse(nodes.SelectSingleNode("").InnerText),
                Title = "코로나 현황"
            }.AddField("검사", nodes.SelectSingleNode("accExamCnt").InnerText + "명", true)
            .AddField("확진자", nodes.SelectSingleNode("decideCnt").InnerText + "명", true)
            .AddField("사망자", nodes.SelectSingleNode("deathCnt").InnerText + "명", true);

            await ctx.RespondAsync(embed: Covid19Embed.Build());
        }

        public async Task GetConveni(CommandContext ctx)
        {
            StringBuilder str = new StringBuilder();
            str.Append("http://data.ex.co.kr/exopenapi/business/conveniServiceArea");
            str.Append($"?serviceKey={DataKey}");
            str.Append("&numOfRows=1");
            WebClient web = new WebClient();
            var response = web.DownloadString(str.ToString());

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(response);

            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(limit: 100));
        }

        private void InitSelenium(ref ChromeDriverService driverService, ref ChromeOptions options, ref ChromeDriver driver)
        {
            driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            options = new ChromeOptions();
            options.AddArgument("disable-gpu");
            options.AddArgument("headless");

            driver = new ChromeDriver(driverService, options);
        }

        [Command("SearchMS")]
        public async Task GetMSDC(CommandContext ctx, string type, string version, [RemainingText] string serach)
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
                throw new ArgumentException($"Version Error\n```Versions : {string.Join(", ", versions[type])}```");

            StringBuilder str = new StringBuilder();
            str.Append("https://docs.microsoft.com/ko-kr/dotnet/api/");
            str.Append($"?view={type}-{version}");
            str.Append($"&term={serach}");

            InitSelenium(ref _driverService, ref _options, ref _driver);

            _driver.Navigate().GoToUrl(str.ToString());

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var msg = await ctx.RespondAsync("Wait for ready");

            for (int i = 0; i < 5; i++)
            {
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, Numbers[i]));
                await Task.Delay(500);
            }

            await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, DiscordEmojis["NotCorrect"]));

            Console.WriteLine(_driver.Url);

            var results = _driver.FindElementsByCssSelector("#main > div.api-browser-results-container > table > tbody");

            string s = string.Empty;
            foreach (var r in results.Where(l => !l.Text.Contains(new string[]
            { "Constructor", "Field", "Method", "Property" })))
                s += $"{r.Text} ";

            if (string.IsNullOrEmpty(s))
            { await ctx.RespondAsync("No search result"); return; }

            Dictionary<string, Tuple<string, string>> dic = new Dictionary<string, Tuple<string, string>>();

            var length = GetMaxNum(str.Length, 5);

            for (int i = 0; i < length; i++)
            {
                var sr = s.Split('\n');
                var ss = sr[i].Split(' ');
                dic.Add(ss[0], new Tuple<string, string>(ss[1], string.Join(' ', ss[2..^1])));
            }

            await msg.ModifyAsync(content: string.Empty, embed: new DiscordEmbedBuilder()
            {
                Title = "검색 결과",
                Color = DiscordColor.Blue,
                Description = Formatter.MaskedUrl("Go to url", new Uri(_driver.Url)),
                Footer = GetFooter(ctx),
                Timestamp = DateTime.Now
            }.AddField("Select one", dic.ToString(5)).Build());

            var interactivity = ctx.Client.GetInteractivity();
            var result = (await interactivity.WaitForReactionAsync(l =>
                l.User == ctx.User && l.Channel == ctx.Channel
                && (Numbers.Contains(l.Emoji.GetDiscordName()) || l.Emoji.Id == DiscordEmojis["NotCorrect"]))).Result;

            if (result != null)
            {
                string sw = string.Empty;
                if (result.Emoji.Id == DiscordEmojis["NotCorrect"])
                { await msg.DeleteAsync(); return; }

                var num = -1;

                num = result.Emoji.GetDiscordName() switch
                {
                    ":one:" => 0,
                    ":two:" => 1,
                    ":three:" => 2,
                    ":four:" => 3,
                    ":five:" => 4,
                    _ => throw new Exception()
                };

                var d = dic.ElementAt(num);

                _driver.Navigate().GoToUrl($"https://docs.microsoft.com/ko-kr/dotnet/api/{d.Key.ToLower()}?view={type}-{version}");

                await Task.Delay(200);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"{d.Key} : {d.Value.Item1}",
                    Color = DiscordColor.Blue,
                    Footer = GetFooter(ctx),
                    Timestamp = DateTime.Now
                }.AddField("Description",
                    _driver.FindElementByCssSelector("#main > div.summaryHolder > div").Text + 
                    $"\n{Formatter.MaskedUrl(d.Key, new Uri(_driver.Url))}");

                if (d.Value.Item1.ToLower() != "namespace")
                {
                    var r = _driver.FindElementByClassName("metadata");
                    embed.AddField("Info", $"{r.Text}");
                }

                await msg.DeleteAllReactionsAsync();
                await msg.ModifyAsync(embed: embed.Build());
            }
        }

        private int GetMaxNum(int num, int max)
        {
            if (num > max)
                return max;
            else
                return num;
        }

        private int GetMinNum(int num, int min)
        {
            if (num < min)
                return min;
            else
                return num;
        }

        protected ChromeDriverService CustomChromeDriverService;
        protected ChromeOptions CustomChromeOptions;
        protected ChromeDriver CustomChromeDriver;

        [Command("ChromeNavigate")]
        public async Task ChromeNavigate(CommandContext ctx, string url)
        {
            InitSelenium(ref CustomChromeDriverService, ref CustomChromeOptions, ref CustomChromeDriver);

            CustomChromeDriver.Navigate().GoToUrl(url);
            var msg = await ctx.RespondAsync("페이지 이동중. 잠시만 기다려 주세요");
            await Task.Delay(1000);
            await msg.ModifyAsync(content: "CssSelector로 탐색해주세요");

            var interactivity = ctx.Client.GetInteractivity();
            var result = (await interactivity.WaitForMessageAsync(l =>
                l.Author == ctx.User && l.Channel == ctx.Channel)).Result;

            if (result is not null)
            {
                var element = CustomChromeDriver.FindElementByCssSelector(result.Content);
                await ctx.RespondAsync(element.Text);
            }
        }
    }
}
