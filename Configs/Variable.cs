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
        private static Random rnd = new Random();

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

        /// <summary>
        /// 어드민 아이디를 얻는 명령어
        /// </summary>
        /// 
        /// <returns>Return AdminIds (ulong[])</returns>
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
                else if (l == '!')
                    return false;
                else if (l == '?')
                    return false;
                else
                    return true;
            }).ToArray();

            return new string(a);
        }

        public static readonly string WordPath = "Data/Words.dat";

        public async Task CallName(DiscordMessage msg)
        {
            await msg.RespondAsync("네? 저 부르셨나요?");
        }

        public static string GetRandom(int[] percent, string[] value)
        {
            if (percent.Length < 1 || value.Length < 1)
                throw new ArgumentException("Argument cannot be null");

            if (percent.Length != value.Length)
                throw new ArgumentException("percent array's length and value array's length must be equal");

            foreach (int i in percent)
            {
                if (i <= 0)
                    throw new ArgumentException("percent value must be positive number");
            }

            int total = 0;
            foreach (int i in percent)
                total += i;

            List<int> keylist = new List<int>();
            for (int i = 0; i < percent.Length; i++)
            {
                int k = 0;
                for (int j = 0; j < i + 1; j++)
                    k += percent[j];
                keylist.Add(k);
            }
            int[] key = keylist.ToArray();

            Random rnd = new Random();

            int result = rnd.Next(0, total);

            for (int i = 0; i < percent.Length; i++)
            {
                if (i == 0)
                {
                    if (0 <= result && result <= key[i])
                        return value[i];
                }
                else
                {
                    if (key[i - 1] <= result && result <= key[i])
                        return value[i];
                }
            }

            return string.Empty;
        }

        public static int BitcoinPrice = 0;

        public static string RemoveSpace(string content)
        {
            return new string(content.Where(c => c != ' ').ToArray());
        }

        public static int EvalInequalityCount(string expression)
        {
            string content = RemoveSpace(expression);
            return 0;
            //TODO: 부등호 기준으로 나누기;
        }

        public static int[] intToArray(int num)
        {
            string s = num.ToString();
            List<int> list = new List<int>();
            foreach (char c in s.ToCharArray())
            {
                Console.Write(c.ToString() + " ");
                list.Add(int.Parse(c.ToString()));
            }
            return list.ToArray();
        }

        public static string[] pownumber =
        {
            "⁰",
            "¹",
            "²",
            "³",
            "⁴",
            "⁵",
            "⁶",
            "⁷",
            "⁸",
            "⁹"
        };

        public static string GetPowString(int number)
        {
            int[] numarr = intToArray(number);

            string output = string.Empty;
            foreach (int i in numarr)
            {
                output += pownumber[i];
            }

            return output;
        }

        /*TODO: 사용자와 정보 비교시 적거나 많은 경우를 따져서 특정 메서드로 따로 받게하기
         * ex) ui.Money < 1 => 돈이 부족해요! 현재 소지금 : {ui.Money} */

        public static DiscordColor GetRandomColor() =>
            new DiscordColor(rnd.Next(0, 16777215));

        public static string GetDate(TimeSpan subtime)
        {
            string date = string.Empty;

            if (subtime.Days != 0)
                date += $"{subtime.Days}일 ";

            if (subtime.Hours != 0)
                date += $"{subtime.Hours}시간 ";

            if (subtime.Minutes != 0)
                date += $"{subtime.Minutes}분 ";

            date += $"{subtime.Seconds}초";

            return date;
        }

        public async Task SendWebhook(CommandContext ctx, DiscordWebhook webhook, string title, string content)
        {
            DiscordEmbedBuilder WebhookEmbed = new DiscordEmbedBuilder()
            {
                Title = title,
                Description = $"User Id : {ctx.User.Id}",
                Timestamp = DateTime.Now,
                Footer = GetFooter(ctx)
            }.AddField("Content", content);

            await ctx.RespondAsync("처리되었습니다!");
            await webhook.ExecuteAsync(embeds: new DiscordEmbed[] { WebhookEmbed.Build() });
        }
    }
}
