using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Exceptions;
using DiscordBot.Exceptions;
using DiscordBot.Commands;
using static DiscordBot.Variable;
using DSharpPlus;
using System.Linq;
using DiscordBot.Attributes;
using System.Runtime.InteropServices.ComTypes;

namespace DiscordBot
{
    partial class Index
    {
        private async Task Command_Errored(CommandErrorEventArgs e)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "Error",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now,
                Footer = GetFooter(e.Context)
            };

            CommandHelp help = new CommandHelp();

            if (e.Command != null)
            {
                if (e.Command.Description == "NeedManageMessages" && e.Exception is ChecksFailedException)
                    await e.Context.RespondAsync("라히는 다음 권한이 필요해요!\n" +
                        "```메시지 관리권한```");

                else if (e.Context.Message.Content == "라히야 큐브")
                    await help.CubeHelp(e.Context);

                else if (e.Context.Message.Content.StartsWith("라히야 암호"))
                {
                    if (e.Exception is EncryptionException)
                        await e.Context.RespondAsync("암호화 방식이 맞지 않습니다!");
                    else if (e.Exception is EncryptionArgumentException)
                    {
                        if (e.Exception.Message == "Encryption")
                            dmb.AddField("EncryptionArgumentException", "암호화 옵션이 잘못되었습니다\n" +
                                "```사용 가능한 옵션 : Base32, Base64```");
                        else if (e.Exception.Message == "Encoding")
                            dmb.AddField("EncryptionArgumentException", "인코딩 옵션이 잘못되었습니다\n" +
                                "```사용 가능한 옵션 : UTF7, UTF8, UTF32, Unicode, ASCII```");
                        else
                            dmb.AddField("EncryptionArgumentException", "알 수 없는 오류가 발생하였습니다!");

                        await e.Context.RespondAsync(embed: dmb.Build());
                    }
                    else
                        await help.EncryptionHelp(e.Context);
                }

                else if (e.Command.Name == "랜덤")
                    await help.RandomHelp(e.Context);

                else if (e.Command.Name == "변환")
                    await e.Context.RespondAsync("`라히야 변환 도움말` 을 참고해 주세요!");

                else if (e.Exception is ChecksFailedException err)
                {
                    if (err.FailedChecks.Contains(new DoNotUseAttribute()))
                        await e.Context.RespondAsync("해당 명령어는 실험중이거나 사용할 수 없는 명령어 입니다.");
                    else
                        return;
                }

                else if (e.Command.Name == "RunException")
                    await e.Context.RespondAsync(e.Exception.ToString());

                else
                {
                    await e.Context.Channel.SendMessageAsync(e.Exception.Message);
                    discord.DebugLogger.LogMessage(LogLevel.Error, e.Command.Name, e.Exception.ToString(), DateTime.Now);
                }
            }
            else if (e.Exception is EncryptionException)
            {
                dmb.AddField(e.Exception.InnerException.ToString(), "암호화 옵션을 확인해 주세요!");
            }

            else if (e.Context.Message.Content.StartsWith("라히야 큐브"))
                await help.CubeHelp(e.Context);

            else
            {
                if (UseSaying)
                {
                    Say say = new Say();
                    string content = e.Context.Message.Content.Split("라히야")[1].Trim();

                    if (string.IsNullOrEmpty(RemoveSpace(content)))
                    {
                        Variable v = new Variable();
                        v.CallName(e.Context.Message);
                    }
                    else
                        await say.Saying(e.Context, content);

                    //TODO: 같은 설명의 단어는 추가 건너뛰기
                }
                else
                {
                    dmb.Description = $"요청하신 명령어 `{e.Context.Message.Content}`(을)를 찾을수 없습니다!";

                    dmb.AddField("Error with CommandNotFoundException", "[라히야 추가 (기능과 설명)]으로 추가요청할 수 있습니다!");
                    await e.Context.RespondAsync(embed: dmb.Build());
                }
            }
        }
    }
}
