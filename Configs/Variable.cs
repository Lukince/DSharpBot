using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Net;
using static DiscordBot.Index;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using System;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Variable
    {
        public static DiscordColor[] RandomColor =
        {
            DiscordColor.Aquamarine,
            DiscordColor.Azure,
            DiscordColor.Black,
            DiscordColor.Blue,
            DiscordColor.Blurple,
            DiscordColor.Brown,
            DiscordColor.Chartreuse,
            DiscordColor.CornflowerBlue,
            DiscordColor.Cyan,
            DiscordColor.DarkBlue,
            DiscordColor.DarkButNotBlack,
            DiscordColor.DarkGray,
            DiscordColor.DarkGreen,
            DiscordColor.DarkRed,
            DiscordColor.Gold,
            DiscordColor.Goldenrod,
            DiscordColor.Gray,
            DiscordColor.Grayple,
            DiscordColor.Green,
            DiscordColor.HotPink,
            DiscordColor.IndianRed,
            DiscordColor.LightGray,
            DiscordColor.Lilac,
            DiscordColor.Magenta,
            DiscordColor.MidnightBlue,
            DiscordColor.NotQuiteBlack,
            DiscordColor.Orange,
            DiscordColor.PhthaloBlue,
            DiscordColor.PhthaloGreen,
            DiscordColor.Purple,
            DiscordColor.Red,
            DiscordColor.Rose,
            DiscordColor.SapGreen,
            DiscordColor.Sienna,
            DiscordColor.SpringGreen,
            DiscordColor.Teal,
            DiscordColor.Turquoise,
            DiscordColor.VeryDarkGray,
            DiscordColor.Violet,
            DiscordColor.Wheat,
            DiscordColor.White,
            DiscordColor.Yellow
        };

        public static string[] languagelist =
        {
            "ko",   //한국어
            "en",   //영어
            "ja",   //일본어
            "zh",   //중국어
            "de",   //독일어
            "ru"    //러시아
        };

        public async Task BugReport(CommandContext ctx, Exception e = null)
        {
            string err;

            if (e == null)
                err = string.Empty;
            else
                err = $"```{e.Data}\n{e.Message}\n{e.StackTrace}\n{e.InnerException}\n{e.Source}```";

            var webhook = ctx.Client.GetWebhookAsync(716214815634227252).GetAwaiter().GetResult();

            await webhook.ExecuteAsync($"{ctx.Message.Content}\n{err}");
        }

        public static string[] Numbers =
        {
            ":one:",
            ":two:",
            ":three:",
            ":four:",
            ":five:",
            ":six:",
            ":seven:",
            ":eight:",
            ":nine:",
        };

        public const int MaxPage = 4;

        public static ulong[] AdminIdList =
        {
            378535260754935819
        };

        public static EmbedFooter GetFooter(CommandContext ctx)
        {
            try
            {
                EmbedFooter footer = new EmbedFooter()
                {
                    IconUrl = ctx.Member.AvatarUrl,
                    Text = $"Request by {ctx.User.Username}#{ctx.User.Discriminator}"
                };

                return footer;
            }
            catch (Exception e)
            {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "GetFooter() Method", e.ToString(), DateTime.Now);
                throw new Exception(e.Message);
            }
        }

        public static string HistoryPath = "Data/History.txt";

        public class TestVariables
        {
            public DiscordMessage Message { get; set; }
            public DiscordChannel Channel { get; set; }
            public DiscordGuild Guild { get; set; }
            public DiscordUser User { get; set; }
            public DiscordMember Member { get; set; }
            public CommandContext Context { get; set; }

            public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx)
            {
                this.Client = client;

                this.Message = msg;
                this.Channel = msg.Channel;
                this.Guild = this.Channel.Guild;
                this.User = this.Message.Author;
                if (this.Guild != null)
                    this.Member = this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                this.Context = ctx;
            }

            public DiscordClient Client;
        }
    }
}
