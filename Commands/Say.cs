using System;
using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using static DiscordBot.Variable;
using DSharpPlus.Interactivity;
using System.IO;
using IronPython.Runtime;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiscordBot.Commands
{
    public static class SayVariable
    {
        public static string[] Hello =
        {
            "안녀엉! :laughing:",
            "반가워어~ :wink:",
            "반가워 난 라히얌 ;)",
            ":wave:",
            "하이욤! :person_raising_hand:",
         "안뇽! :kissing_heart:"
        };
    }

    [BlackList]
    class Say
    {
        [Command("단어추가")]
        public async Task WordAdd(CommandContext ctx, string word, params string[] description)
        {
            string content = string.Join(' ', description);
            var compair = File.ReadAllLines(WordPath).Where(l => l.Split('|')[1] == content);

            if (compair.Count() > 0)
            {
                string c;
                if (compair.First().Split('|').Length == 2)
                    c = "이미 개발자에 의해 정의된 설명이예요! 다른 설명을 추가해볼까요?";
                else
                    c = compair.First().Split('|')[2] + "님이 먼저 가르쳐 주셨어요! 혹시 또다른 설명이 있나요..?";
                await ctx.RespondAsync(c); return;
            }

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "추가 확인",
                Footer = GetFooter(ctx),
                Timestamp = DateTime.Now
            };

            dmb.AddField($"{word} => {string.Join(' ', description)}",
                "단어 추가 유의 사항\n" +
                "1. 단어를 추가하게 되면 모든 이들이 사용할 수 있게 됩니다.\n" +
                "2. 단어나 문장에 욕설이나 비하 등은 작성하지 마십시오\n" +
                "3. 다른이가 올린 단어와 겹치는 경우 랜덤한 확률로 나옵니다\n" +
                "4. 단어는 띄어쓰기 없이 써주세요");

            var msg = await ctx.RespondAsync(embed: dmb.Build());
            await Task.Delay(1000);
            await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("Correct")));
            await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("NotCorrect")));

            var interactivity = ctx.Client.GetInteractivityModule();
            var reactions = await interactivity.WaitForReactionAsync(l => l.Id == GetEmoji("Correct") || l.Id == GetEmoji("NotCorrect"), ctx.User);

            if (reactions != null)
            {
                if (reactions.Emoji.Id == GetEmoji("Correct"))
                {
                    AddWords(ctx, word, string.Join(' ', description));
                    await msg.DeleteAsync();
                    await ctx.RespondAsync($"`{word}`(은)는 `{string.Join(' ', description)}`이군요!");
                }
                else
                {
                    await msg.DeleteAsync();
                    await ctx.RespondAsync("취소되었습니다");
                }

            }
        }

        private void AddWords(CommandContext ctx, string word, string description)
        {
            string content = $"{word}|{description}|{ctx.User.Username}#{ctx.User.Discriminator}";

            if (!File.Exists(WordPath))
                File.Create(WordPath);

            File.AppendAllText(WordPath, content + Environment.NewLine);
        }

        public async Task Saying(CommandContext ctx, string Content)
        {
            Random rnd = new Random();

            if (Content == "생일")
            {
                var date = ctx.Client.CurrentUser.CreationTimestamp;
                int age = DateTime.Now.Year - date.Year;
                await ctx.RespondAsync($"라히의 생일은 {date.Month}월 {date.Day}일 이예요! 이제 만 {age}세예요!");
            }

            else if (Content.Contains("안녕") || Content.Contains("안뇽")
             || Content.Contains("안녀엉") || Content.Contains("반가워")
             || Content.Contains("안뇨옹") || Content.Contains("헬로")
             || Content.Contains("핼로") || Content.Contains("하이"))
            {
                await ctx.RespondAsync(SayVariable.Hello[rnd.Next(0, SayVariable.Hello.Length - 1)]);
            }
            else
            {
                if (!File.Exists(WordPath))
                    return;

                string[] lines = File.ReadAllLines(WordPath);
                List<string> list = new List<string>();

                foreach (string content in lines)
                {
                    string[] s = content.Split('|');
                    string word = s[0];

                    if (word == RemoveSpace(Content))
                        list.Add(content);
                }

                string[] directory = list.ToArray();

                if (directory.Length < 1)
                {
                    string[] unknown = { $"`{Content}`?", "모르는 말이예요!", "무슨말인지 잘 모르겠어요", "그게 뭐죠..?" };
                    await ctx.RespondAsync(unknown[rnd.Next(0, unknown.Length - 1)]);
                    return;
                }

                string[] ss = directory[rnd.Next(0, directory.Length - 1)].Split('|');

                string c = ss[1];
                if (c.Contains("//Word:"))
                {
                    string word = c.Split("//Word:")[1].Split("//")[0];
                    await Saying(ctx, c.Replace($"//Word:{word}//", word));
                    return;
                }

                if (ss.Length != 3)
                {
                    c = ss[1]
                            .Replace("//Username//", ctx.User.Username)
                            .Replace("//Displayname//", ctx.Member.Username)
                            .Replace("//Mention//", ctx.User.Mention)
                            .Replace("//Id//", ctx.User.Id.ToString())
                            .Replace("//Channelname//", ctx.Channel.Name)
                            .Replace("//Channelid//", ctx.Channel.Id.ToString())
                            .Replace("//Channeltopic//", ctx.Channel.Topic)
                            .Replace("//Guildname//", ctx.Guild.Name)
                            .Replace("//Guildid//", ctx.Guild.Id.ToString());
                }

                string sss = $"{c}";
                if (ss.Length == 3)
                    sss += $"\n```{ss[2]}님이 알려주셨어요!```";

                await ctx.RespondAsync(sss);
            }
        }
    }
}
