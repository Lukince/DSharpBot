using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using static DiscordBot.Index;
using System.Collections;
using System.Linq;

namespace DiscordBot.Attributes
{
    class BlackListAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null)
                return Task.FromResult(false);

            if (ctx.Client.CurrentApplication.Owners.First() == ctx.User)
                return Task.FromResult(true);

            string BlacklistPath = "Data/Blacklist.txt";
            if (File.Exists(BlacklistPath))
            {
                string[] blacklist = File.ReadAllLines(BlacklistPath);
                foreach (string id in blacklist)
                {
                    if (ctx.User.Id.ToString() == id)
                    {
                        ctx.Client.DebugLogger.LogMessage(LogLevel.Warning, $"{ctx.User.Username}#{ctx.User.Discriminator}", $"BlackList User used command : {ctx.Message.Content}", DateTime.Now);
                        ctx.RespondAsync("블랙리스트에 등록되어 있어요! 관리자에게 문의하세요!");
                        throw new ChecksFailedException(ctx.Command, ctx, Enumerable.Empty<CheckBaseAttribute>());
                    }
                }
            }
            else
            {
                File.Create(BlacklistPath);
            }

            return Task.FromResult(true);
        }
    }
}
