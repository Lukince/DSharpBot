using ByteSizeLib;
using DiscordBot.Attributes;
using DiscordBot.Configs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using IronPython.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NGit.Api;
using static DiscordBot.Account;
using static DiscordBot.Index;
using static DiscordBot.Utils;
using static DiscordBot.Variable;
using NGit.Storage.File;

namespace DiscordBot
{
    [BlackList, CheckAdmin]
    class Management
    {
        [Command("Info")]
        public async Task Info(CommandContext ctx, ulong id)
        {
            string Id = $"Account/{id}";

            if (File.Exists(Id))
            {
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
                {
                    Title = $"File. {id}",
                    Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
                };

                dmb.AddField("Name", ui.Name);
                dmb.AddField("Id", ui.UserId.ToString(), true);
                dmb.AddField("Regist DateTime", $"{ui.RegDate}");
                dmb.AddField("Cube Count", ui.Cube.ToString(), true);
                dmb.AddField("Money", ui.Money.ToString());
                dmb.AddField("Reward DateTime", $"{ui.RewardTime}", true);
                dmb.AddField("CubeTime", ui.CubeTimeHour.ToString());
                dmb.AddField("CubeFinishTime", $"{ui.OpenTime}", true);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                await ctx.Member.SendMessageAsync(embed: dmb.Build());
            }
            else
            {
                await ctx.RespondAsync("No user info. Check data or regist log");
            }
        }

        [Command("Send")]
        public async Task Send(CommandContext ctx, ulong channelid, params string[] content)
        {
            string message = string.Join(" ", content);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            await ctx.Client.GetChannelAsync(channelid).GetAwaiter().GetResult().SendMessageAsync(message);
        }

        [Command("Open")]
        public async Task Open(CommandContext ctx, ulong id)
        {
            string Id = $"Account/{id}";

            if (File.Exists(Id))
            {
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                if (ui.CubeTimeHour > 0 && ui.OpenTime.Subtract(DateTime.Now).TotalSeconds > 0)
                {
                    ui.OpenTime = DateTime.Now;

                    string sjson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                    File.WriteAllText(Id, sjson);

                    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                }
                else if (ui.CubeTimeHour > 0 && ui.OpenTime.Subtract(DateTime.Now).TotalSeconds <= 0)
                {
                    await ctx.RespondAsync("Cube is already open.");
                }
                else if (ui.CubeTimeHour == 0)
                {
                    await ctx.RespondAsync("No cube registed");
                }
                else
                {
                    await ctx.RespondAsync("An error occurred during processing.");
                }
            }
            else
            {
                await ctx.RespondAsync("No user info. Check data or regist log");
            }
        }

        [Command("CubeAdd")]
        public async Task CubeAdd(CommandContext ctx, ulong id, uint cubecount)
        {
            string Id = $"Account/{id}";

            if (File.Exists(Id))
            {
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                ui.Cube += cubecount;

                string sjson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                File.WriteAllText(Id, sjson);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.RespondAsync("No user info. Check data or regist log");
            }
        }

        [Command("MoneyAdd")]
        public async Task MoneyAdd(CommandContext ctx, ulong id, ulong moneycount)
        {
            string Id = $"Account/{id}";

            if (File.Exists(Id))
            {
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                ui.Money += moneycount;

                string sjson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                File.WriteAllText(Id, sjson);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.RespondAsync("No user info. Check data or regist log");
            }
        }

        [Command("RegistCube")]
        public async Task RegistCube(CommandContext ctx, ulong id, uint cubetime)
        {
            string Id = $"Account/{id}";

            if (File.Exists(Id))
            {
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                ui.CubeTimeHour = cubetime;
                ui.OpenTime = DateTime.Now.AddHours(cubetime);

                string sjson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                File.WriteAllText(Id, sjson);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.RespondAsync("No user info. Check data or regist log");
            }
        }

        [Command("ServerList")]
        public async Task ServerList(CommandContext ctx)
        {
            string msg = string.Empty;

            foreach (ulong serverid in ctx.Client.Guilds.Keys)
            {
                var guild = await ctx.Client.GetGuildAsync(serverid);
                msg += $"{serverid} : {guild.Name}\n";
            }

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
            {
                Title = "Server List",
                Color = DiscordColor.Blue
            };

            dmb.AddField("Server Id : Server Name", msg);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            await ctx.Member.SendMessageAsync(embed: dmb.Build());
        }

        [Command("SetPresence")]
        public async Task Presence(CommandContext ctx, string status)
        {
            changePresence = false;

            string message = string.Empty;

            string[] content = ctx.Message.Content.Split(' ');

            for (int i = 3; i < content.Length; i++)
            {
                message += $"{content[i]} ";
            }

            UserStatus us = UserStatus.Online;

            if (status == "Idle")
                us = UserStatus.Idle;

            else if (status == "dnd")
                us = UserStatus.DoNotDisturb;

            else if (status == "Offline")
                us = UserStatus.Offline;

            else if (status == "Invisible")
                us = UserStatus.Invisible;

            else
            {
                changePresence = true;
                message = $"성장중인 라히예요! | Ping : {ctx.Client.Ping}";
                await ctx.RespondAsync("Reset completed");
            }

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            await ctx.Client.UpdateStatusAsync(new DiscordGame(message), us);
        }

        [Command("UserCount")]
        public async Task UserCount(CommandContext ctx)
        {
            await ctx.RespondAsync($"{Directory.GetFiles(IdLocation).Length} Users");
        }

        [Command("GetRoles")]
        public async Task GetRoles(CommandContext ctx)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder();

            string Roles = string.Empty;

            foreach (DiscordRole dr in ctx.Guild.Roles)
            {
                if (dr.Name != "@everyone")
                    Roles += $"{dr.Name} / ";
            }

            dmb.Color = DiscordColor.Cyan;
            dmb.AddField("Roles List", Roles);

            Roles = string.Empty;

            foreach (DiscordRole dr in ctx.Guild.CurrentMember.Roles)
            {
                Roles += $"{dr.Name} / ";
            }

            dmb.AddField("Bot's Roles", Roles);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            await ctx.Member.SendMessageAsync(embed: dmb.Build());
        }

        [Command("ResetReward")]
        public async Task ResetReward(CommandContext ctx, ulong id = 378535260754935819)
        {
            string Id = $"Account/{id}";

            if (File.Exists(Id))
            {
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                ui.RewardTime = ui.RewardTime.AddDays(-1);

                string sjson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                File.WriteAllText(Id, sjson);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.RespondAsync("No user info. Check data or regist log");
            }
        }

        [Command("BlackList")]
        public async Task BList(CommandContext ctx, string option, ulong id = 0)
        {
            string BlacklistPath = "Data/Blacklist.txt";

            if (option == "Add")
            {
                File.AppendAllText(BlacklistPath, $"{id}\n");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }

            if (option == "Remove")
            {
                var ids = File.ReadAllLines(BlacklistPath).Where(l => l != id.ToString());

                File.WriteAllLines(BlacklistPath, ids);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }

            if (option == "list")
            {
                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
                {
                    Title = "Blacklist Id list"
                };

                string ids = string.Empty;

                ids = string.Join("\n", File.ReadAllLines(BlacklistPath));
                dmb.AddField("Id", ids);

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                await ctx.Member.SendMessageAsync(embed: dmb.Build());
            }
        }

        [Command("Restart"), DoNotUse]
        public async Task Restart(CommandContext ctx)
        {
            var msg = await ctx.RespondAsync("Restarting...");

            await ctx.Client.DisconnectAsync();
            await Task.Delay(2000);
            await ctx.Client.ReconnectAsync();
            await msg.ModifyAsync("Finish!");
        }

        [Command("Files")]
        public async Task GetFiles(CommandContext ctx)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
            {
                Title = "Project Name : Discord Bot",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
            };

            string Files = string.Empty;

            foreach (string path in Directory.GetDirectories("../../").Where(l => Path.GetDirectoryName(l) != "bin" || Path.GetDirectoryName(l) != "obj"))
            {
                Files += $"{DiscordEmoji.FromGuildEmote(ctx.Client, 715420547038838815)} {Path.GetFileName(path)}";
                foreach (string file in Directory.GetFiles(path))
                {
                    Files += $"└ {DiscordEmoji.FromGuildEmote(ctx.Client, 715420547005284372)} {Path.GetFileName(file)}";
                }
            }

            foreach (string path in Directory.GetFiles("../../"))
            {
                Files += $"{DiscordEmoji.FromGuildEmote(ctx.Client, 715420547005284372)} {Path.GetFileName(path)}";
            }

            Console.WriteLine(Files);

            dmb.AddField($"{DiscordEmoji.FromGuildEmote(ctx.Client, 715420547038838815)} FolderName : DiscordBot", $"```{Files}```");

            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("SendBug")]
        public async Task SendBug(CommandContext ctx, params string[] content)
        {
            //BugReport(ctx);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("Eval")]
        public async Task EvalCommand(CommandContext ctx, [RemainingText] string code)
        {
            var msg = ctx.Message;

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.");

            var cs = code.Substring(cs1, cs2 - cs1);

            msg = await ctx.RespondAsync("", embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Evaluating...")
                .Build()).ConfigureAwait(false);

            try
            {
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx);

                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity");
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                var result = await script.RunAsync(globals).ConfigureAwait(false);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Result", Description = result.ReturnValue.ToString(), Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
                else
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Successful", Description = "No result was returned.", Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Evaluation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color = new DiscordColor("#FF0000") }.Build()).ConfigureAwait(false);
            }

        }

        [Command("SendDM")]
        public async Task SendDM(CommandContext ctx, ulong id, params string[] content)
        {
            string text = string.Join(" ", content);

            id.Send(ctx, text);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("AttributeTest"), RequireNsfw]
        public async Task CheckNsfw(CommandContext ctx)
        {
            await ctx.RespondAsync("true");
        }

        private string GetVersion(int[] v)
        {
            return $"{v[0]}.{v[1]}.{v[2]}";
        }

        private void RecordHistory(string content, int n)
        {
            int[] version;

            if (!File.Exists(HistoryPath))
            {
                File.WriteAllText(HistoryPath, $"{DateTime.Now}|0.0.1|{content}\n");
                return;
            }
            else
            {
                version = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    string[] s = File.ReadLines(HistoryPath).ToArray();
                    string[] v = s[s.Length - 1].Split('|')[1].Split('.');
                    version[i] = int.Parse(v[i]);
                }

                switch (n)
                {
                    case 1:
                        version[2]++;
                        break;
                    case 2:
                        version[1]++;
                        break;
                    case 3:
                        version[0]++;
                        break;
                }

                File.AppendAllText(HistoryPath, $"{DateTime.Now}|{GetVersion(version)}|{content}\n");
                return;
            }
        }

        [Command("History")]
        public async Task Histroy(CommandContext ctx, int option, params string[] content)
        {
            string history = string.Join(" ", content);

            if (option == 1 || option == 2 || option == 3)
            {
                try
                {
                    RecordHistory(history, option);
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync("O_O! ERROR!\n" +
                        $"{e}");
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else
            {
                await ctx.RespondAsync($"Option is not correct\n" +
                    $"```1 = SmallVersion, 2. MiddleVersion, 3. ReleaseVersion```");
            }
        }

        [Command("Commands")]
        public async Task GetCommand(CommandContext ctx)
        {
            var command = ctx.Client.GetCommandsNext().RegisteredCommands;

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "Commands",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)],
                Timestamp = DateTime.Now,
                Footer = GetFooter(ctx),
                Description = $"Command Count : {command.Count}"
            };

            string commandlist = string.Empty;

            foreach (Command c in command.Values.ToArray())
            {
                commandlist += $"`{c.Name}` ";
            }

            dmb.AddField("Command List", commandlist);

            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("GetAll")]
        public async Task GetAllUsernGuilds(CommandContext ctx)
        {
            var guilds = ctx.Client.Guilds;

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "All Users and Guilds Count",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)],
                Timestamp = DateTime.Now,
                Footer = GetFooter(ctx),
                Description = $"Guild Count : {guilds.Count}"
            };

            DiscordGuild[] gs = guilds.Values.ToArray();

            for (int i = 0; i < gs.Length; i++)
            {
                var members = gs[i].Members;
                dmb.AddField(gs[i].Name, $"All : {members.Count}, Member : {members.Where(l => !l.IsBot).Count()}, Bot : {members.Where(l => l.IsBot).Count()}");
            }

            await ctx.Member.SendMessageAsync(embed: dmb.Build());
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("Sudo")]
        public async Task Sudo(CommandContext ctx, params string[] content)
        {
            await ctx.Client.GetCommandsNext().SudoAsync(ctx.User, ctx.Channel, string.Join(" ", content));
        }

        [Command("py"), DoNotUse]
        public async Task PyRunner(CommandContext ctx)
        {
            try
            {
                WebClient pyfile = new WebClient();
                pyfile.DownloadFile(ctx.Message.Attachments[0].Url, "run.py");

                ScriptEngine engine = Python.CreateEngine();
                //ScriptScope scope = engine.CreateScope();
                ScriptSource source = engine.CreateScriptSourceFromFile("run.py");
                dynamic result = source.Execute();

                await ctx.RespondAsync(result.function().ToString());
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(e.Message);
            }
        }

        private async Task Commit(CommandContext ctx, string commit)
        {
            string directory = "https://github.com/Lukince/DSharpBot.git";
            Git git = new Git(new FileRepository(directory));
            git.Add().AddFilepattern("*.*").Call();
            git.Commit().SetMessage(commit).Call();
            git.Push().SetRemote(commit).Call();
            
        }

        [Command("Backup")]
        public async Task Backup(CommandContext ctx, params string[] message)
        {
            DateTime starttime = DateTime.Now;
            string commit = string.Join(" ", message);
            var msg = await ctx.RespondAsync("Backup...");
            long size = 0;

            try
            {
                foreach (string path in Directory.GetFiles("../../../", "*.cs", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(path) == "Config.cs" || Path.GetFullPath(path).Contains("obj"))
                        continue;
                    size += File.ReadAllBytes(path).Length;
                    string[] p = Path.GetFullPath(path).Split('\\');
                    string location;
                    if (path.EndsWith("Index.cs"))
                    {
                        location = Path.GetFileName(path);
                    }
                    else
                    {
                        location = p[4] + "/" + Path.GetFileName(path);
                    }
                    File.Copy(path, "D:/Backup/DSharpBot/" + location, true);
                }

                ByteSize bytesize = ByteSize.FromBytes(size);
                string s = "Finish!" +
                    $" `{bytesize.KibiBytes:n0} KB / {DateTime.Now.Subtract(starttime).TotalMilliseconds}ms`";
                await msg.ModifyAsync(s);

                await Task.Delay(1000);

                starttime = DateTime.Now;
                await msg.ModifyAsync($"{s}\n\nGit Pusing...");

                try
                {
                    Commit(ctx, commit);
                }
                catch (Exception e)
                {
                    ctx.RespondAsync(e.Message);
                    return;
                }

                await msg.ModifyAsync($"{s}\n\nFinish! `{DateTime.Now.Subtract(starttime).TotalMilliseconds}ms`");

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await msg.ModifyAsync("Fail!");
            }
        }
    }
}