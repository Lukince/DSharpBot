using System.Linq;
using static DiscordBot.Index;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading;

namespace DiscordBot.Configs
{
    public class CommandExtensions
    {
        public async Task<DiscordMessage> SendDM(CommandContext ctx, ulong id, string content = null, DiscordEmbed embed = null)
        {
            var user = await ctx.Guild.GetMemberAsync(id);
            return await user.SendMessageAsync(content, embed: embed);
        }

        public async Task CreateReactionsAsync(DiscordMessage msg, DiscordEmoji[] emojis, int waitTime = 0)
        {
            foreach (var emoji in emojis)
            {
                await msg.CreateReactionAsync(emoji);
                await Task.Delay(waitTime);
            }
        }
    }

    public static class Extensions
    {
        public static string DeleteString(this string content, string Delstring)
        {
            string[] s = content.Split(Delstring);
            string output = string.Join(string.Empty, s);
            return output;
        }

        public static string DeleteString(this string content, string[] Delstring)
        {
            string s = content;
            foreach (string d in Delstring)
                s = string.Join(string.Empty, s.DeleteString(d));

            return s;
        }

        public static string[] Combine(this string[] array1, string[] array2)
        {
            List<string> output = new List<string>();
            foreach (string s in array1)
                output.Add(s);
            foreach (string s in array2)
                output.Add(s);
            return output.ToArray();
        }

        public static T[] Map<T>(this T[] array, Func<T, T> func) //where T : IEnumerable<T>
        {
            List<T> list = new List<T>();
            foreach (T data in array)
            {
                list.Add(func.Invoke(data));
            }

            return list.ToArray();
        }

        public static string ToString(this DiscordChannel[] chns)
        {
            string output = string.Empty;
            foreach (var chn in chns)
                output += $"{chn.Mention} ";
            return output.TrimEnd();
        }

        public static bool Contains<T>(this T[] ts, T t)
        {
            foreach (T tt in ts)
                if (tt.Equals(t))
                    return true;
            return false;
        }

        public static void Add(this Dictionary<DiscordGuild, CancellationToken[]> Queue, DiscordGuild guild, CancellationToken token)
        {
            if (Queue.ContainsKey(guild))
            {
                List<CancellationToken> tokens = new List<CancellationToken>();

                foreach (var t in Queue[guild])
                    tokens.Add(t);
                tokens.Add(token);
                Queue[guild] = tokens.ToArray();
            }
            else
                Queue.Add(guild, new CancellationToken[] { token });
        }
    }
}
