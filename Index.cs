using DiscordBot.Commands;
using DiscordBot.Exceptions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
//using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using static DiscordBot.Variable;

namespace DiscordBot
{
    class Index
    {
        public DiscordClient discord { get; private set; }
        public CommandsNextModule commands { get; set; }
        public InteractivityModule interactivity { get; private set; }
        public DiscordWebhook webhook { get; private set; }
        public static VoiceNextClient voiceNext;

        public static string StartDate;
        public static bool changePresence = true;
        public static DateTime StartTime = DateTime.Now;

        static void Main()
        {
            var prog = new Index();
            prog.MainAsync().GetAwaiter().GetResult();
        }

        async Task MainAsync()
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                LogLevel = LogLevel.Debug,
                AutoReconnect = true,
                UseInternalLogHandler = true
            });

            discord.Ready += async e =>
            {
                DateTime date = DateTime.Now;
                StartTime = date;
                StartDate = $"{date.Year}_{date.Month}_{date.Day};{date.Hour}_{date.Minute}_{date.Second}";
                discord.DebugLogger.LogMessage(LogLevel.Info, discord.CurrentUser.Username, $"{discord.CurrentUser.Username} is ready!", DateTime.Now);
                File.WriteAllText($"DebugLog/{StartDate}.txt", $"Bot Start on : {date}");
            };

            discord.MessageCreated += async e =>
            {
                if (!e.Author.IsBot)
                {
                    if (e.Message.Content.StartsWith("라히야"))
                    {
                        string log = $"\n{DateTime.Now} : {e.Guild.Name}에서 {e.Channel.Name}에 {e.Author.Username}님이 {e.Message.Content}를 사용하였습니다.";
                        discord.DebugLogger.LogMessage(LogLevel.Debug, $"{e.Author.Username}#{e.Author.Discriminator}", $"{e.Guild.Name}#{e.Channel.Name}:{e.Message.Content}", DateTime.Now);
                        File.AppendAllText($"DebugLog/{StartDate}.txt", log);

                        if (e.Message.Content.Trim() == "라히야")
                        {
                            await e.Message.RespondAsync("네? 저 부르셨나요?");
                        }
                    }
                    else if (e.Message.Content.StartsWith("라히야 관리자"))
                    {
                        DiscordUser Owner = e.Client.CurrentApplication.Owner;
                        await e.Channel.SendMessageAsync($"{Owner.Username}#{Owner.Discriminator}");
                    }
                }
            };

            discord.Heartbeated += async e =>
            {
                if (changePresence == true)
                    await discord.UpdateStatusAsync(new DiscordGame($"성장중인 라히예요! | Ping : {e.Ping}"));
            };

            discord.GuildCreated += async e =>
            {
                DiscordChannel chn = await discord.GetChannelAsync(716214655390842880);
                await chn.SendMessageAsync("New Guild : " + e.Guild.ToString());
            };

            discord.GuildDeleted += async e =>
            {
                DiscordChannel chn = await discord.GetChannelAsync(716214655390842880);
                await chn.SendMessageAsync("Out Guild : " + e.Guild.ToString());
            };

            voiceNext = discord.UseVoiceNext();

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "라히야",
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,
                IgnoreExtraArguments = false,
                EnableDms = false
            });

            interactivity = discord.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            voiceNext = discord.UseVoiceNext();

            commands.CommandErrored += Command_Errored;

            commands.RegisterCommands<Say>();
            commands.RegisterCommands<Utils>();
            commands.RegisterCommands<Gaming>();
            commands.RegisterCommands<Account>();
            commands.RegisterCommands<Management>();
            commands.RegisterCommands<HelpCommand>();
            commands.RegisterCommands<VoiceCommand>();
            commands.RegisterCommands<Moderator>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

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

                else if (e.Context.Message.Content.StartsWith("라히야 큐브"))
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

                else if (e.Exception is ChecksFailedException)
                    return;

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
            else
            {
                dmb.Description = $"요청하신 명령어 `{e.Context.Message.Content}`(을)를 찾을수 없습니다!";

                dmb.AddField("Error with CommandNotFoundException", "[라히야 추가 (기능과 설명)]으로 추가요청할 수 있습니다!");
                await e.Context.RespondAsync(embed: dmb.Build());
            }
        }
    }
}
