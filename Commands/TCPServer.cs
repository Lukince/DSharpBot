using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FUP;
using static FUP.CONSTANTS;
using static DiscordBot.Variable;
using DiscordBot.Configs;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace DiscordBot.Commands
{
    [Group("Server"), CheckAdmin]
    class TCPServer : BaseCommandModule
    {
        IPEndPoint ServerIp = null;
        TcpListener Server = null;
        CommandExtensions extensions = new CommandExtensions();

        [Command("Start")]
        public async Task ServerStart(CommandContext ctx, string ip = "192.168.200.179", int port = 5536)
        {
            uint msgId = 0;

            const string dir = "Music";

            try
            {
                ServerIp = new IPEndPoint(IPAddress.Parse(ip), port);
                Server = new TcpListener(ServerIp);
                Server.Start();

                await ctx.RespondAsync($"Server Starts\n```ip : {ip}, port : {port}```");

                while (true)
                {
                    TcpClient client = Server.AcceptTcpClient();
                    await ctx.RespondAsync($"New Client : {(IPEndPoint)client.Client.RemoteEndPoint}");

                    NetworkStream stream = client.GetStream();

                    Message reqMsg = MessageUtil.Receive(stream);

                    if (reqMsg.Header.MSGTYPE != REQ_FILE_SEND)
                    {
                        stream.Close();
                        client.Close();
                        continue;
                    }

                    BodyRequest reqBody = (BodyRequest)reqMsg.Body;


                    DiscordEmoji[] emojis = new DiscordEmoji[]
                    {
                        DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("Correct")),
                        DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("NotCorrect"))
                    };

                    var discordmsg = await ctx.RespondAsync("New Upload Request\n" +
                        $"```Client : {(IPEndPoint)client.Client.RemoteEndPoint}\n" +
                        $"FileName : {Encoding.UTF8.GetString(reqBody.FILENAME)}, FileSize : {reqBody.FILESIZE}```");
                    await extensions.CreateReactionsAsync(discordmsg, emojis);
                    var interactivty = ctx.Client.GetInteractivity();
                    var response = (await interactivty.WaitForReactionAsync(l =>
                        l.User == ctx.User && l.Channel == ctx.Channel && emojis.Contains(l.Emoji))).Result;

                    Message rspMsg = new Message();
                    rspMsg.Body = new BodyResponse()
                    {
                        MSGID = reqMsg.Header.MSGID,
                        RESPONSE = ACCEPTED
                    };

                    rspMsg.Header = new Header()
                    {
                        MSGID = msgId++,
                        MSGTYPE = REP_FILE_SEND,
                        BODYLEN = (uint)rspMsg.Body.GetSize(),
                        FRAGMENTED = NOT_FRAGMENTED,
                        LASTMSG = LASTMSG,
                        SEQ = 0
                    };

                    if (response != null)
                    {
                        if (response.Emoji == emojis[0])
                            MessageUtil.Send(stream, rspMsg);
                        else
                        {
                            rspMsg.Body = new BodyResponse()
                            {
                                MSGID = reqMsg.Header.MSGID,
                                RESPONSE = DENIED
                            };

                            MessageUtil.Send(stream, rspMsg);
                            stream.Close();
                            client.Close();

                            continue;
                        }
                    }

                    await ctx.RespondAsync("Start Recieve File");
                    var progmsg = await ctx.RespondAsync("Downloading...");

                    long fileSize = reqBody.FILESIZE;
                    string filename = Path.GetFileName(Encoding.UTF8.GetString(reqBody.FILENAME));
                    FileStream file =
                        new FileStream(Path.Combine(dir, filename), FileMode.Create);

                    uint? dataMsgId = null;
                    ushort prevSeq = 0;
                    while ((reqMsg = MessageUtil.Receive(stream)) != null)
                    {
                        Console.WriteLine(reqMsg.Header.FRAGMENTED);
                        if (reqMsg.Header.MSGTYPE != FILE_SEND_DATA)
                            break;

                        if (dataMsgId == null)
                            dataMsgId = reqMsg.Header.MSGID;
                        else
                        {
                            if (dataMsgId != reqMsg.Header.MSGID)
                                break;
                        }

                        if (prevSeq++ != reqMsg.Header.SEQ)
                        { await ctx.RespondAsync($"{prevSeq}, {reqMsg.Header.SEQ}"); break; }

                        file.Write(reqMsg.Body.GetBytes(), 0, reqMsg.Body.GetSize());

                        if (reqMsg.Header.LASTMSG == LASTMSG)
                        { Console.WriteLine("LastMessage"); break; }

                    }

                    long recvFileSize = file.Length;
                    file.Close();

                    await progmsg.ModifyAsync($"Received Size : {recvFileSize} bytes");

                    Message rstMsg = new Message
                    {
                        Body = new BodyResult()
                        {
                            MSGID = reqMsg.Header.MSGID,
                            RESULT = SUCCESS
                        }
                    };

                    rstMsg.Header = new Header()
                    {
                        MSGID = msgId++,
                        MSGTYPE = FILE_SEND_RES,
                        BODYLEN = (uint)rstMsg.Body.GetSize(),
                        FRAGMENTED = NOT_FRAGMENTED,
                        LASTMSG = LASTMSG,
                        SEQ = 0
                    };

                    if (fileSize == recvFileSize)
                        MessageUtil.Send(stream, rstMsg);
                    else
                    {
                        rstMsg.Body = new BodyResult()
                        {
                            MSGID = reqMsg.Header.MSGID,
                            RESULT = FAIL
                        };

                        MessageUtil.Send(stream, rstMsg);
                    }

                    await ctx.RespondAsync("Finished!");

                    stream.Close();
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Server.Stop();
            }

            await ctx.RespondAsync("Server Ends");
        }

        [Command("Stop")]
        public async Task ServerStop(CommandContext ctx)
        {
            if (Server == null)
            {
                await ctx.RespondAsync("Server is not running now");
                return;
            }

            Server.Stop();
        }
    }
}
