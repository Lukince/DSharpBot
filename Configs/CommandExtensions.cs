using System.Linq;
using static DiscordBot.Index;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
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
    }
}
