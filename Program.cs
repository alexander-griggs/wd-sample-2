using System;
using System.Reflection;
//for dictionaries, lists
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.Interactive;
using Npgsql;

//For running async without awaiting
#pragma warning disable CS4014
namespace warriordream
{
	class Program
	{
	// The soul.
	private DiscordSocketClient _client;
	// Interprets which of the messages it sees are commands.
	public static CommandHandler _commandhandler;
	// Interprets which of the reactions it sees are relavent to the game.
	private ReactionHandler _reactionhandler;
	/*
   
	GLOBAL METHODS

	*/
	// The standard way of talking to the database.
	public static NpgsqlConnection Conn()
	{
		return new NpgsqlConnection("Host=db;Port=5432;Username=postgres;Password=vnnVtVcAUGj2rmQ6");
	}
	// For naming and shaming those who give the bot bad links.
	public static bool ValidImgur(string s)
	{
		if (!s.Contains("imgur.com")) return false;
		if (!s.EndsWith(".png") && !s.EndsWith(".jpg") && !s.EndsWith(".gif")) return false;
		return true;
	}
	// The colors associated with the tiers. Specifically colors in a form that can be used in embeds.
	public static Color ColorOfTier(int t)
	{
		switch (t)
		{
			case 0: return Color.Red;
			case 1: return Color.Gold;
			case 2: return Color.Green;
			case 3: return Color.Blue;
			case 4: return Color.Purple;
			case 5: return new Color(0xFFFFFF);
			default: return new Color(0x000000);
		}
	}
	// Tiers are stored as ints, so this turns them into strings.
	public static string LetterOfTier(int t)
	{
		switch (t)
		{
			case 0: return "F";
			case 1: return "D";
			case 2: return "C";
			case 3: return "B";
			case 4: return "A";
			case 5: return "S";
			default: return "?";
		}
	}
	// Same as above, but the other way around.
	public static int TierOfLetter(string s)
	{
		switch (s.ToUpper())
		{
			case "F": return 0;
			case "D": return 1;
			case "C": return 2;
			case "B": return 3;
			case "A": return 4;
			case "S": return 5;
			default: return 404;
		}
	}
	// Warrior classes are stored as chars.
	public static string ClassOfChar(char c)
	{
		switch (c)
		{
			case 's': return "Master";
			case 'i': return "Maid";
			case 'd': return "Demon";
			case 'c': return "Magician";
			case 'g': return "Guardian";
			case 'a': return "Attendant";
			default: return "Unknown Class";
		}
	}
	// Looks through the dictionary of fundamental emotes in Globals.
	public static string UIEmote(string s)
	{
		try {return Globals.UIEmote[s];}
		catch {throw new Exception("Invalid UI emote name: " + s);}
	}
	// Warrior types are stored as ints.
	public static string NameOfType(int t)
	{
		try {return Globals.NameOfType[t];}
		catch {throw new Exception("Invalid type");}
	}
	// Not used yet. Maybe later.
	public static string DescOfType(int t)
	{
		switch (t)
		{
			default: return "Placeholder for type description.";
		}
	}
	// Abilities are stored as ints.
	public static string NameOfAbility(int t)
	{
		try {return Globals.NameOfAbility[t];}
		catch {throw new Exception("Invalid ability");}
	}
	// Constructs a string dynamically with help from a Globals dictionary.
	public static string DescOfAbility(int a, int stren)
	{
		string val = Globals.ValOfAbilityString[(a, stren)];
		switch (a)
		{
			case 1: return val + "% chance to increase card DMG by 20%.";
			case 2: return val + "% chance to increase card HP by 20%.";
			case 3: return val + "% increased card DMG per 10 mana stored.";
			case 4: return val + "% increased card HP per 10 mana stored.";
			case 5: return "Gain " + val + " mana. Mana gained is doubled if the card survives.";
			case 6: return val + "% chance to gain 10 mana.";
			case 7: return "2% chance to increase card HP by " + val + "%.";
			case 8: return val + "% increased point reward at the end of the match.";
			case 9: return val + "% chance to increase point reward at the end of the match by 20%.";
			case 10: return val + "% increased hero card HP.";
			case 11: return val + "% increased hero card DMG.";
			case 12: return val + "% increased DMG for adjacent allies.";
			case 13: return val + "% increased HP for adjacent allies.";
			case 14: return val + "% chance to increase DMG of adjacent allies by 15%.";
			case 15: return val + "% chance to increase HP of adjacent allies by 15%.";
			case 16: return val + "% increased DMG for adjacent allies per 10 mana stored.";
			case 17: return val + "% increased HP for adjacent allies per 10 mana stored.";
			case 18: return val + "% increased stat bonuses from abilities for adjacent allies.";
			case 19: return "Steal " + val + " mana from the opponent.";
			case 20: return "Reduce opponent's mana by " + val + ".";
			case 21: return "2% chance to increase card DMG by " + val + "%.";
			case 22: return val + "% reduced DMG for the enemy card in the same position.";
			case 23: return val + "% reduced HP for the enemy card in the same position.";
			case 24: return val + "% decreased stat bonuses from abilities for the enemy card in the same position.";
			default: return "Placeholder for ability description.";
		}
	}
	// For abilities.
	public static string Roman(int n)
	{
		switch (n)
		{
			case 1: return "I";
			case 2: return "II";
			case 3: return "III";
			case 4: return "IV";
			default: return "?";
		}
	}
	public static string AbilityAndStrength(int a, int s)
	{
		return NameOfAbility(a) + " " + Roman(s);
	}
	// The age of a card relative to all other cards of the same warrior.
	public static string AgeRating(int beforecount, int aftercount, int beforeliving)
	{
		if (beforeliving == 0) return "(Eldest)";
		double perc = (double)aftercount / (double)(beforecount + aftercount);
		if (perc <= 0.01) return "(Mint)";
		else if (perc <= 0.1) return "(Brand New)";
		else if (perc <= 0.5) return "(New)";
		else if (perc <= 0.9) return "(Old)";
		else if (perc <= 0.99) return "(Ancient)";
		else return "(Primordial)";
	}
	// Makes sure that a string will display with some limit on width by adding newlines.
	// Added to enforce a particular format on the card viewer since format gets unpredicable when inline embed fields have unpredictable character width.
	// It's probably used for something else too, by now. Pretty handy.
	public static string WrapToMaxWidth(string s, int maxchars)
	{
		string[] words = s.Split(' ');
		System.Text.StringBuilder newSentence = new System.Text.StringBuilder();
		string line = "";
		foreach (string word in words)
		{
			if ((line + word).Length > maxchars)
			{
				newSentence.AppendLine(line);
				line = "";
			}
			line += string.Format("{0} ", word);
		}
		if (line.Length > 0)
			newSentence.AppendLine(line);
		return newSentence.ToString();
	}
	// Hand position is stored as a pair of coordinate ints.
	public static int ColumnOfLetter(char s)
	{
		switch (Char.ToUpper(s))
		{
			case 'A': return 0;
			case 'B': return 1;
			case 'C': return 2;
			case 'D': return 3;
			case 'F': return 4;
			default: throw new Exception();
		}
	}
	public static string LetterOfColumn(int s)
	{
		switch (s)
		{
			case 0: return "A";
			case 1: return "B";
			case 2: return "C";
			case 3: return "D";
			case 4: return "F";
			default: throw new Exception();
		}
	}
	// You need to ask the server to get someone's nickname, so this does it for you.
	public static string NickOrReal(ulong id, SocketGuild s)
	{
		try { var r = s.GetUser(id).Nickname; if (r == null) throw new Exception(); return r;}
		catch {return s.GetUser(id).ToString();}
	}
	// Each warrior class has a stat modifier.
	public static Single HpMultOfClass(char c)
	{
		switch(c)
		{
			case 's': return 1f;
			case 'i': return 1.2f;
			case 'd': return 1f;
			case 'c': return 1.2f;
			case 'g': return 1.6f;
			case 'a': return 1.4f;
			default: throw new Exception("Invalid class");
		}
	}
	public static Single DmgMultOfClass(char c)
	{
		switch(c)
		{
			case 's': return 1.2f;
			case 'i': return 1.2f;
			case 'd': return 1.6f;
			case 'c': return 1.4f;
			case 'g': return 1f;
			case 'a': return 1.2f;
			default: throw new Exception("Invalid class");
		}
	}
	// Tiers and levels modify stats.
	public static Single MultOfTier(int t)
	{
		switch(t)
		{
			case 0: return 1f;
			case 1: return 1.25f;
			case 2: return 1.5f;
			case 3: return 1.75f;
			case 4: return 2f;
			case 5: return 2f;
			default: throw new Exception("Invalid tier");
		}
	}
	public static Single MultOfLevel(int l)
	{
		return 1f + (Convert.ToSingle(l - 1) / 20f);
	}
	// Retrieves the specific numbers you need to model the effect of an ability.
	// Sometimes ints, sometimes floats.
	public static int ValFromAbilityInt(int aid, int stren)
	{
		try {return Globals.ValOfAbilityInt[(aid, stren)];}
		catch {throw new Exception();}
	}
	public static Single ValFromAbilityFloat(int aid, int stren)
	{
		try {return Globals.ValOfAbilityFloat[(aid, stren)];}
		catch {throw new Exception();}
	}
	public static string ValOfAbilityString(int aid, int stren)
	{
		try {return Globals.ValOfAbilityString[(aid, stren)];}
		catch {throw new Exception();}
	}
	/*
	
	CLASSES
	
	*/
	// For keeping a bunch of info and specialized job groups tidy.
	public class Globals
	{
		// Used to direct interactive reactions to the objects that they apply to.
		// Also keeps those objects in scope until they remove themselves from the dictionary.
		public static Dictionary<ulong, MultiPageMessage> multipagemessages;
		public static Dictionary<ulong, RollPost> rollposts;
		public static RollingSystem rollingsystem;
		public static CardGenerator cardgenerator;
		// Arbitrary int value that every card starts with.
		// Every time you see HP or DMG displayed as an int, it's just this multiplied by several floats then rounded.
		public static int BaseHp;
		public static int BaseDmg;
		// Limits for tiers and levels.
		public static int MinTier;
		public static int MaxTier;
		public static int MinLevel;
		public static int MaxLevel;
		public static Dictionary<int, string> NameOfType;
		public static Dictionary<int, string> NameOfAbility;
		public static Dictionary<(int, int), float> ValOfAbilityFloat;
		public static Dictionary<(int, int), int> ValOfAbilityInt;
		public static Dictionary<(int, int), string> ValOfAbilityString;
		public static Dictionary<string, string> UIEmote;
		// User IDs with the authority for admin commands.
		public static ulong[] gameadmins;
		public static void Initialize()
		{
			multipagemessages = new Dictionary<ulong, MultiPageMessage>();
			rollposts = new Dictionary<ulong, RollPost>();
			NameOfType = new Dictionary<int, string> {{1, "Addict"}, {2, "Anti-hero"}, {3, "Apathetic"}, {4, "Beast"}, {5, "Big Brother"}, {6, "Big Sister"}, {7, "Boudere"}, {8, "Bully"}, {9, "Childhood Friend"}, {10, "Childish"},
			{11, "Chuuni"}, {12, "Clumsy"}, {13, "Cold"}, {14, "Cool Beauty"}, {15, "Coward"}, {16, "Crossdresser"}, {17, "Cynic"}, {18, "Delinquent"}, {19, "Dense"}, {20, "Depressed"},
			{21, "Foreigner"}, {22, "Genki"}, {23, "Glutton"}, {24, "God Complex"}, {25, "Gold Digger"}, {26, "Idiot"}, {27, "Incompetent"}, {28, "Kemonomimi"}, {29, "Kind"}, {30, "Idol"},
			{31, "Little Brother"}, {32, "Little Sister"}, {33, "Loli"}, {34, "Lovey-Dovey"}, {35, "Magical Girl"}, {36, "Manly"}, {37, "NEET"}, {38, "Neko"}, {39, "Noble"}, {40, "Ojisan"},
			{41, "Old Fashioned"}, {42, "Otaku"}, {43, "Outcast"}, {44, "Pervert"}, {45, "Princess"}, {46, "Professor"}, {47, "Psychopath"}, {48, "Quiet"}, {49, "Sadist"}, {50, "Servant"},
			{51, "Sexy"}, {52, "Shy"}, {53, "Sibling"}, {54, "Timeless"}, {55, "Tomboy"}, {56, "Tsundere"}, {57, "Tsunpure"}, {58, "Two-faced"}, {59, "Weak"}, {60, "Yandere"}};
			NameOfAbility = new Dictionary<int, string> {{1, "Critical"}, {2, "Adrenaline"}, {3, "Mana Strengthen"}, {4, "Mana Vigor"}, {5, "Mana Boost"}, {6, "Mana Surge"}, {7, "Invincibility"}, {8, "Invest"},
			{9, "Gamble"}, {10, "Shield's Blessing"}, {11, "Might's Blessing"}, {12, "Precision"}, {13, "Vitality"}, {14, "Rally Critical"}, {15, "Rally Adrenaline"}, {16, "Mana Empower"},
			{17, "Mana Heal"}, {18, "Boost"}, {19, "Mana Drain"}, {20, "Mana Burst"}, {21, "Execution"}, {22, "Fear"}, {23, "Weaken"}, {24, "Negation"}};
			BaseHp = 100;
			BaseDmg = 100;
			MinTier = 0;
			MaxTier = 5;
			MinLevel = 1;
			MaxLevel = 20;
			ValOfAbilityFloat = new Dictionary<(int, int), float> {{(1, 1), 0.16f}, {(1, 2), 0.24f}, {(1, 3), 0.32f}, {(1, 4), 0.4f}, {(2, 1), 0.16f}, {(2, 2), 0.24f}, {(2, 3), 0.32f}, {(2, 4), 0.4f},
			{(3, 1), 0.005f}, {(3, 2), 0.01f}, {(3, 3), 0.015f}, {(3, 4), 0.02f}, {(4, 1), 0.005f}, {(4, 2), 0.01f}, {(4, 3), 0.015f}, {(4, 4), 0.02f},
			{(6, 1), 0.08f}, {(6, 2), 0.12f}, {(6, 3), 0.16f}, {(6, 4), 0.2f}, {(7, 1), 0.4f}, {(7, 2), 0.5f}, {(7, 3), 0.6f}, {(7, 4), 0.7f},
			{(8, 1), 0.04f}, {(8, 2), 0.06f}, {(8, 3), 0.08f}, {(8, 4), 0.1f}, {(9, 1), 0.12f}, {(9, 2), 0.18f}, {(9, 3), 0.24f}, {(9, 4), 0.3f},
			{(10, 1), 0.06f}, {(10, 2), 0.09f}, {(10, 3), 0.12f}, {(10, 4), 0.15f}, {(11, 1), 0.06f}, {(11, 2), 0.09f}, {(11, 3), 0.12f}, {(11, 4), 0.15f},
			{(12, 1), 0.04f}, {(12, 2), 0.06f}, {(12, 3), 0.08f}, {(12, 4), 0.1f}, {(13, 1), 0.04f}, {(13, 2), 0.06f}, {(13, 3), 0.08f}, {(13, 4), 0.1f},
			{(14, 1), 0.16f}, {(14, 2), 0.24f}, {(14, 3), 0.32f}, {(14, 4), 0.4f}, {(15, 1), 0.16f}, {(15, 2), 0.24f}, {(15, 3), 0.32f}, {(15, 4), 0.4f},
			{(16, 1), 0.003f}, {(16, 2), 0.006f}, {(16, 3), 0.009f}, {(16, 4), 0.012f}, {(17, 1), 0.003f}, {(17, 2), 0.006f}, {(17, 3), 0.009f}, {(17, 4), 0.012f},
			{(18, 1), 0.5f}, {(18, 2), 1f}, {(18, 3), 1.5f}, {(18, 4), 2f}, {(21, 1), 0.4f}, {(21, 2), 0.5f}, {(21, 3), 0.6f}, {(21, 4), 0.7f},
			{(22, 1), 0.06f}, {(22, 2), 0.09f}, {(22, 3), 0.12f}, {(22, 4), 0.15f}, {(23, 1), 0.06f}, {(23, 2), 0.09f}, {(23, 3), 0.12f}, {(23, 4), 0.15f},
			{(24, 1), 0.25f}, {(24, 2), 0.5f}, {(24, 3), 0.75f}, {(24, 4), 1f}};
			ValOfAbilityInt = new Dictionary<(int, int), int> {{(5, 1), 2}, {(5, 2), 3}, {(5, 3), 4}, {(5, 4), 5}, {(19, 1), 1}, {(19, 2), 2}, {(19, 3), 3}, {(19, 4), 4},
			{(20, 1), 2}, {(20, 2), 3}, {(20, 3), 4}, {(20, 4), 5}};
			ValOfAbilityString = new Dictionary<(int, int), string> {{(1, 1), "16"}, {(1, 2), "24"}, {(1, 3), "32"}, {(1, 4), "40"}, {(2, 1), "16"}, {(2, 2), "24"}, {(2, 3), "32"}, {(2, 4), "40"},
			{(3, 1), "0.5"}, {(3, 2), "1"}, {(3, 3), "1.5"}, {(3, 4), "2"}, {(4, 1), "0.5"}, {(4, 2), "1"}, {(4, 3), "1.5"}, {(4, 4), "2"},
			{(5, 1), "2"}, {(5, 2), "3"}, {(5, 3), "4"}, {(5, 4), "5"},
			{(6, 1), "8"}, {(6, 2), "12"}, {(6, 3), "16"}, {(6, 4), "20"}, {(7, 1), "40"}, {(7, 2), "50"}, {(7, 3), "60"}, {(7, 4), "70"},
			{(8, 1), "4"}, {(8, 2), "6"}, {(8, 3), "8"}, {(8, 4), "10"}, {(9, 1), "12"}, {(9, 2), "18"}, {(9, 3), "24"}, {(9, 4), "30"},
			{(10, 1), "6"}, {(10, 2), "9"}, {(10, 3), "12"}, {(10, 4), "15"}, {(11, 1), "6"}, {(11, 2), "9"}, {(11, 3), "12"}, {(11, 4), "15"},
			{(12, 1), "4"}, {(12, 2), "6"}, {(12, 3), "8"}, {(12, 4), "10"}, {(13, 1), "4"}, {(13, 2), "6"}, {(13, 3), "8"}, {(13, 4), "10"},
			{(14, 1), "16"}, {(14, 2), "24"}, {(14, 3), "32"}, {(14, 4), "40"}, {(15, 1), "16"}, {(15, 2), "24"}, {(15, 3), "32"}, {(15, 4), "40"},
			{(16, 1), "0.3"}, {(16, 2), "0.6"}, {(16, 3), "0.9"}, {(16, 4), "1.2"}, {(17, 1), "0.3"}, {(17, 2), "0.6"}, {(17, 3), "0.9"}, {(17, 4), "1.2"},
			{(18, 1), "50"}, {(18, 2), "100"}, {(18, 3), "150"}, {(18, 4), "200"}, {(19, 1), "1"}, {(19, 2), "2"}, {(19, 3), "3"}, {(19, 4), "4"},
			{(20, 1), "2"}, {(20, 2), "3"}, {(20, 3), "4"}, {(20, 4), "5"}, {(21, 1), "40"}, {(21, 2), "50"}, {(21, 3), "60"}, {(21, 4), "70"},
			{(22, 1), "6"}, {(22, 2), "9"}, {(22, 3), "12"}, {(22, 4), "15"}, {(23, 1), "6"}, {(23, 2), "9"}, {(23, 3), "12"}, {(23, 4), "15"},
			{(24, 1), "25"}, {(24, 2), "50"}, {(24, 3), "75"}, {(24, 4), "100"}};
			UIEmote = new Dictionary<string, string> {{"f1", "<:F1:620796170695999509>"}, {"f2", "<:F2:620796170830217236>"}, {"f3", "<:F3:620796170427564043>"}, {"f4", "<:F4:620796170796531742>"},
			{"f5", "<:F5:620796170771365889>"}, {"f6", "<:F6:620796170679353345>"}, {"f7", "<:F7:620796170654056461>"}, {"f8", "<:F8:620796170804920359>"}, {"f9", "<:F9:620796170855383040>"},
			{"f10", "<:F10:620796170637410325>"}, {"f11", "<:F11:620796171006378014>"}, {"f12", "<:F12:620796171287265280>"}, {"f13", "<:F13:620796170997858304>"}, {"f14", "<:F14:620796171010441217>"},
			{"f15", "<:F15:620796171043995698>"}, {"f16", "<:F16:620796171215962112>"}, {"f17", "<:F17:620796171094589450>"}, {"f18", "<:F18:620796171375345664>"}, {"f19", "<:F19:620796171115560960>"}, 
			{"f20", "<:F20:620796170746331138>"},
			{"d1", "<:D1:620796131709812797>"}, {"d2", "<:D2:620796131949019159>"}, {"d3", "<:D3:620796131823321088>"}, {"d4", "<:D4:620796131932373002>"},
			{"d5", "<:D5:620796131655548950>"}, {"d6", "<:D6:620796131860938752>"}, {"d7", "<:D7:620796131907076116>"}, {"d8", "<:D8:620796131965927434>"}, {"d9", "<:D9:620796131949019176>"},
			{"d10", "<:D10:620796132007739402>"}, {"d11", "<:D11:620796132137762854>"}, {"d12", "<:D12:620796132162928658>"}, {"d13", "<:D13:620796132062265365>"}, {"d14", "<:D14:620796132058071042>"},
			{"d15", "<:D15:620796132574101534>"}, {"d16", "<:D16:620796132607393814>"}, {"d17", "<:D17:620796132649467934>"}, {"d18", "<:D18:620796134474121226>"}, {"d19", "<:D19:620796134314475581>"}, 
			{"d20", "<:D20:620796134398361630>"},
			{"c1", "<:C1:620796051674365973>"}, {"c2", "<:C2:620796051892469770>"}, {"c3", "<:C3:620796052001259550>"}, {"c4", "<:C4:620796051913310223>"},
			{"c5", "<:C5:620796051904790538>"}, {"c6", "<:C6:620796051888144394>"}, {"c7", "<:C7:620796051921567769>"}, {"c8", "<:C8:620796051917373440>"}, {"c9", "<:C9:620796051724697631>"},
			{"c10", "<:C10:620796052030750730>"}, {"c11", "<:C11:620796096716734485>"}, {"c12", "<:C12:620796094967971862>"}, {"c13", "<:C13:620796094980423681>"}, {"c14", "<:C14:620796095307579392>"},
			{"c15", "<:C15:620796095492128814>"}, {"c16", "<:C16:620796095441666048>"}, {"c17", "<:C17:620796095513231360>"}, {"c18", "<:C18:620796095529877511>"}, {"c19", "<:C19:620796095521357863>"}, 
			{"c20", "<:C20:620796095584272385>"},
			{"b1", "<:B1:620796021525446686>"}, {"b2", "<:B2:620796020829323277>"}, {"b3", "<:B3:620796021231976459>"}, {"b4", "<:B4:620796021219524623>"},
			{"b5", "<:B5:620796021198422016>"}, {"b6", "<:B6:620796021181644820>"}, {"b7", "<:B7:620796021357805578>"}, {"b8", "<:B8:620796021420589128>"}, {"b9", "<:B9:620796021517189140>"},
			{"b10", "<:B10:620796021328314417>"}, {"b11", "<:B11:620796021395423234>"}, {"b12", "<:B12:620796021198290961>"}, {"b13", "<:B13:620796021219524614>"}, {"b14", "<:B14:620796021298954253>"},
			{"b15", "<:B15:620796021475246080>"}, {"b16", "<:B16:620796021433303042>"}, {"b17", "<:B17:620796021194096680>"}, {"b18", "<:B18:620796021529772032>"}, {"b19", "<:B19:620796021517320192>"}, 
			{"b20", "<:B20:620796021600944128>"},
			{"a1", "<:A1:620795994799341601>"}, {"a2", "<:A2:620795995135148065>"}, {"a3", "<:A3:620795994703003654>"}, {"a4", "<:A4:620795994941947915>"},
			{"a5", "<:A5:620795994895941663>"}, {"a6", "<:A6:620795995181154315>"}, {"a7", "<:A7:620795995222966272>"}, {"a8", "<:A8:620795994958725131>"}, {"a9", "<:A9:620795995219034112>"},
			{"a10", "<:A10:620795995336474624>"}, {"a11", "<:A11:620795995294400512>"}, {"a12", "<:A12:620795995332018227>"}, {"a13", "<:A13:620795995382349837>"}, {"a14", "<:A14:620795995025965067>"},
			{"a15", "<:A15:620795995428618250>"}, {"a16", "<:A16:620795996774858782>"}, {"a17", "<:A17:620795995311177761>"}, {"a18", "<:A18:620795997030973441>"}, {"a19", "<:A19:620795995516829726>"}, 
			{"a20", "<:A20:620795996921921537>"},
			{"s1", "<a:S1:622917268078198817>"}, {"s2", "<a:S2:622917268313079808>"}, {"s3", "<a:S3:622917267763626009>"}, {"s4", "<a:S4:622917269130969129>"},
			{"s5", "<a:S5:622917268698824714>"}, {"s6", "<a:S6:622917268132724742>"}, {"s7", "<a:S7:622917267889455125>"}, {"s8", "<a:S8:622917268258684948>"}, {"s9", "<a:S9:622917268166279179>"},
			{"s10", "<a:S10:622917268233388035>"}, {"s11", "<a:S11:622917268107689995>"}, {"s12", "<a:S12:622917268212285482>"}, {"s13", "<a:S13:622917268757544970>"}, {"s14", "<a:S14:622917268438777875>"},
			{"s15", "<a:S15:622917271471521811>"}, {"s16", "<a:S16:622917268283850773>"}, {"s17", "<a:S17:622917268292239370>"}, {"s18", "<a:S18:622917268413612053>"}, {"s19", "<a:S19:622917268342308884>"}, 
			{"s20", "<a:S20:622917482021126205>"},
			{"row0reg", "<:1Regular:622942672734126081>"}, {"row1reg", "<:2regular:622942672432267284>"}, {"row2reg", "<:3Regular:622942672235003931>"}, {"col0reg", "<:ARegular:622942672373415948>"},
			{"col1reg", "<:BRegular:622942672633593857>"}, {"col2reg", "<:CRegular:622942672784457758>"}, {"col3reg", "<:DRegular:622942672428204033>"}, {"col4reg", "<:FRegular:622942672398712853>"},
			{"row0hero", "<:1Hero:622943501549568011>"}, {"row1hero", "<:2Hero:622943501801488384>"}, {"row2hero", "<:3Hero:622943501776322610>"}, {"col0hero", "<:AHero:622943501771866122>"},
			{"col1hero", "<:BHero:622943501935575065>"}, {"col2hero", "<:CHero:622943501587447851>"}, {"col3hero", "<:DHero:622943502208073749>"}, {"col4hero", "<:FHero:622943501583384628>"},
			{"tier0", "<:STier:622919172980211733>"}, {"tier1", "<:ATier:622919172879286315>"}, {"tier2", "<:BTier:622919172816371712>"}, {"tier3", "<:CTier:622919172845731855>"},
			{"tier4", "<:DTier:622919173106040862>"}, {"tier5", "<:FTier:622919172917297162>"},
			{"smalltier0", "<:STiers:634528877103808563>"}, {"smalltier1", "<:ATiers:634528877015990302>"}, {"smalltier2", "<:BTiers:634528876714000407>"}, {"smalltier3", "<:CTiers:634528877145751552>"},
			{"smalltier4", "<:DTiers:634528877061996575>"}, {"smalltier5", "<:FTiers:634528876835504130>"},
			{"classs", "<:Master:622924580100767744>"}, {"classi", "<:Maid:622924579928539176>"}, {"classc", "<:Magician:622924580184522763>"}, {"classg", "<:Guardian:622924579987521576>"}, 
			{"classd", "<:Demon:622924579660234825>"}, {"classa", "<:Attendant:622924579740057621>"},
			{"nocard", "<:NoCard:620801512444264459>"}, {"lvltier", "<:LVLTier:622933814133522443>"}, {"handindicator", "<:HandIndicator:620785188842110977>"}, {"enemy", "<a:Enemy:620855156766146571>"},
			{"deckview", "<:DeckView:622933814137847808>"}, {"class", "<:Class:622933814129459210>"}, {"cardpos", "<:CardPOS:622933814045442068>"}, {"card", "<:Card:622933814452158494>"},
			{"blankslot", "<:BlankSlot:622938654884495380>"} };
			gameadmins = new ulong[] {263109548640960514, 84014499543154688, 189520407370530816};
		}
	}
	// I don't know why, but you have to use this for the random numbers to work right.
	public class RandomHolder
	{
		private static Random _instance;
		public static Random Instance
		{
			get { return _instance ?? (_instance = new Random()); }
		}
	}
	public class RollingSystem
	{
		// For keeping track of how many rolls have been used up. It assumes all of them are left if it can't find a number here.
		// Dictionary<server id, Dictionary<user id, number of rolls used>>
		public Dictionary<ulong, Dictionary<ulong, int>> rolldeductions;
		// For keeping track of who can't claim yet. It assumes you can if it can't find you here.
		// Dictionary<server id, Hashset<user id>>
		public Dictionary<ulong, HashSet<ulong>> usedclaims;
		// For keeping track of which channels are owed more rollposts.
		public Dictionary<SocketTextChannel, int> rollqueues;
		// For directing more roll requests to the loop for that channel, in case one is already going.
		public Dictionary<SocketTextChannel, Task> rollingloops;
		DiscordSocketClient client;
		public RollingSystem(DiscordSocketClient c)
		{
			rolldeductions = new Dictionary<ulong, Dictionary<ulong, int>>();
			usedclaims = new Dictionary<ulong, HashSet<ulong>>();
			rollqueues = new Dictionary<SocketTextChannel, int>();
			rollingloops = new Dictionary<SocketTextChannel, Task>();
			client = c;
			ResetCycle();
		}
		// Resets rolls and claims on a schedule.
		public async Task ResetCycle()
		{
			HashSet<ulong> checkclaims;
			Dictionary<ulong, int> intervalofserver;
			DateTime trigger;
			var lastmin = DateTime.Now.Minute;
			// Why does it go every minute? Shouldn't it be every hour, since that's the shortest a cycle can be?
			// That would be easier, but if every server reset at the same time every hour, the rolls at that time would put a huge strain on the bot.
			// Instead I distributed the resets across every minute of the hour.
			// The timing is based on the number of minutes after the hour that a server was created, which is both random in origin and consistent within a server.
			while (true)
			{
				if (DateTime.Now.Minute != lastmin)
				{
					// The exact mechanism of the reset is simply to delete some roll/claim restrictions from the dictionary.
					checkclaims = new HashSet<ulong>();
					intervalofserver = new Dictionary<ulong, int>();
					foreach (SocketGuild s in client.Guilds)
					{
						if (s.CreatedAt.Minute == DateTime.Now.Minute)
						{
							rolldeductions.Remove(s.Id);
							checkclaims.Add(s.Id);
						}
					}
					// Placeholder. Eventually it'll need to ask the database what a particular server's interval is.
					foreach (ulong s in checkclaims) intervalofserver.Add(s, 1);
					foreach (ulong s in checkclaims)
					{
						if (DateTime.Now.Hour % intervalofserver[s] == 0)
						{
							usedclaims.Remove(s);
						}
					}
					lastmin = DateTime.Now.Minute;
				}
				// Waiting until EXACTLY the next minute isn't possible, if it tries it'll be a little off.
				// I think it's still accurate to 1 or 2 hundredths of a second.
				trigger = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
				DateTime.Now.Hour, DateTime.Now.Minute, 0, 10);
				trigger = trigger.AddMinutes(1);
				await Task.Delay(trigger - DateTime.Now);
			}
		}
		public int RollsLeft(ulong s, ulong u)
		{
			// Placeholder, calculate this from the database later
			int maxperhour = 10;
			if (rolldeductions.ContainsKey(s))
			{
				if (rolldeductions[s].ContainsKey(u))
				{
					return Math.Max(0, maxperhour - rolldeductions[s][u]);
				}
			}
			return maxperhour;
		}
		public Boolean ClaimLeft(ulong s, ulong u)
		{
			if (usedclaims.ContainsKey(s))
			{
				if (usedclaims[s].Contains(u)) return false;
			}
			return true;
		}
		public TimeSpan TimeToClaimReset(IGuild s)
		{
			// Placeholder
			int interval = 1;
			var time = DateTime.Today.AddMinutes(s.CreatedAt.Minute);
			for (int i=0;i<25;i++)
			{
				if (DateTime.Now < time) return time - DateTime.Now;
				time = time.AddHours(interval);
			}
			throw new Exception("Claim reset estimate error");
		}
		public TimeSpan TimeToRollReset(IGuild s)
		{
			int interval = 1;
			var time = DateTime.Today.AddMinutes(s.CreatedAt.Minute);
			for (int i=0;i<25;i++)
			{
				if (DateTime.Now < time) return time - DateTime.Now;
				time = time.AddHours(interval);
			}
			throw new Exception("Roll reset estimate error");
		}
		public string ClaimLeftString(IGuild s, IUser u)
		{
			if (ClaimLeft(s.Id, u.Id)) return u.Mention + " You can claim immediately."; 
			var span = TimeToClaimReset(s);
			var minutes = span.Minutes + 1;
			var hours = span.Hours;
			var m = u.Mention + " You can't claim again for another ";
			if (hours == 1) m += "1 hour and ";
			else if (hours > 1) m += hours.ToString() + " hours and ";
			if (minutes == 1) m += "1 minute.";
			else m += minutes.ToString() + " minutes.";
			return m;
		}
		public string RollsLeftString(IGuild s, IUser u)
		{
			var left = RollsLeft(s.Id, u.Id);
			if (left == 0)
			{
				string m = u.Mention + " You're out of rolls for the next ";
				var mins = TimeToRollReset(s).Minutes + 1;
				if (mins == 1) m += "1 minute.";
				else m += mins.ToString() + " minutes.";
				return m;
			}
			return u.Mention + " You have " + left + " rolls left.";
		}
		// The task of rolling multiple times needs to be kept track of in some centralized place since many people can contribute to it.
		public async Task RollingLoop(SocketTextChannel ch)
		{
			while (rollqueues[ch] > 0)
			{
				await RollPost.CreateAsync(ch);
				rollqueues[ch] --;
				// Wait for an amount of time depending on the queue size.
				if (rollqueues[ch] > 60) await Task.Delay(1500);
				else await Task.Delay(2000);
			}
			//Finally, remove the task from scope/access right as it finishes.
			rollingloops.Remove(ch);
		}
	}
	// I figured this code should be in a central location so that rollposts, slot machines, and booster packs can all access it.
	public class CardGenerator
	{
		public CardGenerator()
		{

		}
		// This is what actually creates the card.
		public async Task<int> GiveCard(int wid, int tier, int level, ulong sid, ulong uid)
		{
			int cid;
			using (var c = Conn())
			{
				await c.OpenAsync();
				var trans = c.BeginTransaction();
				try {
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "INSERT INTO card (tier, level) VALUES (@t, @l) RETURNING cid;";
					cmd.Parameters.AddWithValue("t", tier);
					cmd.Parameters.AddWithValue("l", level);
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						await dr.ReadAsync();
						cid = dr.GetInt32(0);
					}
				}
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "INSERT INTO is_warrior (cid, wid) VALUES (@cid, @wid);" +
					// Add server and user to db if not there
					" INSERT INTO wuser (uid) VALUES (@uid) ON CONFLICT DO NOTHING; INSERT INTO server (sid) VALUES (@sid) ON CONFLICT DO NOTHING;" +
					" INSERT INTO owns_card (uid, sid, cid) VALUES (@uid, @sid, @cid);";
					cmd.Parameters.AddWithValue("cid", cid);
					cmd.Parameters.AddWithValue("wid", wid);
					cmd.Parameters.AddWithValue("uid", uid.ToString());
					cmd.Parameters.AddWithValue("sid", sid.ToString());
					await cmd.ExecuteNonQueryAsync();
				}
				}
				catch (Exception e)
				{
					await trans.RollbackAsync();
					throw new Exception("Card creation error: " + e.Message);
				}
				await trans.CommitAsync();
			}
			return cid;
		}
		// For selecting a random warrior to roll (when the tier is already decided)
		public async Task<int> RandWidOfTier(int tier)
		{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT wid FROM warrior WHERE starttier=@t ORDER BY RANDOM() LIMIT 1;";
					cmd.Parameters.AddWithValue("t", tier);
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (!dr.HasRows)
						{
							throw new Exception("No warrior found of tier " + tier);
						}
						await dr.ReadAsync();
						return dr.GetInt32(0);
					}
				}
			}
		}
		public int RandTier()
		{
			double temp = RandomHolder.Instance.NextDouble();
			if (temp <= 0.6) return 0;
			else if (temp <= 0.85) return 1;
			else if (temp <= 0.96) return 2;
			else if (temp <= 0.99) return 3;
			else return 4;
		}
		// Rare nonstandard tiers.
		public int RandTierMod()
		{
			double temp = RandomHolder.Instance.NextDouble();
			if (temp <= (double) 4965/5000) return 0;
			else if (temp <= (double) 4990/5000) return 1;
			else if (temp <= (double) 4995/5000) return 2;
			else if (temp <= (double) 4999/5000) return -1;
			else return 3;
		}
		// You can't have tiers worse than F or better than S.
		public int LimitTierMod(int tier, int mod)
		{
			while (tier + mod > Globals.MaxTier) {mod--;}
			while (tier + mod < Globals.MinTier) {mod++;}
			return mod;
		}
		public int RandLevel()
		{
			double temp = RandomHolder.Instance.NextDouble();
			if (temp <= (double) 4485/5000) return 1;
			else if (temp <= (double) 4785/5000) return 2;
			else if (temp <= (double) 4885/5000) return 3;
			else if (temp <= (double) 4935/5000) return 4;
			else if (temp <= (double) 4960/5000) return 5;
			else if (temp <= (double) 4965/5000) return 6;
			else if (temp <= (double) 4970/5000) return 7;
			else if (temp <= (double) 4975/5000) return 8;
			else if (temp <= (double) 4980/5000) return 9;
			else if (temp <= (double) 4985/5000) return 10;
			else if (temp <= (double) 4987/5000) return 11;
			else if (temp <= (double) 4989/5000) return 12;
			else if (temp <= (double) 4991/5000) return 13;
			else if (temp <= (double) 4993/5000) return 14;
			else if (temp <= (double) 4995/5000) return 15;
			else if (temp <= (double) 4996/5000) return 16;
			else if (temp <= (double) 4997/5000) return 17;
			else if (temp <= (double) 4998/5000) return 18;
			else if (temp <= (double) 4999/5000) return 19;
			else return 20;
		}
	}
	// This represents the thing that pops up when you roll.
	// There's a lot of interactivity and logic behind it, so it needs its own class.
	public class RollPost
	{
		SimWarrior warrior;
		int tiermod;
		int level;
		string claimemote;
		string expiredemote;
		string claimertitle;
		public Discord.Rest.RestUserMessage msg;
		private RollPost()
		{
			claimemote = "\U0001F496";
			expiredemote = "\U000026D4";
			claimertitle = "Claimers";
		}
		private async Task<RollPost> InitializeAsync(SocketTextChannel channel)
		{
			warrior = await SimWarrior.CreateAsync(await Globals.cardgenerator.RandWidOfTier(Globals.cardgenerator.RandTier()));
			tiermod = Globals.cardgenerator.LimitTierMod(warrior.starttier, Globals.cardgenerator.RandTierMod());
			level = Globals.cardgenerator.RandLevel();
			try
			{
				var builder = new EmbedBuilder();
				builder.WithAuthor(warrior.wname + " (#" + warrior.wid + ")");
				builder.WithImageUrl(warrior.portraitsfree[0]);
				var desc = warrior.SeriesStr() + "\nLevel: **" + level + "** | Tier: **" +
				LetterOfTier(warrior.starttier + tiermod) + "** | Class: **" + ClassOfChar(warrior.wclass) + "**\n";
				if (tiermod != 0)
				{
					if (tiermod > 0) desc += "**+" + tiermod + " TIER!** ";
					else desc += "**" + tiermod + " TIER!** ";
				}
				if (level > 1) desc += "**+" + (level - 1) + " LEVEL!**";
				builder.WithDescription(desc);
				builder.WithColor(ColorOfTier(warrior.starttier + tiermod));
				msg = await channel.SendMessageAsync(embed: builder.Build());
				await msg.AddReactionAsync(new Emoji(claimemote));
				Globals.rollposts.Add(msg.Id, this);
				DetermineWinner();
			}
			catch (Exception e) {Console.WriteLine("Roll init error: " + e.Message); return this;}
			return this;
		}
		public static async Task<RollPost> CreateAsync(SocketTextChannel channel)
		{
			var ret = new RollPost();
			return await ret.InitializeAsync(channel);
		}
		public async Task DetermineWinner()
		{
			await Task.Delay(1000 * 30);
			// Detecting reactions in real time is unreliable at best, so this only looks at them after time is up.
			await msg.AddReactionAsync(new Emoji(expiredemote));
			List<IUser> claimers = (await msg.GetReactionUsersAsync(new Emoji(claimemote), 150).FlattenAsync()).ToList();
			// Make sure the claim is valid.
			claimers.RemoveAll(u => u.Id == msg.Author.Id);
			claimers.RemoveAll(u => u.IsBot);
			claimers.RemoveAll(u => !Globals.rollingsystem.ClaimLeft((msg.Channel as IGuildChannel).Guild.Id, u.Id));
			IUser winner;
			// Don't waste any more time on cards that nobody claimed.
			if (claimers.Count() < 1)
			{
				Globals.rollposts.Remove(msg.Id);
				return;
			}
			else if (claimers.Count() == 1) winner = claimers[0];
			else winner = claimers[RandomHolder.Instance.Next(claimers.Count())];
			int cid = await Globals.cardgenerator.GiveCard(warrior.wid, (warrior.starttier + tiermod), level, (msg.Channel as IGuildChannel).GuildId, winner.Id);
			// Register the claim so that no more can be made until the reset.
			if (!Globals.rollingsystem.usedclaims.ContainsKey((msg.Channel as IGuildChannel).GuildId))
			{
				Globals.rollingsystem.usedclaims.Add((msg.Channel as IGuildChannel).GuildId, new HashSet<ulong>());
			}
			Globals.rollingsystem.usedclaims[(msg.Channel as IGuildChannel).GuildId].Add(winner.Id);
			string m = "**" + NickOrReal(winner.Id, (msg.Channel as SocketGuildChannel).Guild) + "** has claimed the **" + warrior.wname + "** card";
			if (claimers.Count == 1) m += " uncontested!";
			else
			{
				claimers.Remove(winner);
				m += ", besting: ";
				foreach (IUser u in claimers) m += "**" + NickOrReal(u.Id, (msg.Channel as SocketGuildChannel).Guild) + "**, ";
				m = m.Substring(0, m.Length - 2);
			}
			m += "\nCard serial number: **" + cid + "**";
			await msg.Channel.SendMessageAsync(m);
			// Take the object out of scope and out of the global dictionary so that it doesn't get bothered with reactions any more.
			Globals.rollposts.Remove(msg.Id);
		}
		public async Task OnReact(IEmote e, ulong uid)
		{
			// Only listen to claims.
			if (e.Name != claimemote) return;
			var s = (msg.Channel as IGuildChannel).Guild;
			var u = await s.GetUserAsync(uid);
			// As a courtesy, tell people when they aren't allowed to claim.
			try
			{
				if (!Globals.rollingsystem.ClaimLeft((msg.Channel as IGuildChannel).Guild.Id, uid))
				{
					await msg.Channel.SendMessageAsync(Globals.rollingsystem.ClaimLeftString(s, u as IUser));
					return;
				}
			}
			catch (Exception x) {Console.WriteLine("Overclaim notification error: " + x.Message); return;}
			// Displays a list of claimers. This might be what makes the bot unstable sometimes.
			// Also it misses people a lot. As stated above, real time reactions are NOT reliable.
			// Mix those with real time post edits and you've got a flickering, laggy mess.
			var b = msg.Embeds.First().ToEmbedBuilder();
			EmbedFieldBuilder field = null;
			foreach (EmbedFieldBuilder f in b.Fields)
			{
				if (f.Name == claimertitle) field = f;
			}
			if (field == null) b.AddField(claimertitle, u.Mention);
			else if (!field.Value.ToString().Contains(u.Mention)) field.Value += " " + u.Mention;
			else return;
			await msg.ModifyAsync(x => x.Embed = b.Build());
		}
		// Added specifically to update the visible list when someone unclaims. And caus didn't even know it was possible.
		public async Task OnRemove(IEmote e, ulong uid)
		{
			if (e.Name != claimemote) return;
			var u = await (msg.Channel as IGuildChannel).Guild.GetUserAsync(uid);
			var b = msg.Embeds.First().ToEmbedBuilder();
			foreach (EmbedFieldBuilder f in b.Fields)
			{
				if (f.Name == claimertitle)
				{
					var m = f.Value.ToString().Replace(" " + u.Mention, "").Replace(u.Mention, "");
					if (m == "") m = "_ _";
					f.WithValue(m);
				}
			}
			await msg.ModifyAsync(x => x.Embed = b.Build());
		}
	}
	// A late effort to cut redundant SQL, like SimCard.
	public class SimWarrior
	{
		public int wid;
		public string wname;
		public char wclass;
		public int starttier;
		public Single hp;
		public Single dmg;
		public string emotestr;
		public int aid;
		public int astren;
		public string expansion;
		public string sprimary;
		public string ssecondary;
		public (int, string) baseform;
		public string portrait_s;
		public string portraitfinishing;
		public (int, string)[] components;
		public (int, string)[] transformations;
		public (int, string, int, string)[] fusions;
		public int[] tids;
		public string[] nicks;
		public string[] portraitsfree;
		public string[] portraitsextra;
		public SimWarrior()
		{

		}
		private async Task<SimWarrior> InitializeAsync(int wid)
		{
			this.wid = wid;
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT w.wname, w.wclass, w.starttier, w.hp, w.dmg, w.emote, w.aid, w.astren," +
					" ex.ename, sp.sname, ss.sname, b.wid, b.wname, st.pimgurl, fi.pimgurl" +
					" FROM warrior w LEFT JOIN in_expansion AS ie ON ie.wid=w.wid LEFT JOIN expansion AS ex ON ex.eid=ie.eid INNER JOIN in_series AS isp ON isp.wid=w.wid AND isp.sprimary=TRUE" +
					" INNER JOIN series AS sp ON sp.sid=isp.sid LEFT JOIN in_series AS iss ON iss.wid=w.wid AND iss.sprimary=FALSE" +
					" LEFT JOIN series AS ss ON ss.sid=iss.sid LEFT JOIN transformation_of AS tof ON tof.transid=w.wid" +
					" LEFT JOIN warrior AS b ON b.wid=tof.baseid INNER JOIN has_portrait AS st ON st.wid=w.wid AND st.stier=TRUE" +
					" INNER JOIN has_portrait AS fi ON fi.wid=w.wid AND fi.finishing=TRUE WHERE w.wid=@wid;" +
					" SELECT hp.pimgurl FROM has_portrait hp WHERE wid=@wid AND free=TRUE ORDER BY pdefault DESC;" +
					" SELECT tid FROM is_type WHERE wid=@wid;" +
					" SELECT w.wid, w.wname FROM warrior w INNER JOIN fusion_of AS fo ON (fo.compa=w.wid OR fo.compb=w.wid) AND fo.fusion=@wid;" +
					" SELECT t.transid, tw.wname FROM transformation_of t INNER JOIN warrior AS tw ON tw.wid=t.transid WHERE t.baseid=@wid;" +
					" SELECT f.compa, wa.wname, f.compb, wb.wname, f.fusion, wf.wname FROM fusion_of f INNER JOIN warrior AS wa ON wa.wid=f.compa INNER JOIN warrior AS wb ON wb.wid=f.compb INNER JOIN warrior AS wf ON wf.wid=f.fusion" +
					" WHERE f.compa=@wid OR f.compb=@wid;" +
					" SELECT nbody FROM has_nickname WHERE wid=@wid;" +
					" SELECT pimgurl FROM has_portrait WHERE wid=@wid AND stier=FALSE AND finishing=FALSE AND free=FALSE;";
					cmd.Parameters.AddWithValue("wid", wid);
					await cmd.PrepareAsync();
					using (var r = await cmd.ExecuteReaderAsync())
					{
						if (!r.HasRows) throw new Exception("Warrior not found");
						await r.ReadAsync();
						wname = r.GetString(0);
						wclass = r.GetChar(1);
						starttier = r.GetInt32(2);
						hp = r.GetFloat(3);
						dmg = r.GetFloat(4);
						emotestr = r.GetString(5);
						aid = r.GetInt32(6);
						astren = r.GetInt32(7);
						if (await r.IsDBNullAsync(8)) expansion = null;
						else expansion = r.GetString(8);
						sprimary = r.GetString(9);
						if (await r.IsDBNullAsync(10)) ssecondary = null;
						else ssecondary = r.GetString(10);
						if (await r.IsDBNullAsync(11)) baseform = (-1, null);
						else baseform = (r.GetInt32(11), r.GetString(12));
						portrait_s = r.GetString(13);
						
						portraitfinishing = r.GetString(14);
						//Variable length fields
						var pfreel = new List<string>();
						var tidsl = new List<int>();
						var compl = new List<(int, string)>();
						var transl = new List<(int, string)>();
						var fusl = new List<(int, string, int, string)>();
						var nicksl = new List<string>();
						var pextral = new List<string>();
						await r.NextResultAsync();
						while (await r.ReadAsync()) pfreel.Add(r.GetString(0));
						await r.NextResultAsync();
						while (await r.ReadAsync()) tidsl.Add(r.GetInt32(0));
						await r.NextResultAsync();
						while (await r.ReadAsync()) compl.Add((r.GetInt32(0), r.GetString(1)));
						await r.NextResultAsync();
						while (await r.ReadAsync()) transl.Add((r.GetInt32(0), r.GetString(1)));
						await r.NextResultAsync();
						while (await r.ReadAsync())
						{
							if (wid == r.GetInt32(0)) fusl.Add((r.GetInt32(2), r.GetString(3), r.GetInt32(4), r.GetString(5)));
							else fusl.Add((r.GetInt32(0), r.GetString(1), r.GetInt32(4), r.GetString(5)));
						}
						await r.NextResultAsync();
						while (await r.ReadAsync()) nicksl.Add(r.GetString(0));
						await r.NextResultAsync();
						while (await r.ReadAsync()) pextral.Add(r.GetString(0));
						portraitsfree = pfreel.ToArray();
						tids = tidsl.ToArray();
						components = compl.ToArray();
						transformations = transl.ToArray();
						fusions = fusl.ToArray();
						nicks = nicksl.ToArray();
						portraitsextra = pextral.ToArray();
					}
				}
			}
			return this;
		}
		public static async Task<SimWarrior> CreateAsync(int wid)
		{
			var ret = new SimWarrior();
			return await ret.InitializeAsync(wid);
		}
		public string SeriesStr()
		{
			if (ssecondary == null) return sprimary;
			else return ssecondary + " (" + sprimary + ")";
		}
	}
	// An all purpose class that takes everything you would want to know about a card from the database and makes it conveniently available.
	// I wasted a lot of time writing very specialized SQL for everything that this class does. Who knew that a card game would involve so many cards?
	public class SimCard
	{
		public int cid;
		public DateTime createdat;
		public int tier;
		public int level;
		public ulong uid;
		public ulong sid;
		public int nickunlockablecount;
		public string[] nicksunlocked;
		public string nickselected;
		public int portraitunlockablecount;
		public string[] portraitsunlocked;
		public string[] portraitsfree;
		public string portrait_s;
		public string portraitfinishing;
		public string portraitselected;
		public int wid;
		public string wname;
		public char wclass;
		public int starttier;
		public Single whpmult;
		public Single wdmgmult;
		public string emotestr;
		public string expansion;
		public int aid;
		public int astren;
		public int[] tids;
		public string ename;
		public (int, string) baseform;
		public (int, string)[] components;
		public (int, string)[] transformations;
		public (int, string, int, string)[] fusions;
		public string sprimary;
		public string ssecondary;
		public SimCard()
		{

		}
		private async Task<SimCard> InitializeAsync(int incid)
		{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT c.cid, c.tier, c.level, o.uid, o.sid, ns.nbody, w.wid, w.wname, w.wclass, w.starttier, w.hp, w.dmg, w.emote," +
					" w.aid, w.astren, ex.ename, sp.sname, ss.sname, b.wid, b.wname, st.pimgurl, fi.pimgurl, sel.pimgurl, c.createdat" +
					" FROM card c INNER JOIN owns_card AS o ON o.cid=c.cid LEFT JOIN nickname_selected AS ns ON ns.cid=c.cid" +
					" INNER JOIN is_warrior AS iw ON iw.cid=c.cid INNER JOIN warrior AS w ON w.wid=iw.wid" +
					" LEFT JOIN in_expansion AS ie ON ie.wid=w.wid LEFT JOIN expansion AS ex ON ex.eid=ie.eid INNER JOIN in_series AS isp ON isp.wid=w.wid AND isp.sprimary=TRUE" +
					" INNER JOIN series AS sp ON sp.sid=isp.sid LEFT JOIN in_series AS iss ON iss.wid=w.wid AND iss.sprimary=FALSE" +
					" LEFT JOIN series AS ss ON ss.sid=iss.sid LEFT JOIN transformation_of AS tof ON tof.transid=w.wid" +
					" LEFT JOIN warrior AS b ON b.wid=tof.baseid INNER JOIN has_portrait AS st ON st.wid=w.wid AND st.stier=TRUE" +
					" INNER JOIN has_portrait AS fi ON fi.wid=w.wid AND fi.finishing=TRUE LEFT JOIN portrait_selected AS sel ON sel.cid=c.cid" +
					" WHERE c.cid=@cid;" +
					" SELECT nbody FROM nickname_unlocked WHERE cid=@cid;" +
					" SELECT pimgurl FROM portrait_unlocked WHERE cid=@cid;" +
					" SELECT hp.pimgurl FROM card c INNER JOIN is_warrior AS iw ON iw.cid=c.cid INNER JOIN has_portrait AS hp ON hp.wid=iw.wid AND hp.free=TRUE WHERE c.cid=@cid ORDER BY pdefault DESC;" +
					" SELECT it.tid FROM card c INNER JOIN is_warrior AS iw ON iw.cid=c.cid INNER JOIN is_type AS it ON it.wid=iw.wid WHERE c.cid=@cid;" +
					" SELECT w.wid, w.wname FROM warrior w INNER JOIN fusion_of AS fo ON (fo.compa=w.wid OR fo.compb=w.wid) AND" +
					" fo.fusion=(SELECT w.wid FROM warrior w INNER JOIN is_warrior AS iw ON iw.wid=w.wid WHERE iw.cid=@cid);" +
					" SELECT t.transid, tw.wname FROM card c INNER JOIN is_warrior AS iw ON iw.cid=c.cid INNER JOIN transformation_of AS t ON t.baseid=iw.wid INNER JOIN warrior AS tw ON tw.wid=t.transid WHERE c.cid=@cid;" +
					" SELECT f.compa, wa.wname, f.compb, wb.wname, f.fusion FROM card c INNER JOIN is_warrior AS iw ON iw.cid=c.cid INNER JOIN fusion_of AS f ON f.compa=iw.wid OR f.compb=iw.wid" +
					" INNER JOIN warrior AS wa ON wa.wid=f.compa INNER JOIN warrior AS wb ON wb.wid=f.compb WHERE c.cid=@cid;" +
					" SELECT COUNT(*) FROM is_warrior iw INNER JOIN has_nickname AS hn ON hn.wid=iw.wid WHERE iw.cid=@cid;" +
					" SELECT COUNT(*) FROM is_warrior iw INNER JOIN has_portrait AS pct ON pct.wid=iw.wid AND pct.free=FALSE AND pct.finishing=FALSE AND pct.stier=FALSE WHERE iw.cid=@cid";
					cmd.Parameters.AddWithValue("cid", incid);
					await cmd.PrepareAsync();
					using (var r = await cmd.ExecuteReaderAsync())
					{
						if (!r.HasRows) throw new Exception("Card not found");
						await r.ReadAsync();
						cid = incid;
						tier = r.GetInt32(1);
						level = r.GetInt32(2);
						if (await r.IsDBNullAsync(3)) uid = 0;
						else uid = Convert.ToUInt64(r.GetString(3));
						if (await r.IsDBNullAsync(4)) sid = 0;
						else sid = Convert.ToUInt64(r.GetString(4));
						if (await r.IsDBNullAsync(5)) nickselected = null;
						else nickselected = r.GetString(5);
						wid = r.GetInt32(6);
						wname = r.GetString(7);
						wclass = r.GetChar(8);
						starttier = r.GetInt32(9);
						whpmult = r.GetFloat(10);
						wdmgmult = r.GetFloat(11);
						emotestr = r.GetString(12);
						aid = r.GetInt32(13);
						astren = r.GetInt32(14);
						if (await r.IsDBNullAsync(15)) expansion = null;
						else expansion = r.GetString(15);
						sprimary = r.GetString(16);
						if (await r.IsDBNullAsync(17)) ssecondary = null;
						else ssecondary = r.GetString(17);
						if (await r.IsDBNullAsync(18)) baseform = (-1, null);
						else baseform = (r.GetInt32(18), r.GetString(19));
						portrait_s = r.GetString(20);
						portraitfinishing = r.GetString(21);
						if (await r.IsDBNullAsync(22)) portraitselected = null;
						else portraitselected = r.GetString(22);
						createdat = r.GetDateTime(23);
						//Variable length fields
						var nicksl = new List<string>();
						var punll = new List<string>();
						var pfreel = new List<string>();
						var tidsl = new List<int>();
						var compl = new List<(int, string)>();
						var transl = new List<(int, string)>();
						var fusl = new List<(int, string, int, string)>();
						await r.NextResultAsync();
						while (await r.ReadAsync()) nicksl.Add(r.GetString(0));
						await r.NextResultAsync();
						while (await r.ReadAsync()) punll.Add(r.GetString(0));
						await r.NextResultAsync();
						while (await r.ReadAsync()) pfreel.Add(r.GetString(0));
						await r.NextResultAsync();
						while (await r.ReadAsync()) tidsl.Add(r.GetInt32(0));
						await r.NextResultAsync();
						while (await r.ReadAsync()) compl.Add((r.GetInt32(0), r.GetString(1)));
						await r.NextResultAsync();
						while (await r.ReadAsync()) transl.Add((r.GetInt32(0), r.GetString(1)));
						await r.NextResultAsync();
						while (await r.ReadAsync()) fusl.Add((r.GetInt32(0), r.GetString(1), r.GetInt32(2), r.GetString(3)));
						await r.NextResultAsync();
						await r.ReadAsync();
						nickunlockablecount = r.GetInt32(0);
						await r.NextResultAsync();
						await r.ReadAsync();
						portraitunlockablecount = r.GetInt32(0);
						nicksunlocked = nicksl.ToArray();
						portraitsunlocked = punll.ToArray();
						portraitsfree = pfreel.ToArray();
						tids = tidsl.ToArray();
						components = compl.ToArray();
						transformations = transl.ToArray();
						fusions = fusl.ToArray();
					}
				}
			}
			return this;
		}
		public static async Task<SimCard> CreateAsync(int cid)
		{
			var ret = new SimCard();
			return await ret.InitializeAsync(cid);
		}
		public static async Task<SimCardForDeck> CreateForDeckAsync(int cid)
		{
			var ret = new SimCardForDeck();
			return await ret.InitializeAsync(cid) as SimCardForDeck;
		}
		public string NickOrReal()
		{
			if (nickselected != null) return nickselected;
			else return wname;
		}
		public string NameTierLevel()
		{
			return NickOrReal() + " (Tier **" + LetterOfTier(tier) + "** | Level **" + level + "**)";
		}
		public string SeriesStr()
		{
			if (ssecondary == null) return sprimary;
			else return ssecondary + " (" + sprimary + ")";
		}
		public int IsolatedHp()
		{
			return (int) Math.Floor(Globals.BaseHp * HpMultOfClass(wclass) * whpmult * MultOfTier(tier) * MultOfLevel(level));
		}
		public int IsolatedDmg()
		{
			return (int) Math.Floor(Globals.BaseDmg * DmgMultOfClass(wclass) * wdmgmult * MultOfTier(tier) * MultOfLevel(level));
		}
	}
	// SimCard was supposed to be "all purpose", but this is the exception.
	// There's too much stuff that applies to cards in the context of a deck but not to cards outside of a deck.
	// So, cards in a deck have their own class. Mostly for combat modeling.
	public class SimCardForDeck : SimCard
	{
		public int row;
		public int column;
		public Boolean hero;
		public Boolean auraability;
		public Boolean debuffability;
		public Boolean herobuffability;
		public Single hpmultfromability;
		public Single dmgmultfromability;
		public Single hpmultfromtransformation;
		public Single dmgmultfromtransformation;
		public Single hpmultfromcombo;
		public Single dmgmultfromcombo;
		public Boolean alive;
		public SimCardForDeck()
		{
			
		}
		public static async Task<SimCardForDeck> Convert(int incid, int row, int column, Boolean hero)
		{
			
			var ret = await SimCard.CreateForDeckAsync(incid);
			ret.row = row;
			ret.column = column;
			ret.hero = hero;
			if ((new int[] {12, 13, 14, 15, 16, 17, 18}).Contains(ret.aid)) ret.auraability = true;
			else ret.auraability = false;
			if ((new int[] {19, 20, 22, 23, 24}).Contains(ret.aid)) ret.debuffability = true;
			else ret.debuffability = false;
			if ((new int[] {10, 11}).Contains(ret.aid)) ret.herobuffability = true;
			else ret.herobuffability = false;
			ret.hpmultfromability = 1f;
			ret.dmgmultfromability = 1f;
			ret.hpmultfromtransformation = 1f;
			ret.dmgmultfromtransformation = 1f;
			ret.hpmultfromcombo = 1f;
			ret.dmgmultfromcombo = 1f;
			ret.alive = true;
			return ret;
			
		}
		public static async Task<SimCardForDeck[,]> CreateBulkAsync(ulong uid, ulong sid)
		{
			var ret =  new SimCardForDeck[3,5];
			try {
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT cid, hrow, hcolumn, hero FROM in_hand WHERE uid=@uid AND sid=@sid";
					cmd.Parameters.AddWithValue("uid", uid.ToString());
					cmd.Parameters.AddWithValue("sid", sid.ToString());
					await cmd.PrepareAsync();
					using (var r = await cmd.ExecuteReaderAsync())
					{
						while (await r.ReadAsync())
						{
							ret[r.GetInt32(1), r.GetInt32(2)] = await Convert(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetBoolean(3));
						}
					}
				}
			}
			} catch (Exception e) {Console.WriteLine("Create bulk error: " + e.Message);}
			return ret;
		}
		public int HandedHp()
		{
			if (hero) return (int) Math.Floor(Globals.BaseHp * HpMultOfClass(wclass) * whpmult * MultOfTier(tier) *
			MultOfLevel(level) * hpmultfromability * hpmultfromtransformation * hpmultfromcombo * 1.2f);
			else return (int) Math.Floor(Globals.BaseHp * HpMultOfClass(wclass) * whpmult * MultOfTier(tier) *
			MultOfLevel(level) * hpmultfromability * hpmultfromtransformation * hpmultfromcombo);
		}
		public int HandedDmg()
		{
			if (hero) return (int) Math.Floor(Globals.BaseDmg * DmgMultOfClass(wclass) * wdmgmult * MultOfTier(tier) *
			MultOfLevel(level) * dmgmultfromability * dmgmultfromtransformation * dmgmultfromcombo * 1.2f);
			else return (int) Math.Floor(Globals.BaseDmg * DmgMultOfClass(wclass) * wdmgmult * MultOfTier(tier) *
			MultOfLevel(level) * dmgmultfromability * dmgmultfromtransformation * dmgmultfromcombo);
		}
		public void DebugLog(string entry)
		{
			Console.WriteLine(DebugName() + " " + entry);
		}
		public string DebugName()
		{
			return cid + " (" + wname + ")";
		}
	}
	// For simulating anything involving the deck. There's a lot of logic that goes into deckbuilding, plus combat modeling.
	// A lot of the code inside is messy and largely experimental.
	public class SimDeck
	{
		public int mana;
		public int manadelta;
		public SimCardForDeck[,] cards;
		public SimCardForDeck hero;
		public Dictionary<int, HashSet<SimCardForDeck>>[] typetriples;
		public Dictionary<int, HashSet<SimCardForDeck>>[] typequints;
		public Dictionary<string, HashSet<SimCardForDeck>>[] seriestriples;
		public Dictionary<string, HashSet<SimCardForDeck>>[] seriesquints;
		public HashSet<(SimCardForDeck, SimCardForDeck)> transformations;
		public HashSet<(SimCardForDeck, SimCardForDeck, SimCardForDeck)> fusions;
		private SimDeck()
		{
			mana = 0;
			manadelta = 0;
			typetriples = new Dictionary<int, HashSet<SimCardForDeck>>[3];
			typequints = new Dictionary<int, HashSet<SimCardForDeck>>[3];
			seriestriples = new Dictionary<string, HashSet<SimCardForDeck>>[3];
			seriesquints = new Dictionary<string, HashSet<SimCardForDeck>>[3];
			for (int i=0;i<3;i++)
			{
				typetriples[i] = new Dictionary<int, HashSet<SimCardForDeck>>();
				typequints[i] = new Dictionary<int, HashSet<SimCardForDeck>>();
				seriestriples[i] = new Dictionary<string, HashSet<SimCardForDeck>>();
				seriesquints[i] = new Dictionary<string, HashSet<SimCardForDeck>>();
			}
			transformations = new HashSet<(SimCardForDeck, SimCardForDeck)>();
			fusions = new HashSet<(SimCardForDeck, SimCardForDeck, SimCardForDeck)>();
		}
		private async Task<SimDeck> InitializeAsync(ulong uid, ulong sid)
		{
			try {await BuildDeck(uid, sid);}
			catch (Exception e) {Console.WriteLine("Deck build error: " + e.Message);}
			try {PreAnalyze();}
			catch (Exception e) {Console.WriteLine("Deck preanalyze error: " + e.Message);}
			return this;
		}
		public static async Task<SimDeck> CreateAsync(ulong uid, ulong sid)
		{
			var ret = new SimDeck();
			return await ret.InitializeAsync(uid, sid);
		}
		private async Task BuildDeck(ulong uid, ulong sid)
		{
			cards = await SimCardForDeck.CreateBulkAsync(uid, sid);
		}
		public void PreAnalyze()
		{
			var typemults = new Dictionary<int, HashSet<SimCardForDeck>>[3];
			for (int i=0;i<3;i++) typemults[i] = new Dictionary<int, HashSet<SimCardForDeck>>();
			var seriesmults = new Dictionary<string, HashSet<SimCardForDeck>>[3];
			for (int i=0;i<3;i++) seriesmults[i] = new Dictionary<string, HashSet<SimCardForDeck>>();
			
			for (int row=0;row<3;row++)
			{
				for (int col=0;col<5;col++)
				{
					var card = cards[row, col];
					if (card != null)
					{
						//Hero detection
						if (card.hero) {hero = card;}
						//Type detection
						foreach (int tid in card.tids)
						{
							if (typemults[row].ContainsKey(tid)) typemults[row][tid].Add(card);
							else typemults[row].Add(tid, new HashSet<SimCardForDeck> {card});
						}
						//Series detection
						if (seriesmults[row].ContainsKey(card.sprimary)) seriesmults[row][card.sprimary].Add(card);
						else seriesmults[row].Add(card.sprimary, new HashSet<SimCardForDeck> {card});
						//Transformation detection
						if (card.baseform.Item1 != -1 && row > 0)
						{
							if (cards[row - 1, col] != null)
							{
								if (cards[row - 1, col].wid == card.baseform.Item1) transformations.Add((cards[row - 1, col], card));
							}
						}
						//Fusion detection
						if (card.components.Length > 0 && row == 2 && cards[1, col] != null && cards[0, col] != null)
						{
							var compwids = new HashSet<int>();
							foreach ((int, string) wid in card.components) compwids.Add(wid.Item1);
							if (compwids.Contains(cards[1, col].wid) && compwids.Contains(cards[0, col].wid)) fusions.Add((cards[0, col], cards[1, col], card));
						}
					}
				}
			}
			//Apply bonuses from transformations and combos
			foreach ((SimCardForDeck, SimCardForDeck, SimCardForDeck) trip in fusions)
			{
				trip.Item3.hpmultfromtransformation = 1.3f;
				trip.Item3.dmgmultfromtransformation = 1.3f;
			}
			foreach ((SimCardForDeck, SimCardForDeck) doub in transformations)
			{
				doub.Item2.hpmultfromtransformation = 1.2f;
				doub.Item2.dmgmultfromtransformation = 1.2f;
			}
			for (int row=0;row<3;row++)
			{
				foreach (int type in typemults[row].Keys)
				{
					if (typemults[row][type].Count > 2 && typemults[row][type].Count < 5) typetriples[row].Add(type, typemults[row][type]);
					else if (typemults[row][type].Count == 5) typequints[row].Add(type, typemults[row][type]);
				}
				foreach (string series in seriesmults[row].Keys)
				{
					if (seriesmults[row][series].Count > 2 && seriesmults[row][series].Count < 5) seriestriples[row].Add(series, seriesmults[row][series]);
					else if (seriesmults[row][series].Count == 5) seriesquints[row].Add(series, seriesmults[row][series]);
				}
				
				foreach (string series in seriestriples[row].Keys)
				{
					foreach (SimCardForDeck c in seriestriples[row][series])
					{
						c.dmgmultfromcombo = 1.05f;
						c.hpmultfromcombo = 1.05f;
					}
				}
				foreach (string series in seriesquints[row].Keys)
				{
					foreach (SimCardForDeck c in seriesquints[row][series])
					{
						c.dmgmultfromcombo = 1.1f;
						c.hpmultfromcombo = 1.1f;
					}
				}
				foreach (int type in typetriples[row].Keys)
				{
					foreach (SimCardForDeck c in typetriples[row][type])
					{
						c.dmgmultfromcombo = Math.Max(1.05f, c.dmgmultfromcombo);
						c.hpmultfromcombo = Math.Max(1.05f, c.hpmultfromcombo);
					}
				}
				foreach (int type in typequints[row].Keys)
				{
					foreach (SimCardForDeck c in typequints[row][type])
					{
						c.dmgmultfromcombo = 1.1f;
						c.hpmultfromcombo = 1.1f;
					}
				}
			}
		}
		public void ModelAbilitiesForViewer()
		{
			try{
			for (int row=0; row<3; row++)
			{
				//Hero boosts
				ApplyAbility(10, row);
				ApplyAbility(11, row);
				//Deterministic auras
				ApplyAbility(12, row);
				ApplyAbility(13, row);
				//Boost
				ApplyAbility(18, row);
			}
			} catch (Exception e) {Console.WriteLine("Model ability for viewer error: " + e.Message);}
		}
		public SimCardForDeck[] AdjacentCards(int row, int col)
		{
			var ret = new List<SimCardForDeck>();
			if (col > 0)
			{
				if (cards[row, col - 1] != null) ret.Add(cards[row, col - 1]);
			}
			if (col < 4)
			{
				if (cards[row, col + 1] != null) ret.Add(cards[row, col + 1]);
			}
			return ret.ToArray();
		}
		public Boolean ContainsCard(int cid)
		{
			for (int row=0;row<3;row++)
			{
				for (int col=0;col<5;col++)
				{
					if (cards[row, col] != null)
					{
						if (cards[row, col].cid == cid) return true;
					}
				}
			}
			return false;
		}
		public void ApplyAbility(int aid, int row)
		{
			for (int col=0;col<5;col++)
			{
				var card = cards[row, col];
				if (card != null)
				{
					if (card.aid == aid)
					{
						if (aid == 1)
						{
							if (RandomHolder.Instance.NextDouble() <= ValFromAbilityFloat(aid, card.astren))
							{
								card.hpmultfromability *= 1.2f;
								card.DebugLog("crit success");
							}
							else card.DebugLog("crit failure");
						}
						else if (aid == 2)
						{
							if (RandomHolder.Instance.NextDouble() <= ValFromAbilityFloat(aid, card.astren))
							{
								card.hpmultfromability *= 1.2f;
								card.DebugLog("adrenaline success");
							}
							else card.DebugLog("adrenaline failure");
						}
						else if (aid == 3)
						{
							card.hpmultfromability *= 1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10);
							card.DebugLog((1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10)).ToString() + " mult from mana strengthen");
						}
						else if (aid == 4)
						{
							card.hpmultfromability *= 1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10);
							card.DebugLog((1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10)).ToString() + " mult from mana vigor");
						}
						else if (aid == 5)
						{
							if (card.alive)
							{
								manadelta += 2 * ValFromAbilityInt(aid, card.astren);
								card.DebugLog((2 * ValFromAbilityInt(aid, card.astren)).ToString() + " mana added from mana boost (alive)");
							}
							else
							{
								manadelta += ValFromAbilityInt(aid, card.astren);
								card.DebugLog((ValFromAbilityInt(aid, card.astren)).ToString() + " mana added from mana boost (dead)");
							}
						}
						else if (aid == 6)
						{
							if (RandomHolder.Instance.NextDouble() <= ValFromAbilityFloat(aid, card.astren))
							{
								manadelta += 10;
								card.DebugLog("mana surge success");
							}
							else card.DebugLog("mana surge failure");
						}
						else if (aid == 7)
						{
							if (RandomHolder.Instance.NextDouble() <= 0.02)
							{
								card.hpmultfromability *= 1f + ValFromAbilityFloat(aid, card.astren);
								card.DebugLog("invincibility success");
							}
							else card.DebugLog("invincibility failure");
						}
						else if (aid == 8)
						{
							//Placeholder for when point rewards are implemented
						}
						else if (aid == 9)
						{
							//Placeholder for when point rewards are implemented
						}
						else if (aid == 10)
						{
							if (hero != null)
							{
								hero.hpmultfromability *= 1f + ValFromAbilityFloat(aid, card.astren);
								card.DebugLog((1f + ValFromAbilityFloat(aid, card.astren)).ToString() + " mult from shield blessing");
							}
							else card.DebugLog("No hero for shield blessing");
						}
						else if (aid == 11)
						{
							if (hero != null)
							{
								hero.dmgmultfromability *= 1f + ValFromAbilityFloat(aid, card.astren);
								card.DebugLog((1f + ValFromAbilityFloat(aid, card.astren)).ToString() + " mult from might blessing");
							}
							else card.DebugLog("No hero for might blessing");
						}
						else if (aid == 12)
						{
							foreach (SimCardForDeck adj in AdjacentCards(row, col))
							{
								adj.dmgmultfromability *= 1f + ValFromAbilityFloat(aid, card.astren);
								card.DebugLog((1f + ValFromAbilityFloat(aid, card.astren)).ToString() + " mult from precision given to " + adj.DebugName());
							}
						}
						else if (aid == 13)
						{
							foreach (SimCardForDeck adj in AdjacentCards(row, col))
							{
								adj.hpmultfromability *= 1f + ValFromAbilityFloat(aid, card.astren);
								card.DebugLog((1f + ValFromAbilityFloat(aid, card.astren)).ToString() + " mult from vitality given to " + adj.DebugName());
							}
						}
						else if (aid == 14)
						{
							foreach (SimCardForDeck adj in AdjacentCards(row, col))
							{
								if (RandomHolder.Instance.NextDouble() <= ValFromAbilityFloat(aid, card.astren))
								{
									adj.dmgmultfromability *= 1.15f;
									card.DebugLog("rally critical success for " + adj.DebugName());
								}
								else card.DebugLog("rally critical failure for " + adj.DebugName());
							}
						}
						else if (aid == 15)
						{
							foreach (SimCardForDeck adj in AdjacentCards(row, col))
							{
								if (RandomHolder.Instance.NextDouble() <= ValFromAbilityFloat(aid, card.astren))
								{
									adj.hpmultfromability *= 1.15f;
									card.DebugLog("rally adrenaline success for " + adj.DebugName());
								}
								else card.DebugLog("rally adrenaline failure for " + adj.DebugName());
							}
						}
						else if (aid == 16)
						{
							foreach (SimCardForDeck adj in AdjacentCards(row, col))
							{
								adj.dmgmultfromability *= 1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10);
								card.DebugLog((1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10)).ToString() + " mult from mana empower given to " + adj.DebugName());
							}
						}
						else if (aid == 17)
						{
							foreach (SimCardForDeck adj in AdjacentCards(row, col))
							{
								adj.hpmultfromability *= 1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10);
								card.DebugLog((1f + (ValFromAbilityFloat(aid, card.astren) * Convert.ToSingle(mana) / 10)).ToString() + " mult from mana heal given to " + adj.DebugName());
							}
						}
						else if (aid == 18)
						{
							foreach (SimCardForDeck adj in AdjacentCards(row, col))
							{
								var old = adj.hpmultfromability;
								adj.hpmultfromability = ((adj.hpmultfromability - 1f) * (1f + ValFromAbilityFloat(aid, card.astren))) + 1f;
								adj.dmgmultfromability = ((adj.dmgmultfromability - 1f) * (1f + ValFromAbilityFloat(aid, card.astren))) + 1f;
								card.DebugLog("Boost given to " + adj.DebugName() + " (" + old + " --> " + adj.hpmultfromability + ")");
							}
						}
						else if (aid == 19)
						{
							//Placeholder for combat
						}
						else if (aid == 20)
						{
							//Placeholder for combat
						}
						else if (aid == 21)
						{
							if (RandomHolder.Instance.NextDouble() <= 0.02)
							{
								card.dmgmultfromability *= 1f + ValFromAbilityFloat(aid, card.astren);
								card.DebugLog("execution success");
							}
							else card.DebugLog("execution failure");
						}
						else if (aid == 22)
						{
							//Placeholder for combat
						}
						else if (aid == 23)
						{
							//Placeholder for combat
						}
						else if (aid == 24)
						{
							//Placeholder for combat
						}
					}
				}
			}
		}
	}
	// The fundamental class for posts with pages you can turn. Very handy.
	public class MultiPageMessage
	{
		public DateTime lastused;
		public Discord.Rest.RestUserMessage msg;
		public int index;
		public virtual async Task PageTurn(string name)
		{}
		public async Task DeathTimer()
		{
			while (true)
			{
				await Task.Delay(1000 * 100);
				if ((DateTime.Now - lastused).TotalSeconds > 30)
				{
					Globals.multipagemessages.Remove(msg.Id);
					break;
				}
			}
		}
	}
	// The most versatile type of multipage. Takes lines of text and formats them into pages. Simple.
	public class MultiPageParsesLines : MultiPageMessage
	{
		string[] pages;
		string leftemote;
		string rightemote;
		private MultiPageParsesLines(string[] lines, int maxchars, int maxlines)
		{
			leftemote = "\U00002B05";
			rightemote = "\U000027A1";
			int linesonpage = 0;
			index = 0;
			lastused = DateTime.Now;
			var lpages = new List<string>() {""};
			foreach (string line in lines)
			{
				if (line.Length + lpages.Last().Length <= maxchars && linesonpage < maxlines)
				{
					lpages[lpages.Count-1] += line + "\n";
					linesonpage ++;
				}
				else
				{
					lpages.Add(line + "\n");
					linesonpage = 1;
				}
			}
			pages = lpages.ToArray();
		}
		private async Task<MultiPageParsesLines> InitializeAsync(SocketTextChannel channel, EmbedBuilder builder)
		{
			builder.Description = pages[0];
			builder.WithFooter("(1/" + pages.Length.ToString() + ")");
			msg = await channel.SendMessageAsync(embed: builder.Build());
			await msg.AddReactionsAsync(new[] {new Emoji(leftemote), new Emoji(rightemote)});
			Globals.multipagemessages.Add(msg.Id, this);
			await DeathTimer().ConfigureAwait(false);
			return this;
		}
		public static async Task<MultiPageParsesLines> CreateAsync(string[] entry, EmbedBuilder builder, SocketTextChannel channel, int maxchars=1500, int maxlines=30)
		{
			var ret = new MultiPageParsesLines(entry, maxchars, maxlines);
			return await ret.InitializeAsync(channel, builder);
		}
		public async Task UpdateMsg()
		{
			var build = msg.Embeds.First().ToEmbedBuilder();
			build.Description = pages[index];
			build.WithFooter("(" + (index+1).ToString() + "/" + pages.Length.ToString() + ")");
			await msg.ModifyAsync(x => x.Embed = build.Build());
		}
		public override async Task PageTurn(string name)
		{
			if (name == rightemote)
			{
				if (index < pages.Length - 1) index++;
				else index = 0;
			}
			if (name == leftemote)
			{
				if (index > 0) index--;
				else index = pages.Length - 1;
			}
			await UpdateMsg();
			lastused = DateTime.Now;
		}
	}
	// Views the deck. Needs its own class for nonstandard format.
	public class DeckViewMessage : MultiPageMessage
	{
		(string, string) user;
		string[] grids;
		string[] handsa;
		string[] handsb;
		string[] combos;
		string[] numberemotes;
		private DeckViewMessage()
		{
			numberemotes = new string[] {"\U00000031\U000020e3", "\U00000032\U000020e3", "\U00000033\U000020e3"};
			lastused = DateTime.Now;
			index = 0;
		}
		private async Task<DeckViewMessage> InitializeAsync(SocketTextChannel channel)
		{
			Embed e;
			try {e = MsgEmbed();}
			catch (Exception x) {Console.WriteLine("Deck viwer error: " + x.Message); return this;}
			msg = await channel.SendMessageAsync(embed: e);
			await msg.AddReactionsAsync(new [] {new Emoji(numberemotes[0]), new Emoji(numberemotes[1]), new Emoji(numberemotes[2])});
			Globals.multipagemessages.Add(msg.Id, this);
			await DeathTimer().ConfigureAwait(false);
			return this;
		}
		//await DeckViewMessage.CreateAsync(Context.Channel as SocketTextChannel, (NickOrReal(Context.User.Id, Context.Guild), Context.User.GetAvatarUrl()), tables, combos);
		public static async Task<DeckViewMessage> CreateAsync(SocketTextChannel channel, (string, string) user, string[] grids, string[] handsa, string[] handsb, string[] combos)
		{
			var ret = new DeckViewMessage();
			ret.user = user;
			ret.grids = grids;
			ret.handsa = handsa;
			ret.handsb = handsb;
			ret.combos = combos;
			return await ret.InitializeAsync(channel);
		}
		public Embed MsgEmbed()
		{
			var b = new EmbedBuilder();
			b.WithAuthor("Hand " + (index + 1));
			b.AddField("Deck", grids[index]);
			b.AddField("Hand (Class, Level, Tier, HP, DMG)", handsa[index]);
			b.AddField("Hand (ID, Ability)", handsb[index]);
			b.AddField("Set bonus", combos[index]);
			b.WithFooter(user.Item1, user.Item2);
			b.WithColor(Color.DarkGrey);
			return b.Build();
		}
		public async Task UpdateMsg()
		{
			await msg.ModifyAsync(x => x.Embed = MsgEmbed());
		}
		public override async Task PageTurn(string name)
		{
			if (numberemotes.Contains(name))
			{
				index = Array.IndexOf(numberemotes, name);
				await UpdateMsg();
				lastused = DateTime.Now;
			}
		}
	}
	// Tells you about a card. Kind of complicated.
	public class InfoCardMessage : MultiPageMessage
	{
		string[] images;
		string[] labels;
		string leftemote;
		string rightemote;
		private InfoCardMessage()
		{
			leftemote = "\U00002B05";
			rightemote = "\U000027A1";
			lastused = DateTime.Now;
		}
		private async Task<InfoCardMessage> InitializeAsync(int cid, SocketTextChannel channel, DiscordSocketClient client)
		{
			Embed e;
			try {e = await CardViewEmbed(cid, client, channel.Guild);}
			catch {return this;}
			msg = await channel.SendMessageAsync(embed: e);
			await msg.AddReactionsAsync(new[] {new Emoji(leftemote), new Emoji(rightemote)});
			Globals.multipagemessages.Add(msg.Id, this);
			Task.Run(() => DeathTimer());
			return this;
		}
		public static async Task<InfoCardMessage> CreateAsync(int cid, SocketTextChannel channel, DiscordSocketClient client)
		{
			var ret = new InfoCardMessage();
			return await ret.InitializeAsync(cid, channel, client);
		}
		public async Task<Embed> CardViewEmbed(int cid, DiscordSocketClient client, SocketGuild server)
		{
			SimCard card;
			int age;
			string agerating;
			//var speed = DateTime.Now;
			try {card = await SimCard.CreateAsync(cid);}
			catch {throw new Exception("Card not found");}
			//Console.WriteLine((DateTime.Now - speed).TotalMilliseconds);
			using (var c = Conn())
			{
				await c.OpenAsync();
				try{
				//Might delete if too slow, calculates age relative to all other cards of the same warrior
				age = (int) Math.Floor((DateTime.Now - card.createdat).TotalDays);
				int beforecount;
				int aftercount;
				int beforeliving;
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT COUNT(*) FROM (SELECT * FROM card WHERE cid IN (SELECT cid FROM is_warrior WHERE wid=@wid) AND createdat < @c) as x;" +
					" SELECT COUNT(*) FROM (SELECT * FROM card WHERE cid IN (SELECT cid FROM is_warrior WHERE wid=@wid) AND createdat > @c) as x;" +
					" SELECT COUNT(*) FROM (SELECT * FROM card INNER JOIN owns_card AS o ON o.cid=card.cid WHERE card.cid IN (SELECT cid FROM is_warrior WHERE wid=@wid) AND card.createdat < @c) as x;";
					cmd.Parameters.AddWithValue("wid", card.wid);
					cmd.Parameters.AddWithValue("c", card.createdat);
					await cmd.PrepareAsync();
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						await dr.ReadAsync();
						beforecount = dr.GetInt32(0);
						await dr.NextResultAsync();
						await dr.ReadAsync();
						aftercount = dr.GetInt32(0);
						await dr.NextResultAsync();
						await dr.ReadAsync();
						beforeliving = dr.GetInt32(0);
					}
				}
				agerating = AgeRating(beforecount, aftercount, beforeliving);
				} catch (Exception e) {Console.WriteLine("Card view SQL error: " + e.Message); throw;}
			}
			try
			{
				var b = new EmbedBuilder();
				string[] portraits;
				if (card.tier == 5) portraits = (new string[] {card.portrait_s}).Concat(card.portraitsfree).ToArray().Concat(card.portraitsunlocked).ToArray();
				else portraits = card.portraitsfree.Concat(card.portraitsunlocked).ToArray();
				int pindex = 0;
				if (card.portraitselected != null) {if (portraits.Contains(card.portraitselected)) pindex = Array.IndexOf(portraits, card.portraitselected);}
				string owner;
				if (card.uid == 0) owner = "Lord Death";
				else if (server.Id != card.sid)
				{
					try {owner = "Someone in " + client.GetGuild(card.sid).Name;}
					catch (Exception) {owner = "Someone in an unknown server";}
				}
				else try {owner = server.GetUser(card.uid).ToString();}
				catch (Exception) {owner = "Unknown";}
				string author = card.NickOrReal() + " (#" + card.wid + "), " + card.SeriesStr();
				if (card.expansion == null) card.expansion = "None";
				string mtier;
				if (card.tier == card.starttier) mtier = LetterOfTier(card.tier);
				else mtier = LetterOfTier(card.starttier) + " -> " + LetterOfTier(card.tier);
				string mtypes = "";
				foreach (int t in card.tids) mtypes += NameOfType(t) + ", ";
				mtypes = mtypes.Substring(0, mtypes.Length - 2);
				string mtrans = "";
				foreach ((int, string) t in card.transformations) mtrans += t.Item2 + ", ";
				if (mtrans.Length > 2) mtrans = mtrans.Substring(0, mtrans.Length - 2);
				string mfus = "";
				foreach ((int, string, int, string) f in card.fusions) mfus += f.Item4 + ", ";
				if (mfus.Length > 2) mfus = mfus.Substring(0, mfus.Length - 2);
				//Start building embed
				//Enforce a GANGSTER SANS format
				int maxwidth = 28;
				try {b.WithAuthor(author, Emote.Parse(card.emotestr).Url);}
				catch {b.WithAuthor(author);}
				b.AddField("Card Information", "Owner: " + owner + "\nExpansion: " + card.expansion);
				b.AddField("Merit", "Tier: " + mtier + "\nLevel: " + card.level, true);
				b.AddField("Combat Stats", "HP: " + card.IsolatedHp() + "\nDMG: " + card.IsolatedDmg(), true);
				b.AddField("\u200b", "_ _");
				b.AddField("Warrior Information", "Class: " + ClassOfChar(card.wclass) + "\n" + WrapToMaxWidth(mtypes, maxwidth), true);
				b.AddField("Distinction", "Serial number: " + card.cid + "\n" + WrapToMaxWidth("Age: " + age + " days " + agerating, maxwidth), true);
				b.AddField(NameOfAbility(card.aid) + " " + Roman(card.astren), DescOfAbility(card.aid, card.astren));
				if (card.transformations.Length > 0) b.AddField("Transformations", WrapToMaxWidth(mtrans, maxwidth), true);
				if (card.fusions.Length > 0) b.AddField("Fusions", WrapToMaxWidth(mfus, maxwidth), true);
				if (card.baseform.Item2 != null) b.AddField("Transformation of " + card.baseform.Item2,
				"A transformation recieves +20% HP and DMG if positioned in the hand directly below its base form in the same column.");
				if (card.components.Length > 0) b.AddField("Fusion of " + card.components[0].Item2 + " and " + card.components[1].Item2,
				"A fusion recieves +30% HP and DMG if positioned in the bottom hand and in the same column as both of its components.");
				b.AddField("Unlocked Cosmetics", WrapToMaxWidth(card.portraitsunlocked.Length + "/" + card.portraitunlockablecount + " portraits, " +
				card.nicksunlocked.Length + "/" + card.nickunlockablecount + " nicknames", maxwidth), true);
				b.WithImageUrl(portraits[pindex]);
				b.WithColor(ColorOfTier(card.tier));
				b.WithFooter("(" + (pindex + 1) + "/" + portraits.Length + ")");
				index = pindex;
				images = portraits;
				return b.Build();
			}
			catch (Exception e) {Console.WriteLine("Card view error: ", e.Message); throw;}
		}
		public async Task UpdateMsg()
		{
			var build = msg.Embeds.First().ToEmbedBuilder();
			build.WithImageUrl(images[index]);
			build.WithFooter("(" + (index + 1) + "/" + images.Length + ")");
			await msg.ModifyAsync(x => x.Embed = build.Build());
		}
		public override async Task PageTurn(string name)
		{
			if (name == rightemote)
			{
				if (index < images.Length - 1) index++;
				else index = 0;
			}
			if (name == leftemote)
			{
				if (index > 0) index--;
				else index = images.Length - 1;
			}
			await UpdateMsg();
			lastused = DateTime.Now;
		}
	}
	// Tells you about a warrior. Needs its own class for nonstandard format.
	public class InfoWarriorMessage : MultiPageMessage
	{
		string[] images;
		string[] labels;
		string leftemote;
		string rightemote;
		private InfoWarriorMessage(string[] entry)
		{
			leftemote = "\U00002B05";
			rightemote = "\U000027A1";
			images = entry;
			labels = new string[] {"Free", "Free", "Finishing", "S Tier"};
			index = 0;
			lastused = DateTime.Now;
		}
		private async Task<InfoWarriorMessage> InitializeAsync(SocketTextChannel channel, Embed e)
		{
			msg = await channel.SendMessageAsync(embed: e);
			await msg.AddReactionsAsync(new[] {new Emoji(leftemote), new Emoji(rightemote)});
			Globals.multipagemessages.Add(msg.Id, this);
			await DeathTimer().ConfigureAwait(false);
			return this;
		}
		public static async Task<InfoWarriorMessage> CreateAsync(string[] entry, Embed e, SocketTextChannel channel)
		{
			var ret = new InfoWarriorMessage(entry);
			return await ret.InitializeAsync(channel, e);
		}
		public async Task UpdateMsg()
		{
			var build = msg.Embeds.First().ToEmbedBuilder();
			build.WithImageUrl(images[index]);
			foreach (EmbedFieldBuilder field in build.Fields)
			{
				if (field.Name == "Portrait")
				{
					field.Value = "**(" + (index + 1).ToString() + "/" + images.Length.ToString() + ")**";
					if (index < labels.Length) field.Value += " " + labels[index];
				}
			}
			await msg.ModifyAsync(x => x.Embed = build.Build());
		}
		public override async Task PageTurn(string name)
		{
			if (name == rightemote)
			{
				if (index < images.Length - 1) index++;
				else index = 0;
			}
			if (name == leftemote)
			{
				if (index > 0) index--;
				else index = images.Length - 1;
			}
			await UpdateMsg();
			lastused = DateTime.Now;
		}
	}
	/*
	
	COMMANDS

	This library tries to makes commands easy. Sometimes they are, sometimes they get so stupid that it makes you wonder why they even tried.
	I really did try to do commands the "right" way, as that should be the most sustainable for big projects.
	That was a nightmare to get working, but now it's fine. And still in somewhat proper form. But not entirely, because then it wouldn't work.
	The "module" categories are just an arbitrary way to group similar comamnds. It makes them easier to locate once you know where they are.
	
	*/
	// Restricts a command to game admins.
	// This is the right way to validate a command. I think it's ugly.
	// It's not much more useful than just doing that inside the command, but at least it cuts redundancy.
	public class RequireGameAdminAttribute : PreconditionAttribute
	{
		private ulong[] whitelist;
		public RequireGameAdminAttribute()
		{
			whitelist = Globals.gameadmins;
		}
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (whitelist.Contains(context.User.Id)) return Task.FromResult(PreconditionResult.FromSuccess());
			else return Task.FromResult(PreconditionResult.FromError("This command is restricted to game administrators."));
		}
	}
	// These serve no practical purpose. Well, besides checking if the bot can hear you.
	public class EasterEggModule : InteractiveBase<SocketCommandContext>
	{
		// The oldest command in the game.
		[Command("turnthatpoop", RunMode = RunMode.Async)]
		[Summary("easter egg")]
		public async Task TurnThatPoop()
		{
			await Context.Channel.SendMessageAsync("*into wine*");
		}
		// I'm a computer
		[Command("biggestthingintheworld", RunMode = RunMode.Async)]
		[Summary("easter egg")]
		public async Task BiggestThingInTheWorld()
		{
			await Context.Channel.SendMessageAsync("https://www.youtube.com/watch?v=G9FGgwCQ22w");
		}
		// I'm a computery guy
		[Command("showtime", RunMode = RunMode.Async)]
		[Summary("easter egg")]
		public async Task Showtime()
		{
			await Context.Channel.SendMessageAsync("https://www.youtube.com/watch?v=kfVaA2vLytM");
		}
		// Caus wanted this for posting right before the reset
		[Command("hold", RunMode = RunMode.Async)]
		[Summary("easter egg")]
		public async Task EasterHold()
		{
			await Context.Channel.SendMessageAsync("https://www.youtube.com/watch?v=9uNLn75BEFA&feature=youtu.be&t=46");
		}
		[Command("test", RunMode = RunMode.Async)]
		[Summary("easter egg")]
		public async Task TestCommand()
		{
			await Context.Channel.SendMessageAsync("test");
		}
		}
	public class GameplayModule : InteractiveBase<SocketCommandContext>
	{
		// I had to separate the logic of rolling from the command itself to make sure that rollall is both easy to access (ra and r a both work) and without redundant code.
		// This first command applies when there's an int parameter given.
		[Command("roll", RunMode = RunMode.Async)]
		[Alias("r")]
		[RequireContext(ContextType.Guild)]
		public async Task RollDefault(int count=1)
		{
			await Roll(Context.Guild, Context.Channel, Context.User, count);
		}
		// This applies when a string parameter is given. It just checks for "roll all" with a space in between.
		[Command("roll", RunMode = RunMode.Async)]
		[Alias("r")]
		[RequireContext(ContextType.Guild)]
		public async Task RollWithParam(string entry)
		{
			if ((new string[] {"a", "all"}).Contains(entry.ToLower())) await Roll(Context.Guild, Context.Channel, Context.User, 1, true);
		}
		// Checks for rollall with no space.
		[Command("rollall", RunMode = RunMode.Async)]
		[Alias("ra")]
		[RequireContext(ContextType.Guild)]
		public async Task RollAll()
		{
			await Roll(Context.Guild, Context.Channel, Context.User, 1, true);
		}
		// The actual mechanism of rolling. Doesn't technically count as a command but it should be here anyway.
		// Kind of ugly because of all of the nested objects with long names, but at least the command is small.
		// Those objects do most of the work instead of the command.
		public async Task Roll(SocketGuild guild, ISocketMessageChannel channel, SocketUser user, int count, Boolean all=false)
		{
			int maxcount = Globals.rollingsystem.RollsLeft(guild.Id, user.Id);
			if (maxcount < 1)
			{
				await channel.SendMessageAsync((Globals.rollingsystem.RollsLeftString(guild, user)));
				return;
			}
			if (count > maxcount || all) count = maxcount;
			if (count < 1) count = 1;
			if (Globals.rollingsystem.rollqueues.ContainsKey(channel as SocketTextChannel)) Globals.rollingsystem.rollqueues[channel as SocketTextChannel] += count;
			else Globals.rollingsystem.rollqueues.Add(channel as SocketTextChannel, count);
			if (Globals.rollingsystem.rolldeductions.ContainsKey(guild.Id))
			{
				if (Globals.rollingsystem.rolldeductions[guild.Id].ContainsKey(user.Id)) Globals.rollingsystem.rolldeductions[guild.Id][user.Id] += count;
				else Globals.rollingsystem.rolldeductions[guild.Id].Add(user.Id, count);
			}
			else
			{
				Globals.rollingsystem.rolldeductions.Add(guild.Id, new Dictionary<ulong, int> {{user.Id, count}});
			}
			if (!Globals.rollingsystem.rollingloops.ContainsKey(channel as SocketTextChannel))
			{
				Globals.rollingsystem.rollingloops.Add(channel as SocketTextChannel, Globals.rollingsystem.RollingLoop(channel as SocketTextChannel));
				Globals.rollingsystem.rollingloops[channel as SocketTextChannel].Start();
			}
		}
		// Another small command that passes work to the big object.
		[Command("rollcheck", RunMode = RunMode.Async)]
		[Alias("rc")]
		[RequireContext(ContextType.Guild)]
		public async Task RollCheck()
		{
			var m = Globals.rollingsystem.ClaimLeftString(Context.Guild, Context.User);
			m += "\n" + Globals.rollingsystem.RollsLeftString(Context.Guild, Context.User);
			await ReplyAsync(m);
		}
		// This one was tough to figure out, but it works now.
		// Let's hope we never have other sizes of deck. Yikes.
		[Command("editdeck", RunMode = RunMode.Async)]
		[Alias("ed")]
		[RequireContext(ContextType.Guild)]
		public async Task EditDeck(int cid, string coord)
		{
			// Verify valid inputs
			int row;
			int column;
			int oldrow = -1;
			int oldcolumn = -1;
			int occupier = -1;
			var deck = await SimDeck.CreateAsync(Context.User.Id, Context.Guild.Id);
			if (coord.Length != 2) return;
			try
			{
				column = ColumnOfLetter(coord[1]);
				row = Convert.ToInt32(coord[0].ToString());
				row--;
			}
			catch {return;}
			if (row < 0 || row > 2) return;
			var temp = deck.cards[row, column];
			if (temp != null)
			{
				if (cid == temp.cid)
				{
					await ReplyAsync("That card is already there.");
					return;
				}
				occupier = temp.cid;
			}
			for (int y=0; y<3; y++)
			{
				for (int x=0; x<5; x++)
				{
					if (deck.cards[y, x] != null)
					{
						if (deck.cards[y, x].cid == cid)
						{
							oldrow = y;
							oldcolumn = x;
						}
					}
				}
			}
			var card = await SimCard.CreateAsync(cid);
			if (card.uid != Context.User.Id)
			{
				await ReplyAsync("You don't own a card with that serial number.");
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				if (deck.cards[row, i] != null)
				{
					if (deck.cards[row, i].wclass == card.wclass && column != i)
					{
						await ReplyAsync("You can't put two cards of the same class in a hand. There's already a " + ClassOfChar(card.wclass) +
						" in Hand " + (row+1) + ".");
						return;
					}
				}
			}
			// In a position swap, make sure that the occupier isn't being placed invalidly
			if (occupier != -1 && oldrow != -1 && oldrow != row)
			{
				var forbidden = deck.cards[row, column].wclass;
				for (int x=0; x<5; x++)
				{
					if (deck.cards[oldrow, x] != null)
					{
						if (deck.cards[oldrow, x].wclass == forbidden && x != oldcolumn)
						{
							await ReplyAsync("You can't put two cards of the same class in a hand. There's already a " +
							ClassOfChar(deck.cards[oldrow, x].wclass) + " in Hand " + (oldrow+1) + ".");
							return;
						}
					}
				}
			}
			using (var c = Conn())
			{
				await c.OpenAsync();
				var trans = c.BeginTransaction();
				try{
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					//From out of hand to an empty slot
					if (oldrow == -1 && occupier == -1)
					{
						cmd.CommandText = "INSERT INTO in_hand (uid, sid, cid, hrow, hcolumn) VALUES (@uid, @sid, @cid, @hrow, @hcolumn);";
					}
					//From out of hand to an occupied slot
					else if (oldrow == -1 && occupier != -1)
					{
						cmd.CommandText = "UPDATE in_hand SET cid=@cid WHERE uid=@uid AND sid=@sid AND hrow=@hrow AND hcolumn=@hcolumn;";
					}
					//From in hand to an empty slot
					else if (oldrow != -1 && occupier == -1)
					{
						cmd.CommandText = "UPDATE in_hand SET hrow=@hrow, hcolumn=@hcolumn WHERE cid=@cid;";
					}
					//Position swap
					else
					{
						cmd.CommandText = "UPDATE in_hand SET hrow=@hrow, hcolumn=@hcolumn WHERE cid=@cid;" +
						" UPDATE in_hand SET hrow=@oldrow, hcolumn=@oldcol WHERE cid=@occ;";
					}
					cmd.Parameters.AddWithValue("cid", cid);
					cmd.Parameters.AddWithValue("hrow", row);
					cmd.Parameters.AddWithValue("hcolumn", column);
					if (oldrow == -1)
					{
						cmd.Parameters.AddWithValue("sid", Context.Guild.Id.ToString());
						cmd.Parameters.AddWithValue("uid", Context.User.Id.ToString());
					}
					else if (occupier != -1)
					{
						cmd.Parameters.AddWithValue("oldrow", oldrow);
						cmd.Parameters.AddWithValue("oldcol", oldcolumn);
						cmd.Parameters.AddWithValue("occ", occupier);
					}
					await cmd.ExecuteNonQueryAsync();
				}
				await trans.CommitAsync();
				} catch (Exception e) {await trans.RollbackAsync(); Console.WriteLine("Deck edit error: " + e.Message); return;}
			}
			await ReplyAsync("Deck edited.");
		}
		// Essentially just loops over code copied from editdeck.
		// Kind of an ad hoc solution, but it seems to work.
		[Command("sethand", RunMode = RunMode.Async)]
		[Alias("sh")]
		[RequireContext(ContextType.Guild)]
		public async Task SetHand(params int[] entry)
		{
			if (entry.Length == 0) {await SetHandHelp(); return;}
			if (entry.Length != 6) {await ReplyAsync("You must set the hand to have 5 cards."); return;}
			int row;
			int[] cids;
			SimCard[] cards;
			try {row = Convert.ToInt32(entry[0]); row--;}
			catch {return;}
			if (row < 0 || row > 2) return;
			cids = entry.Skip(1).ToArray();
			if (cids.Distinct().Count() != cids.Length) {await ReplyAsync("You can't give duplicate serial numbers."); return;}
			var temp = new List<SimCard>();
			foreach (int cid in cids)
			{
				try {temp.Add(await SimCard.CreateAsync(cid));}
				catch (Exception) {await ReplyAsync("Card #" + cid + " not found."); return;}
			}
			cards = temp.ToArray();
			
			var classes = new HashSet<char>();
			foreach (SimCard card in cards)
			{
				if (card.uid != Context.User.Id) {await ReplyAsync("You don't own card #" + card.cid + "."); return;}
				if (classes.Contains(card.wclass)) {await ReplyAsync("You can't have two cards of the " + ClassOfChar(card.wclass) + " class in the same hand."); return;}
				else classes.Add(card.wclass);
			}
			var deck = await SimDeck.CreateAsync(Context.User.Id, Context.Guild.Id);
			using (var c = Conn())
			{
				await c.OpenAsync();
				var trans = c.BeginTransaction();
				try{
				foreach (SimCard card in cards)
				{
					int column = Array.IndexOf(cards, card);
					int oldrow = -1;
					int oldcolumn = -1;
					int occupier = -1;
					for (int y=0; y<3; y++)
					{
						for (int x=0; x<5; x++)
						{
							if (deck.cards[y, x] != null)
							{
								if (deck.cards[y, x].cid == card.cid)
								{
									oldrow = y;
									oldcolumn = x;
								}
							}
						}
					}
					if (deck.cards[row, column] != null)
					{
						occupier = deck.cards[row, column].cid;
					}
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						//From out of hand to an empty slot
						if (oldrow == -1 && occupier == -1)
						{
							cmd.CommandText = "INSERT INTO in_hand (uid, sid, cid, hrow, hcolumn) VALUES (@uid, @sid, @cid, @hrow, @hcolumn);";
						}
						//From out of hand to an occupied slot
						else if (oldrow == -1 && occupier != -1)
						{
							cmd.CommandText = "UPDATE in_hand SET cid=@cid WHERE uid=@uid AND sid=@sid AND hrow=@hrow AND hcolumn=@hcolumn;";
						}
						//From in hand to an empty slot
						else if (oldrow != -1 && occupier == -1)
						{
							cmd.CommandText = "UPDATE in_hand SET hrow=@hrow, hcolumn=@hcolumn WHERE cid=@cid;";
						}
						//Position swap
						else
						{
							cmd.CommandText = "UPDATE in_hand SET hrow=@hrow, hcolumn=@hcolumn WHERE cid=@cid;" +
							" UPDATE in_hand SET hrow=@oldrow, hcolumn=@oldcol WHERE cid=@occ;";
						}
						cmd.Parameters.AddWithValue("cid", card.cid);
						cmd.Parameters.AddWithValue("hrow", row);
						cmd.Parameters.AddWithValue("hcolumn", column);
						if (oldrow == -1)
						{
							cmd.Parameters.AddWithValue("sid", Context.Guild.Id.ToString());
							cmd.Parameters.AddWithValue("uid", Context.User.Id.ToString());
						}
						else if (occupier != -1)
						{
							cmd.Parameters.AddWithValue("oldrow", oldrow);
							cmd.Parameters.AddWithValue("oldcol", oldcolumn);
							cmd.Parameters.AddWithValue("occ", occupier);
						}
						await cmd.ExecuteNonQueryAsync();
					}
				}
				await trans.CommitAsync();
				} catch (Exception e) {await trans.RollbackAsync(); Console.WriteLine("Deck bulk edit error: " + e.Message); return;}
			}
			await ReplyAsync("Hand set.");
		}
		public async Task SetHandHelp()
		{
			await ReplyAsync("To use `sethand`, enter the number of the hand to define as the first parameter and a list of card serial numbers afterward." +
			"\nSeparate them with spaces, like this: `sethand 2 500 1439 841 950 435`" +
			"\nThis will set that hand to contain those cards in the order given. You can see the results by using `deck`.");
		}
		[Command("sethero", RunMode = RunMode.Async)]
		[RequireContext(ContextType.Guild)]
		public async Task SetHero(string entry)
		{
			
			int cid;
			var deck = await SimDeck.CreateAsync(Context.User.Id, Context.Guild.Id);
			try
			{
				cid = Convert.ToInt32(entry);
			}
			catch (Exception)
			{
				cid = deck.cards[Convert.ToInt32(entry.Substring(1)) - 1, ColumnOfLetter(entry[0])].cid;
			}
			if (!deck.ContainsCard(cid)) {await ReplyAsync("Card #" + cid + " isn't in your hand."); return;}
			using (var c = Conn())
			{
				await c.OpenAsync();
				var trans = c.BeginTransaction();
				try{
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "UPDATE in_hand SET hero=FALSE WHERE uid=@uid AND sid=@sid;" +
					" UPDATE in_hand SET hero=TRUE WHERE cid=@cid;";
					cmd.Parameters.AddWithValue("uid", Context.User.Id.ToString());
					cmd.Parameters.AddWithValue("sid", Context.Guild.Id.ToString());
					cmd.Parameters.AddWithValue("cid", cid);
					await cmd.ExecuteNonQueryAsync();
				}
				await trans.CommitAsync();
				} catch (Exception e)
				{
					await trans.RollbackAsync();
					Console.WriteLine("Hero set error: " + e.Message);
					return;
				}
			}
			await ReplyAsync("Hero set.");
		}
		// Not especially complicated considering how important it is.
		// Above all else, do NOT let this command give cards screwed up levels or tiers.
		[Command("levelup", RunMode = RunMode.Async)]
		[Alias("lu")]
		[RequireContext(ContextType.Guild)]
		public async Task LevelUp(int cid)
		{
			var card = await SimCard.CreateAsync(cid);
			// To be consumed
			SimCard target;
			int targetid;
			string m = "";
			if (card.uid != Context.User.Id)
			{
				await ReplyAsync("You don't own that card. Level up cancelled.");
				return;
			}
			if (card.tier == Globals.MaxTier && card.level == Globals.MaxLevel)
			{
				await ReplyAsync(card.NickOrReal() + " has reached the pinnacle of perfection: maximum level and tier.");
				return;
			}
			if (card.level == Globals.MaxLevel)
			{
				m += card.NickOrReal() + " has reached maximum level. Instead of leveling up, this will remove all of its levels and upgrade its tier by one.\n";
			}
			m += "Enter the serial number of the card that " + card.NameTierLevel() + " will consume to level up.";
			await ReplyAsync(m);
			try {targetid = Convert.ToInt32((await NextMessageAsync(timeout: TimeSpan.FromSeconds(30))).Content);}
			catch (Exception) {await ReplyAsync("Couldn't interpret as an integer, level up cancelled."); return;}
			try {target = await SimCard.CreateAsync(targetid);}
			catch (Exception) {await ReplyAsync("Card not found, level up cancelled."); return;}
			if (target.cid == card.cid)
			{await ReplyAsync("A card can't consume itself."); return;}
			if (target.uid != Context.User.Id || target.sid != card.sid)
			{await ReplyAsync("You can't consume someone else's card."); return;}
			if (target.wid != card.wid) {await ReplyAsync("Both cards must be of the same warrior."); return;}
			await ReplyAsync("Are you sure you want " + target.NameTierLevel() + " to be consumed? (y/n)");
			// This is one of the only times that users are given a (y/n).
			// Are people really dumb enough to not use those properly? They can't be. Not if they can grasp the concept of a card game played through text commands.
			// With that in mind, I didn't bother with comparing the answer to some list of affirmatives. Just "y", "Y" or else.
			if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(30))).Content.ToLower() != "y")
			{await ReplyAsync("Level up cancelled."); return;}
			// Last moment validity checking.
			// You have to do this if it's been a while since the first validity check; a lot can change.
			card = await SimCard.CreateAsync(card.cid);
			target = await SimCard.CreateAsync(target.cid);
			if (card.uid != Context.User.Id || target.uid != Context.User.Id)
			{
				await ReplyAsync("One of the cards involved belongs to someone else now. Level up cancelled.");
				return;
			}
			using (var c = Conn())
			{
				await c.OpenAsync();
				var trans = c.BeginTransaction();
				try{
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					if (card.level == Globals.MaxLevel)
					{
						cmd.CommandText = "UPDATE card SET level=1, tier=@tier WHERE cid=@cid;";
						cmd.Parameters.AddWithValue("tier", card.tier + 1);
					}
					else
					{
						cmd.CommandText = "UPDATE card SET level=@level WHERE cid=@cid;";
						cmd.Parameters.AddWithValue("level", card.level + 1);
					}
					cmd.CommandText += " DELETE FROM owns_card WHERE cid=@target;";
					cmd.Parameters.AddWithValue("cid", card.cid);
					cmd.Parameters.AddWithValue("target", target.cid);
					await cmd.ExecuteNonQueryAsync();
				}
				await trans.CommitAsync();
				} catch (Exception e) {await trans.RollbackAsync(); await ReplyAsync("Unexpected error, level up cancelled."); Console.WriteLine("Level up error: " + e.Message); return;}
			}
			if (card.level == Globals.MaxLevel)
			{
				m = "Congratulations, " + card.NickOrReal() + " has reached Tier **" + LetterOfTier(card.tier + 1) + "**!";
			}
			else m = "Congratulations, " + card.NickOrReal() + " has reached Level **" + (card.level + 1) + "**!";
			await ReplyAsync(m);
		}
	}
	public class CollectionModule : InteractiveBase<SocketCommandContext>
	{
		// Eventually I want a more robust sort editor. This is inconvenient but simple.
		[Command("sortcollection", RunMode = RunMode.Async)]
		[RequireContext(ContextType.Guild)]
		[Alias("sc")]
		public async Task SortCollection(params int[] cids)
		{
			if (cids.Length == 0)
			{
				await SortCollectionHelp();
				return;
			}
			using (var c = Conn())
			{
				await c.OpenAsync();
				var trans = c.BeginTransaction();
				try{
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "UPDATE owns_card SET customorder=NULL WHERE uid=@uid AND sid=@sid;";
					for (int i=0; i<cids.Length; i++)
					{
						cmd.CommandText += "UPDATE owns_card SET customorder=" + i + " WHERE cid=@card" + i + " AND uid=@uid AND sid=@sid; ";
						cmd.Parameters.AddWithValue("card" + i, cids[i]);
					}
					cmd.Parameters.AddWithValue("sid", Context.Guild.Id.ToString());
					cmd.Parameters.AddWithValue("uid", Context.User.Id.ToString());
					await cmd.ExecuteNonQueryAsync();
				}
				await trans.CommitAsync();
				} catch (Exception e)
				{
					await trans.RollbackAsync();
					Console.WriteLine("Custom sort error: " + e.Message);
					return;
				}
			}
			await ReplyAsync("Collection sorted.");
		}
		public async Task SortCollectionHelp()
		{
			await ReplyAsync("To use `sortcollection`, enter the serial numbers of your cards in the order that you want them sorted by default." +
			"\nExample: `sortcollection 3454 2434 5423 7694 2356 2347`");
		}
		// I made several commands for the collection viewer since it takes several types of optional inputs.
		// No params collection viewer.
		[Command("mycollection", RunMode = RunMode.Async)]
		[Alias("mc")]
		[RequireContext(ContextType.Guild)]
		public async Task InfoCollectionParser()
		{
			await InfoCollection(Context, null);
		}
		// One param string.
		[Command("mycollection", RunMode = RunMode.Async)]
		[Alias("mc")]
		[RequireContext(ContextType.Guild)]
		public async Task InfoCollectionParser(string s)
		{
			if ((new string[] {"u", "unique"}).Contains(s.ToLower())) await InfoCollection(Context, null, -1, true);
			else await InfoCollection(Context, s);
		}
		// One param int.
		[Command("mycollection", RunMode = RunMode.Async)]
		[Alias("mc")]
		[RequireContext(ContextType.Guild)]
		public async Task InfoCollectionParser(int searchindex)
		{
			await InfoCollection(Context, null, searchindex);
		}
		// Two params, unique?
		[Command("mycollection", RunMode = RunMode.Async)]
		[Alias("mc")]
		[RequireContext(ContextType.Guild)]
		public async Task InfoCollectionParser(string sortmethod, string u)
		{
			if ((new string[] {"u", "unique"}).Contains(u.ToLower())) await InfoCollection(Context, sortmethod, -1, true);
		}
		// Two params, a string and an int.
		[Command("mycollection", RunMode = RunMode.Async)]
		[Alias("mc")]
		[RequireContext(ContextType.Guild)]
		public async Task InfoCollectionParser(string sortmethod, int searchindex)
		{
			await InfoCollection(Context, sortmethod, searchindex);
		}
		[Command("trade", RunMode = RunMode.Async)]
		[Alias("t")]
		[RequireContext(ContextType.Guild)]
		public async Task Trade(string entry)
		{
			var userA = Context.User;
			SocketUser userB;
			SimCard cardA;
			SimCard cardB;
			try {userB = Context.Guild.GetUser(Convert.ToUInt64(entry));}
			catch (Exception) {userB = Context.Message.MentionedUsers.First();}
			if (userB.Id == userA.Id) {await ReplyAsync("You can't trade with yourself."); return;}
			if (userB.IsBot || userA.IsBot) {await ReplyAsync("Bots cannot engage in commerce. It would make us too powerful."); return;}
			InfoCollection(Context, null, -1, false, userA);
			//Console.WriteLine("asdf");
			await ReplyAsync(userA.Mention + " What will you offer to " + userB.Mention + "? Enter a card serial number.");
			int cidA;
			try {cidA = Convert.ToInt32((await NextMessageAsync(timeout: TimeSpan.FromSeconds(30))).Content);}
			catch (Exception) {await ReplyAsync("Couldn't interpret as an integer, trade cancelled."); return;}
			try {cardA = await SimCard.CreateAsync(cidA);}
			catch (Exception) {await ReplyAsync("Card not found, trade cancelled."); return;}
			if (cardA.uid != userA.Id || cardA.sid != Context.Guild.Id) {await ReplyAsync("You don't own that card, trade cancelled."); return;}
			await InfoCardMessage.CreateAsync(cardA.cid, Context.Channel as SocketTextChannel, Context.Client);
			await Task.Delay(2000);
			InfoCollection(Context, null, -1, false, userB);
			await ReplyAsync(userB.Mention + " What will you offer to " + userA.Mention + "? Enter a card serial number.");
			int cidB;
			var crit = new Criteria<SocketMessage>() {};
			crit.AddCriterion(new EnsureFromUserCriterion(userB.Id));
			crit.AddCriterion(new EnsureSourceChannelCriterion());
			try {cidB = Convert.ToInt32((await NextMessageAsync(criterion:  crit, timeout: TimeSpan.FromSeconds(30))).Content);}
			catch (Exception) {await ReplyAsync("Couldn't interpret as an integer, trade cancelled."); return;}
			try {cardB = await SimCard.CreateAsync(cidB);}
			catch (Exception) {await ReplyAsync("Card not found, trade cancelled."); return;}
			if (cardB.uid != userB.Id || cardA.sid != cardB.sid) {await ReplyAsync("You don't own that card, trade cancelled."); return;}
			await InfoCardMessage.CreateAsync(cardB.cid, Context.Channel as SocketTextChannel, Context.Client);
			await ReplyAsync(userA.Mention + " Will you give " + cardA.NameTierLevel() + " receive " + cardB.NameTierLevel() + "? (y/n)");
			if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(30))).Content.ToLower() != "y")
			{await ReplyAsync("Trade cancelled."); return;}
			// Last moment validity checking for sneaky fuckers who lose cards in the time they get for inputs.
			cardA = await SimCard.CreateAsync(cardA.cid);
			cardB = await SimCard.CreateAsync(cardB.cid);
			if (cardA.uid != userA.Id || cardB.uid != userB.Id)
			{
				await ReplyAsync("One of the cards being traded belongs to someone else now. Trade cancelled.");
				return;
			}
			using (var c = Conn())
			{
				await c.OpenAsync();
				var trans = c.BeginTransaction();
				try{
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "DELETE FROM owns_card WHERE cid=@cida OR cid=@cidb;" +
					" INSERT INTO owns_card (uid, sid, cid) VALUES (@uida, @sid, @cidb), (@uidb, @sid, @cida);" +
					" INSERT INTO trade (sid, usera, userb, carda, cardb) VALUES (@sid, @uida, @uidb, @cida, @cidb);";
					cmd.Parameters.AddWithValue("cida", cardA.cid);
					cmd.Parameters.AddWithValue("cidb", cardB.cid);
					cmd.Parameters.AddWithValue("uida", userA.Id.ToString());
					cmd.Parameters.AddWithValue("uidb", userB.Id.ToString());
					cmd.Parameters.AddWithValue("sid", Context.Guild.Id.ToString());
					await cmd.ExecuteNonQueryAsync();
				}
				await trans.CommitAsync();
				} catch (Exception e) {await trans.RollbackAsync(); await ReplyAsync("Unexpected error, trade cancelled."); Console.WriteLine("Trade error: " + e.Message); return;}
			}
			await ReplyAsync("Trade completed.");
		}
		// The actual mechanism of the collection viewer.
		// Will this slow down some day when there are too many cards? I wonder...
		public async Task InfoCollection(SocketCommandContext Context, string sortmethod, int searchindex=-1, bool unique=false, SocketUser user=null)
		{
			/*
			Sort methods:
				User ordered (null) (default)
				Date aquired (d)
				Alphabetical (a)
				Merit (m)
				Serial (s)
				Unique (u) (Not actually for sorting, just gives a card count of each warrior)
			*/
			if (user == null) user = Context.User;
			var names = new List<string>();
			//For unique
			var counts = new Dictionary<string, int>();
			var realnames = new List<string>();
			var cids = new List<int>();
			var tiers = new List<int>();
			var levels = new List<int>();
			var lines = new List<string>();
			try{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					// Fancy SQL string generator.
					cmd.CommandText = "SELECT c.cid, c.tier, c.level, w.wname, ns.nbody FROM owns_card o INNER JOIN card AS c" +
					" ON c.cid=o.cid INNER JOIN is_warrior AS iw ON iw.cid=o.cid INNER JOIN warrior AS w ON w.wid=iw.wid" +
					" LEFT JOIN nickname_selected AS ns ON ns.cid=o.cid WHERE o.uid=@uid AND o.sid=@sid";
					if (sortmethod == null)
					{
						cmd.CommandText += " ORDER BY o.customorder ASC, o.aquired DESC;";
					}
					else if ((new string[] {"d", "date", "dateaquired", "date_aquired", "date-aquired"}).Contains(sortmethod.ToLower()))
					{
						cmd.CommandText += " ORDER BY o.aquired DESC;";
					}
					else if ((new string[] {"a", "alphabetical", "alphabet"}).Contains(sortmethod.ToLower()))
					{
						cmd.CommandText += " ORDER BY w.wname ASC, c.tier ASC, c.level DESC;";
					}
					else if ((new string[] {"m", "merit"}).Contains(sortmethod.ToLower()))
					{
						cmd.CommandText += " ORDER BY c.tier ASC, c.level DESC, w.wname ASC;";
					}
					else if ((new string[] {"s", "serial", "serialnumber", "serial_number", "serial-number"}).Contains(sortmethod.ToLower()))
					{
						cmd.CommandText += " ORDER BY c.cid ASC;";
					}
					else return;
					if (unique)
					{
						cmd.CommandText += " SELECT w.wname, COUNT(*) FROM owns_card o INNER JOIN is_warrior AS iw ON iw.cid=o.cid" +
						" INNER JOIN warrior AS w ON w.wid=iw.wid WHERE o.uid=@uid AND o.sid=@sid GROUP BY w.wname;";
					}
					cmd.Parameters.AddWithValue("uid", user.Id.ToString());
					cmd.Parameters.AddWithValue("sid", Context.Guild.Id.ToString());
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						while (await dr.ReadAsync())
						{
							cids.Add(dr.GetInt32(0));
							tiers.Add(dr.GetInt32(1));
							levels.Add(dr.GetInt32(2));
							if (await dr.IsDBNullAsync(4)) names.Add(dr.GetString(3));
							else names.Add(dr.GetString(4));
							realnames.Add(dr.GetString(3));
						}
						if (unique)
						{
							await dr.NextResultAsync();
							while (await dr.ReadAsync()) counts.Add(dr.GetString(0), dr.GetInt32(1));
						}
					}
				}
			}
			// A specific card has been looked up.
			if (searchindex != -1 && !unique)
			{
				int cid;
				try {cid = cids[searchindex - 1];}
				catch {return;}
				await InfoCardMessage.CreateAsync(cid, Context.Channel as SocketTextChannel, Context.Client);
				return;
			}
			// A list of all cards has been requested.
			if (unique)
			{
				var taken = new HashSet<string>();
				for (int i=0; i<cids.Count; i++)
				{
					if (counts[realnames[i]] < 2) lines.Add(names[i] +
					" [" + LetterOfTier(tiers[i]) + "] [" + levels[i] + "] [#" + cids[i] + "]");
					else if (!taken.Contains(realnames[i]))
					{
						lines.Add(realnames[i] + " (" + counts[realnames[i]] + ")");
						taken.Add(realnames[i]);
					}
				}
			}
			else
			{
				for (int i=0; i<cids.Count; i++) lines.Add("**" + (i+1) + ".** " + names[i] +
				" [" + LetterOfTier(tiers[i]) + "] [" + levels[i] + "] [#" + cids[i] + "]");
			}
			var b = new EmbedBuilder();
			b.WithColor(Color.DarkGrey);
			b.WithAuthor(NickOrReal(user.Id, Context.Guild) + "'s Cards", user.GetAvatarUrl());
			await MultiPageParsesLines.CreateAsync(lines.ToArray(), b, Context.Channel as SocketTextChannel);
			} catch (Exception e) {Console.WriteLine("Collection view error: " + e.Message);}
		}
	}
	public class InfoModule : InteractiveBase<SocketCommandContext>
	{
		// These are some of the oldest commands because they were the best way to see what was happening with the construction of the most fundamental features.
		// They're probably going to need some polish later on.
		[Command("infoexpansion", RunMode = RunMode.Async)]
		[Alias("ie")]
		public async Task InfoExpansion(params string[] entry)
		{
			var embed = new EmbedBuilder();
			embed.WithColor(Color.DarkGrey);
			var lines = new List<string>();
			if (entry.Length == 0)
			{
				var expansions = new List<string>();
				var stamps = new List<DateTime>();
				embed.WithAuthor("Every Expansion");
				using (var c = Conn())
				{
					await c.OpenAsync();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "SELECT ename, estamp FROM expansion ORDER BY estamp DESC;";
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							while (await dr.ReadAsync())
							{
								expansions.Add(dr.GetString(0));
								stamps.Add(dr.GetDateTime(1));
							}
						}
					}
				}
				for (int i=0;i<expansions.Count;i++) lines.Add(expansions[i] + " **(" + stamps[i].ToShortDateString() + ")**");
				await MultiPageParsesLines.CreateAsync(lines.ToArray(), embed, Context.Channel as SocketTextChannel);
				return;
			}
			int eid;
			string ename;
			DateTime estamp;
			var warriors = new List<string>();
			var tiers = new List<int>();
			var wids = new List<int>();
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT eid, ename, estamp FROM expansion WHERE LOWER(ename)=LOWER(@n);";
					cmd.Parameters.AddWithValue("n", String.Join(' ', entry));
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (!dr.HasRows) return;
						await dr.ReadAsync();
						eid = dr.GetInt32(0);
						ename = dr.GetString(1);
						estamp = dr.GetDateTime(2);
					}
				}
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT wname, starttier, wid FROM warrior WHERE " +
					"wid IN (SELECT wid FROM in_expansion WHERE eid=@i) ORDER BY starttier ASC;";
					cmd.Parameters.AddWithValue("i", eid);
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						while (await dr.ReadAsync())
						{
							warriors.Add(dr.GetString(0));
							tiers.Add(dr.GetInt32(1));
							wids.Add(dr.GetInt32(2));
						}
					}
				}
			}
			for (int i=0;i<warriors.Count;i++) lines.Add(warriors[i] + " [" + LetterOfTier(tiers[i]) + "] [#" + wids[i].ToString() + "]");
			embed.WithAuthor(ename);
			embed.WithTimestamp(estamp);
			await MultiPageParsesLines.CreateAsync(lines.ToArray(), embed, Context.Channel as SocketTextChannel);
		}
		[Command("infoseries", RunMode = RunMode.Async)]
		[Alias("is")]
		public async Task InfoSeries(params string[] entry)
		{
			var embed = new EmbedBuilder();
			embed.WithColor(Color.DarkGrey);
			var lines = new List<string>();
			if (entry.Length == 0)
			{
				embed.WithAuthor("Every Series");
				using (var c = Conn())
				{
					await c.OpenAsync();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "SELECT sname FROM series ORDER BY sname ASC;";
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							while (await dr.ReadAsync()) lines.Add(dr.GetString(0));
						}
					}
				}
				await MultiPageParsesLines.CreateAsync(lines.ToArray(), embed, Context.Channel as SocketTextChannel);
				return;
			}
			int sid;
			string sname;
			string simgurl;
			var warriors = new List<string>();
			var tiers = new List<int>();
			var wids = new List<int>();
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT * FROM series WHERE LOWER(sname)=LOWER(@n);";
					cmd.Parameters.AddWithValue("n", String.Join(' ', entry));
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (!dr.HasRows) return;
						await dr.ReadAsync();
						sid = dr.GetInt32(0);
						sname = dr.GetString(1);
						simgurl = dr.GetString(2);
					}
				}
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT wname, starttier, wid FROM warrior WHERE " +
					"wid IN (SELECT wid FROM in_series WHERE sid=@i) ORDER BY starttier ASC;";
					cmd.Parameters.AddWithValue("i", sid);
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						while (await dr.ReadAsync())
						{
							warriors.Add(dr.GetString(0));
							tiers.Add(dr.GetInt32(1));
							wids.Add(dr.GetInt32(2));
						}
					}
				}
			}
			for (int i=0;i<warriors.Count;i++) lines.Add(warriors[i] + " [" + LetterOfTier(tiers[i]) + "] [#" + wids[i].ToString() + "]");
			embed.WithAuthor(sname);
			embed.WithThumbnailUrl(simgurl);
			await MultiPageParsesLines.CreateAsync(lines.ToArray(), embed, Context.Channel as SocketTextChannel);
		}
		[Command("infowarrior", RunMode = RunMode.Async)]
		[Alias("iw")]
		public async Task InfoWarrior(params string[] entry)
		{
			try{
			var embed = new EmbedBuilder();
			//List all warriors
			if (entry.Length == 0)
			{
				embed.WithAuthor("All Warriors");
				embed.WithColor(Color.DarkGrey);
				var lines = new List<string>();
				using (var c = Conn())
				{
					await c.OpenAsync();
					try{
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "SELECT wname, starttier FROM warrior ORDER BY wname ASC;";
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							while (await dr.ReadAsync()) lines.Add(dr.GetString(0) + " " + UIEmote("smalltier" + dr.GetInt32(1)));
						}
					}
					} catch (Exception e) {Console.WriteLine(e.Message);}
				}
				await MultiPageParsesLines.CreateAsync(lines.ToArray(), embed, Context.Channel as SocketTextChannel);
				return;
			}
			//Look up a particular warrior
			int wid;
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n);";
					cmd.Parameters.AddWithValue("n", String.Join(' ', entry));
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (dr.HasRows)
						{
							await dr.ReadAsync();
							wid = dr.GetInt32(0);
						}
						else
						{
							try {wid = Convert.ToInt32(entry[0]);}
							catch {return;}
						}
					}
				}
			}
			var warrior = await SimWarrior.CreateAsync(wid);
			var desc = "**Warrior ID:** " + wid.ToString() + "\n**Series:** ";
			if (warrior.ssecondary == null) desc += warrior.sprimary;
			else desc += warrior.ssecondary + " (" + warrior.sprimary + ")";
			if (warrior.nicks.Length > 0)
			{
				desc += "\n**Nicknames:** ";
				foreach (string n in warrior.nicks) desc += n + ", ";
				desc = desc.Substring(0, desc.Length - 2);
			}
			desc += "\n**Types:** ";
			foreach (int t in warrior.tids) desc += NameOfType(t) + ", ";
			desc = desc.Substring(0, desc.Length - 2);
			if (warrior.baseform.Item2 != null) desc += "\n**Base Form:** " + warrior.baseform.Item2;
			else if (warrior.components.Length > 0) desc += "\n**Components:** " + warrior.components[0].Item2 + ", " + warrior.components[1].Item2;
			if (warrior.fusions.Length > 0)
			{
				desc += "\n**Fusions:** ";
				foreach ((int, string, int, string) f in warrior.fusions) desc += f.Item4 + ", ";
				desc = desc.Substring(0, desc.Length - 2);
			}
			if (warrior.transformations.Length > 0)
			{
				desc += "\n**Transformations:** ";
				foreach ((int, string) t in warrior.transformations) desc += t.Item2 + ", ";
				desc = desc.Substring(0, desc.Length - 2);
			}
			desc += "\n**Tier:** " + LetterOfTier(warrior.starttier) + "\n**Class:** " +
			ClassOfChar(warrior.wclass) + "\n**HP multiplier:** " + warrior.hp.ToString() +
			"\n**DMG multiplier:** " + warrior.dmg.ToString() + "\n**Ability:** " +
			NameOfAbility(warrior.aid) + " " + Roman(warrior.astren);
			try {embed.WithAuthor(warrior.wname, Emote.Parse(warrior.emotestr).Url);}
			catch (Exception e) {
				Console.WriteLine("Emote of " + warrior.wname + " not found: " + e.Message);
				embed.WithAuthor(warrior.wname);}
			embed.WithDescription(desc);
			var portraits = warrior.portraitsfree.Concat(new string[] {warrior.portraitfinishing})
			.Concat(new string[] {warrior.portrait_s}).Concat(warrior.portraitsextra).ToArray();
			embed.AddField("Portrait", "**(1/" + portraits.Length.ToString() + ")** Free");
			embed.WithImageUrl(portraits[0]);
			if (warrior.expansion != null) embed.WithFooter(warrior.expansion);
			embed.WithColor(ColorOfTier(warrior.starttier));
			await InfoWarriorMessage.CreateAsync(portraits, embed.Build(), Context.Channel as SocketTextChannel);
			} catch (Exception e) {Console.WriteLine("Infowarrior error: " + e.Message);}
		}
		// This one is messy because it's gone through so many changes, and will probably go through a lot more.
		// One of the most complicated parts of the game, and one of the hardest to design.
		[Command("mydeck", RunMode = RunMode.Async)]
		[Alias("md")]
		[RequireContext(ContextType.Guild)]
		public async Task MyDeck()
		{
			try{
			//var slotsstr = new string[] {"A1", "B1", "C1", "D1", "E1", "A2", "B2", "C2", "D2", "E2", "A3", "B3", "C3", "D3", "E3"};
			var deck = await SimDeck.CreateAsync(Context.User.Id, Context.Guild.Id);
			deck.ModelAbilitiesForViewer();
			//var numemotes = new string[] {":one:", ":two:", ":three:"};
			//var letteremojis = new string[] {":regional_indicator_a:", ":regional_indicator_b:", ":regional_indicator_c:", ":regional_indicator_d:", ":regional_indicator_e:"};
			var tables = new string[3];
			for (int hand=0; hand<3; hand++)
			{
				string table = UIEmote("blankslot") + UIEmote("deckview");
				for (int col=0; col<5; col++)
				{
					if (deck.hero == null) table += UIEmote("col" + col + "reg");
					else if (deck.hero.column == col) table += UIEmote("col" + col + "hero");
					else table += UIEmote("col" + col + "reg");
				}
				for (int row=0; row<3; row++)
				{
					if (row == hand) table += "\n" + UIEmote("handindicator");
					else table += "\n" + UIEmote("blankslot");
					if (deck.hero == null) table  += UIEmote("row" + row + "reg");
					else if (deck.hero.row == row) table += UIEmote("row" + row + "hero");
					else table += UIEmote("row" + row + "reg");
					for (int col=0; col<5; col++)
					{
						if (deck.cards[row, col] != null) table += deck.cards[row, col].emotestr;
						else table += UIEmote("nocard");
					}
					//table += "\n";
				}
				tables[hand] = table;
			}
			var handsa = new string[3];
			var handsb = new string[3];
			var combos = new string[3];
			var body = "";
			for (int row=0; row<3; row++)
			{
				handsa[row] = UIEmote("cardpos") + UIEmote("class") + UIEmote("lvltier") + UIEmote("card") + "\n";
				for (int col=0; col<5; col++)
				{
					var card = deck.cards[row, col];

					//HANDS A
					//Pos
					if (card == null) handsa[row] += UIEmote("col" + col + "reg");
					else if (card.hero) handsa[row] += UIEmote("col" + col + "hero");
					else handsa[row] += UIEmote("col" + col + "reg");
					//Class
					if (card == null) handsa[row] += UIEmote("nocard") + UIEmote("nocard") + UIEmote("nocard");
					else handsa[row] += UIEmote("class" + card.wclass);
					if (card != null)
					{
						//Lvltier
						handsa[row] += UIEmote(LetterOfTier(card.tier).ToLower() + card.level);
						//Card
						handsa[row] += card.emotestr;
						handsa[row] += " [**HP: **" + card.HandedHp() + "] [**DMG: **" + card.HandedDmg() + "]";
						
					}
					handsa[row] += "\n";

					
					{/*
					body = "_ _";
					if (card != null)
					{
						body = "**HP:** " + card.HandedHp() + " | **DMG:** " + card.HandedDmg() + "\n";
						body += "**" + AbilityAndStrength(card.aid, card.astren) + "**";
						if (card.auraability) body += " (" + deck.AdjacentCards(row, col).Length + "/2 cards affected)";
						if (card.baseform.Item2 != null)
						{
							if (deck.transformations.Select(set => set.Item2).Contains(card)) body += "\nTransformation from " + deck.cards[row - 1, col].NickOrReal() + " active, "
							+ "**+20% HP and DMG**";
							else body += "\nTransformation from " + card.baseform.Item2 + " inactive";
						}
						if (card.components.Length > 0)
						{
							if (deck.fusions.Select(set => set.Item3).Contains(card)) body += "\nFusion of " + deck.cards[row - 1, col].NickOrReal() + " and " +
							deck.cards[row - 2, col].NickOrReal() + " active, " + "**+30% HP and DMG**";
							else body += "\nFusion of **" + card.components[0].Item2 + "** and **" + card.components[1].Item2 + "** inactive";
						}
					}
					carddescs[row, col] = body;
					*/}
				}
				handsb[row] = UIEmote("cardpos") + UIEmote("card") + "\n";
				for (int col=0; col<5; col++)
				{
					var card = deck.cards[row, col];
					//Pos
					if (card == null) handsb[row] += UIEmote("col" + col + "reg");
					else if (card.hero) handsb[row] += UIEmote("col" + col + "hero");
					else handsb[row] += UIEmote("col" + col + "reg");
					if (card != null)
					{
						//Card
						handsb[row] += card.emotestr;
						handsb[row] += " [#" + card.cid + "] **" + AbilityAndStrength(card.aid, card.astren) + ":** ";
						if (card.auraability) foreach (SimCardForDeck c in deck.AdjacentCards(card.row, card.column)) handsb[row] += c.emotestr;
						else if (card.debuffability) handsb[row] += UIEmote("enemy");
						else if (card.herobuffability)
						{
							if (deck.hero == null) handsb[row] += UIEmote("nocard");
							else handsb[row] += deck.hero.emotestr;
						}
						else handsb[row] += card.emotestr;
					}
					else handsb[row] += UIEmote("nocard");
					handsb[row] += "\n";
				}
				body = "None";
				if (deck.typequints[row].Count > 0) body = "**+10% HP and DMG to the 5 " + NameOfType(deck.typequints[row].Keys.First()) + " cards.";
				else if (deck.seriesquints[row].Count > 0) body = "**+10% HP and DMG** to the 5 " + deck.seriesquints[row].Keys.First() + " cards.";
				else if (deck.typetriples[row].Count > 0) body = "**+5% HP and DMG** to the " + deck.typetriples[row].First().Value.Count + " " + NameOfType(deck.typetriples[row].Keys.First()) + " cards.";
				else if (deck.seriestriples[row].Count > 0) body = "**+5% HP and DMG** to the " + deck.seriestriples[row].First().Value.Count + " " + deck.seriestriples[row].Keys.First() + " cards.";
				combos[row] = body;
			}
			await DeckViewMessage.CreateAsync(Context.Channel as SocketTextChannel, (NickOrReal(Context.User.Id, Context.Guild), Context.User.GetAvatarUrl()), tables, handsa, handsb, combos);
			} catch (Exception e) {Console.WriteLine("Deck viwer error: " + e.Message);}
		}
		// Pass the work to an object since this isn't the only command that uses that code.
		[Command("infocard", RunMode = RunMode.Async)]
		[Alias("ic")]
		[RequireContext(ContextType.Guild)]
		public async Task InfoCard(int cid)
		{
			await InfoCardMessage.CreateAsync(cid, Context.Channel as SocketTextChannel, Context.Client);
		}
	}
	public class GameAdminCommandModule : InteractiveBase<SocketCommandContext>
	{
		[Command("admintest", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task AdminTest()
		{
			await Context.Channel.SendMessageAsync("You are a game administrator.");
		}
		// Deaf mode makes the bot ignore all commands that aren't from an admin.
		// Intended for testing the bot in scenarios where letting users play would be either dangerous for us or annoying for them.
		// Like if the bot is running on an old copy of the database. That would cause some panic if people saw it.
		[Command("deaf", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task Deaf()
		{
			await _commandhandler.ToggleDeaf();
			if (_commandhandler._deaf) await ReplyAsync("Deaf mode on.");
			else await ReplyAsync("Deaf mode off.");
		}
		// Cheats.
		[Command("resetmyclaim", RunMode = RunMode.Async)]
		[Alias("resetmyclaims")]
		[RequireGameAdminAttribute()]
		[RequireContext(ContextType.Guild)]
		public async Task ResetMyClaim()
		{
			if (Globals.rollingsystem.usedclaims.ContainsKey(Context.Guild.Id))
			{
				Globals.rollingsystem.usedclaims[Context.Guild.Id].Remove(Context.User.Id);
			}
			await ReplyAsync(Context.User.Mention + " You can now claim immediately.");
		}
		[Command("resetmyrolls", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		[RequireContext(ContextType.Guild)]
		public async Task ResetMyRolls()
		{
			if (Globals.rollingsystem.rolldeductions.ContainsKey(Context.Guild.Id))
			{
				Globals.rollingsystem.rolldeductions[Context.Guild.Id].Remove(Context.User.Id);
			}
			await ReplyAsync(Context.User.Mention + " You can now roll immediately.");
		}
		[Command("givemecards", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		[RequireContext(ContextType.Guild)]
		public async Task GiveMeCards(int count)
		{
			for(int i=0;i<count;i++)
			{
				var warrior = await SimWarrior.CreateAsync(await Globals.cardgenerator.RandWidOfTier(Globals.cardgenerator.RandTier()));
				var tiermod = Globals.cardgenerator.LimitTierMod(warrior.starttier, Globals.cardgenerator.RandTierMod());
				var level = Globals.cardgenerator.RandLevel();
				await Globals.cardgenerator.GiveCard(warrior.wid, (warrior.starttier + tiermod), level,
				Context.Guild.Id, Context.User.Id);
			}
			await ReplyAsync(count + " random cards given.");
		}
		// These are the admin tools used to add more content. If you had to write SQL strings to do that, it would be such a pain in the ass.
		// These are some of the oldest commands in the game, and aren't visible to anyone except game staff.
		// Those two things combined are why they tend to be jankier than other commands.
		// They might need an overhaul some day since I didn't know what I was doing when I made them. But they work.
		[Command("startexpansion", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task StartExpansion()
		{
			string ename;
			await ReplyAsync("What's the name of the new expansion?");
			ename = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				using (var c = Conn())
				{
					await c.OpenAsync();
					var t = c.BeginTransaction();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE expansion SET active=FALSE WHERE active=TRUE;" +
						"INSERT INTO expansion (ename, active) VALUES (@e, TRUE);";
						cmd.Parameters.AddWithValue("e", ename);
						await cmd.ExecuteNonQueryAsync();
					}
					await t.CommitAsync();}
					catch (Exception e)
					{
						await t.RollbackAsync();
						await ReplyAsync("Expansion not added: `" + e.Message + "`");
						return;
					}
				}
			await ReplyAsync("All warriors added from now until you use the !endexpansion command will automatically count as " +
			"being part of `" + ename + "`.");
		}
		[Command("endexpansion", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task EndExpansion()
		{
			string ename;
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT ename FROM expansion WHERE active=TRUE;";
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (!dr.HasRows)
						{
							await ReplyAsync("ERROR: no expansion is active.");
							return;
						}
						await dr.ReadAsync();
						ename = dr.GetString(0);
					}
				}
				await ReplyAsync("Are you sure that there are no more warriors to add to `" + ename + "`? (y/n)");
				if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content != "y") return;
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "UPDATE expansion SET active=FALSE WHERE active=TRUE;";
					await cmd.ExecuteNonQueryAsync();
				}
			}
			await ReplyAsync("`" + ename + "` is now closed.");
		}
		[Command("addseries", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task AddSeries()
		{
			await Context.Channel.SendMessageAsync("Enter the name of the series.");
			var name = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			await Context.Channel.SendMessageAsync("Enter the imgur url of the series thumbnail.\n" +
			"**MAKE SURE THAT THE LINK ENDS WITH A FILE EXTENSION AND THAT IT ISN'T JUST THE PAGE IMGUR SHOWS YOU.**\n" +
			"You can get the proper link with \"copy image location\" or \"view image\".");
			var thumb = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			if (!ValidImgur(thumb))
			{
				await Context.Channel.SendMessageAsync("Series not added: invalid url.");
				return;
			}
			await Context.Channel.SendMessageAsync("Are you sure you want to make a series called `" +
			name + "`? (y/n)\nPlease make sure it's not already in the game. I would check, but it could be worded slightly differently.");
			var reponse = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			if (reponse != "y") {await Context.Channel.SendMessageAsync("Series not added."); return;}
			try{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "INSERT INTO series (sname, simgurl) VALUES (@n, @i);";
					cmd.Parameters.AddWithValue("n", name);
					cmd.Parameters.AddWithValue("i", thumb);
					await cmd.ExecuteNonQueryAsync();
				}
			}
			await Context.Channel.SendMessageAsync("Series added.");
			}
			catch (Exception e) {await Context.Channel.SendMessageAsync("Unexpected error. Series not added."); Console.WriteLine(e.Message);}
		}
		[Command("editseries", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task EditSeries()
		{
			string newname = null;
			string newthumb = null;
			await Context.Channel.SendMessageAsync("Enter the name of the series to edit.");
			var name = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			await Context.Channel.SendMessageAsync("Enter the field that you want to change." +
			"\nValid Fields: `name`, `thumbnail`");
			var temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			switch (temp.ToLower())
			{
				case "name":
					await ReplyAsync("Enter the new name.");
					newname = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					break;
				case "thumbnail":
					await ReplyAsync("Enter the new thumbnail.");
					newthumb = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					if (!ValidImgur(newthumb))
					{
						await ReplyAsync("Invalid link.");
						return;
					}
					break;
				default:
					await Context.Channel.SendMessageAsync("Field not recognized.");
					return;
			}
			try{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					if (newname != null)
					{
						cmd.CommandText = "UPDATE series SET sname=@n WHERE LOWER(sname)=LOWER(@o);";
						cmd.Parameters.AddWithValue("n", newname);
					}
					if (newthumb != null)
					{
						cmd.CommandText = "UPDATE series SET simgurl=@t WHERE LOWER(sname)=LOWER(@o);";
						cmd.Parameters.AddWithValue("t", newthumb);
					}
					cmd.Parameters.AddWithValue("o", name);
					if (await cmd.ExecuteNonQueryAsync() == 1) await Context.Channel.SendMessageAsync("Series edited.");
					else await Context.Channel.SendMessageAsync("It didn't work. Are you sure you spelled the series right?");
				}
			}
			}
			catch (Exception e) {await Context.Channel.SendMessageAsync("Unexpected error. Series not edited."); Console.WriteLine(e.Message);}
		}
		[Command("deleteseries", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task DeleteSeries()
		{
			await Context.Channel.SendMessageAsync("Enter the name of the series to delete.");
			var sname = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			int sid;
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT sid FROM series WHERE LOWER(sname)=LOWER(@n);";
					cmd.Parameters.AddWithValue("n", sname);
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (!dr.HasRows)
						{
							await Context.Channel.SendMessageAsync("Series not found.");
							return;
						}
						await dr.ReadAsync();
						sid = dr.GetInt32(0);
					}
				}
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT 1 FROM in_series WHERE sid=@i AND sprimary=TRUE;";
					cmd.Parameters.AddWithValue("i", sid);
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (dr.HasRows)
						{
							await Context.Channel.SendMessageAsync("Reassign or delete all of its warriors first. Every warrior needs a series.");
							return;
						}
					}
				}
			}
			await Context.Channel.SendMessageAsync("Are you sure you want to delete `" + sname + "`? (y/n)");
			var reponse = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			if (reponse != "y") {await Context.Channel.SendMessageAsync("Series not deleted."); return;}
			try{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "DELETE FROM series WHERE sid=@i;";
					cmd.Parameters.AddWithValue("i", sid);
					await cmd.ExecuteNonQueryAsync();
				}
			}
			}
			catch (Exception e) {await Context.Channel.SendMessageAsync("Unexpected error:" + e.Message); return;}
			await Context.Channel.SendMessageAsync("Series deleted.");
		}
		// The most important admin tool. Also the command that demands the most user inputs.
		[Command("addwarrior", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task AddWarrior()
		{
			bool exactive = false;
			string temp;
			//Build lists of abilities and types for later
			try{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT 1 FROM expansion WHERE active=TRUE;";
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						if (dr.HasRows) exactive = true;
					}
				}
			}} catch (Exception e) {Console.WriteLine(e.Message); return;}
			await ReplyAsync("Alright, this admin tool is now in Easy Mode.\n" +
			"If you fail at any step along the way, it'll let you retry as many times as you need.\n" +
			"Say \"cancel\" at any time to cancel making the warrior. If you actually need one of the fields" +
			" to be \"cancel\" just edit the warrior afterward, wise guy.\nAlso, feel free to look things up" +
			" with other commands during the process, **but only in a different channel.**");
			string name = null;
			//NEW! Progress saver
			bool exit = false;
			string cancelconf = "Cancelled.\nhttps://i.imgur.com/XUDyhT1.jpg";
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the name of the new warrior.");
				name = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (name == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "SELECT 1 FROM warrior WHERE LOWER(wname)=LOWER(@n)";
						cmd.Parameters.AddWithValue("n", name);
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							if (dr.HasRows)
							{
								await Context.Channel.SendMessageAsync("That name is already taken.\nMaybe try including the name of the series" +
								" afterward in parentheses. That's what Mudae does.");
								exit = false;
							}
						}
					}
				}
			}
			int primsid = -1;
			string primsname = null;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the name of the warrior's primary series." +
				"\nRemember that this is the LEAST specific series, within reason. Example: Love Live, not Love Live Sunshine.");
				primsname = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (primsname == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "SELECT sid FROM series WHERE LOWER(sname)=LOWER(@n)";
						cmd.Parameters.AddWithValue("n", primsname);
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							if (!dr.HasRows)
							{
								await Context.Channel.SendMessageAsync("Series not found.");
								exit = false;
							}
							else
							{
								await dr.ReadAsync();
								primsid = dr.GetInt32(0);
							}
						}
					}
				}
			}
			int secsid = -1;
			string secsname = null;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Do you also want it to have a secondary series? (y/n)");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				if (temp == "y")
				{
					await Context.Channel.SendMessageAsync("Enter the name of the secondary series.");
					secsname = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					if (secsname == "cancel") return;
					using (var c = Conn())
					{
						await c.OpenAsync();
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "SELECT sid FROM series WHERE LOWER(sname)=LOWER(@n)";
							cmd.Parameters.AddWithValue("n", secsname);
							using (var dr = await cmd.ExecuteReaderAsync())
							{
								if (!dr.HasRows)
								{
									await Context.Channel.SendMessageAsync("Series not found.");
									exit = false;
								}
								else
								{
									await dr.ReadAsync();
									secsid = dr.GetInt32(0);
								}
							}
						}
					}
				}
			}
			var baseids = new List<int>();
			string basename = null;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Is this warrior a transformation, a fusion, or neither? (t/f/n)");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (temp == "t")
				{
					await Context.Channel.SendMessageAsync("Enter the name of this warrior's base form.");
					basename = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					if (basename == "cancel")
					{
						await ReplyAsync(cancelconf);
						return;
					}
					using (var c = Conn())
					{
						await c.OpenAsync();
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n)";
							cmd.Parameters.AddWithValue("n", basename);
							using (var dr = await cmd.ExecuteReaderAsync())
							{
								if (!dr.HasRows)
								{
									await Context.Channel.SendMessageAsync("Base form not found.");
									exit = false;
								}
								else
								{
									await dr.ReadAsync();
									baseids.Add(dr.GetInt32(0));
								}
							}
						}
					}
				}
				else if (temp == "f")
				{
					await ReplyAsync("You can have a maximum of 2 components.\nEnter the name of one component.");
					basename = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					if (basename == "cancel")
					{
						await ReplyAsync(cancelconf);
						return;
					}
					await ReplyAsync("Enter the name of the other component.");
					var basename2 = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					if (basename2 == "cancel")
					{
						await ReplyAsync(cancelconf);
						return;
					}
					using (var c = Conn())
					{
						await c.OpenAsync();
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n);" +
							" SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n2);";
							cmd.Parameters.AddWithValue("n", basename);
							cmd.Parameters.AddWithValue("n2", basename2);
							using (var dr = await cmd.ExecuteReaderAsync())
							{
								if (!dr.HasRows)
								{
									await Context.Channel.SendMessageAsync("First component not found.");
									exit = false;
								}
								else
								{
									await dr.ReadAsync();
									baseids.Add(dr.GetInt32(0));
								}
								await dr.NextResultAsync();
								if (!dr.HasRows)
								{
									await Context.Channel.SendMessageAsync("Second component not found.");
									exit = false;
								}
								else
								{
									await dr.ReadAsync();
									baseids.Add(dr.GetInt32(0));
								}
							}
						}
					}
					if (baseids[0] == baseids[1])
					{
						await ReplyAsync("Duplicate components not allowed.");
						exit = false;
					}
				}
				else if (temp != "n")
				{
					await ReplyAsync("Invalid response.");
					exit = false;
				}
			}
			string wclass = null;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the class of the new warrior as a single letter.\n\"s\" = Master\n\"i\" = Maid\n\"d\" = Demon" +
				"\n\"c\" = Magician\n\"g\" = Guardian\n\"a\" = Attendant");
				wclass = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (wclass == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				if (!(new string[] {"s", "i", "d", "c", "g", "a"}).Contains(wclass))
				{
					await Context.Channel.SendMessageAsync("Class not recognized.");
					exit = false;
				}
			}
			int tier = -1;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the starting tier of the new warrior. (a/b/c/d/f)");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				tier = TierOfLetter(temp.ToUpper());
				if (tier > Globals.MaxTier - 1 || tier < Globals.MinTier)
				{
					await Context.Channel.SendMessageAsync("Invalid tier.");
					exit = false;
				}
			}
			float hpmult = -1;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the HP multiplier of the new warrior.\n" +
				"This should be between 0.7 and 1.3 unless it's an extreme case.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				try 
				{
					hpmult = Convert.ToSingle(temp);
					if (hpmult <= 0)
					{
						await Context.Channel.SendMessageAsync("It must be higher than 0.");
						exit = false;
					}
				}
				catch (Exception)
				{
					await ReplyAsync("Couldn't interpret as a number.");
					exit = false;
				}
			}
			float dmgmult = -1;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the DMG multiplier of the new warrior.\n" +
				"This should be between 0.7 and 1.3 unless it's an extreme case.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				try 
				{
					dmgmult = Convert.ToSingle(temp);
					if (dmgmult <= 0)
					{
						await Context.Channel.SendMessageAsync("It must be higher than 0.");
						exit = false;
					}
				}
				catch (Exception)
				{
					await ReplyAsync("Couldn't interpret as a number.");
					exit = false;
				}
			}
			var types = new HashSet<int>();
			string list = "";
			string currentlist;
			string current;
			string m;
			var validnames = Globals.NameOfType.ToDictionary(x => x.Value.ToLower(), x => x.Key);
			foreach (string t in Globals.NameOfType.Values)
			{
				list += "`" + t + "`, ";
			}
			list = list.Substring(0, list.Length - 2);
			exit = false;
			while (!exit)
			{
				currentlist = "";
				foreach (int t in types) currentlist += "`" + NameOfType(t) + "`, ";
				if (currentlist.Length > 0) currentlist = currentlist.Substring(0, currentlist.Length - 2);
				m = "Enter a type, or enter \"done\" if you're done adding types.\n" +
				"**Valid types:** " + list + "\n**Selected types:** " + currentlist;
				if (types.Count > 1) m += "\nRemember that more than 2 types should be rare.";
				await ReplyAsync(m);
				current = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (current == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (current == "done")
				{
					if (types.Count < 1)
					{
						await ReplyAsync("Must have a minimum of 1 type.");
					}
					else exit = true;
				}
				else if (validnames.Keys.Contains(current)) types.Add(validnames[current]);
				else
				{
					await Context.Channel.SendMessageAsync("Type not recognized.");
				}
			}
			int ability = -1;
			int astren = -1;
			list = "";
			foreach (string x in Globals.NameOfAbility.Values) {list += "`" + x + "`, ";}
			list = list.Substring(0, list.Length - 2);
			validnames = Globals.NameOfAbility.ToDictionary(x => x.Value.ToLower(), x => x.Key);
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the ability of the new warrior.\n**Valid abilities:** " + list);
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				if (validnames.ContainsKey(temp)) ability = validnames[temp];
				else
				{
					await Context.Channel.SendMessageAsync("Ability not recognized. If you were trying to specify the strength of the" +
					" ability, remember that comes afterward.");
					exit = false;
				}
			}
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the ability's strength as an integer between 1 and 4.\nI can't tell you exactly " +
				"what the strength of this ability does; that info is probably in a spreadsheet somewhere.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				try 
				{
					astren = Convert.ToInt32(temp);
					if (astren < 1 || astren > 4)
					{
						await ReplyAsync("Strength too high or low.");
						exit = false;
					}
				}
				catch (Exception)
				{
					await ReplyAsync("Couldn't interpret as an integer.");
					exit = false;
				}
			}
			string emotebody = null;
			exit = false;
			while (!exit)
			{
				exit = true;
				await Context.Channel.SendMessageAsync("Enter the warrior's emote as you would normally to make an emote show in discord." +
				"\nThe bot MUST be in the server that hosts this emote.\nThe bot can't find emotes after they get renamed, so " +
				"if you need to rename an emote, tell the bot to use a different one first.");
				emotebody = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (emotebody == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				emotebody = emotebody.Replace(" ", string.Empty);
				try {Emote test = Emote.Parse(emotebody);}
				catch (Exception)
				{
					await ReplyAsync("There's something wrong with this emote.");
					exit = false;
				}
			}
			var nicks = new HashSet<string>();
			while (true)
			{
				currentlist = "";
				foreach (string n in nicks) currentlist += "`" + n + "`, ";
				if (currentlist.Length > 0) currentlist = currentlist.Substring(0, currentlist.Length - 2);
				await ReplyAsync("Nicknames are optional.\nEnter a nickname, or enter \"done\" if you're done entering nicknames." +
				"\n**Nicknames:** " + currentlist);
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (temp == "done") break;
				nicks.Add(temp);
			}
			var portraits = new List<string>();
			exit = false;
			while (!exit)
			{
				exit = true;
				await ReplyAsync("The next fields all require imgur links. **MAKE SURE YOU'RE LINKING TO THE " +
				"IMAGE AND NOT THE ALBUM, OR THAT DUMB PAGE IT SHOWS YOU AFTER UPLOADING. Hint: the link should end with .png, .jpg, or .gif.**" +
				"\nFirst, the warrior needs two free portraits. Enter the default portrait that the emote is from.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (!ValidImgur(temp))
				{
					await Context.Channel.SendMessageAsync("Link invalid.");
					exit = false;
				}
				else portraits.Add(temp);
			}
			exit = false;
			while (!exit)
			{
				exit = true;
				await ReplyAsync("Enter the second free portrait.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (!ValidImgur(temp))
				{
					await Context.Channel.SendMessageAsync("Link invalid.");
					exit = false;
				}
				else if (portraits.Contains(temp))
				{
					await Context.Channel.SendMessageAsync("Duplicate links not allowed.");
					exit = false;
				}
				else portraits.Add(temp);
			}
			exit = false;
			while (!exit)
			{
				exit = true;
				await ReplyAsync("There's a special portrait that only shows when the warrior is someone's favorite, and when" +
				" it's in that player's hand during a victory in battle. Enter the \"finishing move\" portrait.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (!ValidImgur(temp))
				{
					await Context.Channel.SendMessageAsync("Link invalid.");
					exit = false;
				}
				else if (portraits.Contains(temp))
				{
					await Context.Channel.SendMessageAsync("Duplicate links not allowed.");
					exit = false;
				}
				else portraits.Add(temp);
			}
			exit = false;
			while (!exit)
			{
				exit = true;
				await ReplyAsync("There's an even more special portrait that can only be unlocked by upgrading a card to S tier." +
				" Enter the S tier portrait.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (!ValidImgur(temp))
				{
					await Context.Channel.SendMessageAsync("Link invalid.");
					exit = false;
				}
				else if (portraits.Contains(temp))
				{
					await Context.Channel.SendMessageAsync("Duplicate links not allowed.");
					exit = false;
				}
				else portraits.Add(temp);
			}
			await Context.Channel.SendMessageAsync("Finally, we'll add the regular unlockable portraits." +
			"\nEnter the links one at a time, then enter \"done\" when you're done.");
			while (true)
			{
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (temp == "cancel")
				{
					await ReplyAsync(cancelconf);
					return;
				}
				else if (temp == "done") break;
				else if (!ValidImgur(temp)) await ReplyAsync("Link invalid.");
				else if (portraits.Contains(temp)) await ReplyAsync("Duplicate links not allowed.");
				else
				{
					portraits.Add(temp);
					await ReplyAsync("Portrait added.");
				}
			}
			//Enter everything into the database
			int wid;
			using (var c = Conn())
			{
				await c.OpenAsync();
				var transaction = c.BeginTransaction();
				try {
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "SELECT MAX(wid) FROM warrior;";
					using (var dr = await cmd.ExecuteReaderAsync())
					{
						await dr.ReadAsync();
						if (dr.GetValue(0) == DBNull.Value) wid = 1;
						else wid = dr.GetInt32(0) + 1;
					}
				}
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "INSERT INTO warrior (wid, wname, wclass, starttier, hp, dmg, emote, aid, astren) " +
					"VALUES (@wid, @name, @wclass, @tier, @hp, @dmg, @emote, @aid, @astren);";
					cmd.Parameters.AddWithValue("wid", wid);
					cmd.Parameters.AddWithValue("name", name);
					cmd.Parameters.AddWithValue("wclass", wclass[0]);
					cmd.Parameters.AddWithValue("tier", tier);
					cmd.Parameters.AddWithValue("hp", hpmult);
					cmd.Parameters.AddWithValue("dmg", dmgmult);
					cmd.Parameters.AddWithValue("emote", emotebody);
					cmd.Parameters.AddWithValue("aid", ability);
					cmd.Parameters.AddWithValue("astren", astren);
					await cmd.ExecuteNonQueryAsync();
				}
				//Rigid
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = " INSERT INTO in_series (wid, sid, sprimary) VALUES (@wid, @primsid, @true);";
					cmd.CommandText += " INSERT INTO has_portrait (wid, pimgurl, free, stier, finishing, pdefault) VALUES ";
					for (int i=0;i<2;i++)
					{
						if (i == 0)cmd.CommandText += "(@wid, @u" + i.ToString() + ", @true, @false, @false, @true), ";
						else cmd.CommandText += "(@wid, @u" + i.ToString() + ", @true, @false, @false, @false), ";
					}
					cmd.CommandText += "(@wid, @u2, @false, @false, @true, @false), ";
					cmd.CommandText += "(@wid, @u3, @false, @true, @false, @false);";
					//Convert values
					cmd.Parameters.AddWithValue("wid", wid);
					cmd.Parameters.AddWithValue("primsid", primsid);
					cmd.Parameters.AddWithValue("true", true);
					cmd.Parameters.AddWithValue("u0", portraits[0]);
					cmd.Parameters.AddWithValue("u1", portraits[1]);
					cmd.Parameters.AddWithValue("u2", portraits[2]);
					cmd.Parameters.AddWithValue("u3", portraits[3]);
					cmd.Parameters.AddWithValue("false", false);
					await cmd.ExecuteNonQueryAsync();
				}
				//Flexible
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					var typesordered = types.ToArray();
					cmd.CommandText = "INSERT INTO is_type (wid, tid) VALUES ";
					for (int i=0;i<types.Count;i++)
					{
						cmd.CommandText += "(@wid, @type" + i.ToString() + "), ";
						cmd.Parameters.AddWithValue("type" + i.ToString(), typesordered[i]);
					}
					cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 2) + ";";
					if (exactive)
					{
						cmd.CommandText += " INSERT INTO in_expansion (wid, eid) VALUES (@wid, (SELECT eid FROM expansion WHERE active=TRUE));";
					}
					if (baseids.Count == 1)
					{
						cmd.CommandText += " INSERT INTO transformation_of (transid, baseid) VALUES (@wid, @baseid);";
						cmd.Parameters.AddWithValue("baseid", baseids[0]);
					}
					if (baseids.Count == 2)
					{
						cmd.CommandText += " INSERT INTO fusion_of (compa, compb, fusion) VALUES (@b1, @b2, @wid);";
						cmd.Parameters.AddWithValue("b1", baseids[0]);
						cmd.Parameters.AddWithValue("b2", baseids[1]);
					}
					if (secsid != -1)
					{
						cmd.CommandText += " INSERT INTO in_series (wid, sid, sprimary) VALUES (@wid, @sec, @false);";
						cmd.Parameters.AddWithValue("sec", secsid);
						cmd.Parameters.AddWithValue("false", false);
					}
					if (nicks.Count != 0)
					{
						var nicksordered = nicks.ToArray();
						cmd.CommandText += " INSERT INTO has_nickname (wid, nbody) VALUES ";
						for (int i=0;i<nicks.Count;i++)
						{
							cmd.CommandText += "(@wid, @nick" + i.ToString() + "), ";
							cmd.Parameters.AddWithValue("nick" + i.ToString(), nicksordered[i]);
						}
						cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 2) + ";";
					}
					if (portraits.Count > 4)
					{
						cmd.CommandText += " INSERT INTO has_portrait (wid, pimgurl) VALUES ";
						for (int i=4;i<portraits.Count;i++)
						{
							cmd.CommandText += "(@wid, @port" + i.ToString() + "), ";
							cmd.Parameters.AddWithValue("port" + i.ToString(), portraits[i]);
						}
						cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 2) + ";";
					}
					//Convert values
					cmd.Parameters.AddWithValue("wid", wid);
					await cmd.ExecuteNonQueryAsync();
				}
				await transaction.CommitAsync();
				}
				catch (Exception e)
				{
					await Context.Channel.SendMessageAsync("The warrior couldn't be added to the database for some reason." +
					"\nIf this error occurs, it's Pasemi's fault (maybe also yours, maybe not). He'll want to see this: `" + e.Message +"`");
					await transaction.RollbackAsync();
					return;
				}
			}
			await Context.Channel.SendMessageAsync("**" + name + "** has been assigned the ID number `" +
			wid.ToString() + "`." +
			"\nCongratulations, a star was born today!");
		}
		// This doesn't use the SimWarrior class. I guess it would be shorter if it did, but why bother changing it?
		// It would still be REALLY long anyway. Kind of inevitable for a comprehensive editor.
		// It's barely been used, so there might be bugs that nobody has discovered.
		[Command("editwarrior", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task EditWarrior()
		{
			await ReplyAsync("Enter the name or ID number of the warrior to edit.");
			var temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			int wid = -1;
			string wname = null;
			try {wid = Convert.ToInt32(temp);}
			catch {}
			if (wid == -1) wname = temp;
			//Verify existence, or get ID, also get types and abilities
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					if (wid != -1)
					{
						cmd.CommandText = "SELECT 1 FROM warrior WHERE wid=@i;";
						cmd.Parameters.AddWithValue("i", wid);
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							if (!dr.HasRows)
							{
								await ReplyAsync("Nobody found with that ID.");
								return;
							}
						}
					}
					else
					{
						cmd.CommandText = "SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n);";
						cmd.Parameters.AddWithValue("n", wname);
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							if (!dr.HasRows)
							{
								await ReplyAsync("Nobody found with that name.");
								return;
							}
							await dr.ReadAsync();
							wid = dr.GetInt32(0);
						}
					}
				}
			}
			await ReplyAsync("Enter the field to edit." +
			"\nValid fields: `name`, `class`, `tier`, `hp`, `dmg`, `types`, `ability`, `transformation`, `fusion`, " +
			"`series`, `expansion`, `emote`, `portraits`, `nicknames`");
			var field = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
			if (field == "name")
			{
				await ReplyAsync("Enter the new name.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				using (var c = Conn())
				{
					await c.OpenAsync();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE warrior SET wname=@n WHERE wid=@i;";
						cmd.Parameters.AddWithValue("n", temp);
						cmd.Parameters.AddWithValue("i", wid);
						await cmd.ExecuteNonQueryAsync();
					}}
					catch (Exception e)
					{
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "class")
			{
				await ReplyAsync("Enter the new class as a single letter.\n\"s\" = Master\n\"i\" = Maid\n\"d\" = Demon" +
				"\n\"c\" = Magician\n\"g\" = Guardian\n\"a\" = Attendant");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (!(new string[] {"s", "i", "d", "c", "g", "a"}).Contains(temp.ToLower()))
				{
					await ReplyAsync("Class not recognized.");
					return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE warrior SET wclass=@n WHERE wid=@i;";
						cmd.Parameters.AddWithValue("n", temp.ToLower()[0]);
						cmd.Parameters.AddWithValue("i", wid);
						await cmd.ExecuteNonQueryAsync();
					}}
					catch (Exception e)
					{
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "tier")
			{
				await ReplyAsync("Enter the new tier. (a/b/c/d/f)");
				var tier = TierOfLetter((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content);
				if (tier > Globals.MaxTier - 1 || tier < Globals.MinTier)
				{
				await ReplyAsync("Invalid tier.");
				return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE warrior SET starttier=@n WHERE wid=@i;";
						cmd.Parameters.AddWithValue("n", tier);
						cmd.Parameters.AddWithValue("i", wid);
						await cmd.ExecuteNonQueryAsync();
					}}
					catch (Exception e)
					{
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "hp")
			{
				float hpmult;
				await ReplyAsync("Enter the new HP multiplier.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				try {hpmult = Convert.ToSingle(temp);}
				catch (Exception)
				{
					await ReplyAsync("Couldn't parse as a number.");
					return;
				}
				if (hpmult <= 0)
				{
					await ReplyAsync("Must be higher than 0.");
					return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE warrior SET hp=@n WHERE wid=@i;";
						cmd.Parameters.AddWithValue("n", hpmult);
						cmd.Parameters.AddWithValue("i", wid);
						await cmd.ExecuteNonQueryAsync();
					}}
					catch (Exception e)
					{
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "dmg")
			{
				float dmgmult;
				await ReplyAsync("Enter the new DMG multiplier.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				try {dmgmult = Convert.ToSingle(temp);}
				catch (Exception)
				{
					await ReplyAsync("Couldn't parse a number.");
					return;
				}
				if (dmgmult <= 0)
				{
					await ReplyAsync("Must be higher than 0.");
					return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE warrior SET dmg=@n WHERE wid=@i;";
						cmd.Parameters.AddWithValue("n", dmgmult);
						cmd.Parameters.AddWithValue("i", wid);
						await cmd.ExecuteNonQueryAsync();
					}}
					catch (Exception e)
					{
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "types")
			{
				var types = new HashSet<int>();
				var insideout = Globals.NameOfType.ToDictionary(x => x.Value.ToLower(), x => x.Key);
				string list = "";
				string currentlist;
				string current;
				string m;
				foreach (string x in Globals.NameOfType.Values) {list += "`" + x + "`, ";}
				list = list.Substring(0, list.Length - 2);
				var exit = false;
				while (!exit)
				{
					currentlist = "";
					foreach (int t in types) currentlist += "`" + NameOfType(t) + "`, ";
					if (currentlist.Length > 0) currentlist = currentlist.Substring(0, currentlist.Length - 2);
					m = "Enter a type, or enter \"done\" if you're done adding types.\n" +
					"**Valid types:** " + list + "\n**Selected types:** " + currentlist;
					if (types.Count > 1) m += "\nRemember that more than 2 types should be rare.";
					if (types.Count == 0) m = m + "All of the warrior's types will be overwritten.\n";
					await ReplyAsync(m);
					current = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower();
					if (current == "done")
					{
						if (types.Count < 1)
						{
							await ReplyAsync("Must have a minimum of 1 type.");
						}
						else exit = true;
					}
					else if (insideout.ContainsKey(current)) types.Add(insideout[current]);
					else
					{
						await Context.Channel.SendMessageAsync("Type not recognized.");
					}
				}
				var typesa = types.ToArray();
				using (var c = Conn())
				{
					await c.OpenAsync();
					var trans = c.BeginTransaction();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "DELETE FROM is_type WHERE wid=@i; INSERT INTO is_type (wid, tid) VALUES ";
						for (int i=0;i<typesa.Length;i++)
						{
							cmd.CommandText += "(@i, @t" + i.ToString() + "), ";
							cmd.Parameters.AddWithValue("t" + i.ToString(), typesa[i]);
						}
						cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 2) + ";";
						cmd.Parameters.AddWithValue("i", wid);
						await cmd.ExecuteNonQueryAsync();
					}
					await trans.CommitAsync();}
					catch (Exception e)
					{
						await trans.RollbackAsync();
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "ability")
			{
				string list = "";
				foreach (string x in Globals.NameOfAbility.Values) {list += "`" + x + "`, ";}
				list = list.Substring(0, list.Length - 2);
				var insideout = Globals.NameOfAbility.ToDictionary(x => x.Value.ToLower(), x => x.Key);
				int ability;
				int astrength;
				await ReplyAsync("Enter the warrior's new ability.\nValid abilities: " + list);
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				if (!insideout.ContainsKey(temp.ToLower()))
				{
					await ReplyAsync("Invalid ability.");
					return;
				}
				ability = insideout[temp.ToLower()];
				await ReplyAsync("Enter the ability's strength as an integer between 1 and 4.");
				temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				try {astrength = Convert.ToInt32(temp);}
				catch (Exception)
				{
					await ReplyAsync("Couldn't parse as an integer.");
					return;
				}
				if (astrength > 4 || astrength < 1)
				{
					await ReplyAsync("Too high or low.");
					return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE has_ability SET aid=@a, astrength=@s WHERE wid=@i";
						cmd.Parameters.AddWithValue("i", wid);
						cmd.Parameters.AddWithValue("a", ability);
						cmd.Parameters.AddWithValue("s", astrength);
						await cmd.ExecuteNonQueryAsync();
					}}
					catch (Exception e)
					{
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "transformation")
			{
				await ReplyAsync("Do you want this warrior to be a transformation of another? (y/n)");
				if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower() == "n")
				{
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM transformation_of WHERE transid=@i;";
							cmd.Parameters.AddWithValue("i", wid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else
				{
					await ReplyAsync("Enter the name of the warrior's base form.");
					var basename = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					int baseid;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n);";
							cmd.Parameters.AddWithValue("n", basename);
							using (var dr = await cmd.ExecuteReaderAsync())
							{
								if (!dr.HasRows)
								{
									await ReplyAsync("Nobody with that name was found.");
									return;
								}
								await dr.ReadAsync();
								baseid = dr.GetInt32(0);
							}
						}
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM transformation_of WHERE transid=@i; " +
							"DELETE FROM fusion_of WHERE fusion=@i; " +
							"INSERT INTO transformation_of (transid, baseid) VALUES (@i, @b);";
							cmd.Parameters.AddWithValue("i", wid);
							cmd.Parameters.AddWithValue("b", baseid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
			}
			else if (field == "fusion")
			{
				await ReplyAsync("Do you want this warrior to be a fusion? (y/n)");
				if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower() == "n")
				{
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM fusion_of WHERE fusion=@i;";
							cmd.Parameters.AddWithValue("i", wid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else
				{
					await ReplyAsync("You can have a maximum of 2 components.\nEnter the name of one component.");
					var basename = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					await ReplyAsync("Enter the name of the other component.");
					var basename2 = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					int baseid;
					int baseid2;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n);" +
							" SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@m);";
							cmd.Parameters.AddWithValue("n", basename);
							cmd.Parameters.AddWithValue("m", basename2);
							using (var dr = await cmd.ExecuteReaderAsync())
							{
								if (!dr.HasRows)
								{
									await ReplyAsync("First component not found.");
									return;
								}
								await dr.ReadAsync();
								baseid = dr.GetInt32(0);
								await dr.NextResultAsync();
								if (!dr.HasRows)
								{
									await ReplyAsync("Second component not found.");
									return;
								}
								await dr.ReadAsync();
								baseid2 = dr.GetInt32(0);
							}
						}
						if (baseid == baseid2)
						{
							await ReplyAsync("Duplicate components not allowed.");
							return;
						}
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM transformation_of WHERE transid=@i; " +
							"DELETE FROM fusion_of WHERE fusion=@i; " +
							"INSERT INTO fusion_of (compa, compb, fusion) VALUES (@a, @b, @i);";
							cmd.Parameters.AddWithValue("i", wid);
							cmd.Parameters.AddWithValue("b", baseid);
							cmd.Parameters.AddWithValue("a", baseid2);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
			}
			else if (field == "series")
			{
				string sname;
				await ReplyAsync("Which series do you want to change? (primary/secondary)");
				if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower() == "primary")
				{
					await ReplyAsync("Enter the name of the warrior's new primary series.");
					sname = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "UPDATE in_series SET sid=(SELECT sid FROM series WHERE LOWER(sname)=LOWER(@n)) " +
							"WHERE wid=@i AND sprimary=TRUE;";
							cmd.Parameters.AddWithValue("n", sname);
							cmd.Parameters.AddWithValue("i", wid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else
				{
					await ReplyAsync("Do you want the warrior to have a secondary series? (y/n)");
					if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower() == "n")
					{
						using (var c = Conn())
						{
							await c.OpenAsync();
							try {
							using (var cmd = new NpgsqlCommand())
							{
								cmd.Connection = c;
								cmd.CommandText = "DELETE FROM in_series WHERE wid=@i AND sprimary=FALSE;";
								cmd.Parameters.AddWithValue("i", wid);
								await cmd.ExecuteNonQueryAsync();
							}}
							catch (Exception e)
							{
								await ReplyAsync("Error: " + e.Message);
								return;
							}
						}
					}
					else
					{
						await ReplyAsync("Enter the name of the new secondary series.");
						sname = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
						using (var c = Conn())
						{
							await c.OpenAsync();
							var trans = c.BeginTransaction();
							try {
							using (var cmd = new NpgsqlCommand())
							{
								cmd.Connection = c;
								cmd.CommandText = "DELETE FROM in_series WHERE wid=@i AND sprimary=FALSE; " +
								"INSERT INTO in_series (wid, sid, sprimary) VALUES (@i, (SELECT sid FROM series WHERE LOWER(sname)=LOWER(@n)), FALSE);";
								cmd.Parameters.AddWithValue("i", wid);
								cmd.Parameters.AddWithValue("n", sname);
								await cmd.ExecuteNonQueryAsync();
							}
							await trans.CommitAsync();}
							catch (Exception e)
							{
								await trans.RollbackAsync();
								await ReplyAsync("Error: " + e.Message);
								return;
							}
						}
					}
				}
			}
			else if (field == "expansion")
			{
				await ReplyAsync("Do you want the warrior to be part of an expansion? (y/n)");
				if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content.ToLower() == "n")
				{
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM in_expansion WHERE wid=@i;";
							cmd.Parameters.AddWithValue("i", wid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else
				{
					await ReplyAsync("Enter the name of the expansion.");
					var ename = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM in_expansion WHERE wid=@i; INSERT INTO in_expansion (wid, eid) VALUES " +
							"(@i, (SELECT eid FROM expansion WHERE LOWER(ename)=LOWER(@n)));";
							cmd.Parameters.AddWithValue("i", wid);
							cmd.Parameters.AddWithValue("n", ename);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
			}
			else if (field == "emote")
			{
				await ReplyAsync("Enter the new emote.");
				var ebody = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				try {var test = Emote.Parse(ebody);}
				catch (Exception)
				{
					await ReplyAsync("Couldn't parse as an emote.");
					return;
				}
				using (var c = Conn())
				{
					await c.OpenAsync();
					try {
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = c;
						cmd.CommandText = "UPDATE warrior SET emote=@n WHERE wid=@i;";
						cmd.Parameters.AddWithValue("n", ebody);
						cmd.Parameters.AddWithValue("i", wid);
						await cmd.ExecuteNonQueryAsync();
					}}
					catch (Exception e)
					{
						await ReplyAsync("Error: " + e.Message);
						return;
					}
				}
			}
			else if (field == "portraits")
			{
				await ReplyAsync("What do you want to do with them? (add/replace/delete)");
				var action = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				string pbody;
				if (action == "add")
				{
					await ReplyAsync("Enter the new portrait's link.");
					pbody = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					if (!ValidImgur(pbody))
					{
						await ReplyAsync("Invalid link.");
						return;
					}
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "INSERT INTO has_portrait (wid, pimgurl) VALUES (@i, @n);";
							cmd.Parameters.AddWithValue("n", pbody);
							cmd.Parameters.AddWithValue("i", wid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else if (action == "replace")
				{
					await ReplyAsync("Enter the current URL of the portrait to be replaced. You can get it by right clicking on the embed.");
					var oldurl = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					await ReplyAsync("Now enter the new URL.");
					pbody = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					if (!ValidImgur(pbody))
					{
						await ReplyAsync("Invalid link.");
						return;
					}
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "UPDATE has_portrait SET pimgurl=@n WHERE pimgurl=@o AND wid=@i;";
							cmd.Parameters.AddWithValue("n", pbody);
							cmd.Parameters.AddWithValue("o", oldurl);
							cmd.Parameters.AddWithValue("i", wid);
							if (await cmd.ExecuteNonQueryAsync() == 0)
							{
								await ReplyAsync("It didn't work. Make sure that you enter the old portrait correctly, and that" +
								" the warrior doesn't already have the new portrait.");
								return;
							}
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else if (action == "delete")
				{
					await ReplyAsync("Enter the URL of the portrait to delete. You can get it by right clicking on the embed.");
					var oldurl = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "SELECT * FROM has_portrait WHERE pimgurl=@o AND wid=@i;";
							cmd.Parameters.AddWithValue("o", oldurl);
							cmd.Parameters.AddWithValue("i", wid);
							using (var dr = await cmd.ExecuteReaderAsync())
							{
								if (!dr.HasRows)
								{
									await ReplyAsync("Portrait not found.");
									return;
								}
								await dr.ReadAsync();
								if (dr.GetBoolean(2) || dr.GetBoolean(3) || dr.GetBoolean(4))
								{
									await ReplyAsync("This portrait is necessary for the warrior to function. Try replacing it instead.");
									return;
								}
							}
						}
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM has_portrait WHERE pimgurl=@o AND wid=@i;";
							cmd.Parameters.AddWithValue("o", oldurl);
							cmd.Parameters.AddWithValue("i", wid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else return;
			}
			else if (field == "nicknames")
			{
				await ReplyAsync("What do you want to do with them? (add/replace/delete)");
				var action = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
				string nbody;
				if (action == "add")
				{
					await ReplyAsync("Enter the new nickname.");
					nbody = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "INSERT INTO has_nickname VALUES (@i, @n);";
							cmd.Parameters.AddWithValue("n", nbody);
							cmd.Parameters.AddWithValue("i", wid);
							await cmd.ExecuteNonQueryAsync();
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else if (action == "replace")
				{
					await ReplyAsync("Enter the nickname to replace.");
					var oldnick = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					await ReplyAsync("Enter the new nickname.");
					nbody = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "UPDATE has_nickname SET nbody=@n WHERE nbody=@o AND wid=@i;";
							cmd.Parameters.AddWithValue("n", nbody);
							cmd.Parameters.AddWithValue("o", oldnick);
							cmd.Parameters.AddWithValue("i", wid);
							if (await cmd.ExecuteNonQueryAsync() == 0)
							{
								await ReplyAsync("It didn't work. Make sure to enter the old nickname correctly.");
								return;
							}
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else if (action == "delete")
				{
					await ReplyAsync("Enter the nickname to delete.");
					var oldnick = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
					using (var c = Conn())
					{
						await c.OpenAsync();
						try {
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = c;
							cmd.CommandText = "DELETE FROM has_nickname WHERE nbody=@o AND wid=@i;";
							cmd.Parameters.AddWithValue("o", oldnick);
							cmd.Parameters.AddWithValue("i", wid);
							if (await cmd.ExecuteNonQueryAsync() == 0)
							{
								await ReplyAsync("It didn't work. Make sure to enter the nickname correctly.");
								return;
							}
						}}
						catch (Exception e)
						{
							await ReplyAsync("Error: " + e.Message);
							return;
						}
					}
				}
				else return;
			}
			else
			{
				await ReplyAsync("Invalid field.");
				return;
			}
			await ReplyAsync("Warrior edited.");
		}
		[Command("deletewarrior", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]
		public async Task DeleteWarrior()
		{
			await ReplyAsync("Enter the name or ID number of the warrior to delete.");
			var temp = (await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content;
			int wid = -1;
			string wname = null;
			try {wid = Convert.ToInt32(temp);}
			catch {wname = temp;}
			//Verify existence, or get ID
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					if (wid != -1)
					{
						cmd.CommandText = "SELECT wname FROM warrior WHERE wid=@i;";
						cmd.Parameters.AddWithValue("i", wid);
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							if (!dr.HasRows)
							{
								await ReplyAsync("Nobody found with that ID.");
								return;
							}
							await dr.ReadAsync();
							wname = dr.GetString(0);
						}
					}
					else
					{
						cmd.CommandText = "SELECT wid FROM warrior WHERE LOWER(wname)=LOWER(@n);";
						cmd.Parameters.AddWithValue("n", wname);
						using (var dr = await cmd.ExecuteReaderAsync())
						{
							if (!dr.HasRows)
							{
								await ReplyAsync("Nobody found with that name.");
								return;
							}
							await dr.ReadAsync();
							wid = dr.GetInt32(0);
						}
					}
				}
			}
			await ReplyAsync("Are you sure you want to delete `" + wname + "`? (y/n)" +
			"\nThis will also delete its cosmetics, and destroy all of its cards or make them unusable. " +
			"You will also lose most of the stats the game has about it.");
			if ((await NextMessageAsync(timeout: TimeSpan.FromSeconds(200))).Content != "y") return;
			using (var c = Conn())
			{
				await c.OpenAsync();
				try {
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					cmd.CommandText = "DELETE FROM warrior WHERE wid=@i;";
					cmd.Parameters.AddWithValue("i", wid);
					await cmd.ExecuteNonQueryAsync();
				}}
				catch (Exception e)
				{
					await ReplyAsync("Error: " + e.Message);
					return;
				}
			}
			await ReplyAsync("`" + wname + "` has been claimed by the void.");
		}
	}
	// Simple commands that explain other commands.
	// Most of them have the same name as the real command, but take no parameters. That makes them easy to find.
	// However, not every help command is here. Sometimes they have to be elsewhere so that commands in other modules can access them. Pretty annoying.
	public class HelpModule : InteractiveBase<SocketCommandContext>
	{
		[Command("help", RunMode = RunMode.Async)]
		public async Task InfoHelp()
		{
			try {
			var builders = new EmbedBuilder[3];
			for (int i=0;i<builders.Length;i++) builders[i] = new EmbedBuilder();
			builders[0].WithAuthor("Welcome to Warriordream, the virtual trading card game!");
			builders[0].WithDescription(
			"This game is currently in alpha, which makes you one of the first to try it out. We appreciate your feedback." +
			"\nWarriordream is about obtaining and trading cards, then building a deck with them to compete with others.");
			builders[1].WithAuthor("Gameplay commands");
			builders[1].WithDescription(
			"`roll` (`r`) - Spawn a random card that anyone can claim. Click on the reaction to try to claim it, and a winner will be chosen randomly after 30 seconds. Include a number to roll multiple times." +
			"\n`rollall` (`ra`) - Roll as many times as possible." +
			"\n`rollcheck` (`rc`) - Check if you can roll and claim." +
			"\n`editdeck` (`ed`) - Place a card in your deck. Enter `editdeck` by itself for more details." +
			"\n`sethand` (`sh`) - Place several cards in your deck at the same time. Enter `sethand` by itself for more details." +
			"\n`sethero` - Designate a card as your hero, which makes it stronger. Enter `sethero` by itself for more details." +
			"\n`trade` (`t`) - Trade cards with another player. Enter `trade` by itself for more details." +
			"\n`levelup` (`lu`) - Upgrade one of your cards. Enter `levelup` by itself for more details.");
			builders[2].WithAuthor("Information commands");
			builders[2].WithDescription(
			"`sortcollection` (`sc`) - Give an order for `mycollection` to sort your cards with by default. Enter `sortcollection` by itself for more details." +
			"\n`mycollection` (`mc`) - View a list of cards that you own. By default it sorts in the order given by `sortcollection`, then by date aquired. Add `d` (date aquired), `a`, (alphabetical), `m` (merit), or `s` (serial number) as a parameter to specify a sort order. Add `u` (unique) as a parameter to compress the list to one entry per warrior. Add a number as a parameter to view the card in that position of your collection." +
			"\n`mydeck` (`md`) - View your deck." +
			"\n`infocard` (`ic`) - View a card. Enter `card` by itself for more details." +
			"\n`infowarrior` (`iw`) - Enter a warrior's name as a parameter to see its information, or leave it blank to see a list of all warriors." +
			"\n`infoseries` (`is`) - Enter a series' name as a parameter to see its information, or leave it blank to see a list of all series." +
			"\n`infoexpansion` (`ie`) - Enter an expansion's name to see its information, or leave it blank to see a list of all expansions.");
			foreach (EmbedBuilder b in builders)
			{
				b.WithColor(Color.DarkGrey);
				await Context.User.SendMessageAsync(embed: b.Build());
			}
			await ReplyAsync("Help message sent privately.");
			}
			catch (Exception e) {Console.WriteLine("Help error: " + e.Message);}
		}
		[Command("adminhelp", RunMode = RunMode.Async)]
		[RequireGameAdminAttribute()]

		public async Task AdminHelp()
		{
			string m = "**Admin help**" +
			"\n`help`: View commands that everyone can use." +
			"\n**Admin commands:**" +
			"\n`admintest` Check to see if the bot recognizes your authority. No parameters." +
			"\n`deaf`: Toggle deaf mode, which makes the bot ignore all commands except those from admins. No parameters." +
			"\n`resetmyclaim` Resets your ability to claim immediately. No parameters." +
			"\n`resetmyrolls` Resets your rolls immediately. No parameters." +
			"\n`givemecards` Gives you random cards. Enter the number of cards to give as a parameter." +
			"\n`startexpansion` Begin an expansion so that new warriors are marked as having been added in that expansion. No parameters." +
			"\n`endexpansion` End an expansion so that new warriors stop being marked as having been added in that expansion. No parameters." +
			"\n`addseries` Make a new series. No parameters." +
			"\n`editseries` Edit a series. No parameters." +
			"\n`deleteseries` Delete a series. No parameters." +
			"\n`addwarrior` Make a new warrior. No parameters." +
			"\n`editwarrior` Edit a warrior. No parameters." +
			"\n`deletewarrior` Delete a warrior. No parameters.";
			await Context.User.SendMessageAsync(m);
			await ReplyAsync("Admin help message sent privately.");
		}
		
		[Command("trade", RunMode = RunMode.Async)]
		[Alias("t")]
		[RequireContext(ContextType.Guild)]
		public async Task Trade()
		{
			await ReplyAsync("To use `trade`, either mention another player or paste their discord ID number." +
			"\nExamples: `trade @Victoria`, `trade 267679326479187968`" +
			"\nTrading in this game is a simple exchange of one card for another. The bot will guide you through the rest.");
		}
		[Command("editdeck", RunMode = RunMode.Async)]
		[Alias("ed")]
		public async Task EditDeck()
		{
			await ReplyAsync("To use `editdeck`, enter a card's serial number as the first parameter and the slot to put it in (in 1A form) as the second parameter." +
			"\nExample: `editdeck 500 2c`\nThis works even if the card was already in your hand, so it's good for moving things around. You can see the results by using `mydeck`.");
		}
		[Command("sethero", RunMode = RunMode.Async)]
		public async Task SetHero()
		{
			await ReplyAsync("To use `sethero`, enter the serial number of the card to make it your hero, or enter its position in your deck in A1 form. Either will work." +
			"\nExamples: `sethero 3841`, `sethero b4`" +
			"\nYour hero card recieves a 20% boost to HP and DMG.");
		}
		[Command("levelup", RunMode = RunMode.Async)]
		[Alias("lu")]
		[RequireContext(ContextType.Guild)]
		public async Task LevelUp()
		{
			await ReplyAsync("To use `levelup`, enter the serial number of a card to increase its level by one. You will then be prompted to sacrifice another card of the same warrior." +
			"\nExample: `levelup 3841`" +
			"\nEach level gives another 5% boost to HP and DMG." +
			"\nIf a card is at level 20 when you upgrade it, the upgrade will instead remove all of its levels and increase its tier by one." +
			"\nEach tier gives another 25% boost to HP and DMG, except for S tier which has the same effect on HP and DMG as A tier. S tier is just for bragging rights.");
		}
		[Command("card", RunMode = RunMode.Async)]
		[Alias("ic")]
		public async Task InfoCard()
		{
			await ReplyAsync("To use `infocard`, enter the serial number of the card you want to view." +
			"\nExample: `infocard 6480`");
		}
	}
	/*
	
	THE GUTS

	Most of this either wasn't written by me or was written by me against my will.
	What I mean is, the fundamental parts of getting a bot to work in this library don't make a lot of sense to me. I had to copy a lot of code from the docs.
	Even then the docs weren't nearly descriptive enough and I had to figure out the specifics on my own.
	I don't like to come down here unless I have to.

	*/
	public class Initialize
	{
		private readonly CommandService _commands;
		private readonly DiscordSocketClient _client;

		public Initialize(CommandService commands = null, DiscordSocketClient client = null)
		{
			_commands = commands ?? new CommandService();
			_client = client ?? new DiscordSocketClient();
		}

		public IServiceProvider BuildServiceProvider() => new ServiceCollection()
			.AddSingleton(_client)
			.AddSingleton(_commands)
			// You can pass in an instance of the desired type
			.AddSingleton<InteractiveService>()
			// ...or by using the generic method.
			//
			// The benefit of using the generic method is that 
			// ASP.NET DI will attempt to inject the required
			// dependencies that are specified under the constructor 
			// for us.
			.AddSingleton<CommandHandler>()
			.BuildServiceProvider();
	}
	// Simple enough. Tracks every reaction the bot sees and directs it to an object if it seems important.
	public class ReactionHandler
	{
		private readonly DiscordSocketClient _client;
		public ReactionHandler(DiscordSocketClient client)
		{
			_client = client;
			client.ReactionAdded += OnReact;
			client.ReactionRemoved += OnRemove;
		}
		public async Task OnReact(Cacheable<IUserMessage, UInt64> m, ISocketMessageChannel ch, SocketReaction reac)
		{
			if (reac.UserId == _client.CurrentUser.Id) return;
			if (Globals.multipagemessages.ContainsKey(m.Id))
			{
				Globals.multipagemessages[m.Id].PageTurn(reac.Emote.Name);
				return;
			}
			if (Globals.rollposts.ContainsKey(m.Id))
			{
				Globals.rollposts[m.Id].OnReact(reac.Emote, reac.UserId);
				return;
			}
		}
		public async Task OnRemove(Cacheable<IUserMessage, UInt64> m, ISocketMessageChannel ch, SocketReaction reac)
		{
			if (Globals.rollposts.ContainsKey(m.Id))
			{
				Globals.rollposts[m.Id].OnRemove(reac.Emote, reac.UserId);
				return;
			}
		}
	}
	// Tracks every post that the bot sees --> ??? --> commands happen
	// I added deaf mode as well.
	public class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IServiceProvider _services;
		public Boolean _deaf;
		public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, Boolean deaf)
		{
			_commands = commands;
			_client = client;
			_services = services;
			_deaf = deaf;
		}
		public async Task InstallCommandsAsync()
		{
			// Hook the MessageReceived event into our command handler
			_client.MessageReceived += HandleCommandAsync;

			// Here we discover all of the command modules in the entry 
			// assembly and load them. Starting from Discord.NET 2.0, a
			// service provider is required to be passed into the
			// module registration method to inject the 
			// required dependencies.
			//
			// If you do not use Dependency Injection, pass null.
			// See Dependency Injection guide for more information.
			await _commands.AddModulesAsync(assembly: System.Reflection.Assembly.GetEntryAssembly(), 
											services: _services);
		}
		private async Task HandleCommandAsync(SocketMessage messageParam)
		{
			// Don't process the command if it was a system message
			var message = messageParam as SocketUserMessage;
			if (message == null) return;

			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;

			// Determine if the message is a command based on the prefix and make sure no bots trigger commands
			if (!(message.HasCharPrefix('.', ref argPos) || 
				message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
				message.Author.IsBot)
				return;
			
			//Deaf Mode
			if (_deaf)
			{
				if (!Globals.gameadmins.Contains(message.Author.Id)) return;
			}

			// Create a WebSocket-based command context based on the message
			var context = new SocketCommandContext(_client, message);

			// Execute the command with the command context we just
			// created, along with the service provider for precondition checks.
			
			// Keep in mind that result does not indicate a return value
			// rather an object stating if the command executed successfully.
			var result = await _commands.ExecuteAsync(
				context: context, 
				argPos: argPos,
				services: _services);

			// Optionally, we may inform the user if the command fails
			// to be executed; however, this may not always be desired,
			// as it may clog up the request queue should a user spam a
			// command.
			// if (!result.IsSuccess)
			// await context.Channel.SendMessageAsync(result.ErrorReason);
		}
		// Deaf mode needs to persist when the bot goes down, but asking the database every time the bot sees a post is a terrible idea.
		// Solution: Consult the database once on startup. Remember what it said. Toggle in memory and in the database when commanded to. Simple enough.
		public async Task ToggleDeaf()
		{
			using (var c = Conn())
			{
				await c.OpenAsync();
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = c;
					if (_deaf) cmd.CommandText = "UPDATE deaf SET val=FALSE;";
					else cmd.CommandText = "UPDATE deaf SET val=TRUE;";
					await cmd.ExecuteNonQueryAsync();
				}
			}
			if (_deaf) _deaf = false;
			else _deaf = true;
		}
	}
	public async Task Ready()
	{
		await _client.SetGameAsync(".help");
	}
	static void Main(string[] args)
	{
		Globals.Initialize();
		new Program().MainAsync().GetAwaiter().GetResult();
	}
	public async Task MainAsync()
	{
		_client = new DiscordSocketClient();
		_client.Log += Log;
		_client.Ready += Ready;

		// Consult database for startup config. Only does a little for now. If it gets more complicated maybe give it a function.
		Boolean deaf = false;
		using (var c = Conn())
		{
			await c.OpenAsync();
			using (var cmd = new NpgsqlCommand())
			{
				cmd.Connection = c;
				cmd.CommandText = "SELECT 1 FROM deaf WHERE val=TRUE;";
				using (var r = await cmd.ExecuteReaderAsync())
				{
					if (r.HasRows)
					{
						deaf = true;
						Console.WriteLine("Starting in deaf mode");
					}
				}
			}
		}

		//Start command handler
		_commandhandler = new CommandHandler(_client, new CommandService(), new Initialize().BuildServiceProvider(), deaf);
		await _commandhandler.InstallCommandsAsync();

		//Start reaction handler
		_reactionhandler = new ReactionHandler(_client);

		//Start rolling system
		Globals.rollingsystem = new RollingSystem(_client);

		//Start card generator
		Globals.cardgenerator = new CardGenerator();

		// Get bot token from external file
		var exeDir = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		var tokenPath = System.IO.Path.Combine(exeDir, "warriordream_client_token.txt");
		await _client.LoginAsync(TokenType.Bot, 
			System.IO.File.ReadAllText(tokenPath).TrimEnd());
		await _client.StartAsync();
		
		
		// Block this task forever.
		await Task.Delay(-1);
	}
	// Maybe I'll use this later. I've just been using Console.Writeline instead.
	private Task Log(LogMessage msg)
	{
		Console.WriteLine(msg.ToString());
		return Task.CompletedTask;
	}
	}
}