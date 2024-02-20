using LibCpp2IL.BinaryStructures;
using ProjectM;
using RPGAddOnsEx.Core;
using RPGMods.Commands;
using RPGMods.Systems;
using Steamworks;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using VampireCommandFramework;
using static RPGMods.Utils.Prefabs;

namespace RPGAddOnsEx.Augments
{
    public class PrestigeData
    {
        public int Prestiges { get; set; }
        public int PlayerBuff { get; set; }

        public PrestigeData(int prestiges, int playerbuff)
        {
            Prestiges = prestiges;
            PlayerBuff = playerbuff;
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
                        ChatCommands.SavePlayerPrestige();
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
            private static readonly double resists = 0.01;

            public static (string, PrefabGUID) ItemCheck()
            {
                // need to return a tuple with itemname and itemguid
                PrefabGUID itemguid = new(Plugin.ItemPrefab);
                //string itemName = AdminCommands.Data.Items.GiveableItems.FirstOrDefault(item => item.PrefabGUID.Equals(Plugin.ItemPrefab)).OverrideName;
                string itemName = AdminCommands.ECSExtensions.LookupName(itemguid);

                return (itemName, itemguid);
            }

            public static void BuffChecker(ChatCommandContext ctx, int buff, PrestigeData data)
            {
                var buffstring = Plugin.BuffPrefabsPrestige;
                var buffList = Regex.Matches(buffstring, @"-?\d+")
                                   .Cast<Match>()
                                   .Select(m => int.Parse(m.Value))
                                   .ToList();
                //first validate input from user
                if (buff > 0 && buff <= buffList.Count)
                {
                    if (buff == 1)
                    {
                        // always grant first unlocked buff after first prestige
                        if (data.Prestiges < 1)
                        {
                            ctx.Reply("You must prestige at least once to unlock this buff.");
                            return;
                        }
                        // but for every buff application first check if they already have one and remove it if they do
                        if (data.PlayerBuff == 0)
                        {
                            PrefabGUID buffguid = new(buffList[buff]);
                            // buff good to apply, 0 means no buff
                            WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguid, 0, true);
                            data.PlayerBuff = buffList[buff];
                            ChatCommands.SavePlayerPrestige();
                            ctx.Reply($"Visual buff #{buff} has been applied.");
                            return;
                        }
                        else
                        {
                            // remove buff using buffs data before applying new buff
                            PrefabGUID buffguidold = new(data.PlayerBuff);
                            WillisCore.Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffguidold);
                            PrefabGUID buffguidnew = new(buffList[buff]);
                            WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguidnew, 0, true);
                            data.PlayerBuff = buffList[buff];
                            ChatCommands.SavePlayerPrestige();
                            ctx.Reply($"Visual buff #{buff} has been applied.");
                            return;
                        }
                    }
                    else
                    {
                        // check if high enough prestige
                        if (data.Prestiges >= buff * 3)
                        {
                            if (data.PlayerBuff == 0)
                            {
                                PrefabGUID buffguid = new(buffList[buff - 1]);
                                // buff good to apply, 0 means no buff
                                WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguid, 0, true);
                                data.PlayerBuff = buffList[buff - 1];
                                ChatCommands.SavePlayerPrestige();
                                ctx.Reply($"Visual buff #{buff} has been applied.");
                                return;
                            }
                            else
                            {
                                // remove buff using buffs data before applying new buff
                                PrefabGUID buffguidold = new(data.PlayerBuff);
                                WillisCore.Helper.UnbuffCharacter(ctx.Event.SenderCharacterEntity, buffguidold);
                                PrefabGUID buffguidnew = new(buffList[buff - 1]);
                                WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguidnew, 0, true);
                                data.PlayerBuff = buffList[buff - 1];
                                ChatCommands.SavePlayerPrestige();
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
                    ctx.Reply($"Choice must be greater than 0 and less than or equal to {buffList.Count}.");
                }
            }

            public static void PlayerPrestige(ChatCommandContext ctx, string playerName, ulong SteamID, PrestigeData data)
            {
                // add resistance powerup in here where it makes sense directly proportional to player prestige level
                ctx.Reply($"Your level has been reset!");
                Experience.setXP(ctx, playerName, 0);

                if (Plugin.ItemReward)
                {
                    int itemQuantity;
                    if (Plugin.ItemMultiplier)
                    {
                        itemQuantity = Plugin.ItemQuantity * data.Prestiges;
                    }
                    else
                    {
                        itemQuantity = Plugin.ItemQuantity;
                    }

                    if (itemQuantity == 0)
                    {
                        itemQuantity = 1;
                    }

                    var (itemName, itemguid) = ItemCheck();
                    RPGMods.Utils.Helper.AddItemToInventory(ctx, itemguid, itemQuantity);
                    string quantityString = RPGAddOnsEx.Core.FontColors.Yellow(itemQuantity.ToString());
                    string itemString = RPGAddOnsEx.Core.FontColors.Purple(itemName);
                    ctx.Reply($"You've been awarded with: {quantityString} {itemString}");
                }
                //ApplyResists(ctx, playerName, SteamID, data);
                data.Prestiges++;
                ChatCommands.SavePlayerPrestige();
                return;
            }

            public static void ApplyResists(ChatCommandContext ctx, string playerName, ulong SteamID, PrestigeData data)
            {
                int preHealth = 0;
                int prePhysicalPower = 0;
                int preSpellPower = 0;
                int prePhysicalResistance = 0;
                int preSpellResistance = 0;

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

                // set stat bonus values and add pre-existing bonuses for continuity

                int extraHealth = preHealth;
                int extraPhysicalPower = prePhysicalPower;
                int extraSpellPower = preSpellPower;
                float extraPhysicalResistance = (float)resists + prePhysicalResistance;
                float extraSpellResistance = (float)resists + preSpellResistance;
                if (data.Prestiges > 10)
                {
                    // dont give more resists after 10
                    return;
                }
                PowerUp.powerUP(ctx, playerName, "add", extraHealth, extraPhysicalPower, extraSpellPower, extraPhysicalResistance, extraSpellResistance);
            }
        }
    }
}