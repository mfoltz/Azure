using LibCpp2IL.BinaryStructures;
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
        public int Buffs { get; set; }

        public PrestigeData(int prestiges, int buffs)
        {
            Prestiges = prestiges;
            Buffs = buffs;
        }
    }

    public class PrestigeSystem
    {
        public static void PrestigeCheck(ChatCommandContext ctx, string playerName, ulong SteamID)
        {
            if (ExperienceSystem.getLevel(SteamID) >= ExperienceSystem.MaxLevel)
            {
                // check for null reference
                if (Databases.playerPrestige != null)
                {
                    // check for player data and reset level if below max resets else create data and reset level
                    if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
                    {
                        if (data.Prestiges >= Plugin.MaxPrestiges && Plugin.MaxPrestiges != -1)
                        {
                            ctx.Reply("You have reached the maximum number of resets.");
                            return;
                        }
                        PrestigeFunctions.PlayerPrestige(ctx, playerName, SteamID, data);
                        return;
                    }
                    else
                    {
                        // create new data then call prestige level function

                        PrestigeData prestigeData = new PrestigeData(0, 0);
                        Databases.playerPrestige.Add(SteamID, prestigeData);
                        Commands.SavePlayerPrestige();
                        data = Databases.playerPrestige[SteamID];
                        PrestigeFunctions.PlayerPrestige(ctx, playerName, SteamID, data);
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

        public class PrestigeFunctions

        {
            public static (string, PrefabGUID) ItemCheck()
            {
                // need to return a tuple with itemname and itemguid
                PrefabGUID itemguid = new(Plugin.ItemPrefab);
                //string itemName = AdminCommands.Data.Items.GiveableItems.FirstOrDefault(item => item.PrefabGUID.Equals(Plugin.ItemPrefab)).OverrideName;
                string itemName = AdminCommands.ECSExtensions.LookupName(itemguid);

                return (itemName, itemguid);
            }

            public static void BuffCheck(ChatCommandContext ctx, int buff, PrestigeData data)
            {
                var buffstring = Plugin.BuffPrefabsPrestige;
                var buffList = Regex.Matches(buffstring, @"-?\d+")
                                   .Cast<Match>()
                                   .Select(m => int.Parse(m.Value))
                                   .ToList();
                //first validate input from user
                if (buff >= 0 && buff <= buffList.Count)
                {
                    if (buff == 1)
                    {
                        // always grant first unlocked buff after first prestige
                        // but for every buff application first check if they already have one and remove it if they do
                        if (data.Buffs == 0)
                        {
                            PrefabGUID buffguid = new(buffList[buff]);
                            // buff good to apply, 0 means no buff
                            WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguid, -1, true);
                            data.Buffs = buffList[buff];
                            Commands.SavePlayerPrestige();
                            ctx.Reply($"Visual buff #{buff} has been applied.");
                            return;
                        }
                        else
                        {
                            // remove buff using buffs data before applying new buff
                            PrefabGUID buffguidold = new(data.Buffs);
                            WillisCore.Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffguidold);
                            PrefabGUID buffguidnew = new(buffList[buff]);
                            WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguidnew, -1, true);
                            data.Buffs = buffList[buff];
                            Commands.SavePlayerPrestige();
                            ctx.Reply($"Visual buff #{buff} has been applied.");
                            return;
                        }
                    }
                    else
                    {
                        // check if high enough prestige
                        if (data.Prestiges >= buff * 3)
                        {
                            if (data.Buffs == 0)
                            {
                                PrefabGUID buffguid = new(buffList[buff]);
                                // buff good to apply, 0 means no buff
                                WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguid, -1, true);
                                data.Buffs = buffList[buff];
                                Commands.SavePlayerPrestige();
                                ctx.Reply($"Visual buff #{buff} has been applied.");
                                return;
                            }
                            else
                            {
                                // remove buff using buffs data before applying new buff
                                PrefabGUID buffguidold = new(data.Buffs);
                                WillisCore.Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffguidold);
                                PrefabGUID buffguidnew = new(buffList[buff]);
                                WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguidnew, -1, true);
                                data.Buffs = buffList[buff];
                                Commands.SavePlayerPrestige();
                                ctx.Reply($"Visual buff #{buff} has been applied.");
                                return;
                            }
                        }
                        else
                        {
                            ctx.Reply($"This visual buff requires prestige {buff * 3}.");
                        }
                    }
                }
                else
                {
                    ctx.Reply($"Choice must be greater than 0 and less than or equal to {buffList.Count - 1}.");
                }
            }

            public static void PlayerPrestige(ChatCommandContext ctx, string playerName, ulong SteamID, PrestigeData data)
            {
                // fallback to prefab if name not found, tired of dealing with this

                ctx.Reply($"Your level has been reset!");
                Experience.setXP(ctx, playerName, 0);

                if (Plugin.ItemReward)
                {
                    int numFrags = Plugin.ItemQuantity * data.Prestiges;
                    if (numFrags == 0)
                    {
                        numFrags = 1;
                    }
                    var (itemName, itemguid) = ItemCheck();
                    RPGMods.Utils.Helper.AddItemToInventory(ctx, itemguid, numFrags);
                    ctx.Reply($"You've been awarded with: {Plugin.ItemQuantity} {itemName}");
                }
                if (Plugin.BuffRewardsPrestige)
                {
                    ctx.Reply($"You've unlocked a visual buff!");
                }

                data.Prestiges++;
                Commands.SavePlayerPrestige();
                return;
            }
        }
    }
}