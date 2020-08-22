using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Attributes
{
    class ContentFilterAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
                return Task.FromResult(false);

            if (ctx.Client.CurrentApplication.Owners.First() == ctx.User)
                return Task.FromResult(true);

            return Task.FromResult(true); //필터링 작업 하기
        }
    }
}
