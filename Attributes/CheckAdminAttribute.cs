using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Linq;
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

            if (GetAdminIds().Contains(ctx.User.Id))
                return Task.FromResult(true);

            if (ctx.Command.ExecutionChecks.Contains(new DoNotUseAttribute()))
                return Task.FromResult(false);

            ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            ctx.RespondAsync("봇을 관리할 권한이 없어요!");
            return Task.FromResult(false);
        }
    }
}
