using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DiscordBot.Variable;
using static DiscordBot.Index;

namespace DiscordBot.Commands
{
    [BlackList, CheckAdmin, Group("영단어")]
    class EnglishWord
    {
        private static Random rnd = new Random();
        private static readonly string WordListPath = "Data/EnglishWord.dat";
        public static Dictionary<string, Dictionary<string, string>> WordList = new Dictionary<string, Dictionary<string, string>>();

        public static void ReloadWords()
        {
            string[] wordsperday = File.ReadAllText(WordListPath).Split('%');
            for (int i = 0; i < wordsperday.Length; i++)
                WordList.Add($"{i + 1}일차", GetWords(wordsperday[i]));
        }

        private static Dictionary<string, string> GetWords(string s)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string[] words = s.Split('\n');
            for (int i = 0; i < words.Length - 1; i++)
            {
                string[] w = words[i].Split('|');
                output.Add(w[0], w[1]);
            }

            return output;
        }

        [Command("WordReload")]
        public async Task ReloadingWords(CommandContext ctx)
        {
            WordList.Clear();
            ReloadWords();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("신고")]
        public async Task EnglishWordReport(CommandContext ctx, params string[] content)
        {
            Variable v = new Variable();
            await v.SendWebhook(ctx, BugReport, "영단어", string.Join(' ', content));
        }

        [Command("단어")]
        public async Task English_Word(CommandContext ctx, int page, bool blind = false)
        {
            if (page > WordList.Values.Count)
            {
                await ctx.RespondAsync($"단어장은 {WordList.Values.Count}일차 까지 있어요!");
                return;
            }

            string lockfile = $"tmp/u{ctx.User.Id}";
            if (File.Exists(lockfile))
            {
                await ctx.RespondAsync("이미 실행중인 영단어장이 있어요!");
                return;
            }

            File.Create(lockfile).Dispose();

            string[] tips =
            {
                "해당 단어들은 능률 보카 고교 필수편과 커스텀 단어장 등을 기반으로 제작되었습니다",
                "단어에 오타나 오역 등이 있다면 `라히야 영단어 신고 [내용]` 으로 신고해주세요",
                "여러분을 위한 커스텀 단어장 등을 구상하고 있습니다! 조금만 기다려 주세요",
                "단어장을 준비하는 2초 동안 이곳에 팁이 표시되요!",
                "단어를 외울때 시간을 정해놓고 외우면 좋아요!",
                "`라히야 영단어 단어 [번호] true` 를 통해서 영어 단어를 블라인드 처리 할 수 있어요!",
                "현재 해당 단어장으로 문제를 만드는 시스템을 구상하고 있어요!",
                "팁은 계속해서 늘어날 거예요! 자신만에 단어 외우는 비법도 `라히야 영단어 신고 [내용]` 으로 얘기해줘요!"
            };

            DiscordEmbedBuilder WordEmbed = new DiscordEmbedBuilder()
            {
                Title = "잠시만 기다려 주세요",
                Description = tips[rnd.Next(0, tips.Length - 1)],
                Footer = GetFooter(ctx),
                Timestamp = DateTime.Now,
                Color = GetRandomColor()
            };

            DiscordEmoji[] Emoji =
            {
                DiscordEmoji.FromName(ctx.Client, ":arrow_left:"),
                DiscordEmoji.FromName(ctx.Client, ":stop_button:"),
                DiscordEmoji.FromName(ctx.Client, ":arrow_right:")
            };

            if (WordList.TryGetValue($"{page}일차", out Dictionary<string, string> word))
            {
                string[] EnglishList = word.Keys.ToArray();
                string[] KoreanList = word.Values.ToArray();

                var msg = await ctx.RespondAsync(embed: WordEmbed.Build());

                await Task.Delay(1000);

                foreach (var emoji in Emoji)
                {
                    await msg.CreateReactionAsync(emoji);
                    await Task.Delay(500);
                }

                await Task.Delay(1000);

                WordEmbed.Title = $"{page}일차 : 1 / {word.Count}";
                WordEmbed.Description = null;
                if (blind)
                    WordEmbed.AddField($"||{EnglishList[0]}||", KoreanList[0]);
                else
                    WordEmbed.AddField($"{EnglishList[0]}", KoreanList[0]);
                await msg.ModifyAsync(embed: WordEmbed.Build());

                int index = 0;

                while (true)
                {
                    var interactivity = ctx.Client.GetInteractivityModule();
                    var reaction = await interactivity.WaitForReactionAsync(l => Emoji.Contains(l), ctx.User);

                    if (reaction != null)
                    {
                        if (reaction.Emoji == Emoji[0])
                        {
                            if (index > 0)
                                index--;
                        }
                        else if (reaction.Emoji == Emoji[1])
                        {
                            await ctx.RespondAsync($"{page}일차 단어 학습 끝!\n" +
                                $"```학습 시간 : {GetDate(DateTime.Now.Subtract(msg.Timestamp.DateTime))}```");
                            await msg.DeleteAsync();
                            break;
                        }
                        else if (reaction.Emoji == Emoji[2])
                        {
                            if (index < word.Count - 1)
                                index++;
                        }

                        await msg.DeleteReactionAsync(reaction.Emoji, ctx.User);
                        WordEmbed.ClearFields();
                        WordEmbed.Title = $"{page}일차 : {index + 1} / {word.Count}";
                        if (blind)
                            WordEmbed.AddField($"||{EnglishList[index]}||", KoreanList[index]);
                        else
                            WordEmbed.AddField($"{EnglishList[index]}", KoreanList[index]);
                        await msg.ModifyAsync(embed: WordEmbed.Build());
                    }
                }
            }

            File.Delete(lockfile);
        }
    }
}
