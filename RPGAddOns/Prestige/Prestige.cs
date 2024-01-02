using ProjectM;
using RPGAddOns.Core;
using RPGMods.Commands;
using RPGMods.Systems;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using VampireCommandFramework;
using static RPGMods.Utils.Prefabs;

namespace RPGAddOns.Prestige
{
    public class PrestigeData
    {
        public int Prestiges { get; set; }
        public List<int> Buffs { get; set; } = new List<int>();

        public PrestigeData(int prestiges, List<int> buffs)
        {
            Prestiges = prestiges;
            Buffs = buffs;
        }
    }

    public class ResetLevel
    {
        public static void ResetPlayerLevel(ChatCommandContext ctx, string playerName, ulong SteamID)
        {
            if (ExperienceSystem.getLevel(SteamID) >= ExperienceSystem.MaxLevel)
            {
                // check for null reference
                if (Databases.playerPrestige != null)
                {
                    // check for player data and reset level if below max resets else create data and reset level
                    if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
                    {
                        if (data.Prestiges >= Plugin.MaxPrestiges)
                        {
                            ctx.Reply("You have reached the maximum number of resets.");
                            return;
                        }
                        ResetLevelFunctions.PlayerPrestige(ctx, playerName, SteamID, data);
                        return;
                    }
                    else
                    {
                        // create new data then call reset level function
                        List<int> playerBuffs = new List<int>();
                        PrestigeData prestigeData = new PrestigeData(0, playerBuffs);
                        Databases.playerPrestige.Add(SteamID, prestigeData);
                        Commands.SavePlayerPrestige();
                        data = Databases.playerPrestige[SteamID];
                        ResetLevelFunctions.PlayerPrestige(ctx, playerName, SteamID, data);
                        return;
                    }
                }
            }
            else
            {
                ctx.Reply("You have not reached the maximum level yet.");
                return;
            }
        }

        public class ResetLevelFunctions

        {
            public static (string, PrefabGUID, bool) BuffCheck(PrestigeData data)
            {
                bool buffFlag = false;
                string buffname = "placeholder";

                List<int> playerBuffs = data.Buffs;

                var buffList = Regex.Matches(Plugin.BuffPrefabsPrestige, @"-?\d+")
                                   .Cast<Match>()
                                   .Select(m => int.Parse(m.Value))
                                   .ToList();
                playerBuffs.Add(buffList[data.Prestiges]);
                PrefabGUID buffguid = new(buffList[data.Prestiges]);
                buffname = AdminCommands.ECSExtensions.LookupName(buffguid);
                if (buffList[data.Prestiges] == 0)
                {
                    buffname = "string";
                }
                if (buffList.Count == Plugin.MaxPrestiges)
                {
                    buffFlag = true;
                    return (buffname, buffguid, buffFlag);
                }
                else
                {
                    return (buffname, buffguid, buffFlag);
                }
            }

            public static (string, PrefabGUID) ItemCheck()
            {
                // need to return a tuple with itemname and itemguid
                PrefabGUID itemguid = new(Plugin.ItemPrefab);
                //string itemName = AdminCommands.Data.Items.GiveableItems.FirstOrDefault(item => item.PrefabGUID.Equals(Plugin.ItemPrefab)).OverrideName;
                string itemName = AdminCommands.ECSExtensions.LookupName(itemguid);

                return (itemName, itemguid);
            }

            public static void PlayerPrestige(ChatCommandContext ctx, string playerName, ulong SteamID, PrestigeData data)
            {
                // fallback to prefab if name not found, tired of dealing with this
                List<int> playerBuffs = data.Buffs;
                var buffstring = Plugin.BuffPrefabsPrestige;

                var intList = Regex.Matches(buffstring, @"-?\d+")
                                   .Cast<Match>()
                                   .Select(m => int.Parse(m.Value))
                                   .ToList();

                int preHealth = 0;
                int prePhysicalPower = 0;
                int preSpellPower = 0;
                int prePhysicalResistance = 0;
                int preSpellResistance = 0;

                //PrefabGUID buffguid = new PrefabGUID(0);
                //string buffname = FindPrefabName(buffguid);
                //string itemName = AdminCommands.Data.Items.GiveableItems.FirstOrDefault(item => item.PrefabGUID.Equals(Plugin.ItemPrefab)).OverrideName;
                //PrefabGUID itemguid = new PrefabGUID(Plugin.ItemPrefab);
                //ctx.Reply("1");
                if (RPGMods.Utils.Database.PowerUpList.ContainsKey(SteamID) != null)
                {
                    if (RPGMods.Utils.Database.PowerUpList.TryGetValue(SteamID, out RPGMods.Utils.PowerUpData preStats))
                    {
                        preHealth = (int)preStats.MaxHP;
                        prePhysicalPower = (int)preStats.PATK;
                        preSpellPower = (int)preStats.SATK;
                        prePhysicalResistance = (int)preStats.PDEF;
                        preSpellResistance = (int)preStats.SDEF;
                    }
                }

                // set stat bonus values
                int extraHealth = Plugin.ExtraHealth + preHealth;
                int extraPhysicalPower = Plugin.ExtraPhysicalPower + prePhysicalPower;
                int extraSpellPower = Plugin.ExtraSpellPower + preSpellPower;
                int extraPhysicalResistance = Plugin.ExtraPhysicalResistance + prePhysicalResistance;
                int extraSpellResistance = Plugin.ExtraSpellResistance + preSpellResistance;

                // Use the PowerUpAdd command to apply the stats and inform the player
                ctx.Reply($"Your level has been reset!");
                Experience.setXP(ctx, playerName, 0);
                if (Plugin.BuffRewardsPrestige)
                {
                    var (buffname, buffguid, buffFlag) = BuffCheck(data);
                    if (!buffFlag)
                    {
                        ctx.Reply("Unable to parse buffs, make sure number of buff prefabs equals the number of max resets in configuration.");
                        return;
                    }
                    if (buffname != "string") // this is a hacky way to skip a buff, leave buffs you want skipped as 0s in config
                    {
                        WillisCore.Helper.BuffPlayerByName(playerName, buffguid, 0, true);
                        ctx.Reply($"You've been granted a permanent buff: {buffname}");
                    }
                }
                if (Plugin.ItemReward)
                {
                    var (itemName, itemguid) = ItemCheck();
                    RPGMods.Utils.Helper.AddItemToInventory(ctx, itemguid, Plugin.ItemQuantity);
                    ctx.Reply($"You've been awarded with: {Plugin.ItemQuantity} {itemName}");
                }
                if (Plugin.PrestigeStats)
                {
                    PowerUp.powerUP(ctx, playerName, "add", extraHealth, extraPhysicalPower, extraSpellPower, extraPhysicalResistance, extraSpellResistance);
                    ctx.Reply($"You've gained: MaxHealth {FontColors.Red(Plugin.ExtraHealth.ToString())}, PAtk {FontColors.Yellow(Plugin.ExtraPhysicalPower.ToString())}, SAtk {FontColors.Purple(Plugin.ExtraSpellPower.ToString())}, PDef {FontColors.Green(Plugin.ExtraPhysicalResistance.ToString())}, SDef {FontColors.Blue(Plugin.ExtraSpellResistance.ToString())}");
                }
                // log player ResetData and save, take away exp

                data.Prestiges++; data.Buffs = playerBuffs;
                Commands.SavePlayerPrestige();
                return;
            }
        }
    }
}