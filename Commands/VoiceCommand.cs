using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [BlackList, CheckAdmin, Group("음성")]
    class VoiceCommand : BaseCommandModule
    {
        /*
        [Command("Join")]
        public async Task VoiceJoin(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new InvalidOperationException("이미 해당 길드에 있습니다");

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("음성채널에 접속해 있지 않습니다!");

            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync($"`{vnc.Channel.Name}`에 접속 완료!");
        }

        [Command("Leave")]
        public async Task VoiceLeave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("해당 길드에 연결되어 있지 않습니다!");

            vnc.Disconnect();
            await ctx.RespondAsync($"{vnc.Channel.Name}`에서 나갔습니다!");
        }
        */
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc = await vnext.ConnectAsync(chn);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            vnc.Disconnect();
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":wave:"));
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string file)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            if (!File.Exists(file))
                throw new FileNotFoundException("File was not found.");

            await ctx.RespondAsync("👌");
            await vnc.SendSpeakingAsync(true); // send a speaking indicator

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

        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            if (vnc.IsPlaying)
            {
                vnc.Pause();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
            }
            else
                throw new InvalidOperationException("None of music playing now");
        }

        [Command("resume")]
        public async Task Resnume(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            if (vnc.IsPlaying)
            {
                await vnc.ResumeAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
            }
            else
                throw new InvalidOperationException("Already playing now");
        }

        [Command("skip")]
        public async Task Skip(CommandContext ctx, [RemainingText] string reason)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            if (vnc.IsPlaying)
            {
                var transmitstream = vnc.GetTransmitStream();
                //TODO: skip 시스템 만들기
            }
            else
                throw new InvalidOperationException("Nothing in playing");
        }

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            await ctx.RespondAsync(vnc.UdpPing.ToString());
        }
    }
}
