using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Linq;
using static DiscordBot.Variable;
using IronPython.Runtime;
using System.Collections.Generic;

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

            CheckBaseAttribute check = new CheckAttribute();
            var CommandList = ctx.CommandsNext.RegisteredCommands.Where(l =>
                l.Value.ExecutionChecks.Contains(check));
            string[] StringCommandlist;
            List<string> list = new List<string>();

            foreach (var i in CommandList)
            {
                list.Add(i.Key);
            }

            StringCommandlist = list.ToArray();

            if (!StringCommandlist.Contains(ctx.Command.Name))
            {
                ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
                ctx.RespondAsync("해당 명령어는 최고관리자 전용이에요");
                return Task.FromResult(false);
            }

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
