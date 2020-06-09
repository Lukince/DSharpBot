using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [BlackList]
    class Say
    {
        [Command("안뇽"), Hidden]
        public async Task Hi2(CommandContext ctx)
        {
            await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, $"라히야 안녕");
        }

        [Command("안녀엉"), Hidden]
        public async Task Hi3(CommandContext ctx)
        {
            await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, $"라히야 안녕");
        }

        [Command("안녕")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync("안뇽! :kissing_heart:");
        }
    }
}
