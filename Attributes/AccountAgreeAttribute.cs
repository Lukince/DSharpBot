using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using static DiscordBot.Account;
using static DiscordBot.Variable;
using static DiscordBot.Utils;

namespace DiscordBot.Attributes
{
    class AccountAgreeAttribute : CheckBaseAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            string Id = $"{IdLocation}/{ctx.User.Id}";
            string OldId = $"ReloadAc/{ctx.User.Id}";

            if (!File.Exists(Id))
            {
                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
                {
                    Title = "사용자 정보 사용 동의",
                    Description = "봇에 가입함으로써 동의하는 사항\n" +
                    "1. 디스코드 계정 정보(아이디 및 유저이름, 길드 내에서의 기록)를 사용하게 됩니다.\n" +
                    "2. 라히봇을 통해서 사용하는 모든 명령어는 로그로 기록됩니다.\n" +
                    "3. 해킹시도나 여러 방법으로 봇에 피해를 주는경우 블랙리스트에 등록될수도 있습니다.\n" +
                    "*현재 테스트 중인 봇으로 정식 출시 이후나 테스트 중 계정이 삭제될수도 있음을 알립니다.\n" +
                    "또한 테스트 중에 사용해주신 분들에게는 추가로 보상이 지급될 예정입니다.",
                    Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)],
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://i.imgur.com/wjV4Ab1.png" }
                };

                if (File.Exists(OldId))
                {
                    dmb.Description += "\n현재 사용자의 계정은 보관이 된 상태이며 동의 이후 정상 진행됩니다.";
                }

                DiscordEmoji Correct = DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("Correct"));
                DiscordEmoji NotCorrect = DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("NotCorrect"));

                var msg = await ctx.RespondAsync(embed: dmb.Build());
                await msg.CreateReactionAsync(Correct);
                await msg.CreateReactionAsync(NotCorrect);
                await Task.Delay(1000);

                var interactivity = ctx.Client.GetInteractivity();
                var reactions = (await interactivity.WaitForReactionAsync(x => x.Emoji.Id == Correct.Id || x.Emoji.Id == NotCorrect.Id, ctx.User, TimeSpan.FromMinutes(5))).Result;

                if (reactions != null)
                {
                    if (reactions.Emoji == NotCorrect)
                    {
                        await msg.DeleteAsync();
                        await ctx.RespondAsync("나중에라도 다시 가입해주실꺼죠? 기다리고 있을게요!");
                        return await Task.FromResult(false);
                    }

                    else if (reactions.Emoji == Correct)
                    {
                        await msg.DeleteAsync();
                        return await Task.FromResult(true);
                    }

                    else
                        return await Task.FromResult(false);
                }
                else
                {
                    await msg.DeleteAsync();
                    await ctx.RespondAsync("이런! 5분이 지나도록 처리를 하지 못했어요.. 다시 시도해주세요!");
                    return await Task.FromResult(false);
                }
            }
            else
            {
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                await ctx.RespondAsync($"이미 가입을 했어요! 가입한 일시 : {ui.RegDate}");
                return await Task.FromResult(false);
            }
        }
    }
}
