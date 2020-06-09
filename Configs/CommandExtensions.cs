using System.Linq;
using static DiscordBot.Index;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;

namespace DiscordBot.Configs
{
    public static class CommandExtensions
    {
        public static void Send(this ulong id, CommandContext ctx, string content)
        {
            var user = ctx.Guild.GetMemberAsync(id).GetAwaiter().GetResult();
            user.SendMessageAsync(content);
        }
    }
}
