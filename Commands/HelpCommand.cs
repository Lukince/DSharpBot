using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Threading.Tasks;
using static DiscordBot.Utils;
using static DiscordBot.Variable;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace DiscordBot.Commands
{
    [BlackList]
    public static class HelpTexts
    {
        public static DiscordEmbedBuilder GetHelp(CommandContext ctx, int page, int rnd, bool test = false)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Color = RandomColor[rnd]
            };

            if (test == true)
            {
                EmbedFooter footer = new EmbedFooter()
                {
                    Text = "Tip! : 도움말 명령어는 봇의 리소스가 낭비되는 것을 위해 5분동안만 작동해요!"
                };
                dmb.Footer = footer;
            }
            else
            {
                dmb.Footer = GetFooter(ctx);
            }

            if (page == 1)
            {
                dmb.Title = $"도움말 1/{MaxPage}";
                dmb.Description = "계정";
                dmb.AddField("라히야 가입", "라히와 함께 할수 있어요! 여러가지 게임도 하고.. 정말 좋겠죠?");
                dmb.AddField("라히야 탈퇴", "헤어지는 시간은 늘 있는법이죠... 아픈 경험을 할것만 같은 명령어예요.");
            }
            else if (page == 2)
            {
                dmb.Title = $"도움말 2/{MaxPage}";
                dmb.Description = "게임";
                dmb.AddField("라히야 큐브", "라히는 큐브 시스템을 운영하고 있어요! 자세한건 `라히야 큐브`를 직접 입력해 보아요!");
                dmb.AddField("라히야 리워드", "매일 한번씩 큐브를 받을 수 있어요! 운만 좋다면 돈도 받을수 있고요!");
                dmb.AddField("라히야 주사위", "랜덤으로 주사위를 굴려드릴께요!");
            }
            else if (page == 3)
            {
                dmb.Title = $"도움말 3/{MaxPage}";
                dmb.Description = "기능 ( 1 / 2 )";
                dmb.AddField("라히야 안녕", "인사하시려고요? 여기서도 인사드릴게요! 반가워요! 라히예요!");
                dmb.AddField("라히야 초대", "저를 초대해주세요! 라히의 성장에 동참할 수 있는 기회를 열어주는거예요!");
                dmb.AddField("라히야 시간", "라히와 함께한 시간을 알수 있어요! 대신 먼저 가입을 해주세요!");
                dmb.AddField("라히야 추가", "원하시는 기능이 있으시면 말씀해 주세요!");
                dmb.AddField("라히야 핑", "라히의 핑을 알수 있어요. 190~240 사이라면 아주 잘 되고 있는거예요!");
                dmb.AddField("라히야 말해", "제가 대신 말해드릴께요! 얼마든지 말씀하세요!");
            }
            else if (page == 4)
            {
                dmb.Title = $"도움말 4/{MaxPage}";
                dmb.Description = "기능 ( 2 / 2 )";
                dmb.AddField("라히야 계산", "똑똑한 라히가 계산 해줄께요!");
                dmb.AddField("라히야 업타임", "제가 얼마나 온라인 상태였는지 알 수 있어요! 근데... 왜 궁금하세요?");
                dmb.AddField("라히야 랜덤 [최소] [최대]", "랜덤은 언제나 필요한 것이예요! 랜덤한 숫자를 생성해드릴게요!");
                dmb.AddField("라히야 숫자야구게임", "게임 한판 해보자고요! 룰을 모르면 [라히야 룰 숫자야구게임]을 이용해주세요!");
            }
            else
            {
                throw new ArgumentException("페이지가 맞지 않습니다.");
            }

            return dmb;
        }
    }

    class HelpCommand
    {
        [Command("?"), Hidden]
        public async Task Help3(CommandContext ctx)
        {
            await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, "라히야 도움말");
        }

        [Command("도움"), Hidden]
        public async Task Help2(CommandContext ctx)
        {
            await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, "라히야 도움말");
        }

        [Command("도움말")]
        public async Task Help(CommandContext ctx, int page = -714576)
        {
            bool nul = false;
            if (page < 0 && page != -714576)
            {
                await ctx.RespondAsync("페이지는 0 이상이여야 해요!");
                return;
            }
            else if (page > MaxPage)
            {
                await ctx.RespondAsync($"페이지는 {MaxPage} 이상일 수 없어요!");
                return;
            }

            if (page == -714576)
            {
                nul = true;
                page = 1;
            }

            DiscordEmbedBuilder dmb = HelpTexts.GetHelp(ctx, page, rnd.Next(0, RandomColor.Length - 1));
            if (nul)
            {
                EmbedFooter footer = new EmbedFooter()
                {
                    Text = "[라히야 도움말 (Page)]로 다른 도움말을 확인할 수 있어요!"
                };

                dmb.Footer = footer;
            }
            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("도움말TEST"), DoNotUse]
        public async Task THelp(CommandContext ctx)
        {
            int page = 0;

            var msg = ctx.RespondAsync("번호를 골라주세요!").GetAwaiter().GetResult();

            for (int i = 0; i < MaxPage; i++)
            {
                try
                {
                    await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, Numbers[i]));
                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync($"으악! 에러가 났어요!\n```{e.Message}```");
                    break;
                }
            }

            int random = rnd.Next(0, RandomColor.Length - 1);

            Console.WriteLine("Finish!");

            var interactivity = ctx.Client.GetInteractivityModule();

            try
            {
                var reactions = interactivity.WaitForReactionAsync(l =>
                {
                    for (int i = 0; i < MaxPage - 1; i++)
                    {
                        if (l == DiscordEmoji.FromName(ctx.Client, Numbers[i]))
                        {
                            page = i;
                            return true;
                        }
                    }
                    return false;
                }, ctx.User, TimeSpan.FromSeconds(30));

                if (reactions != null)
                {
                    DiscordEmbedBuilder dmb = HelpTexts.GetHelp(ctx, page, random, true);
                    await msg.ModifyAsync(embed: dmb.Build());
                }
                else
                {
                    await ctx.RespondAsync("다른 이모지에 답하면 안되요!");
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"으악! 에러가 났어요!\n```{e.Message}```");
            }
            /*
                        var reations = await interactivity.WaitForReactionAsync(l => l.Id == , ctx.User, TimeSpan.FromMinutes(5)); */

        }

        [Group("룰")]
        class Rules
        {
            [Command("숫자야구게임")]
            public async Task NBGHelp(CommandContext ctx, string option = null)
            {
                await ctx.RespondAsync("Help : 숫자야구게임\n" +
                    "```숫자야구게임은 랜덤으로 정해지는 숫자를 맞추는 게임이예요!\n" +
                    "먼저 3개의 숫자가 정해집니다. 예를들어 볼까요? 2 4 5 라고 정해졌다고 칩시다\n" +
                    "이제 사용자가 입력을 해줍니다 방식은 [a b c] 형태로요\n" +
                    "또다시 예를 들어볼까요? 사용자가 1 2 3 을 입력했어요!\n" +
                    "이에 대해서 같은 자리에 있고 숫자가 같으면 1 Strike!\n" +
                    "자리가 같지는 않지만 숫자가 같으면 1 Ball 이랍니다!\n" +
                    "이 정보들을 바탕으로 랜덤한 숫자가 무엇인지 찾으면 되는거랍니다!```");
            }
        }
    }

    public class CommandHelp
    {
        public async Task CubeHelp(CommandContext ctx)
        {
            DiscordEmbedBuilder CubeEmbed = new DiscordEmbedBuilder
            {
                Title = "큐브 시스템",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)],
                Timestamp = DateTime.Now,
                Footer = GetFooter(ctx)
            };
            CubeEmbed.AddField("라히야 큐브 갯수", "현재 가지고 있는 큐브 갯수를 알수 있어요! 큐브를 열려면 먼저 큐브가 몇개인지 알아야 겠죠?");
            CubeEmbed.AddField("라히야 큐브 오픈", "가지고 있는 큐브를 열어줘요! 랜덤으로 시간이 주어지고 시간이 길수록 더 큰 보상이 있겠죠?");
            CubeEmbed.AddField("라히야 큐브 확인", "오픈된 큐브를 확인할 수 있어요! 확인을 통해 회수를 해야 다음 큐브를 열수 있어요!");
            CubeEmbed.AddField("라히야 큐브 상점", "큐브를 구매하거나 포션을 살수 있어요!");

            await ctx.RespondAsync(embed: CubeEmbed.Build());
        }

        public async Task EncryptionHelp(CommandContext ctx)
        {
            await ctx.RespondAsync("사용법 : 라히야 암호 [암호화/해독] [암호화 옵션] [인코딩 옵션] [내용]\n" +
                "```암호화 옵션 : Base32, Base64\n" +
                "인코딩 옵션 : UTF7, UTF8, UTF32, Unicode, ASCII```");
        }

        public async Task RandomHelp(CommandContext ctx)
        {
            await ctx.RespondAsync("랜덤 함수 사용법 : 라히야 랜덤 [최소] [최대] [옵션 : 10]\n" +
                                         $"```최대, 최소의 값은 ±{int.MaxValue}를 넘을수 없어요!\n" +
                                         $"옵션 : 2(2진수), 8(8진수), 10(10진수) 16(16진수)```");
        }

        public async Task ConvertHelp(CommandContext ctx)
        {

        }
    }

}

