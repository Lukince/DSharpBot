using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public static string DeleteString(this string content, IEnumerable<string> Delstring)
        {
            string s = content;
            foreach (string d in Delstring)
                s = string.Join(string.Empty, s.DeleteString(d));

            return s;
        }

        public static IEnumerable<string> Combine(this IEnumerable<string> array1, IEnumerable<string> array2)
        {
            List<string> output = new List<string>();
            foreach (string s in array1)
                output.Add(s);
            foreach (string s in array2)
                output.Add(s);
            return output.ToArray();
        }

        public static T[] Map<T>(this IEnumerable<T> array, Func<T, T> func) //where T : IEnumerable<T>
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

        public static string ToString(this Dictionary<string, Tuple<string, string>> dic, uint max = 0)
        {
            int m = (int)max;
            if (max == 0)
                m = dic.Count;

            string str = string.Empty;

            for (int i = 0; i < max; i++)
                str += $"{dic.Keys.ToArray()[i]} : {dic.Values.ToArray()[i].Item1}\n";

            return str[0..^1];
        }

        public static bool Contains(this string cnt, IEnumerable<string> values)
        {
            foreach (var value in values)
                if (cnt.Contains(value))
                    return true;
            return false;
        }
    }
}
