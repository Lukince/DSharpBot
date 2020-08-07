using ByteSizeLib;
using DecimalConverter;
using DiscordBot.Attributes;
using DiscordBot.Configs;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using IronPython.Hosting;
using IronPython.Runtime.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static DiscordBot.Account;
using static DiscordBot.Index;
using static DiscordBot.Utils;
using static DiscordBot.Variable;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace DiscordBot
{
    [BlackList, CheckAdmin]
    class Management
    {
        [Command("Info"), Check]
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

        [Command("Open"), Check]
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

        [Command("CubeAdd"), Check]
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

        [Command("MoneyAdd"), Check]
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

        [Command("RegistCube"), Check]
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

        [Command("ServerList"), Check]
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

        [Command("GetRoles"), Check]
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

        [Command("ResetReward"), Check]
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

        [Command("BlackList"), Check]
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

        [Command("Files"), Check]
        public async Task GetFiles(CommandContext ctx)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
            {
                Title = "Project Name : Discord Bot",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
            };

            string Files = string.Empty;
            foreach (string filepath in Directory.GetFiles("../../../", "*.*", SearchOption.TopDirectoryOnly))
                Files += $"{DiscordEmoji.FromGuildEmote(ctx.Client, 715420547005284372)} {Path.GetFileName(filepath)}\n";

            dmb.AddField($"{DiscordEmoji.FromGuildEmote(ctx.Client, 715420547038838815)} FolderName : DiscordBot", Files);

            foreach (string directorypath in Directory.GetDirectories("../../../"))
            {
                if (Path.GetFullPath(directorypath).Contains("bin") || Path.GetFullPath(directorypath).Contains("obj"))
                    continue;

                string s = string.Empty;

                foreach (string filepath in Directory.GetFiles(directorypath).Where(l => l.EndsWith(".cs")))
                    s += $"{DiscordEmoji.FromGuildEmote(ctx.Client, 715420547005284372)} {Path.GetFileName(filepath)}\n";

                dmb.AddField($"{DiscordEmoji.FromGuildEmote(ctx.Client, 715420547038838815)} {Path.GetFullPath(directorypath).Split('\\')[5]}\n", s);
            }

            await ctx.RespondAsync(embed: dmb.Build());
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

            var cs = code[cs1..cs2];

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

            await SendDM(ctx, id, text);
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
                    string[] v = s[^1].Split('|')[1].Split('.');
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

        [Command("Commands"), Check]
        public async Task GetCommand(CommandContext ctx, params string[] option)
        {
            CheckBaseAttribute checkadmin = new CheckAdminAttribute();
            CheckBaseAttribute checkdonotuse = new DoNotUseAttribute();
            var command = ctx.CommandsNext.RegisteredCommands;
            int commandcount = 0;
            int groupcount = 0;
            int groupchildren = 0;

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "Commands",
                Color = GetRandomColor(),
                Timestamp = DateTime.Now,
                Footer = GetFooter(ctx)
            };

            string commandlist = string.Empty;
            string grouplist = string.Empty;

            foreach (Command c in command.Values.ToArray()
                .Where(l => {
                    if (l.IsHidden)
                        return false;
                    else if (l.ExecutionChecks.Contains(checkadmin))
                        return false;
                    else if (l.ExecutionChecks.Contains(checkdonotuse))
                        return false;
                    else if (l is CommandGroup)
                    {
                        groupcount++;
                        CommandGroup cg = l as CommandGroup;
                        groupchildren += cg.Children.Count();
                        grouplist += $"`{cg.Name}` ";
                        return false;
                    }
                    else
                        return true;
                }))
            {
                commandlist += $"`{c.Name}` ";
                commandcount++;
            }

            dmb.AddField("Command List", commandlist);

            foreach (string o in option.Where(l => l.StartsWith("--")))
            {
                string op = o.ToLower();
                if (op == "--hidden")
                {
                    commandlist = string.Empty;
                    foreach (Command c in command.Values.ToArray()
                        .Where(l => l.IsHidden))
                    {
                        commandlist += $"`{c.Name}` ";
                        commandcount++;
                    }

                    if (string.IsNullOrEmpty(commandlist))
                        continue;

                    dmb.AddField("Hidden Commands", commandlist);
                }
                else if (op == "--admin")
                {
                    commandlist = string.Empty;
                    foreach (Command c in command.Values.ToArray()
                        .Where(l => l.ExecutionChecks.Contains(checkadmin)))
                    {
                        if (c is CommandGroup)
                        {
                            CommandGroup cg = c as CommandGroup;
                            groupcount++;
                            groupchildren += cg.Children.Count();
                            grouplist += $"`{cg.Name}` ";
                        }
                        else
                        {
                            commandlist += $"`{c.Name}` ";
                            commandcount++;
                        }
                    }

                    if (string.IsNullOrEmpty(commandlist))
                        continue;

                    dmb.AddField("Admin Commands", commandlist);
                }
                else if (op == "--notuse")
                {
                    commandlist = string.Empty;
                    foreach (Command c in command.Values.ToArray()
                        .Where(l => l.ExecutionChecks.Contains(checkdonotuse)))
                    {
                        commandlist += $"`{c.Name}` ";
                        commandcount++;
                    }

                    if (string.IsNullOrEmpty(commandlist))
                        continue;

                    dmb.AddField("NotUse Commands", commandlist);
                }
                else if (op == "--help")
                {
                    await ctx.RespondAsync("Options : Admin, NotUse, Hidden");
                    return;
                }
                else if (op == "--all")
                {
                    await Sudo(ctx, "라히야 Commands --admin --hidden --notuse");
                    return;
                }
            }

            dmb.AddField("Group List", grouplist);

            dmb.Description = $"GroupCount : {groupcount}, Command Count : {commandcount} + {groupchildren}";
            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("GetAll"), Check]
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
                var bot = await gs[i].GetMemberAsync(ctx.Client.CurrentUser.Id);
                var members = gs[i].Members;
                dmb.AddField(gs[i].Name, $"All : {members.Count}, Member : {members.Where(l => !l.IsBot).Count()}, Bot : {members.Where(l => l.IsBot).Count()} - 봇 이름: \"{bot.DisplayName}\"");
            }

            await ctx.Member.SendMessageAsync(embed: dmb.Build());
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("Sudo")]
        public async Task Sudo(CommandContext ctx, params string[] content)
        {
            await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, string.Join(" ", content));
        }

        [Command("BotSudo"), DoNotUse]
        public async Task BotSudo(CommandContext ctx, params string[] content)
        {
            await ctx.CommandsNext.SudoAsync(ctx.Client.CurrentUser, ctx.Channel, string.Join(" ", content));
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

        [Command("Backup")]
        public async Task Backup(CommandContext ctx)
        {
            DateTime starttime = DateTime.Now;
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
                    if (path.EndsWith("Index.cs") || path.EndsWith("CommandError.cs"))
                    {
                        location = Path.GetFileName(path);
                    }
                    else
                    {
                        if (!Directory.Exists("D:/Backup/DSharpBot/" + p[5]))
                            Directory.CreateDirectory("D:/Backup/DSharpBot/" + p[5]);

                        location = p[5] + "/" + Path.GetFileName(path);
                    }

                    File.Copy(path, "D:/Backup/DSharpBot/" + location, true);
                }

                ByteSize bytesize = ByteSize.FromBytes(size);
                string s = "Backup : Finish!" +
                    $" `{bytesize.KibiBytes:n0} KB / {Math.Round(DateTime.Now.Subtract(starttime).TotalSeconds, 2)} sec`";
                await msg.ModifyAsync(s);

                await Task.Delay(1000);

                starttime = DateTime.Now;
                await msg.ModifyAsync($"{s}\n\nGit Pusing...");

                try
                {
                    Process.Start("Backup.bat").WaitForExit();
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync(e.Message);
                    return;
                }

                await msg.ModifyAsync($"{s}\n\nGit Push : Finish! `{Math.Round(DateTime.Now.Subtract(starttime).TotalSeconds, 2)} sec`");

                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await msg.ModifyAsync("Fail!");
            }
        }

        [Command("#GitTool")]
        public async Task GitTool(CommandContext ctx)
        {
            await ctx.RespondAsync("Now in Git Mode");

            while (true)
            {
                try
                {
                    var interactivity = ctx.Client.GetInteractivityModule();
                    var reactions = await interactivity.WaitForMessageAsync(l => l.Author == ctx.User, TimeSpan.FromMinutes(5));

                    if (reactions != null)
                    {
                        if (reactions.Message.Content == "#Exit")
                        {
                            await ctx.RespondAsync("Now in Normal Mode");
                            return;
                        }

                        var proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "git",
                                Arguments = $"{reactions.Message.Content}",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                        };

                        proc.Start();

                        string Output = string.Empty;
                        string Error = string.Empty;

                        while (!proc.StandardOutput.EndOfStream)
                        {
                            Output += proc.StandardOutput.ReadLine();
                        }

                        /*
                        while (!proc.StandardError.EndOfStream)
                        {
                            Error += proc.StandardError.ReadLine();
                        }*/

                        if (Output == string.Empty)
                        {
                            await ctx.RespondAsync("Succeed");
                            continue;
                        }

                        if (Output.Length < 1024)
                        {
                            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                            {
                                Title = "Output",
                                Color = DiscordColor.Cyan,
                                Timestamp = DateTime.Now,
                                Footer = GetFooter(ctx)
                            };

                            dmb.AddField($"> {reactions.Message.Content}", Output);

                            await ctx.RespondAsync(embed: dmb.Build());
                        }
                        else
                        {
                            File.WriteAllText("Output.txt", Output);
                            await ctx.RespondWithFileAsync(content: "Output File:", file_path: "Output.txt");
                            File.Delete("Output.txt");
                        }
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                        {
                            Title = "Error!",
                            Color = DiscordColor.Red,
                            Timestamp = DateTime.Now,
                            Footer = GetFooter(ctx)
                        };

                        dmb.AddField("Error on", e.ToString());
                    }
                    catch (Exception err)
                    {
                        await ctx.RespondAsync(err.ToString());
                    }
                }
            }
        }

        [Command("Shutdown")]
        public async Task Shutdown(CommandContext ctx)
        {
            await ctx.RespondAsync("Shutdown Program");
            Environment.Exit(0);
        }

        [Command("Restart")]
        public async Task Reboot(CommandContext ctx)
        {
            await ctx.RespondAsync("Rebooting");
            Process.Start("DiscordBot.exe");
            Environment.Exit(0);
        }

        [Group("Admin"), CheckAdmin]
        class Admin
        {
            private readonly string AdminId = "Data/AdminId.txt";

            [Command("Add")]
            public async Task AddAdmin(CommandContext ctx, ulong id)
            {
                if (File.ReadAllLines(AdminId).Contains(id.ToString()))
                {
                    await ctx.RespondAsync("Already id exist");
                    return;
                }

                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                {
                    Title = "관리자 승인",
                    Description = "관리자로 부터 관리자 권한에 대한 승인 요청이 왔습니다.",
                    Footer = new EmbedFooter()
                    {
                        IconUrl = ctx.Client.CurrentApplication.Owner.AvatarUrl,
                        Text = $"Request by {ctx.Client.CurrentApplication.Owner.Username}#{ctx.Client.CurrentApplication.Owner.Discriminator}"
                    },
                    Timestamp = DateTime.Now
                };

                dmb.AddField("관리자 방침", "1. 관리자는 라히봇을 관리하며 내부 데이터 - 큐브시스템, 서버리스트 - 에 대해서 접근할 수 있는 권한이 있다.\n" +
                    "2. 관리자는 블랙리스트, 버그나 오류로 인한 데이터 수정등을 처리하는 일을 담당한다.\n" +
                    "3. 관리자의 권력행사나 이유 없는 수정등의 행동으로 인해 관리자 권한이 박탈 당할 수 있다.\n" +
                    "4. 관리자는 어드민 명령어중 일부 명령어만 사용할 수 있다. [AdminCommands]로 확인할 수 있다.\n" +
                    "위 사항에 동의하고 방침을 지키겠다면 아래의 체크 버튼을 눌러서 관리자가 될 수 있다.");

                DiscordEmoji Correct = DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("Correct"));
                DiscordEmoji NotCorrect = DiscordEmoji.FromGuildEmote(ctx.Client, GetEmoji("NotCorrect"));

                CommandExtensions extensions = new CommandExtensions();

                var msg = await extensions.SendDM(ctx, id, embed: dmb.Build());
                await msg.CreateReactionAsync(Correct);
                await msg.CreateReactionAsync(NotCorrect);
                await Task.Delay(1000);

                var interactivity = ctx.Client.GetInteractivityModule();
                var reaction = await interactivity.WaitForReactionAsync(x => x.Id == Correct.Id || x.Id == NotCorrect.Id, ctx.User);

                if (reaction != null)
                {
                    if (reaction.Emoji == NotCorrect)
                    {
                        await extensions.SendDM(ctx, id, "확인했어요. 싫으시다면 제가 강요할 필요는 없는거니깐요.");
                        return;
                    }
                }

                await extensions.SendDM(ctx, id, "새 관리자가 되신걸 축하해요! 앞으로 잘 부탁한다구요! :wink:");
                File.AppendAllText(AdminId, $"{id}\n");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }

            [Command("Remove")]
            public async Task RemoveAdmin(CommandContext ctx, ulong id)
            {
                if (!File.ReadAllLines(AdminId).Contains(id.ToString()))
                {
                    await ctx.RespondAsync($"There's no id : {id}");
                    return;
                }

                string[] content = File.ReadAllLines(AdminId).Where(l => Convert.ToUInt64(l) == id).ToArray();
                File.WriteAllLines(AdminId, content);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
        }

        [Flags]
        public enum Attributes
        {
            None = 0,
            CheckAdminAttribute = 1,
            DoNotUseAttribute = 2,
            AccountAgreeAttribute = 4,
            AccountCheckAttribute = 8,
            BlackListAttribute = 16
        }

        [Command("GetFlags")]
        public async Task GetFlags(CommandContext ctx, int flag)
        {
            await ctx.RespondAsync($"{(Attributes)flag}");
        }

        [Command("RunSay")]
        public async Task Run_Saying(CommandContext ctx, params string[] Content)
        {
            Commands.Say say = new Commands.Say();
            await say.Saying(ctx, string.Join(" ", Content));
        }

        [Command("RunException")]
        public async Task RunException(CommandContext ctx, params string[] Message)
        {
            throw new Exception(string.Join(" ", Message));
        }

        [Group("Memo")]
        [BlackList, CheckAdmin, Check]
        class Memo
        {
            readonly string memofile = "Data/Memo.txt";

            [Command("Add")]
            public async Task MemoAdd(CommandContext ctx, string key, params string[] content)
            {
                string value = string.Join(" ", content);
                if (!File.Exists(memofile))
                    File.Create(memofile);

                File.AppendAllText(memofile, $"{key}:{value}");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }

            [Command("Remove")]
            public async Task MemoRemove(CommandContext ctx, string key)
            {
                if (!File.Exists(memofile))
                {
                    throw new FileNotFoundException("No file found. Please use [Memo Add] to make file");
                }

                string[] content = File.ReadAllLines(memofile).Where(l => l.Split(':')[0] != key).ToArray();
                File.WriteAllLines(memofile, content);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }

            [Command("Find")]
            public async Task MemoFind(CommandContext ctx, string key)
            {
                foreach (string content in File.ReadAllLines(memofile))
                {
                    string[] ckey = content.Split(':');
                    if (ckey[0] == key)
                    {
                        await ctx.RespondAsync($"{key} = {ckey[1]}");
                        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                        return;
                    }
                }

                await ctx.RespondAsync("No key found");
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            }

            [Command("All")]
            public async Task MemoAll(CommandContext ctx)
            {
                string[] content = File.ReadAllLines(memofile);
                string tmp = string.Empty;

                foreach (string message in content)
                {
                    if ((tmp + message).Length > 2000)
                    {
                        await ctx.RespondAsync(tmp);
                        tmp = string.Empty;
                    }

                    tmp += message.Replace(':', '=') + Environment.NewLine;
                }

                await ctx.RespondAsync(tmp);
            }
        }

        [Command("Mode")]
        public async Task Mode(CommandContext ctx, string option)
        {
            if (option.ToLower() == "chat")
                UseSaying = true;
            else if (option.ToLower() == "error")
                UseSaying = false;
            else
            {
                string mode;

                if (UseSaying)
                    mode = "Chat";
                else
                    mode = "Error";
                throw new ArgumentException($"Current Mode : {mode}\n```사용 가능한 인수 : Chat, Error```");
            }

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("GetTextTest")]
        public async Task GetTextTest(CommandContext ctx, params string[] _)
        {
            await ctx.RespondAsync(ctx.Message.Content.Remove(0, 4));
        }

        [Command("AddWord"), Check]
        public async Task AddWords(CommandContext ctx, string word, params string[] content)
        {
            string s;
            string w;
            if (word.EndsWith("//Lock//"))
            {
                w = word.DeleteString("//Lock//");
                s = "Lock";
            }
            else
            {
                w = word;
                s = "Unlock";
            }
            File.AppendAllText(WordPath, $"{w}|{string.Join(' ', content)}|Admin|{s}\n");
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        private async Task SendEditMessage(DiscordChannel chn, string s, int time, string edited)
        {
            var msg = await chn.SendMessageAsync(s);
            await Task.Delay(1000 * time);
            await msg.ModifyAsync(edited);
        }

        private async Task SendDeleteMessage(DiscordChannel chn, string s, int time)
        {
            var msg = await chn.SendMessageAsync(s);
            await Task.Delay(1000 * time);
            await msg.DeleteAsync();
        }

        [Command("#ConnectChannel")]
        public async Task ChatMode(CommandContext ctx, ulong id, bool createchannel = false)
        {
            var chn = await ctx.Client.GetChannelAsync(id);
            var mchn = ctx.Channel;
            string schn = chn.Name;

            if (createchannel)
            {
                mchn = await ctx.Guild.CreateChannelAsync(chn.Name, ChannelType.Text);
                schn = mchn.Mention;
            }

            var interactivity = ctx.Client.GetInteractivityModule();

            var tmpmsg = await ctx.RespondAsync("This is Chatmode. All of your message will be send." +
                                    $"Connected Channel : {schn}");

            while (true)
            {

                var msg = await interactivity.WaitForMessageAsync(l => l.Author == ctx.User
                                                    && l.Channel == mchn);

                if (msg != null)
                {
                    if (msg.Message.Content == "#Disconnect")
                        break;
                    else
                    {
                        if (msg.Message.Content.StartsWith("//Channel//"))
                        {
                            string s = msg.Message.Content.Split("//Channel//")[1];
                            chn = await ctx.Client.GetChannelAsync(ulong.Parse(s));
                            await mchn.ModifyAsync(chn.Name);
                        }
                        else if (msg.Message.Content.StartsWith("//Sudo//"))
                        {
                            try
                            {
                                string s = msg.Message.Content.Split("//Sudo//")[1];
                                await ctx.CommandsNext.SudoAsync(ctx.User, chn, s);
                                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                            }

                            catch (Exception e)
                            {
                                await chn.SendMessageAsync(e.Message);
                            }
                        }
                        else if (msg.Message.Content.Contains("//Edit:"))
                        {
                            string s = msg.Message.Content;
                            string[] ss = s.Split("//Edit:");
                            string[] sss = ss[1].Split("//");

                            int time = int.Parse(sss[0]);
                            string edited = sss[1];

                            await SendEditMessage(chn, ss[0], time, edited);
                        }
                        else if (msg.Message.Content.Contains("//Delete:"))
                        {
                            string s = msg.Message.Content;
                            string[] ss = s.Split("//Delete:");
                            string[] sss = ss[1].Split("//");

                            int time = int.Parse(sss[0]);

                            await SendDeleteMessage(chn, ss[0], time);
                        }
                        else
                            await chn.SendMessageAsync(msg.Message.Content);
                    }
                }
            }

            if (createchannel)
                await mchn.DeleteAsync();

            await tmpmsg.ModifyAsync("**Disconnected**");
        }

        [Command("GetServerInfo")]
        public async Task GetServerInfo(CommandContext ctx, ulong guildId)
        {
            var Guild = await ctx.Client.GetGuildAsync(guildId);

            DiscordEmoji[] StatusEmoji = {
                DiscordEmoji.FromGuildEmote(ctx.Client, 732242761717121074), //online
        		DiscordEmoji.FromGuildEmote(ctx.Client, 732242745309265972), //idle
        		DiscordEmoji.FromGuildEmote(ctx.Client, 732242728796160011), //dnd
        		DiscordEmoji.FromGuildEmote(ctx.Client, 732242947814457354)  //offline
        	};

            DiscordMember[] GuildMembers = (await Guild.GetAllMembersAsync()).ToArray();

            /*
        	int[] UserCount = {
        		GuildMembers.Where(l => l.Presence.Status == UserStatus.Online).Count(),
        		GuildMembers.Where(l => l.Presence.Status == UserStatus.Idle).Count(),
        		GuildMembers.Where(l => l.Presence.Status == UserStatus.DoNotDisturb).Count(),
        		GuildMembers.Where(l => l.Presence.Status == UserStatus.Offline).Count()
        	};
        	
        	string description = string.Empty;
        	for (int i = 0; i < 4; i++)
        		description += $"{StatusEmoji[i]}-{UserCount[i]}";
        	*/

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = $"{Guild.Name} - {Guild.Id}",
                //Description = description,
                Footer = GetFooter(ctx),
                Timestamp = DateTime.Now,
                ThumbnailUrl = Guild.IconUrl
            };

            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("RandomTest")]
        public async Task Random(CommandContext ctx, params string[] value)
        {
            int total = 0;
            List<int> percent = new List<int>();

            foreach (string i in value)
            {
                int j = int.Parse(i);
                total += j;
                percent.Add(j);
            }

            string result = GetRandom(percent.ToArray(), value);
            double k = double.Parse(result);
            double output = (k / Convert.ToDouble(total));

            Console.WriteLine($"{k} {result} {total} {output} {output * 100}");

            await ctx.RespondAsync($"{output * 100}%");
        }

        [Command("MaskedUrl")]
        public async Task MakeMaskedUrl(CommandContext ctx, string content, string url, string option = "")
        {
            string MaskedUrl = Formatter.MaskedUrl(content, new Uri(url), option);
            var dmb = new DiscordEmbedBuilder();
            dmb.AddField(content, MaskedUrl);
            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("AdminCommands"), Check]
        public async Task AdminCommands(CommandContext ctx)
        {
            var commands = ctx.CommandsNext.RegisteredCommands;
            CheckBaseAttribute Check = new CheckAttribute();
            string StringCommands = string.Empty;

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "Commands",
                Color = GetRandomColor(),
                Timestamp = DateTime.Now,
                Footer = GetFooter(ctx)
            };

            foreach (var c in commands.Where(l => l.Value.ExecutionChecks.Contains(Check)))
                StringCommands += $"`{c.Key}` ";

            await ctx.RespondAsync(embed: dmb.AddField("Management Command List", StringCommands).Build());
        }
    }
}