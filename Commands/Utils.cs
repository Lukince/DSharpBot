using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Abstractions;
using MoreExtension;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using QRCoder;
using static DiscordBot.Account;
using static DiscordBot.Index;
using static DiscordBot.Variable;
using static DiscordBot.Config;
using static DSharpPlus.Entities.DiscordEmbedBuilder;
using System.Drawing;
using System.Windows.Markup;

namespace DiscordBot
{
    [BlackList]
    class Utils
    {
        public static Random rnd = new Random();

        [Command("주사위")]
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

            Random rnd = new Random();

            await ctx.RespondAsync($"{Dice[rnd.Next(0, 5)]}");
        }

        [Command("초대")]
        public async Task Invite(CommandContext ctx)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "초대 링크",
                ThumbnailUrl = ctx.Client.CurrentUser.AvatarUrl,
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
            };

            dmb.AddField("초대", "[초대하기](https://discord.com/api/oauth2/authorize?client_id=661144157313564673&permissions=8&scope=bot)\n라히를 초대할 수 있는 링크예요! 어디에서든 저랑 놀아보자고요!");

            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("추가")]
        public async Task Addtion(CommandContext ctx, params string[] content)
        {
            string suggest = string.Join(" ", content);

            if (suggest == string.Empty || suggest == "")
            {
                await ctx.RespondAsync("아무것도 안적으셨네요! 지금 만족하고 계시다는거죠? :wink:");
            }
            else
            {
                File.AppendAllText($"Suggestion/{ctx.User.Id}", $"\n{DateTime.Now} : {suggest}");

                await ctx.RespondAsync("확인했어요! 개발자에게 전달해줄께요!");
            }
        }

        [Command("시간"), AccountCheck]
        public async Task Time(CommandContext ctx)
        {
            string Id = $"Account/{ctx.User.Id}";
            string json = File.ReadAllText(Id);
            UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(json);

            TimeSpan subtime = DateTime.Now.Subtract(ui.RegDate);

            string date = string.Empty;

            if (subtime.Days != 0)
                date += $"{subtime.Days}일 ";

            if (subtime.Hours != 0)
                date += $"{subtime.Hours}시간 ";

            if (subtime.Minutes != 0)
                date += $"{subtime.Minutes}분 ";

            date += $"{subtime.Seconds}초";

            await ctx.RespondAsync($"{ctx.User.Username}님과 함께한 시간이 벌써 {date} 이나 됬어요!");
        }

        [Command("핑")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync($":ping_pong: 퐁! `{ctx.Client.Ping}`");
        }

        [Command("말해")]
        public async Task Sayit(CommandContext ctx, params string[] content)
        {
            string saying = string.Join(" ", content);

            await ctx.RespondAsync($"{saying}\n```(소근소근)라고 {ctx.User.Username}님이 말했어요!```");
        }

        [Command("계산")]
        public async Task Calc(CommandContext ctx, params string[] content)
        {
            string math = string.Join(" ", content);

            math.Replace('×', '*');
            math.Replace('÷', '/');

            try
            {
                await ctx.RespondAsync(new DataTable().Compute(math, null).ToString());
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"으악! 에러가 났어요!\n```{e.Message}```");
            }
        }

        [Command("업타임")]
        public async Task Uptime(CommandContext ctx)
        {
            TimeSpan uptime = DateTime.Now.Subtract(StartTime);

            string date = string.Empty;

            if (uptime.Days != 0)
                date += $"{uptime.Days}일 ";

            if (uptime.Hours != 0)
                date += $"{uptime.Hours}시간 ";

            if (uptime.Minutes != 0)
                date += $"{uptime.Minutes}분 ";

            date += $"{uptime.Seconds}초";

            await ctx.RespondAsync($"{date}만큼 구동했음!");
        }


        [Command("랜덤")]
        public async Task Random(CommandContext ctx)
        {
            string[] message = ctx.Message.Content.Split(' ');

            if (message.Length < 3)
                await ctx.RespondAsync($"랜덤 함수 사용법 : 라히야 랜덤 [최소] [최대]\n최대, 최소의 값은 ±{int.MaxValue}를 넘을수 없어요!");
            else if (!int.TryParse(message[2], out _) || !int.TryParse(message[3], out int _))
                await ctx.RespondAsync($"최소값은 {int.MinValue}, 최대값은 {int.MaxValue}를 초과 할수 없어요!");
            else
                await ctx.RespondAsync(rnd.Next(int.Parse(message[2]), int.Parse(message[3])).ToString());
        }

        [Command("투표")]
        public async Task Vote(CommandContext ctx, params string[] content)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder
            {
                Title = $"주제 : {string.Join(" ", content)}",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
            };

            var msg = await ctx.RespondAsync(embed: dmb.Build());
            await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 617684865529282570));
            await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 617684780691095555));
        }

        [Command("반전")]
        public async Task Reverse(CommandContext ctx, params string[] content)
        {
            string nomal = string.Join(" ", content);
            await ctx.RespondAsync(nomal.Reverse());
        }

        //[Command("번역"), DoNotUse]
        public async Task Translation(CommandContext ctx, string lang, params string[] content)
        {
            string output = string.Empty;
            foreach (string l in languagelist)
            {
                if (l == lang)
                {
                    DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                    {
                        Title = "번역 결과",
                        Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
                    };

                    dmb.AddField(":inbox_tray: Input", string.Join(" ", content));

                    string url = $"http://www.google.com/translate_t?hl=en&ie=UTF8&text={string.Join(" ", content)}&langpair={lang}";
                    WebClient webClient = new WebClient
                    {
                        Encoding = Encoding.UTF8
                    };
                    string result = webClient.DownloadString(url);
                    result = result.Substring(result.IndexOf("<span title=\"") + "<span title=\"".Length);
                    result = result.Substring(result.IndexOf(">") + 1);
                    result = result.Substring(0, result.IndexOf("</span>"));

                    dmb.AddField(":outbox_tray: Output", $"```{result.Trim()}```");

                    output = dmb.Build().ToString();

                    goto finish;
                }
                else
                {
                    string langlist = string.Join(", ", languagelist);
                    output = $"입력하신 언어를 찾을 수 없습니다. 원하시는 언어를 개발자에게 등록 문의해주세요\n```{langlist}```";
                }
            }

        finish:
            {
                await ctx.RespondAsync(output);
            }
        }

        [Command("깃문자열")]
        public async Task EmbedBuild(CommandContext ctx, string url)
        {
            WebClient wc = new WebClient()
            {
                BaseAddress = "https://raw.githubusercontent.com/",
                Encoding = Encoding.UTF8
            };

            try
            {
                string s = wc.DownloadString(url);
                await ctx.RespondAsync(s);
            }
            catch (Exception e)
            {
                string tip;
                if (e.Message.Contains("400"))
                    tip = "주소가 잘못됬거나 가져올수 없는 문자열이예요! 기본적으로 [https://raw.githubusercontent.com/]를 기반으로 사용합니다. 사용법) [유저이름]/[리포지토리 이름]/[커밋한사람 이름 => 대부분 master]/[파일명]";
                else if (e.Message.Contains("404"))
                    tip = "현재 찾으려는 리포지토리가 비공개 상태인지 확인해보세요!";
                else
                {
                    tip = "팁을 찾을수 없어요!";
                }

                await ctx.RespondAsync($"으엑.. 정보를 받아오지 못했어요..\n```{e.Message}\nTip : {tip}```");
            }
        }

        private DiscordEmbedBuilder GetInfo(CommandContext ctx, ulong UserId)
        {
            try
            {
                var user = ctx.Client.GetUserAsync(UserId).GetAwaiter().GetResult();

                EmbedAuthor author = new EmbedAuthor()
                {
                    IconUrl = user.AvatarUrl,
                    Name = user.Username
                };

                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                {
                    Title = "유저 정보",
                    Author = author,
                    Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)],
                    Footer = GetFooter(ctx),
                    Timestamp = ctx.Message.Timestamp,
                    ThumbnailUrl = user.AvatarUrl
                };

                TransportGame usergame = user.Presence.Game;
                DiscordMember member = ctx.Guild.GetMemberAsync(user.Id).GetAwaiter().GetResult();

                dmb.AddField("이름", user.Username);
                dmb.AddField("아이디", user.Id.ToString());
                dmb.AddField("태그", $"{user.Username}#{user.Discriminator}");
                dmb.AddField("상태", user.Presence.Status.ToString());
                dmb.AddField("게임", $"이름 : {usergame.Name}\n" +
                    $"내용 : {usergame.Details}\n");
                //$"시간 : {DateTime.Now.Subtract(usergame.Timestamps.Start.Value.DateTime)}");
                dmb.AddField("길드 가입일", member.JoinedAt.ToString());
                //dmb.AddField("디스코드 가입일", user.CreatedAt);

                return dmb;
            }
            catch (Exception e)
            {
                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                {
                    Title = "Error!",
                    Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
                };

                dmb.AddField($":inbox_tray: Input", ctx.Message.Content);
                dmb.AddField($":outbox_tray: Output", "```Error!\n" +
                    $"{e.Message}\n" +
                    "Tip! : 아이디가 잘못됬거나 해당 유저와 봇이 같은 서버 내에 있는지 확인하세요!```");

                Console.WriteLine($"{e.Message}\n{e.StackTrace}\n{e.InnerException}");

                return dmb;
            }
        }

        [Command("정보"), CheckAdmin]
        public async Task Info(CommandContext ctx)
        {
            try
            {
                DiscordMember User;
                var count = ctx.Message.MentionedUsers.Count;

                if (count != 0)
                {
                    if (count > 1)
                    {
                        await ctx.RespondAsync("두명의 이상의 유저를 맨션할수 없습니다");
                        return;
                    }
                    else if (count == 1)
                    {
                        User = ctx.Guild.GetMemberAsync(ctx.Message.MentionedUsers[0].Id).GetAwaiter().GetResult();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    User = ctx.Member;
                }

                if (User == null)
                {
                    await ctx.RespondAsync("User is Null");
                    return;
                }
                else
                {
                    await ctx.RespondAsync(User.Id.ToString());
                }

                DiscordEmbedBuilder dmb = GetInfo(ctx, User.Id);
                await ctx.RespondAsync(embed: dmb.Build());
            }
            //catch ()
            catch (Exception e)
            {
                await ctx.RespondAsync("O_O! It has an ERROR!\n" +
                    $"```{e.Message}```");
            }
        }

        [Command("서버인원")]
        public async Task ServerCount(CommandContext ctx)
        {
            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "길드 유저수",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
            };

            var Members = ctx.Guild.GetAllMembersAsync().GetAwaiter().GetResult();

            dmb.AddField("전체 인원", $"{Members.Count}", true);
            dmb.AddField("유저 수", $"{Members.Where(l => !l.IsBot).Count()}", true);
            dmb.AddField("봇 수", $"{Members.Where(l => l.IsBot).Count()}");
            //dmb.AddField("온라인 수", $"{Members.Where(l => l.Presence.Status == UserStatus.Online).Count()}");
            //dmb.AddField("자리비움 수", $"{Members.Where(l => l.Presence.Status == UserStatus.Idle).Count()}");
            //dmb.AddField("다른 용무 중 수", $"{Members.Where(l => l.Presence.Status == UserStatus.DoNotDisturb).Count()}");
            //dmb.AddField("오프라인 수", $"{Members.Where(l => l.Presence.Status == UserStatus.Offline).Count()}");
            //if (ctx.User == ctx.Client.CurrentApplication.Owner)
            //dmb.AddField("Invisible Count", $"{Members.Where(l => l.Presence.Status == UserStatus.Invisible).Count()}");

            await ctx.RespondAsync(embed: dmb.Build());
        }

        [Command("히스토리")]
        public async Task GetHistory(CommandContext ctx)
        {
            if (!File.Exists(HistoryPath))
            {
                await ctx.RespondAsync("아직 히스토리가 없어요!");
                return;
            }

            DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
            {
                Title = "히스토리",
                Color = RandomColor[rnd.Next(0, RandomColor.Length - 1)]
            };

            string[] historylist = File.ReadAllLines(HistoryPath);

            for (int i = 0; i < historylist.Length; i++)
            {
                string[] history = historylist[i].Split('|');
                dmb.AddField(history[1], history[2]);
            }

            if (historylist.Length > 8)
            {
                var msg = await ctx.RespondAsync("*주의 히스토리가 8개 이상으로 많은 히스토리 목록으로 인해 도배가 될수도 있습니다.\n" +
                    "계속 진행을 원하시면 [진행]을 입력해주시고, [DM] 입력시 DM으로 히스토리를 보내드립니다.\n" +
                    "해당 메시지 이후에 30초가 지나거나 [취소]를 입력한 경우 출력이 취소됩니다.");
                var interactivity = ctx.Client.GetInteractivityModule();
                var messages = await interactivity.WaitForMessageAsync(l => l.Id == ctx.User.Id && l.Channel == ctx.Channel &&
                l.Content == "진행" || l.Content.ToLower() == "dm" || l.Content == "취소", TimeSpan.FromSeconds(30));

                if (messages != null)
                {
                    if (messages.Message.Content == "확인")
                        await ctx.RespondAsync(embed: dmb.Build());
                    else if (messages.Message.Content.ToLower() == "dm")
                    {
                        await ctx.Member.SendMessageAsync(embed: dmb.Build());
                        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                    }
                    else if (messages.Message.Content == "취소")
                    {
                        await msg.ModifyAsync("취소되었습니다.");
                    }
                }
                else
                {
                    await msg.ModifyAsync("30초 초과로 취소되었습니다.");
                }
            }
            else
            {
                await ctx.RespondAsync(embed: dmb.Build());
            }
        }

        [Command("qr코드")]
        public async Task CQRCode(CommandContext ctx, string url)
        {
            try
            {
                string file = $"QRCodes/{url.Replace('/', ';').Replace(':', ';')}.png";

                if (!File.Exists(file))
                {

                    QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                    QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.H);
                    QRCode qrCode = new QRCode(qRCodeData);
                    Bitmap qrCodeImage = qrCode.GetGraphic(20);
                    qrCodeImage.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                }

                await ctx.RespondWithFileAsync(file);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"O_O! ERROR!\n```{e.Message}```");
                Console.WriteLine(e);
            }
        }

        [Command("번역")]
        public async Task Translation(CommandContext ctx, string fromlang, string tolang, params string[] content)
        {
            try
            {
                string s = string.Join(" ", content);

                string url = "https://openapi.naver.com/v1/papago/n2mt";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("X-Naver-Client-Id", PapagoId);
                request.Headers.Add("X-Naver-Client-Secret", PapagoSecret);
                request.Method = "POST";
                string query = s;
                byte[] byteDataParams = Encoding.UTF8.GetBytes($"source={fromlang}&target={tolang}&text=" + query);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteDataParams.Length;
                Stream st = request.GetRequestStream();
                st.Write(byteDataParams, 0, byteDataParams.Length);
                st.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                string text = reader.ReadToEnd();
                stream.Close();
                response.Close();
                reader.Close();

                DiscordEmbedBuilder dmb = new DiscordEmbedBuilder()
                {
                    Title = "Papago 번역기",
                    Color = DiscordColor.Green,
                    Timestamp = DateTime.Now,
                    Footer = GetFooter(ctx)
                };

                dmb.AddField(":inbox_tray: 번역 전", $"```{s}```");
                dmb.AddField(":outbox_tray: 번역 후", $"```{text.Split('"')[27]}```");
                await ctx.RespondAsync(embed: dmb.Build());
            }
            catch (Exception e)
            {
                if (e.Message.Contains("400"))
                    await ctx.RespondAsync("알수 없는 언어입니다!\n```사용 가능한 언어 : ko, en, fr, zh-CN, vi 등등```");
            }
        }

        [Command("유튜브"), CheckAdmin, DoNotUse]
        public async Task Youtube(CommandContext ctx)
        {
            
        } //github 사용해서 backup 커맨드 만들기 - Management.cs 코드 파일만 복사
    }
}
