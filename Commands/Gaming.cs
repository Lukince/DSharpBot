using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using static DiscordBot.Account;
using static DiscordBot.Utils;
using static DiscordBot.Variable;

namespace DiscordBot
{
    [BlackList, AccountCheck]
    class Gaming
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

        [Group("큐브")]
        class CubeGame
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
                        ThumbnailUrl = "https://i.imgur.com/NbqU7wO.png"
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
                        ThumbnailUrl = "https://i.imgur.com/pYhK9Ct.png"
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
            [Command("상점")]
            public async Task CubeShop(CommandContext ctx)
            {
                await ctx.RespondAsync("아직 개발중이에요! 조금만 기다려 주세요!");

                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
                {
                    Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
                };
                dmb.AddField("큐브 1개", "1 코인");

                var msg = await ctx.RespondAsync($"라히의 상점이예요!\n```이모지를 클릭하면 상품 목록을 열어드릴게요!\n1. 큐브상점 2. 포션상점 3. 업그레이드```");

                var interactivity = ctx.Client.GetInteractivityModule();
                var reactions = interactivity.CollectReactionsAsync(msg, TimeSpan.FromMinutes(1)).ConfigureAwait(false).GetAwaiter().GetResult();

                foreach (DiscordEmoji de in reactions.Reactions.Keys)
                {
                    if (de.Name == ":one:")
                    {

                    }
                    else if (de.Name == ":two:")
                    {

                    }
                    else if (de.Name == ":three:")
                    {

                    }
                }
            }
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

                    var interactivity = ctx.Client.GetInteractivityModule();
                    var msg = await ctx.RespondAsync("행운의 선물이 도착했어요! 밑에 이모지로 반응하여 열어볼까요?");
                    await msg.CreateReactionAsync(emoji);

                    var reactions = await interactivity.WaitForReactionAsync(x => x == emoji, ctx.User, TimeSpan.FromSeconds(60)).ConfigureAwait(false);

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
            var interactivity = ctx.Client.GetInteractivityModule();
            var input = interactivity.WaitForMessageAsync(l => l.Channel == ctx.Channel && l.Author == ctx.User, TimeSpan.FromSeconds(60));

            try
            {
                if (input != null)
                {
                    var put = input.GetAwaiter().GetResult();
                    string[] content = put.Message.Content.Split(' ');
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

            var interactivity = ctx.Client.GetInteractivityModule();

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
                Task.Delay(500);
            }

            var interactivity = ctx.Client.GetInteractivityModule();
            var reactions = await interactivity.WaitForReactionAsync(l =>
                l == rsp[0] || l == rsp[1] || l == rsp[2], ctx.User, TimeSpan.FromSeconds(10));

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
    }
}
