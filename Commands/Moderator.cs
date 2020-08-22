using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordBot
{
    public class Moderator : BaseCommandModule
    {
        [Command("밴")]
        public async Task Ban(CommandContext ctx, DiscordUser mention, params string[] context)
        {
        	string content = string.Join(" ", context);
        	DiscordMember member = await ctx.Guild.GetMemberAsync(mention.Id);
        	await member.BanAsync(reason: content);
        	await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }
        
        [Command("킥"),RequireUserPermissions(Permissions.KickMembers)]
        public async Task Kick(CommandContext ctx, DiscordUser mention, params string[] context)
        {
        	string content = string.Join(" ", context);
        	DiscordMember member = await ctx.Guild.GetMemberAsync(mention.Id);
        	await member.RemoveAsync(reason: content);
        	await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

    }
}
