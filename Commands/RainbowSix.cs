using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DiscordBot.Attributes;

namespace DiscordBot
{
	[BlackList]
    public class RainbowSix 
    {
        enum R6Attacker
        {
        	Sledge,
        	Thatcher,
        	Ash,
        	Thermite,
        	Twitch,
        	Montagne,
        	Glaz,
        	Fuze,
        	Blitz,
        	Iq,
        	Buck,
        	Blackbeard,
        	Capitao,
        	Hibana,
        	Jackal,
        	Ying,
        	Zofia,
        	Dokkaebi,
        	Lion,
        	Finka,
        	Maveric,
        	Nomad,
        	Gridlock,
        	Nokk,
        	Amaru,
        	Kali,
        	Iana,
        	Ace
        }
        
        enum R6Defender
        {
        	Smoke,
        	Mute,
        	Castle,
        	Pulse,
        	Doc,
        	Rook,
        	Kapkan,
        	Tachanka,
        	Jager,
        	Bandit,
        	Frost,
        	Valkyrie,
        	Caveira,
        	Echo,
        	Mira,
        	Lesion,
        	Ela,
        	Vigil,
        	Maestro,
        	Alibi,
        	Clash,
        	Kaid,
        	Mozzie,
        	Warden,
        	Goyo,
        	Wamai,
        	Oryx,
        	Melusi
        }
        
        enum R6Map
        {
        	Bank, //은행
        	Border, //국경
        	Chalet, //별장
        	Clubhouse, //클럽하우스
        	Coastline, //해안선
        	Consulate, //영사관
        	Favela, //빈민가
        	Fortress, //요새
        	HerefordBase, //해리퍼드기지
        	House, //저택
        	KafeDostoyevsky, //도스예프스키카페
        	Kanal, //운하
        	Oregon, //오리건
        	Outback, //오지
        	PresidentialPlane, //대통령 전용기
        	Skycraper, //마천루
        	ThemePark, //테마파크
        	Tower, //타워
        	Villa, //빌라
        	Yacht //요트
        }
        
        [BlackList, Group("R6Random")]
        class R6Random
        {
        	private T RandomEnum<T>()
			{ 
				Random RNG = new Random();
				
    			Type type = typeof(T);
    			Array values = Enum.GetValues(type);
    			lock(RNG)
    			{
        			object value = values.GetValue(RNG.Next(values.Length));
        			return (T)Convert.ChangeType(value, type);
    			}
			}
        	
        	[Command("Attacker")]
        	public async Task RandomR6Attacker(CommandContext ctx)
        	{
        		await ctx.RespondAsync(RandomEnum<R6Attacker>().ToString());
        	}
        	
        	[Command("Defender")]
        	public async Task RandomR6Defender(CommandContext ctx)
        	{
        		await ctx.RespondAsync(RandomEnum<R6Defender>().ToString());
        	}
        	
        	[Command("Map")]
        	public async Task RandomR6Map(CommandContext ctx)
        	{
        		await ctx.RespondAsync(RandomEnum<R6Map>().ToString());
        	}
        }
        
        /*
        private T[] FindValue<T>()
        {
        	Type type = typeof(T);
        	Array array Enum.GetValues(type);
            array.GetValue();
        }*/
        
        //[Command("R6Operator")]
        public async Task GetR6Operator(CommandContext ctx, string Opername)
        {
        	
        }
    }
}