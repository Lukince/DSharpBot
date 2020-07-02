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
    partial class Index
    {
        public DiscordClient discord { get; private set; }
        public CommandsNextModule commands { get; set; }
        public InteractivityModule interactivity { get; private set; }
        public DiscordWebhook webhook { get; private set; }
        public static VoiceNextClient voiceNext;

        public static bool UseSaying = false;
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
    }
}
