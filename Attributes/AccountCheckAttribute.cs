using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Attributes
{
    class AccountCheckAttribute : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
                return Task.FromResult(false);

            if (ctx.Client.CurrentApplication.Owner == ctx.User)
                return Task.FromResult(true);

            if (ctx.Command.ExecutionChecks.Contains(new DoNotUseAttribute()))
                return Task.FromResult(false);

            string Id = $"Account/{ctx.User.Id}";
            if (File.Exists(Id))
            {
                return Task.FromResult(true);
            }
            else
            {
                ctx.RespondAsync($"{ctx.User.Mention} 가입을 하셔야 사용하실수 있어요! '라히야 가입'으로 어서 가입해봐요!");
                return Task.FromResult(false);
            }
        }
    }
}
