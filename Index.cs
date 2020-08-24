using DiscordBot.Commands;
using DiscordBot.Exceptions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
//using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DiscordBot.Variable;

namespace DiscordBot
{
    partial class Index
    {
        public DiscordClient discord { get; private set; }
        public CommandsNextExtension commands { get; set; }
        public InteractivityExtension interactivity { get; private set; }
        public static VoiceNextExtension voiceNext { get; private set; }
        public static LavalinkExtension lavalink { get; private set; }

        public static bool UseSaying = true;
        public static string StartDate;
        public static bool changePresence = true;
        public static DateTime StartTime = DateTime.Now;
        public static DiscordWebhook BugReport;
        public static Variable Variable = new Variable();

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
                BugReport = await discord.GetWebhookAsync(716214815634227252);
                DateTime date = DateTime.Now;
                StartTime = date;
                StartDate = $"{date.Year}_{date.Month}_{date.Day};{date.Hour}_{date.Minute}_{date.Second}";
                discord.DebugLogger.LogMessage(LogLevel.Info, discord.CurrentUser.Username, $"{discord.CurrentUser.Username} is ready!", DateTime.Now);
                File.WriteAllText($"DebugLog/{StartDate}.txt", $"Bot Start on : {date}");
                try { EnglishWord.ReloadWords(); }
                catch (Exception err) { discord.DebugLogger.LogMessage(LogLevel.Error, err.InnerException.ToString(), err.ToString(), DateTime.Now); }
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
                            await Variable.CallName(e.Message);
                        }
                    }
                    else if (e.Message.Content.StartsWith("라히야 관리자"))
                    {
                        DiscordUser Owner = e.Client.CurrentApplication.Owners.First();
                        await e.Channel.SendMessageAsync($"{Owner.Username}#{Owner.Discriminator}");
                    }
                }
            };

            discord.Heartbeated += async e =>
            {
                if (changePresence == true)
                    await discord.UpdateStatusAsync(activity: new DiscordActivity($"성장중인 라히예요! | Ping : {e.Ping}"));
            };

            discord.GuildCreated += async e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, e.Guild.Name, $"NewGuildDetected\n{e.Guild}", DateTime.Now);
                await Variable.SaveGuildInfo(e.Guild);
            };

            discord.GuildDeleted += async e =>
            {
                discord.DebugLogger.LogMessage(LogLevel.Info, e.Guild.Name, $"GuildDeleted\n{e.Guild}", DateTime.Now);
            };

            voiceNext = discord.UseVoiceNext(new VoiceNextConfiguration()
            {
                EnableIncoming = true
            });

            lavalink = discord.UseLavalink();

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "라히야" },
                EnableMentionPrefix = true,
                EnableDefaultHelp = false,
                IgnoreExtraArguments = false,
                EnableDms = false
            });

            interactivity = discord.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(60)
            });

            commands.CommandErrored += Command_Errored;

            commands.RegisterCommands<Say>();
            commands.RegisterCommands<Utils>();
            commands.RegisterCommands<Gaming>();
            commands.RegisterCommands<Account>();
            commands.RegisterCommands<Management>();
            commands.RegisterCommands<HelpCommand>();
            commands.RegisterCommands<VoiceCommand>();
            commands.RegisterCommands<Moderator>();
            commands.RegisterCommands<RainbowSix>();
            commands.RegisterCommands<EnglishWord>();
            commands.RegisterCommands<EvalCommand>();
            commands.RegisterCommands<TCPServer>();
            commands.RegisterCommands<Search>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
