using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using static DiscordBot.Variable;
using System.Dynamic;

namespace DiscordBot.Commands
{
    [BlackList, CheckAdmin, Group("영단어")]
    class EnglishWord
    {
        private static readonly string WordListPath = "Data/EnglishWord.dat";
        public static Dictionary<string, Dictionary<string, string>> WordList = new Dictionary<string, Dictionary<string, string>>();

        public static void ReloadWords()
        {
            string[] wordsperday = File.ReadAllText(WordListPath).Split('%');
            for (int i = 0; i < wordsperday.Length; i++)
            {
                WordList.Add($"{i + 1}일차", GetWords(wordsperday[i]));
            }
        }

        private static Dictionary<string, string> GetWords(string s)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string[] words = s.Split('\n');
            foreach (string word in words)
            {
                string[] w = word.Split('|');
                Console.WriteLine(word + "*");
            }

            return output;
        }

        [Command("단어")]
        public async Task English_Word(CommandContext ctx, int page)
        {
            ReloadWords();
            DiscordEmbedBuilder WordEmbed = new DiscordEmbedBuilder()
            {
                Title = "잠시만 기다려 주세요",
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
                    await msg.CreateReactionAsync(emoji);

                await Task.Delay(1000);

                WordEmbed.Title = $"{page}일차";
                WordEmbed.AddField($"||{EnglishList[0]}||", KoreanList[0]);
                await msg.ModifyAsync(embed: WordEmbed.Build());

                /*
                while (true)
                {
                    var interactivity = ctx.Client.GetInteractivityModule();
                    var reaction = await interactivity.WaitForReactionAsync(l => Emoji.Contains(l), ctx.User);

                    if (reaction != null)
                    {
                        
                    }
                }
                */
            }
        }
    }
}
