using DiscordBot.Attributes;
using DiscordBot.Configs;
using DiscordBot.Exceptions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DiscordBot.Account;
using static DiscordBot.Utils;
using static DiscordBot.Variable;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace DiscordBot
{
    [BlackList, AccountCheck]
    class Gaming : BaseCommandModule
    {
        public string[,] ResetBoard(string[,] PlayBoard)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    PlayBoard[i, j] = ":question:";
                }
            }

            return PlayBoard;
        }

        [Group("큐브"), BlackList, AccountCheck]
        class CubeGame : BaseCommandModule
        {
            [Command("갯수")]
            public async Task CubeCount(CommandContext ctx)
            {
                string Id = $"Account/{ctx.User.Id}";
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                await ctx.RespondAsync($"{ctx.User.Username}님은 {ui.Cube}개의 큐브를 가지고 있어요!");
            }
            [Command("오픈")]
            public async Task CubeOpen(CommandContext ctx)
            {
                string Id = $"Account/{ctx.User.Id}";
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);
                TimeSpan date = ui.OpenTime - DateTime.Now;
                if (ui.CubeTimeHour > 0 && date.TotalSeconds <= 0)
                {
                    await ctx.RespondAsync("완성된 큐브가 있어요! 먼저 `라히야 큐브 확인`을 통해서 큐브를 열어주세요!");
                }
                else if (ui.CubeTimeHour > 0 && date.TotalSeconds > 0)
                {
                    TimeSpan time = ui.OpenTime - DateTime.Now;
                    string ReamainingTime = $"{time.Hours}시간 {time.Minutes}분 {time.Seconds}초";
                    await ctx.RespondAsync($"이미 오픈중인 큐브가 있어요! 아직 {ReamainingTime} 남았어요!");
                }
                else if (ui.Cube > 0 && ui.CubeTimeHour == 0)
                {
                    uint[] rndtime = { 3, 6, 12, 24 };

                    int rndint = rnd.Next(1, 100);

                    if (1 <= rndint && rndint <= 40)
                    {
                        rndint = 0;
                        ui.OpenTime = DateTime.Now.AddHours(3);
                    }
                    else if (41 <= rndint && rndint <= 70)
                    {
                        rndint = 1;
                        ui.OpenTime = DateTime.Now.AddHours(6);
                    }
                    else if (71 <= rndint && rndint <= 90)
                    {
                        rndint = 2;
                        ui.OpenTime = DateTime.Now.AddHours(12);
                    }
                    else if (91 <= rndint && rndint <= 100)
                    {
                        rndint = 3;
                        ui.OpenTime = DateTime.Now.AddHours(24);
                    }

                    DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
                    {
                        Title = "큐브 여는 중...",
                        Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)],
                        Thumbnail = new EmbedThumbnail { Url = "https://i.imgur.com/NbqU7wO.png" }
                    };

                    ui.Cube -= 1;
                    ui.CubeTimeHour = rndtime[rndint];

                    string sjson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                    File.WriteAllText(Id, sjson);

                    dmb.AddField("큐브 여는중...", $"{rndtime[rndint]}시간 큐브를 여는 중...");

                    await ctx.RespondAsync(embed: dmb.Build());
                }
                else if (ui.Cube == 0)
                {
                    await ctx.RespondAsync("이런.. 아쉽게도 남은 큐브가 없네요");
                }
                else
                {
                    await ctx.RespondAsync("실패한거 같아요!");
                }
            }

            [Command("확인")]
            public async Task CubeCheck(CommandContext ctx)
            {
                string Id = $"Account/{ctx.User.Id}";
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                if (ui.CubeTimeHour > 0 && ui.OpenTime.Subtract(DateTime.Now).TotalSeconds <= 0)
                {
                    ulong money = 0;
                    uint cube = 0;
                    DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
                    {
                        Title = "큐브를 열었어요!",
                        Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)],
                        Thumbnail = new EmbedThumbnail { Url = "https://i.imgur.com/pYhK9Ct.png" }
                    };

                    if (ui.CubeTimeHour == 3)
                    {
                        money = Convert.ToUInt64(rnd.Next(1, 5));
                    }
                    else if (ui.CubeTimeHour == 6)
                    {
                        money = Convert.ToUInt64(rnd.Next(3, 8));
                    }
                    else if (ui.CubeTimeHour == 12)
                    {
                        money = Convert.ToUInt64(rnd.Next(7, 13));
                        cube = Convert.ToUInt32(rnd.Next(0, 1));
                    }
                    else if (ui.CubeTimeHour == 24)
                    {
                        money = Convert.ToUInt64(rnd.Next(10, 20));
                        cube = Convert.ToUInt32(rnd.Next(0, 2));
                    }

                    ui.Money += money;
                    ui.CubeTimeHour = 0;

                    string reward = $"{money}코인";
                    if (cube > 0)
                    {
                        reward += $", 큐브 {cube}개";
                        ui.Cube += cube;
                    }

                    string sjson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                    File.WriteAllText(Id, sjson);

                    dmb.AddField("큐브에서 나온 보상", $"{reward}");

                    await ctx.RespondAsync(embed: dmb.Build());
                }
                else if (ui.CubeTimeHour > 0 && ui.OpenTime.Subtract(DateTime.Now).TotalSeconds > 0)
                {
                    TimeSpan time = ui.OpenTime - DateTime.Now;
                    string ReamainingTime = $"{time.Hours}시간 {time.Minutes}분 {time.Seconds}초";
                    await ctx.RespondAsync($"아직 큐브가 안열렸어요! {ReamainingTime} 남았어요!");
                }
                else
                {
                    await ctx.RespondAsync("큐브가 등록되지 않았어요! `라히야 큐브 오픈`으로 큐브를 열어봐요");
                }
            }
            [Command("상점"), DoNotUse]
            public async Task CubeShop(CommandContext ctx)
            {
            }
        }

        [Command("코인")]
        public async Task CoinCount(CommandContext ctx)
        {
            string Id = $"Account/{ctx.User.Id}";
            string json = File.ReadAllText(Id);
            UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

            await ctx.RespondAsync($"{ctx.User.Username}님은 {ui.Money}코인을 가지고 계세요!");
        }

        [Command("리워드")]
        public async Task Reward(CommandContext ctx)
        {
            string Id = $"Account/{ctx.User.Id}";
            string rewardjson = File.ReadAllText(Id);
            UserInfo rewardui = JsonConvert.DeserializeObject<UserInfo>(rewardjson);

            if (DateTime.Now.Day - rewardui.RewardTime.Day >= 1 || DateTime.Now.Month - rewardui.RewardTime.Month >= 1)
            {
                Random rnd = new Random();

                int rndint = rnd.Next(1, 5);

                string texts = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(texts);

                ui.Cube += 1;
                ui.RewardTime = DateTime.Now;

                await ctx.RespondAsync($"{ctx.User.Username}님에게 리워드를 지급했어요! :ice_cube: `+1`");

                if (rndint == 1)
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":mag:");

                    var interactivity = ctx.Client.GetInteractivity();
                    var msg = await ctx.RespondAsync("행운의 선물이 도착했어요! 밑에 이모지로 반응하여 열어볼까요?");
                    await msg.CreateReactionAsync(emoji);

                    var reactions = (await interactivity.WaitForReactionAsync(x => x.Emoji == emoji, ctx.User, TimeSpan.FromSeconds(60))).Result;

                    if (reactions != null)
                    {
                        ulong rndmoney = Convert.ToUInt64(rnd.Next(1, 10));
                        ui.Money += rndmoney;
                        await msg.ModifyAsync(":gift:");
                        await Task.Delay(4000);
                        await msg.ModifyAsync($":partying_face: 선물에서 {rndmoney}코인이 나왔어요!");
                    }
                    else
                    {
                        await ctx.RespondAsync("이런! 60초가 지나서 선물이 날아가버렸어요...");
                    }
                }

                string json = JsonConvert.SerializeObject(ui, Formatting.Indented);
                File.WriteAllText(Id, json);
            }
            else if (DateTime.Now.Day - rewardui.RewardTime.Day < 1 || DateTime.Now.Month - rewardui.RewardTime.Month >= 1)
                await ctx.RespondAsync($"이미 리워드를 받으셨어요! 내일 다시 해봅시다!");
            else
            {
                await ctx.RespondAsync($"으악! 에러가 났어요!");
            }
        }

        [Command("숫자야구게임"), Description("NeedManageMessages")] //, RequireUserPermissions(Permissions.ManageMessages)
        public async Task NumberBaseBallGame(CommandContext ctx)
        {
            string RecordLocation = $"NBBRecords/{ctx.User.Id}";

            int[] answer = new int[3];
            string Result = string.Empty;
            int count = 0;

            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                    answer[i] = rnd.Next(0, 9);
                else if (i == 1)
                {
                    do
                    {
                        answer[i] = rnd.Next(0, 9);
                    } while (answer[i] == answer[0]);
                }
                else if (i == 2)
                {
                    do
                    {
                        answer[i] = rnd.Next(0, 9);
                    } while (answer[i] == answer[0] || answer[i] == answer[1]);
                }
            }

            var bmsg = await ctx.RespondAsync("기본 제한 시간 : 60초");
            var msg = await ctx.RespondAsync("? Strike, ? Ball : ? ? ?\n" +
                "```Result Board```");

        GetInput:
            var interactivity = ctx.Client.GetInteractivity();
            var input = (await interactivity.WaitForMessageAsync(l => l.Channel == ctx.Channel && l.Author == ctx.User, TimeSpan.FromSeconds(60))).Result;

            try
            {
                if (input != null)
                {
                    string[] content = input.Content.Split(' ');
                    //await put.Message.DeleteAsync();
                    int[] inputs = new int[3];
                    int[] sb = new int[2] { 0, 0 };
                    if (content.Length == 3)
                    {
                        int i = 0;

                        foreach (string c in content)
                        {
                            if (!int.TryParse(c, out int r))
                            {
                                await ctx.RespondAsync("입력 방식 : `a b c`");
                                goto GetInput;
                            }

                            inputs[i] = int.Parse(c);
                            i += 1;
                        }

                        if (inputs[0] == -1 && inputs[1] == -1 && inputs[2] == -1)
                        {
                            await bmsg.DeleteAsync();
                            await msg.DeleteAsync();
                            await ctx.RespondAsync("게임을 종료했습니다.");
                            return;
                        }

                        foreach (int n in inputs)
                        {
                            if (n < 0 || n > 9)
                            {
                                await ctx.RespondAsync("숫자는 0~9까지만 가능합니다");
                                goto GetInput;
                            }
                        }

                        #region Check
                        for (int j = 0; j < 3; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                if (inputs[j] == answer[k] && j == k)
                                    sb[0] += 1;
                                else if (inputs[j] == answer[k] && j != k)
                                    sb[1] += 1;
                            }
                        }

                        count += 1;
                        string inputstring = string.Empty;

                        foreach (int ns in inputs)
                            inputstring += $"{ns} ";

                        Result += $"{count}. {sb[0]} S, {sb[1]} B : {inputstring}\n";
                        await msg.DeleteAsync();
                        await ctx.RespondAsync($"{sb[0]} Strike, {sb[1]} Ball : {inputstring}\n" +
                            "```Result Board\n" +
                            $"{Result}```");

                        if (sb[0] == 3)
                        {
                            await Task.Delay(2000);
                            if (!File.Exists(RecordLocation))
                            {
                                File.WriteAllText(RecordLocation, $"{DateTime.Now}|{count}\n");
                                await msg.DeleteAsync();
                                await ctx.RespondAsync($"새로운 기록! 시도 횟수 : {count} ");
                            }
                            else
                            {
                                string message = string.Empty;

                                foreach (string contents in File.ReadAllLines(RecordLocation))
                                {
                                    if (int.Parse(contents.Split('|')[1]) > count)
                                    {
                                        message = "새로운 기록!";
                                        break;
                                    }
                                    else
                                    {
                                        message = "축하합니다!";
                                        break;
                                    }
                                }

                                File.AppendAllText(RecordLocation, $"{DateTime.Now}|{count}\n");
                                await msg.DeleteAsync();
                                await ctx.RespondAsync($"{message} 시도 횟수 : {count} ");
                            }
                        }
                        else
                        {
                            goto GetInput;
                        }
                        #endregion
                    }
                    else
                    {
                        await ctx.RespondAsync("입력 방식은 [a b c] 로 입력해주세요!");
                        goto GetInput;
                    }
                }
                else
                {
                    await ctx.RespondAsync("제한시간이 지났습니다.");
                    return;
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(e.Message);
                goto GetInput;
            }
        }

        [Command("틱택토"), DoNotUse] //데이터 저장 방식 : 틱택토진행코드(9자리, 빌경우 0으로 표기):승패 결정(지면 0, 이기면 1):선공(유저가 선공이면 0, 봇이 선공이면 1)
        public async Task TickTackTo(CommandContext ctx)
        {
            if (ctx.User.Id != 378535260754935819)
            {
                await ctx.RespondAsync("현재 개발중인 게임이예요!");
                return;
            }

            bool Start = false;
            Random rnd = new Random();

            string GamePath = $"Data/PlayData";
            string[,] PlayBoard = new string[3, 3] { { ":question:", ":question:", ":question:" }, { ":question:", ":question:", ":question:" }, { ":question:", ":question:", ":question:" } };

            PlayBoard = ResetBoard(PlayBoard);

            int Startrnd = rnd.Next(0, 1);

            if (Startrnd == 1)
                Start = true;

            string board = $"{PlayBoard[0, 0] + PlayBoard[0, 1] + PlayBoard[0, 2]}\n{PlayBoard[1, 0] + PlayBoard[1, 1] + PlayBoard[1, 2]}\n{PlayBoard[2, 0] + PlayBoard[2, 1] + PlayBoard[2, 2]}";
            var PlayMessage = await ctx.RespondAsync($"{board}\n번호만 적어주세요!");

            var interactivity = ctx.Client.GetInteractivity();

            //string[] emojis = { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:" };

            bool finish = false;
            string result = string.Empty;
            int num = 0;

            while (finish)
            {
                if (!Start)
                {
                    var reactions = await interactivity.WaitForMessageAsync(x => x.Id == ctx.User.Id && x.Channel == ctx.Channel && int.TryParse(x.Content, out num));

                    if (num > 0 && num < 10)
                    {
                        #region Check Number
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                int number = (i * 3) + j;

                                //if (reactions.Emoji == DiscordEmoji.FromName(ctx.Client, emojis[number]) && PlayBoard[i, j] == ":question:")
                                if (num == number && PlayBoard[i, j] != ":question:")
                                {
                                    PlayBoard[i, j] = ":o:";
                                    result += number.ToString();
                                    await PlayMessage.ModifyAsync($"{board}\n제 차례네요!");
                                }
                            }
                        }
                        #endregion
                    }
                    Start = true;
                }
                else
                {
                    await Task.Delay(1000);

                    string[] Datas = File.ReadAllLines(GamePath);
                    /*
                    foreach (string Data in Datas)
                    {
                        string[] Line = Data.Split(':');
                        string code = Line[0];

                        bool win = false;
                        if (int.Parse(Line[1]) == 1)
                            win = true;

                        bool first = false;
                        if (int.Parse(Line[2]) == 1)
                            first = true;

                        if (code.StartsWith(result))
                        {
                            //파일에서 데이터 불러오는 코드 짜기
                        }
                    }
                    */
                    Start = false;
                }
            }
        }

        [Command("가위바위보")]
        public async Task RSPGame(CommandContext ctx)
        {
            DiscordEmoji[] rsp =
            {
                DiscordEmoji.FromName(ctx.Client, ":hand_splayed:"),
                DiscordEmoji.FromName(ctx.Client, ":fist:"),
                DiscordEmoji.FromName(ctx.Client, ":v:")
            };

            int bot = rnd.Next(0, 2);

            var msg = await ctx.RespondAsync("10초안에 안내면 진거! 가위바위보!");

            for (int i = 0; i < 3; i++)
            {
                await msg.CreateReactionAsync(rsp[i]);
                await Task.Delay(1000);
            }

            var interactivity = ctx.Client.GetInteractivity();
            var reactions = (await interactivity.WaitForReactionAsync(l =>
                l .Emoji== rsp[0] || l.Emoji == rsp[1] || l.Emoji == rsp[2], ctx.User, TimeSpan.FromSeconds(10))).Result;

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
            {
                Title = "결과",
                Color = DiscordColor.Green
            };

            dmb.AddField($"나({ctx.Member.Username}) vs 라히", $"{reactions.Emoji} vs {rsp[bot]}");
            await msg.DeleteAsync();
            await ctx.RespondAsync(embed: dmb.Build());

            if (reactions != null)
            {
                if (reactions.Emoji == rsp[0])
                {
                    if (bot == 0)
                        await ctx.RespondAsync("이런 비겼네요. 좋은 승부였다고요! ~~물론 가위바위보지만~~");
                    else if (bot == 1)
                        await ctx.RespondAsync("아! 제가 졌내요! 다음에는 꼭 이길꺼예요!");
                    else
                        await ctx.RespondAsync("히히 제가 이겼네요!");
                }
                else if (reactions.Emoji == rsp[1])
                {
                    if (bot == 0)
                        await ctx.RespondAsync("히히 제가 이겼네요!");
                    else if (bot == 1)
                        await ctx.RespondAsync("이런 비겼네요. 좋은 승부였다고요! ~~물론 가위바위보지만~~");
                    else
                        await ctx.RespondAsync("아! 제가 졌내요! 다음에는 꼭 이길꺼예요!");
                }
                else if (reactions.Emoji == rsp[2])
                {
                    if (bot == 0)
                        await ctx.RespondAsync("아! 제가 졌내요! 다음에는 꼭 이길꺼예요!");
                    else if (bot == 1)
                        await ctx.RespondAsync("히히 제가 이겼네요!");
                    else
                        await ctx.RespondAsync("이런 비겼네요. 좋은 승부였다고요! ~~물론 가위바위보지만~~");
                }
            }
            else
            {
                await ctx.RespondAsync("아무것도 안내셨군요! 제가 이겼습니다!");
            }
        }

        [Description("도박")]
        [BlackList, AccountCheck]
        class Gambling : BaseCommandModule
        {
            [Command("갈림길"), RequireUserPermissions(Permissions.ManageMessages), DoNotUse]
            public async Task RightOrLeft(CommandContext ctx)
            {
                uint Money = 0;
                uint Cube = 0;

                string Id = $"{IdLocation}/{ctx.User.Id}";
                string json = File.ReadAllText(Id);
                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

                if (ui.Money < 1)
                    throw new NotEnoughItemException();

                EmbedFooter footer = new EmbedFooter()
                {
                    IconUrl = ctx.User.AvatarUrl,
                    Text = $"탐험가 : {ctx.User.Username}#{ctx.User.Discriminator}"
                };

                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                {
                    Title = "오른쪽 왼쪽 갈림길 선택",
                    Footer = footer,
                    Timestamp = DateTime.Now
                };

                var emojileft = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");
                var emojistop = DiscordEmoji.FromName(ctx.Client, ":stop_button:");
                var emojiright = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");

                var msg = await ctx.RespondAsync(embed: dmb.Build());
                await Task.Delay(1000);

                await msg.CreateReactionAsync(emojileft);
                await msg.CreateReactionAsync(emojistop);
                await msg.CreateReactionAsync(emojiright);

                var interactivity = ctx.Client.GetInteractivity();

                Again:

                var reactions = (await interactivity.WaitForReactionAsync(l => l.Emoji == emojileft || l.Emoji == emojiright || l.Emoji == emojistop,
                    ctx.User,
                    TimeSpan.FromMinutes(5))).Result;

                if (reactions != null)
                {
                    if (reactions.Emoji == emojileft || reactions.Emoji == emojiright)
                    {
                        if (reactions.Emoji == emojileft)
                            await msg.DeleteReactionAsync(emojileft, ctx.User);
                        else
                            await msg.DeleteReactionAsync(emojiright, ctx.User);

                        string s = RandomAddtion();

                        if (s == null)
                        {
                            Money = 0;
                            Cube = 0;
                            return;
                        }

                        dmb.Title = s;
                        dmb.Timestamp = DateTime.Now;

                        await msg.ModifyAsync(embed: dmb.Build());

                        goto Again;
                    }
                    else if (reactions.Emoji == emojistop)
                        goto Result;
                }
                else
                {
                    await ctx.RespondAsync("5분동안 탐험하지 않았으므로 탐험이 종료됩니다.");
                    goto Result;
                }

                Result:
                {
                    string s;

                    if (Money == 0 && Cube == 0)
                        s = "얻은 것이 없다.";
                    else if (Money == 0 && Cube != 0)
                        s = $"큐브 {Cube}개를 얻었다!";
                    else if (Money != 0 && Cube == 0)
                        s = $"{Money}코인을 얻었다!";
                    else
                        s = $"큐브 {Cube}개와 {Money}코인을 얻었다!";

                    await msg.DeleteAsync();
                    await ctx.RespondAsync(s);

                    ui.Money += Money;
                    ui.Cube += Cube;
                    ui.Money -= 1;

                    string roljson = JsonConvert.SerializeObject(ui, Formatting.Indented);
                    File.WriteAllText(Id, roljson);
                }

                string RandomAddtion()
                {
                    int random = rnd.Next(1, 100);
                    string s;

                    if (1 <= random && random <= 30)
                    {
                        uint i = (uint)rnd.Next(1, 3);
                        Money += i;
                        s = $"{i}코인을 발견했다!";
                    }
                    else if (31 <= random && random <= 50)
                    {
                        uint i = (uint)rnd.Next(1, 2);
                        Cube += i;
                        s = $"큐브 {i}를 휙득했다!";
                    }
                    else if (51 <= random && random <= 80)
                        s = "아무것도 없었다.";
                    else
                        s = null;

                    return s;
                }
            }

            [Command("주사위굴리기")]
            public async Task RollDice(CommandContext ctx)
            {
                DiscordEmoji[] Dice = new DiscordEmoji[]
                {
                    DiscordEmoji.FromGuildEmote(ctx.Client, 718728815936929815),
                    DiscordEmoji.FromGuildEmote(ctx.Client, 718728815760637953),
                    DiscordEmoji.FromGuildEmote(ctx.Client, 718728816205234186),
                    DiscordEmoji.FromGuildEmote(ctx.Client, 718728816096444526),
                    DiscordEmoji.FromGuildEmote(ctx.Client, 718728815878340679),
                    DiscordEmoji.FromGuildEmote(ctx.Client, 718728816138256394)
                };

                DiscordEmoji RandomEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 770226476922437663);

                string[] s = new string[5];

                for (int i = 0; i < 5; i++)
                    s[i] = RandomEmoji.ToString();

                var DiceEmbed = new DiscordEmbedBuilder()
                {
                    Title = "주사위 굴리기",
                    Color = new DiscordColor(128, 255, 255)
                }.AddField("결과", string.Join(' ', s)).Build();

                var msg = await ctx.RespondAsync(embed: DiceEmbed);

                await Task.Delay(TimeSpan.FromSeconds(1));

                int[] results = new int[5];

                for (int i = 0; i < 5; i++)
                {
                    int r = rnd.Next(0, 6);

                    s[i] = Dice[r];
                    results[i] = r;

                    var ChangeEmbed = new DiscordEmbedBuilder()
                    {
                        Title = "주사위 굴리기",
                        Color = new DiscordColor(128, 255, 255)
                    }.AddField("결과", string.Join(' ', s)).Build();

                    await msg.ModifyAsync(embed: ChangeEmbed);

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                results = results.OrderBy(l => l).ToArray();

                if (results.Distinct().Count() == 1) //야추 : 주사위 5개가 같음
                {
                    await ctx.RespondAsync("대박!");
                    return;
                }   
                else if (results.Distinct().Count() == 5) //스트레이트 : 주사위 5개가 연속됨
                {
                    var r = results.Distinct();

                    if (r.AllIn(1, 2, 3, 4))
                    {
                        await ctx.RespondAsync("스트레이트!");
                        return;
                    }
                }
                else if (results.Distinct().Count() == 2) // 풀하우스 / 포카
                {
                    var list = results.ToList();

                    int Base = list[0];

                    for (int i = 1; i < 5; i++)
                    {
                        if (list[i] != Base)
                            list.RemoveAt(i);
                    }

                    if (list.Count == 4) // 포카 : 4개의 주사위가 같은 경우
                    {
                        await ctx.RespondAsync("포카인드!");
                        return; 
                    }
                    else                // 풀하우스 : 3개가 같고 2개가 같은 경우
                    {
                        await ctx.RespondAsync("풀하우스!");
                        return;
                    }
                }
                await ctx.RespondAsync("꽝.. 다음기회에"); 
            }
        }

        [Command("몬스터"), DoNotUse]
        [SuppressMessage("Style", "IDE0059:불필요한 값 할당", Justification = "<보류 중>")]
        public async Task MonsterGame(CommandContext ctx)
        {
            // Main Variables
            DiscordUser GameUser = ctx.User;
            string ResourceFolder = "GameResource/";
            DiscordEmbedBuilder GameEmbed = new DiscordEmbedBuilder() { Color = GetRandomColor() };

            // Game Variables
#pragma warning disable CS0219 // 변수가 할당되었지만 해당 값이 사용되지 않았습니다.
            int Round;
            int[] HealthList = { 100 }; // TODO: Set each round boss's hp
            float CurrentHealth = HealthList[0];
            int LeftBallCount;
            int PreviousBallIndex;
#pragma warning restore CS0219 // 변수가 할당되었지만 해당 값이 사용되지 않았습니다.

            /* 
             * Each Ball's Ablity
             * 
             * CommonBall => Sometimes damaged 2x
             * FireBall => At next turn it damaged more. ex) 1.5x damage
             * MagicBall => Add one more ball
             * PoisonBall => Each turn it damage little
             * BlackBall => Different damage according to previous Ball
            */

            // Make Images
            Image Monster = Image.FromFile($"{ResourceFolder}Monster.png");
            Image CommonBall = Image.FromFile($"{ResourceFolder}CommonBall");
            Image FireBall = Image.FromFile($"{ResourceFolder}FireBall");
            Image PosionBall = Image.FromFile($"{ResourceFolder}PoisonBall");
            Image MagicBall = Image.FromFile($"{ResourceFolder}MagicBall");
            Image BlackBall = Image.FromFile($"{ResourceFolder}BlackBall");

            // Make DiscordEmoji
            DiscordEmoji ECommonBall = DiscordEmoji.FromGuildEmote(ctx.Client, 739392980984791081);
            DiscordEmoji EFireBall = DiscordEmoji.FromGuildEmote(ctx.Client, 739392980976402524);
            DiscordEmoji EPosionBall = DiscordEmoji.FromGuildEmote(ctx.Client, 739392980904837130);
            DiscordEmoji EMagicBall = DiscordEmoji.FromGuildEmote(ctx.Client, 739392980745715793);
            DiscordEmoji EBlackBall = DiscordEmoji.FromGuildEmote(ctx.Client, 739392980494057485);

            // Setting Array Variables
            //TODO : Set Damage each balls
            Tuple<DiscordEmoji, Image,float>[] BallsArray = new List<Tuple<DiscordEmoji, Image, float>>()
            {
                new Tuple<DiscordEmoji, Image, float>(ECommonBall, CommonBall, 0),
                new Tuple<DiscordEmoji, Image, float>(EFireBall, FireBall, 0),
                new Tuple<DiscordEmoji, Image, float>(EPosionBall, PosionBall, 0),
                new Tuple<DiscordEmoji, Image, float>(EMagicBall, MagicBall, 0),
                new Tuple<DiscordEmoji, Image, float>(EBlackBall, BlackBall, 0)
            }.ToArray();
            DiscordEmoji[] EBallsArray = { ECommonBall, EFireBall, EPosionBall, EMagicBall, EBlackBall };

            // Default Setting
            GameEmbed.WithImageUrl($"{ResourceFolder}Monster.png");
            GameEmbed.WithTitle($"{CurrentHealth}/{HealthList[0]}");
            Round = 0;
            LeftBallCount = 4;
            PreviousBallIndex = -1;

            //GameStart

            /* <TODO LIST>
             * 
             * Gaming: 1. 기본 틀 제작하기
             * Gaming: 2. 공 랜덤으로 고르는 옵션 제작하기
             * Gaming: 3. 공이 모두 소진되거나 hp가 없을때 제작하기
            */

            var Gaming = await ctx.RespondAsync(embed: GameEmbed.Build()); //Main Game Squence
            while (true)
            {
                DiscordEmoji[] RandomEmojis = await RandomReactionCreate(message: Gaming, emojis: EBallsArray);
                var interactivity = ctx.Client.GetInteractivity();
                var reactions = (await interactivity.WaitForReactionAsync(l =>
                        RandomEmojis.Contains(l.Emoji), GameUser, TimeSpan.FromSeconds(30))).Result;

                if (reactions != null)
                {
                    float damage = DamageCalculate(reactions.Emoji, EBallsArray);
                }

            }
        }

        private async Task<DiscordEmoji[]> RandomReactionCreate(DiscordMessage message, DiscordEmoji[] emojis)
        {
            List<DiscordEmoji> selectedEmoji = new List<DiscordEmoji>();
            for (int i = 0; i < 3; i++)
            {
                selectedEmoji.Add(emojis[rnd.Next(0, emojis.Length - 1)]);
                await message.CreateReactionAsync(selectedEmoji.Last());
            }

            return selectedEmoji.ToArray();
        }

        private float DamageCalculate(DiscordEmoji Ball, DiscordEmoji[] BallArray)
        {
            // WorkFirst: Make Damage Calculate
            return 0;
        }
    }
}
