using DiscordBot.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot
{
    [BlackList()]
    public class Account
    {
        public static string IdLocation = $"Account/";

        public class OldInfo
        {
            public string Name { get; set; }
            public ulong UserId { get; set; }
            public DateTime RegDate { get; set; }
            public ulong Money { get; set; }
            public uint Cube { get; set; }
            public DateTime RewardTime { get; set; }
            public uint CubeTimeHour { get; set; }
            public DateTime OpenTime { get; set; }
        }

        public class UserInfo
        {
            public string Name { get; set; }
            public ulong UserId { get; set; }
            public DateTime RegDate { get; set; }
            public ulong Money { get; set; }
            public uint Cube { get; set; }
            public DateTime RewardTime { get; set; }
            public uint CubeTimeHour { get; set; }
            public DateTime OpenTime { get; set; }
        }

        [Command("가입"), AccountAgree]
        public async Task Regin(CommandContext ctx)
        {
            string Id = $"{IdLocation}/{ctx.User.Id}";
            string OldId = $"ReloadAc/{ctx.User.Id}";

            if (File.Exists(OldId))
            {
                await ctx.RespondAsync("이제 정상적으로 이용하실수 있습니다.");
                File.Move(OldId, Id);
                return;
            }

            DateTime dt = DateTime.Now;

            UserInfo ui = new UserInfo
            {
                Name = ctx.User.Username,
                UserId = ctx.User.Id,
                RegDate = dt,
                Money = 0,
                Cube = 3,
                RewardTime = DateTime.Now.AddHours(-24),
                CubeTimeHour = 0,
                OpenTime = DateTime.Now
            };

            string json = JsonConvert.SerializeObject(ui, Formatting.Indented);

            File.WriteAllText(Id, json);

            await ctx.RespondAsync($":partying_face: {ctx.User.Username}님의 가입을 축하드립니다! 가입일시 : {dt}\n그리고 이건 작은 선물이예요... :kissing_closed_eyes: :ice_cube: `+3`");
        }

        [Command("탈퇴")]
        public async Task RegOut(CommandContext ctx)
        {
            string Id = $"{IdLocation}/{ctx.User.Id}";
            if (!File.Exists(Id))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} 가입을 하셔야 사용하실수 있어요! '라히야 가입'으로 어서 가입해봐요!");
            }
            else
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

                var interactivity = ctx.Client.GetInteractivityModule();
                var msg = await ctx.RespondAsync("탈퇴를 원하시면 :white_check_mark:를 눌러주세요 (30초)");
                await msg.CreateReactionAsync(emoji);
                var reactions = await interactivity.WaitForReactionAsync(x => x == emoji, ctx.User, TimeSpan.FromSeconds(30));

                if (reactions != null)
                {
                    File.Delete(Id);
                    await ctx.RespondAsync("즐거웠어요.. 잘가요 :sob:");
                }
                else
                {
                    await ctx.RespondAsync("저랑 헤어지기 싫으신거죠? 알겠어요!");
                }
            }
        }
    }
}
