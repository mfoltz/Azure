using ProjectM;
using RPGMods.Commands;
using RPGMods.Systems;
using System.Text.RegularExpressions;
using VampireCommandFramework;
using BindingFlags = System.Reflection.BindingFlags;
using FieldInfo = System.Reflection.FieldInfo;

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
        public static void ResetPlayerLevel(ChatCommandContext ctx, string playerName, ulong SteamID, string StringID)
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
            public static string FindPrefabName(PrefabGUID targetPrefabGUID)
            {
                Type type = typeof(AdminCommands.Data.Prefabs);

                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (field.FieldType == typeof(PrefabGUID))
                    {
                        PrefabGUID prefabGuid = (PrefabGUID)field.GetValue(null);

                        if (prefabGuid == targetPrefabGUID)
                        {
                            return field.Name;
                        }
                    }
                }

                // If no matching PrefabGUID is found, return null or an appropriate default value.
                return "Prefab name not found.";
            }
            public static List<string> BuffFlag(ResetData data, List<int> intList)
            {

                List<string> buffinfo = new List<string>();
                if (intList.Count == Plugin.MaxResets.Value)
                {
                    List<int> playerBuffs = data.Buffs;
                    playerBuffs.Add(intList[data.ResetCount]);
                    int buffint = intList[data.ResetCount];
                    PrefabGUID buffguid = new PrefabGUID(buffint);
                    // probably don't want to generate mappings every time, do this somewhere else once and store it
                    // guess I need several mappings generated then need to check them all for the buff and handle it if not found

                    if (FindPrefabName(buffguid) != "Prefab name not found.")
                    {
                        string buffname = FindPrefabName(buffguid);

                        buffinfo.Add(buffname); buffinfo.Add(buffint.ToString());
                        return buffinfo;
                        // can't grant rewards until the end if checks in between can fail to prevent edge cases, set flag to true or something
                    }
                    else
                    {

                        return buffinfo;
                    }
                }
                else
                {
                    return buffinfo;
                }



            }
            public static List<string> ItemFlag()
            {
                List<string> iteminfo = new List<string>();
                PrefabGUID itemguid = new PrefabGUID(Plugin.ItemPrefab.Value);
                if (AdminCommands.ECSExtensions.LookupName(itemguid) != "GUID Not Found")
                {
                    string itemName = AdminCommands.Data.Items.GiveableItems.FirstOrDefault(item => item.PrefabGUID.Equals(Plugin.ItemPrefab)).OverrideName;

                    if (Plugin.ItemQuantity.Value > 1)
                    {
                        if (itemName.Last() == 's')
                        {
                            itemName += "'";
                        }
                        else
                        {
                            itemName += "s";
                        }
                    }
                    // same with buffs up there, cant grant rewards until the end if checks in between can fail to prevent edge cases
                    iteminfo.Add(itemName); iteminfo.Add(itemguid.ToString());
                    return iteminfo;
                }
                else
                {
                    return iteminfo;
                }
            }
            public static void PlayerReset(ChatCommandContext ctx, string playerName, ulong SteamID, ResetData data)
            {
                List<int> intList = new List<int>();

                List<int> playerBuffs = data.Buffs;
                var buffstring = Plugin.BuffPrefabsReset.Value;

                List<string> buffList = ConvertStringToList(buffstring);

                static List<string> ConvertStringToList(string buffstring)
                {
                    var matches = Regex.Matches(buffstring, @"\d+");
                    return (from Match match in matches.Cast<Match>() select match.Value).ToList();
                }
                intList = buffList.Select(s => int.Parse(s)).ToList();

                int preHealth = 0;
                int prePhysicalPower = 0;
                int preSpellPower = 0;
                int prePhysicalResistance = 0;
                int preSpellResistance = 0;
                PrefabGUID buffguid = new PrefabGUID(intList[data.ResetCount]);
                string buffname = FindPrefabName(buffguid);
                string itemName = AdminCommands.Data.Items.GiveableItems.FirstOrDefault(item => item.PrefabGUID.Equals(Plugin.ItemPrefab)).OverrideName;
                PrefabGUID itemguid = new PrefabGUID(Plugin.ItemPrefab.Value);
                if (RPGMods.Utils.Database.PowerUpList.ContainsKey(SteamID))
                {
                    var preStats = RPGMods.Utils.Database.PowerUpList[SteamID];
                    preHealth = (int)preStats.MaxHP;
                    prePhysicalPower = (int)preStats.PATK;
                    preSpellPower = (int)preStats.SATK;
                    prePhysicalResistance = (int)preStats.PDEF;
                    preSpellResistance = (int)preStats.SDEF;
                }


                bool buffFlag = false;
                bool itemFlag = false;


                if (Plugin.BuffRewardsReset.Value)
                {

                    var buffinfo = BuffFlag(data, intList);
                    if (buffinfo.Count > 0)
                    {
                        buffFlag = true;
                        buffname = buffinfo[0];
                        int buffint = int.Parse(buffinfo[1]);
                        buffguid = new PrefabGUID(buffint);
                    }
                    else
                    {
                        ctx.Reply($"Unable to parse buff, check BuffPrefabResets configuration.");
                        return;
                    }

                }
                if (Plugin.ItemReward.Value)
                {
                    var iteminfo = ItemFlag();
                    if (iteminfo.Count > 0)
                    {
                        itemFlag = true;
                        itemName = iteminfo[0];
                        int itemint = int.Parse(iteminfo[1]);
                        itemguid = new PrefabGUID(itemint);
                    }
                    else
                    {
                        ctx.Reply($"Unable to parse item, check ItemPrefab configuration.");
                        return;
                    }

                }
                // set stat bonus values
                int extraHealth = Plugin.ExtraHealth.Value + preHealth;
                int extraPhysicalPower = Plugin.ExtraPhysicalPower.Value + prePhysicalPower;
                int extraSpellPower = Plugin.ExtraSpellPower.Value + preSpellPower;
                int extraPhysicalResistance = Plugin.ExtraPhysicalResistance.Value + prePhysicalResistance;
                int extraSpellResistance = Plugin.ExtraSpellResistance.Value + preSpellResistance;

                // Use the PowerUpAdd command to apply the stats and inform the player
                PowerUp.powerUP(ctx, playerName, "add", extraHealth, extraPhysicalPower, extraSpellPower, extraPhysicalResistance, extraSpellResistance);

                ctx.Reply($"Your level has been reset! You've gained: MaxHealth {Plugin.ExtraHealth}, PAtk {Plugin.ExtraPhysicalPower}, SAtk {Plugin.ExtraSpellPower}, PDef {Plugin.ExtraPhysicalResistance}, SDef {Plugin.ExtraSpellResistance}");
                if (buffFlag)
                {
                    WillisCore.Helper.BuffPlayerByName(playerName, buffguid, -1, true);
                    ctx.Reply($"You've been granted a permanent buff: {buffname}");
                }
                if (itemFlag)
                {
                    RPGMods.Utils.Helper.AddItemToInventory(ctx, itemguid, Plugin.ItemQuantity.Value);
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
