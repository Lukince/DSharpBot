using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using static DiscordBot.Variable;

namespace DiscordBot.Attributes
{
    class CheckAdminAttribute : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
                return Task.FromResult(false);

            if (ctx.Client.CurrentApplication.Owner == ctx.User)
                return Task.FromResult(true);

            foreach (ulong id in AdminIdList)
            {
                if (ctx.User.Id == id)
                    Task.FromResult(true);
            }

            ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            ctx.RespondAsync("봇을 관리할 권한이 없어요!");
            return Task.FromResult(false);
        }
    }
}
