using ProjectM;
using System.Text.RegularExpressions;
using VPlus.Core;
using VPlus.Core.Commands;
using VampireCommandFramework;
using Unity.Entities;
using DateTime = System.DateTime;
using VPlus.Core.Toolbox;
using Databases = VPlus.Data.Databases;
using VCreate.Core.Toolbox;

namespace VPlus.Augments.Rank
{
    public class RankData
    {
        public int Rank { get; set; }
        public int Points { get; set; }
        public List<int> Buffs { get; set; } = [];

        public DateTime LastAbilityUse { get; set; }

        public int RankSpell { get; set; }

        public int SpellRank { get; set; }

        public List<int> Spells { get; set; } = new List<int>();

        public string ClassChoice { get; set; } = "default";

        public bool FishingPole { get; set; }

        public RankData(int rank, int points, List<int> buffs, int rankSpell, List<int> spells, string classchoice, bool fishingPole)
        {
            Rank = rank;
            Points = points;
            Buffs = buffs;
            LastAbilityUse = DateTime.MinValue; // Initialize to ensure it's always set
            RankSpell = rankSpell;
            Spells = spells;
            ClassChoice = classchoice;
            FishingPole = fishingPole;
        }
    }

    internal class PvERankSystem
    {
        public static void RankUp(ChatCommandContext ctx, string playerName, ulong SteamID, RankData data)
        {
            List<int> playerBuffs = data.Buffs;

            // reset points, increment rank level, grant buff, save data
            if (Plugin.BuffRewardsRankUp)
            {
                var (buffname, buffguid, buffFlag) = BuffCheck(data);
                if (!buffFlag)
                {
                    ctx.Reply("Unable to parse buffs, make sure number of buff prefabs equals the number of max ranks in configuration.");
                    return;
                }
                if (buffname != "0") // this is a way to skip a buff, leave buffs you want skipped as 0s in config
                {
                    Helper.BuffPlayerByName(playerName, buffguid, 0, true);
                    string colorString = FontColors.Green(buffname);
                    ctx.Reply($"You've been granted a permanent buff: {colorString}");
                }
            }
            data.Rank++;
            data.Points = 0;
            data.Buffs = playerBuffs;
            string rankString = FontColors.Yellow(data.Rank.ToString());
            string playerString = FontColors.Blue(playerName);
            ctx.Reply($"Congratulations {playerString}! You have increased your PvE rank to {rankString}.");
            //lightning bolt goes here

            PrefabGUID lightning = VCreate.Data.Prefabs.AB_Militia_BishopOfDunley_SummonEyeOfGod_AbilityGroup;
            VCreate.Core.Converters.FoundPrefabGuid foundPrefabGuid = new(lightning);
            VCreate.Core.Commands.CoreCommands.CastCommand(ctx, foundPrefabGuid, null);
            ChatCommands.SavePlayerRanks();
            return;
        }

        public static (string, PrefabGUID, bool) BuffCheck(RankData data)
        {
            var buffstring = Plugin.BuffPrefabsRankUp;
            List<int> playerBuffs = data.Buffs;
            var buffList = Regex.Matches(buffstring, @"-?\d+")
                               .Cast<Match>()
                               .Select(m => int.Parse(m.Value))
                               .ToList();
            bool buffFlag = false;
            string buffname = "placeholder";

            playerBuffs.Add(buffList[data.Rank]);
            PrefabGUID buffguid = new(buffList[data.Rank]);
            buffname = ECSExtensions.LookupName(buffguid);
            if (buffList[data.Rank] == 0)
            {
                buffname = "0";
            }
            if (buffList.Count == Plugin.MaxRanks)
            {
                buffFlag = true;
                return (buffname, buffguid, buffFlag);
            }
            else
            {
                return (buffname, buffguid, buffFlag);
            }
        }

        public class Berserker
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Berserker()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
                    { 5, new RankSpellConstructor("GroundSlam", VCreate.Data.Prefabs.AB_Monster_GroundSlam_AbilityGroup.GuidHash, 5) },
                    { 4, new RankSpellConstructor("WarpSlam", VCreate.Data.Prefabs.AB_Monster_WarpSlam_AbilityGroup.GuidHash, 4) },
                    { 3, new RankSpellConstructor("Bulldoze", VCreate.Data.Prefabs.AB_Mutant_FleshGolem_Bulldoze_AbilityGroup.GuidHash, 3) },
                    { 2, new RankSpellConstructor("HookShot", VCreate.Data.Prefabs.AB_SlaveMaster_Hook_AbilityGroup.GuidHash, 2) },
                    { 1, new RankSpellConstructor("HeavyDash", VCreate.Data.Prefabs.AB_Militia_Heavy_Dash_AbilityGroup.GuidHash, 1) },
                };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class Pyromancer
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Pyromancer()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
        {
            { 5, new RankSpellConstructor("FireSpinner", VCreate.Data.Prefabs.AB_ArchMage_FireSpinner_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("MoltenRain", VCreate.Data.Prefabs.AB_Militia_Glassblower_GlassRain_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("RingOfFire", VCreate.Data.Prefabs.AB_Iva_BurningRingOfFire_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("CarpetBomb", VCreate.Data.Prefabs.AB_Gloomrot_AceIncinerator_CarpetIncineration_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("FlameShot", VCreate.Data.Prefabs.AB_Gloomrot_AceIncinerator_FlameShot_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class BladeDancer
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public BladeDancer()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
        {
            { 5, new RankSpellConstructor("LeaderWhirlwindV2", VCreate.Data.Prefabs.AB_Militia_Leader_Whirlwind_v2_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("VoltageWhirlwind", VCreate.Data.Prefabs.AB_Voltage_Whirlwind_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("MilitiaWhirlwind", VCreate.Data.Prefabs.AB_Militia_Whirlwind_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("BatVampireWhirlwind", VCreate.Data.Prefabs.AB_BatVampire_Whirlwind_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("SommelierFlurry", VCreate.Data.Prefabs.AB_Sommelier_Flurry_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class VampireLord
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public VampireLord()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
        {
            { 5, new RankSpellConstructor("BatStorm", VCreate.Data.Prefabs.AB_BatVampire_BatStorm_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("NightlurkerRush", VCreate.Data.Prefabs.AB_Nightlurker_Rush_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("NightDashDash", VCreate.Data.Prefabs.AB_BatVampire_NightDash_Dash_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("BatSwarm", VCreate.Data.Prefabs.AB_BatVampire_BatSwarm_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("NightDash", VCreate.Data.Prefabs.AB_BatVampire_NightDash_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class HolyRevenant
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public HolyRevenant()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
        {
            { 5, new RankSpellConstructor("DivineRays", VCreate.Data.Prefabs.AB_ChurchOfLight_Paladin_DivineRays_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("HolySpinners", VCreate.Data.Prefabs.AB_ChurchOfLight_Paladin_HolySpinners_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("LightNova", VCreate.Data.Prefabs.AB_Cardinal_LightNova_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("LightWave", VCreate.Data.Prefabs.AB_Cardinal_LightWave_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("HealBomb", VCreate.Data.Prefabs.AB_ChurchOfLight_Priest_HealBomb_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class Gunslinger
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Gunslinger()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
            { 5, new RankSpellConstructor("Minigun", VCreate.Data.Prefabs.AB_Gloomrot_SpiderTank_Gattler_Minigun_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("ClusterBomb", VCreate.Data.Prefabs.AB_Bandit_ClusterBombThrow_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("Autofire", VCreate.Data.Prefabs.AB_Iva_Autofire_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("PistolFan", VCreate.Data.Prefabs.AB_SlaveMaster_PistolFan_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("BlastVault", VCreate.Data.Prefabs.AB_VHunter_Jade_BlastVault_Group.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class Inquisitor
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Inquisitor()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
            { 5, new RankSpellConstructor("LightArrow", VCreate.Data.Prefabs.AB_Militia_LightArrow_Throw_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("FireRain", VCreate.Data.Prefabs.AB_VHunter_Leader_FireRain_Group.GuidHash, 4) },
            { 3, new RankSpellConstructor("HolySnipe", VCreate.Data.Prefabs.AB_Militia_LightArrow_Snipe_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("FireArrow", VCreate.Data.Prefabs.AB_Militia_Longbow_FireArrow_Group.GuidHash, 2) },
            { 1, new RankSpellConstructor("Vault", VCreate.Data.Prefabs.AB_Militia_Scribe_Relocate_Travel_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class PlagueShaman
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public PlagueShaman()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
            { 5, new RankSpellConstructor("PlagueBlossom", VCreate.Data.Prefabs.AB_Undead_Priest_Elite_ProjectileNova_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("PlagueNova", VCreate.Data.Prefabs.AB_Spider_Queen_AoE_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("PoisonBurst", VCreate.Data.Prefabs.AB_Vermin_DireRat_PoisonBurst_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("Defile", VCreate.Data.Prefabs.AB_Undead_SkeletonGolem_Swallow_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("FleshWarp", VCreate.Data.Prefabs.AB_Undead_BishopOfDeath_FleshWarp_Travel_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class ThunderLord
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public ThunderLord()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
            { 5, new RankSpellConstructor("VoltDrive", VCreate.Data.Prefabs.AB_Monster_BeamLine_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("ThunderRain", VCreate.Data.Prefabs.AB_Voltage_ElectricRod_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("LightningShot", VCreate.Data.Prefabs.AB_Monster_FinalProjectile_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("ThunderShock", VCreate.Data.Prefabs.AB_Gloomrot_SpiderTank_Zapper_HeavyShot_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("VoltKick", VCreate.Data.Prefabs.AB_Voltage_SprintKick_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class VoidKnight
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public VoidKnight()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
            { 5, new RankSpellConstructor("VoidDash", VCreate.Data.Prefabs.AB_Manticore_AirDash_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("VoidStorm", VCreate.Data.Prefabs.AB_Manticore_WingStorm_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("ChaosWave", VCreate.Data.Prefabs.AB_Bandit_Tourok_VBlood_ChaosWave_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("RumblingChaos", VCreate.Data.Prefabs.AB_Bandit_StoneBreaker_VBlood_MountainRumbler_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("VoidShot", VCreate.Data.Prefabs.AB_Matriarch_Projectile_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class EarthWarden
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public EarthWarden()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
            { 5, new RankSpellConstructor("KongPound", VCreate.Data.Prefabs.AB_Cursed_MountainBeast_KongPound_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("EarthStomp", VCreate.Data.Prefabs.AB_Monster_Stomp_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("Earthquake", VCreate.Data.Prefabs.AB_Gloomrot_SpiderTank_Driller_EarthQuake_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("EarthSmash", VCreate.Data.Prefabs.AB_Geomancer_EnragedSmash_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("Burrow", VCreate.Data.Prefabs.AB_WormTerror_Dig_Travel_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class FrostScion
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public FrostScion()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
            { 5, new RankSpellConstructor("SnowStorm", VCreate.Data.Prefabs.AB_Wendigo_SnowStorm_AbilityGroup.GuidHash, 5) },
            { 4, new RankSpellConstructor("IceBeam", VCreate.Data.Prefabs.AB_Wendigo_IceBeam_First_AbilityGroup.GuidHash, 4) },
            { 3, new RankSpellConstructor("Avalanche", VCreate.Data.Prefabs.AB_Winter_Yeti_Avalanche_AbilityGroup.GuidHash, 3) },
            { 2, new RankSpellConstructor("FrostShatter", VCreate.Data.Prefabs.AB_Winter_Yeti_IceCrack_AbilityGroup.GuidHash, 2) },
            { 1, new RankSpellConstructor("IceBreaker", VCreate.Data.Prefabs.AB_Militia_Guard_VBlood_IceBreaker_AbilityGroup.GuidHash, 1) },
        };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class RankSpellConstructor
        {
            public string Name { get; set; }
            public int SpellGUID { get; set; }
            public int RequiredRank { get; set; }

            public RankSpellConstructor(string name, int spellGUID, int requiredRank)
            {
                Name = name;
                SpellGUID = spellGUID;
                RequiredRank = requiredRank;
            }
        }

        public static class ClassFactory
        {
            public static object CreateClassInstance(string className)
            {
                switch (className.ToLower())
                {
                    case "frostscion": return new FrostScion();
                    case "earthwarden": return new EarthWarden();
                    case "voidknight": return new VoidKnight();
                    case "thunderlord": return new ThunderLord();
                    case "plagueshaman": return new PlagueShaman();
                    case "inquisitor": return new Inquisitor();
                    case "gunslinger": return new Gunslinger();
                    case "holyrevenant": return new HolyRevenant();
                    case "vampirelord": return new VampireLord();
                    case "bladedancer": return new BladeDancer();
                    case "pyromancer": return new Pyromancer();
                    case "berserker": return new Berserker();

                    default: return null;
                }
            }
        }
        [Command(name: "listClasses", shortHand: "classes", adminOnly: false, usage: ".classes", description: "Lists classes available through ranking.")]
        public static void ListClasses(ChatCommandContext ctx)
        {

            ctx.Reply("Classes available: Berserker, Pyromancer, BladeDancer, VampireLord, HolyRevenant, Gunslinger, Inquisitor, PlagueShaman, ThunderLord, VoidKnight, EarthWarden, FrostScion.");
        }

        [Command(name: "chooseSpell", shortHand: "cs", adminOnly: false, usage: ".cs [#]", description: "Sets class spell to shift.")]
        public static void SpellChoice(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData rankData))
            {
                if (DateTime.UtcNow - rankData.LastAbilityUse < TimeSpan.FromSeconds(30))
                {
                    ctx.Reply("You must wait 30s before changing abilities.");
                    return;
                }
                var classInstance = ClassFactory.CreateClassInstance(rankData.ClassChoice);
                if (classInstance is Berserker berserker)
                {
                    if (berserker.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                }
                else if (classInstance is Pyromancer pyromancer)
                {
                    if (pyromancer.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Paladin
                }
                else if (classInstance is BladeDancer bladedancer)
                {
                    if (bladedancer.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is VampireLord vampirelord)
                {
                    if (vampirelord.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is HolyRevenant holyrevenant)
                {
                    if (holyrevenant.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is Gunslinger gunslinger)
                {
                    if (gunslinger.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is Inquisitor inquisitor)
                {
                    if (inquisitor.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is PlagueShaman plagueshaman)
                {
                    if (plagueshaman.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is ThunderLord thunderlord)
                {
                    if (thunderlord.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is VoidKnight voidknight)
                {
                    if (voidknight.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is EarthWarden earthwarden)
                {
                    if (earthwarden.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else if (classInstance is FrostScion frostscion)
                {
                    if (frostscion.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.SpellRank = spellConstructor.RequiredRank;
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
                }
                else
                {
                    ctx.Reply("Invalid class choice.");
                }
            }
            else
            {
                ctx.Reply("Your rank data could not be found.");
            }
        }

        [Command(name: "chooseClass", shortHand: "cc", adminOnly: true, usage: ".cc [class]", description: "Sets class to use spells from.")]
        public static void ChooseClass(ChatCommandContext ctx, string className)
        {
            string nameClass = className.ToLower();
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData rankData))
            {
                // Check if the class name provided is valid
                var classInstance = ClassFactory.CreateClassInstance(nameClass);
                if (classInstance != null)
                {
                    // Update the player's class choice
                    rankData.ClassChoice = nameClass;
                    ctx.Reply($"Class set to {nameClass}. You can now use spells associated with this class.");
                    ChatCommands.SavePlayerRanks();
                }
                else
                {
                    ctx.Reply($"Invalid class name: {className}. Please choose a valid class.");
                }
            }
            else
            {
                ctx.Reply("Your rank data could not be found.");
            }
        }
    }
}