using ProjectM;
using RPGMods.Commands;
using RPGMods.Systems;
using System.Text.RegularExpressions;
using VampireCommandFramework;

namespace RPGAddOns
{
    public class ResetData
    {
        public int ResetCount { get; set; }
        public List<int> Buffs { get; set; }

        public ResetData(int count, List<int> buffs)
        {
            ResetCount = count;
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
                if (Databases.playerResetCountsBuffs != null)
                {
                    // check for player data and reset level if below max resets else create data and reset level
                    if (Databases.playerResetCountsBuffs.TryGetValue(SteamID, out ResetData data))
                    {
                        ResetLevelFunctions.PlayerReset(ctx, playerName, SteamID, data);
                        return;
                    }
                    else
                    {
                        // create new data then call reset level function
                        List<int> playerBuffs = new List<int>();
                        var ResetData = new ResetData(0, playerBuffs);
                        Databases.playerResetCountsBuffs.Add(SteamID, ResetData);
                        Commands.SavePlayerResets();
                        data = Databases.playerResetCountsBuffs[SteamID];
                        ResetLevelFunctions.PlayerReset(ctx, playerName, SteamID, data);
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
            public static (string, PrefabGUID, bool) BuffCheck(ResetData data)
            {
                bool buffFlag = false;
                string buffname = "placeholder";
                
                List<int> playerBuffs = data.Buffs;

                var buffList = Regex.Matches(Plugin.BuffPrefabsReset, @"\d+")
                                   .Cast<Match>()
                                   .Select(m => int.Parse(m.Value))
                                   .ToList();
                playerBuffs.Add(buffList[data.ResetCount]);
                PrefabGUID buffguid = new(buffList[data.ResetCount]);
                buffname = AdminCommands.ECSExtensions.LookupName(buffguid);
                if (buffList[data.ResetCount] == 0)
                {
                    buffname = "string";
                }
                if (buffList.Count == Plugin.MaxResets)
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
            public static void PlayerReset(ChatCommandContext ctx, string playerName, ulong SteamID, ResetData data)
            {
                // fallback to prefab if name not found, tired of dealing with this
                List<int> playerBuffs = data.Buffs;
                var buffstring = Plugin.BuffPrefabsReset;

                var intList = Regex.Matches(buffstring, @"\d+")
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


                PowerUp.powerUP(ctx, playerName, "add", extraHealth, extraPhysicalPower, extraSpellPower, extraPhysicalResistance, extraSpellResistance);

                ctx.Reply($"Your level has been reset! You've gained: MaxHealth {Plugin.ExtraHealth}, PAtk {Plugin.ExtraPhysicalPower}, SAtk {Plugin.ExtraSpellPower}, PDef {Plugin.ExtraPhysicalResistance}, SDef {Plugin.ExtraSpellResistance}");

                if (Plugin.BuffRewardsReset)
                {
                    var (buffname, buffguid, buffFlag) = BuffCheck(data);
                    if (!buffFlag)
                    {
                        ctx.Reply("Unable to parse buffs, make sure number of buff prefabs equals the number of max resets in configuration.");
                        return;
                    }
                    if (buffname != "string")
                    {
                        WillisCore.Helper.BuffPlayerByName(playerName, buffguid, -1, true);
                        ctx.Reply($"You've been granted a permanent buff: {buffname}");
                    }
                    
                }
                if (Plugin.ItemReward)
                {
                    var (itemName, itemguid) = ItemCheck();
                    RPGMods.Utils.Helper.AddItemToInventory(ctx, itemguid, Plugin.ItemQuantity);
                    ctx.Reply($"You've been awarded with: {Plugin.ItemQuantity} {itemName}");
                }
                // log player ResetData and save, take away exp
                Experience.setXP(ctx, playerName, 0);
                data.ResetCount++; data.Buffs = playerBuffs;
                Commands.SavePlayerResets();
                return;
            }
        }
    }
}
