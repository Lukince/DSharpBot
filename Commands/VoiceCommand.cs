using DiscordBot.Attributes;
using DiscordBot.Configs;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;
using static DiscordBot.Variable;

namespace DiscordBot.Commands
{
    [BlackList, CheckAdmin, Group("Voice")]
    class VoiceCommand : BaseCommandModule
    {
        public int CurrentPlayQueue = -1;
        public Dictionary<DiscordGuild, CancellationToken[]> Queue = new Dictionary<DiscordGuild, CancellationToken[]>();

        [Command("Join")]
        public async Task Join(CommandContext ctx, DiscordChannel channel = null)
        {
            var vnext = ctx.Client.GetVoiceNext();

            if (ctx.Member.VoiceState?.Channel == null)
                throw new InvalidOperationException("You're not in any voice channel");

            await vnext.ConnectAsync(channel ?? ctx.Member.VoiceState.Channel);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
        }

        [Command("Leave")]
        public async Task Leave(CommandContext ctx, DiscordChannel channel = null)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var conn = vnext.GetConnection(ctx.Guild);

            if (conn == null)
                throw new InvalidOperationException("No connection in this guild");

            conn.Disconnect();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":wave:"));
        }

        [Command("Play")]
        public async Task Play(CommandContext ctx, [RemainingText] string file)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            if (!File.Exists(file))
                throw new FileNotFoundException("File was not found.");

            await ctx.RespondAsync("👌");
            await vnc.SendSpeakingAsync(true);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{file}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            var ffout = ffmpeg.StandardOutput.BaseStream;

            var txStream = vnc.GetTransmitStream();
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync();

            await vnc.WaitForPlaybackFinishAsync();
        }

        [Command("Volume")]
        public async Task Volume(CommandContext ctx, double v)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            if (v < 0 || v > 100)
                throw new ArgumentOutOfRangeException("Volume is must be in range. [0 ~ 100]");

            vnc.GetTransmitStream().VolumeModifier = v / 100;
            await ctx.RespondAsync($"Now Volume : {v}");
        }

        [Command("Pause")]
        public async Task Pause(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            vnc.Pause();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
        }

        [Command("Resume")]
        public async Task Resume(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            await vnc.ResumeAsync();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
        }

        private WebClient web { get; } = new WebClient();

        [Command("DownloadFile")]
        public async Task DownloadFile(CommandContext ctx, string filename = null)
        {
            var file = ctx.Message.Attachments.First();
            string path = $"Music/{filename ?? file.FileName}";
            if (File.Exists(path))
            {
                await ctx.RespondAsync("File Already Exitst. Set other name");
                return;
            }

            web.DownloadFile(file.Url, path);
        }

        [Command("DownloadUrl")]
        public async Task DownloadUrl(CommandContext ctx, string url, string filename)
        {
            string path = $"Music/{filename}";
            if (File.Exists(path))
            {
                await ctx.RespondAsync("File Already Exitst. Set other name");
                return;
            }

            web.DownloadFile(url, path);
        }

        [Command("DownloadYoutube")]
        public async Task DownloadYoutube(CommandContext ctx, [RemainingText] string uri)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri _))
                throw new ArgumentException("Not Url");

            YouTube you = YouTube.Default;
            Video video = await you.GetVideoAsync(uri);
            string videopath = $"Youtube/{video.FullName}";
            var msg = await ctx.RespondAsync("Wait for downloading...");
            if (!File.Exists(videopath))
                await File.WriteAllBytesAsync(videopath, await video.GetBytesAsync());
            await msg.ModifyAsync($"Download Finish : {video.Title}");
        }

        [Command("Youtube")]
        public async Task Youtube(CommandContext ctx, [RemainingText] string search)
        {
            string url = null;
            if (Uri.TryCreate(search, UriKind.Absolute, out Uri _))
                url = search;
            else
            {
                var youtube = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = Config.GoogleToken,
                    ApplicationName = "Youtube Search"
                });

                var request = youtube.Search.List("snippet");
                request.Q = search;
                request.MaxResults = 5;

                var result = await request.ExecuteAsync();

                string resultstring = string.Empty;
                List<Tuple<string, string>> searchresult = new List<Tuple<string, string>>();

                foreach (var r in result.Items)
                    if (r.Id.Kind == "youtube#video")
                    {
                        resultstring +=
                            $"{Formatter.MaskedUrl(r.Snippet.Title, new Uri("http://youtube.com/watch?v=" + r.Id.VideoId))}\n";
                        searchresult.Add(new Tuple<string, string>(r.Snippet.Title, r.Id.VideoId));
                    }

                string[] s = { "p1", "p2", "p3", "p4", "p5" };

                DiscordEmbedBuilder SelectNumberEmbed = new DiscordEmbedBuilder()
                {
                    Title = $"Search Result : {search}",
                    Footer = GetFooter(ctx),
                    Color = DiscordColor.Red
                }.WithDescription(resultstring);
                await ctx.RespondAsync(embed: SelectNumberEmbed);
                await ctx.RespondAsync("Select Number ex) p3");

                var interactivity = ctx.Client.GetInteractivity();
                var answer = (await interactivity.WaitForMessageAsync(l =>
                l.Author == ctx.User && l.Channel == ctx.Channel && s.Contains(l.Content))).Result;

                if (answer != null)
                {
                    var ids = searchresult.ToArray();
                    var n = int.Parse(answer.Content.Substring(1)) - 1;

                    url = $"http://youtube.com/watch?v={ids[n].Item2}";
                }
            }

            YouTube you = YouTube.Default;
            Video video = await you.GetVideoAsync(url);
            string videopath = $"Youtube/{video.FullName}";
            var msg = await ctx.RespondAsync("Wait for downloading...");
            if (!File.Exists(videopath))
                await File.WriteAllBytesAsync(videopath, await video.GetBytesAsync());
            await msg.ModifyAsync($"Play {video.Title}");
            await this.Play(ctx, videopath);
        }

        [Command("Search")]
        public async Task Search(CommandContext ctx, [RemainingText] string search)
        {
            string url = null;
            if (Uri.TryCreate(search, UriKind.Absolute, out Uri _))
                url = search;
            else
            {
                var youtube = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = Config.GoogleToken,
                    ApplicationName = "Youtube Search"
                });

                var request = youtube.Search.List("snippet");
                request.Q = search;
                request.MaxResults = 5;

                var result = await request.ExecuteAsync();

                string resultstring = string.Empty;
                List<Tuple<string, string>> searchresult = new List<Tuple<string, string>>();

                foreach (var r in result.Items)
                    if (r.Id.Kind == "youtube#video")
                    {
                        resultstring +=
                            $"{Formatter.MaskedUrl(r.Snippet.Title, new Uri("http://youtube.com/watch?v=" + r.Id.VideoId))}\n";
                        searchresult.Add(new Tuple<string, string>(r.Snippet.Title, r.Id.VideoId));
                    }

                string[] s = { "p1", "p2", "p3", "p4", "p5" };

                DiscordEmbedBuilder SelectNumberEmbed = new DiscordEmbedBuilder()
                {
                    Title = $"Search Result : {search}",
                    Footer = GetFooter(ctx),
                    Color = DiscordColor.Red
                }.WithDescription(resultstring);
                await ctx.RespondAsync(embed: SelectNumberEmbed);
                await ctx.RespondAsync("Select Number ex) p3");

                var interactivity = ctx.Client.GetInteractivity();
                var answer = (await interactivity.WaitForMessageAsync(l =>
                l.Author == ctx.User && l.Channel == ctx.Channel && s.Contains(l.Content))).Result;

                if (answer != null)
                {
                    var ids = searchresult.ToArray();
                    var n = int.Parse(answer.Content.Substring(1)) - 1;

                    await ctx.RespondAsync($"http://youtube.com/watch?v={ids[n].Item2}");
                }
            }
        }

        //[Command("Skip")]
        public async Task Skip(CommandContext ctx)
        {
            //if (CurrentPlayQueue == -1)
            //{
            //    await ctx.RespondAsync("Not Playing now");
            //    return;
            //}

            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            var stream = vnc.GetTransmitStream();

            //Queue[ctx.Guild].ToArray()[CurrentPlayQueue].ThrowIfCancellationRequested();
            //await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
        }
    }
}
