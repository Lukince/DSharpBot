using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Net;
using static DiscordBot.Index;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using System;
using System.Threading.Tasks;
using System.Collections;
using IronPython.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Scripting.Actions;

namespace DiscordBot
{
    class Variable
    {
        public static class Urls
        {
            public static string Papago { get; } = "http://blogfiles.naver.net/MjAxNzAxMjVfMjY0/MDAxNDg1MzU1NTY1NTA0.LUHh0-RMYj4x21WjrObA2Ga_WXxQhyYmKZc73qP4rrcg.TfNQRp2SY_dAWQehsRc_i18-zPyGoMnDluGxll4fJCIg.PNG.thankhawaii/%BD%BA%C5%A9%B8%B0%BC%A6_2017-01-25_%BF%C0%C0%FC_9.16.58.png";
        }

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

        public const int MaxPage = 5;

        public static ulong[] GetAdminIds()
        {
            List<ulong> list = new List<ulong>();

            foreach (string s in File.ReadLines("Data/AdminId.txt"))
            {
                list.Add(Convert.ToUInt64(s));
            }

            return list.ToArray();
        }

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

        public static Dictionary<string, ulong> DiscordEmojis = new Dictionary<string, ulong>()
        {
            { "Correct", 617684865529282570 },
            { "NotCorrect", 617684780691095555 }
        };

        public static ulong GetEmoji(string Key)
        {
            if (DiscordEmojis.TryGetValue(Key, out ulong value))
                return value;
            else
                throw new ArgumentException("Wrong Key");
        }

        public static string RemoveSpace(params string[] content)
        {
            string s = string.Join(' ', content);

            char[] a = s.Where(l =>
            {
                if (l == ' ')
                    return false;
                else
                    return true;
            }).ToArray();

            return new string(a);
        }

        public static readonly string WordPath = "Data/Words.dat";

        /*TODO: 사용자와 정보 비교시 적거나 많은 경우를 따져서 특정 메서드로 따로 받게하기
         * ex) ui.Money < 1 => 돈이 부족해요! 현재 소지금 : {ui.Money} */
    }
}
