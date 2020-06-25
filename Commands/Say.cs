using System;
using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public static class SayVariable
    {
        public static string[] Hello =
        {
            "안녀엉! :laughing:",
            "반가워어~ :wink:",
            "반가워 난 라히얌 ;)",
            ":wave:",
            "하이욤! :person_raising_hand:",
         "안뇽! :kissing_heart:"
        };
    }

    [BlackList]
    class Say
    {
        public async Task Saying(CommandContext ctx, string Content)
        {
            Random rnd = new Random();

            if (Content.Contains("안녕") || Content.Contains("안뇽")
             || Content.Contains("안녀엉") || Content.Contains("반가워")
             || Content.Contains("안뇨옹") || Content.Contains("헬로")
             || Content.Contains("핼로") || Content.Contains("하이"))
            {
                await ctx.RespondAsync(SayVariable.Hello[rnd.Next(0, SayVariable.Hello.Length - 1)]);
            }
        }
    }
}
