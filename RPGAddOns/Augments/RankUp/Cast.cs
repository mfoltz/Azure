using AdminCommands;
using AdminCommands.Commands.Converters;
using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using RPGAddOnsEx.Core;
using RPGMods.Utils;
using Unity.Entities;
using Unity.Mathematics;
using VampireCommandFramework;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

#nullable disable

namespace RPGAddOnsEx.Augments.RankUp
{
    [CommandGroup(name: "casting", shortHand: "c")]
    internal class CastCommands
    {
        private static readonly int cd = 0; //cd in hours

        [Command("cast", null, null, "Cast any ability", null, true)]
        public static void CastCommand(ChatCommandContext ctx, FoundPrefabGuid prefabGuid, FoundPlayer player = null)
        {
            PlayerService.Player player1;
            Entity entity1;
            if ((object)player == null)
            {
                entity1 = ctx.Event.SenderUserEntity;
            }
            else
            {
                player1 = player.Value;
                entity1 = player1.User;
            }
            Entity entity2 = entity1;
            Entity entity3;
            if ((object)player == null)
            {
                entity3 = ctx.Event.SenderCharacterEntity;
            }
            else
            {
                player1 = player.Value;
                entity3 = player1.Character;
            }
            Entity entity4 = entity3;
            FromCharacter fromCharacter = new FromCharacter()
            {
                User = entity2,
                Character = entity4
            };
            DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            CastAbilityServerDebugEvent serverDebugEvent = new CastAbilityServerDebugEvent()
            {
                AbilityGroup = prefabGuid.Value,
                AimPosition = new Nullable_Unboxed<float3>(entity2.Read<EntityInput>().AimPosition),
                Who = entity4.Read<NetworkId>()
            };
            existingSystem.CastAbilityServerDebugEvent(entity2.Read<User>().Index, ref serverDebugEvent, ref fromCharacter);
        }

        public class VBloodSpells
        {
            /*
            [Command(name: "risenangel", shortHand: "ra", adminOnly: true, usage: "", description: "Summon the divine angel that once aided Solarus. It serves a new master now...")]
            public static void DivineAngelCast(ChatCommandContext ctx)
            {
                // going to use Rank for now until the ascension system is created
                // might also cost something expensive to cast

                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;
                if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
                {
                    if (data.Rank < 5)
                    {
                        ctx.Reply("You must be rank 5 to use this ability.");
                        return;
                    }
                    else
                    {
                        PrefabGUID angel_cast = AdminCommands.Data.Prefabs.AB_ChurchOfLight_Paladin_SummonAngel_AbilityGroup;
                        FoundPrefabGuid foundPrefabGuid = new(angel_cast);
                        CastCommand(ctx, foundPrefabGuid, null);
                        // give player virtual point once per X that caps at Y that the casting costs to simulate cooldown
                    }
                }
                else
                {
                    ctx.Reply("You must be rank 5 to use this ability.");
                    return;
                }
            }
            */

            // need to put in a check for the player's rank or ascension or whatever this is going to be

            [Command(name: "firespinner", shortHand: "4", adminOnly: false, usage: "", description: "")]
            public static void FireSpinnerCast(ChatCommandContext ctx)
            {
                // going to use Rank for now until the ascension system is created
                // might also cost something expensive to cast

                // Check player rank
                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;

                if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData rankData))
                {
                    if (rankData.Rank < 4)
                    {
                        ctx.Reply("You must be at least rank 4 to use this ability.");
                        return;
                    }

                    // Check if cooldown has passed
                    if (DateTime.UtcNow - rankData.LastAbilityUse >= TimeSpan.FromHours(cd))
                    {
                        // Update LastAbilityUse to current time
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        DataStructures.playerRanks[SteamID] = rankData; // Save changes to RankData

                        PrefabGUID firespinner_cast = new PrefabGUID(1217615468);
                        FoundPrefabGuid foundPrefabGuid = new(firespinner_cast);
                        CastCommand(ctx, foundPrefabGuid, null);
                        ChatCommands.SavePlayerRanks();
                    }
                    else
                    {
                        var waited = DateTime.UtcNow - rankData.LastAbilityUse;
                        var cooldown = TimeSpan.FromHours(cd) - waited;
                        ctx.Reply($"Ability is on cooldown. {((int)cooldown.TotalMinutes)} minutes remaining.");
                    }
                }
                else
                {
                    ctx.Reply("Your rank data could not be found.");
                }
            }

            [Command(name: "batstorm", shortHand: "3", adminOnly: false, usage: "", description: "")]
            public static void BatStormCast(ChatCommandContext ctx)
            {
                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;

                if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData rankData))
                {
                    if (rankData.Rank < 3)
                    {
                        ctx.Reply("You must be at least rank 3 to use this ability.");
                        return;
                    }

                    if (DateTime.UtcNow - rankData.LastAbilityUse >= TimeSpan.FromHours(cd))
                    {
                        rankData.LastAbilityUse = DateTime.UtcNow; // Update last use time
                        DataStructures.playerRanks[SteamID] = rankData; // Save updated RankData

                        PrefabGUID batstorm_cast = new PrefabGUID(-254080557);
                        FoundPrefabGuid foundPrefabGuid = new(batstorm_cast);
                        CastCommand(ctx, foundPrefabGuid, null);
                        ChatCommands.SavePlayerRanks();
                    }
                    else
                    {
                        var waited = DateTime.UtcNow - rankData.LastAbilityUse;
                        var cooldown = TimeSpan.FromHours(cd) - waited;
                        ctx.Reply($"Ability is on cooldown. {((int)cooldown.TotalMinutes)} minutes remaining.");
                    }
                }
                else
                {
                    ctx.Reply("Your rank data could not be found.");
                }
            }

            /*
            [Command(name: "batleap", shortHand: "bl", adminOnly: false, usage: "", description: "")]
            public static void ByeFeliciaCast(ChatCommandContext ctx)
            {
                // going to use Rank for now until the ascension system is created
                // might also cost something expensive to cast

                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;
                PrefabGUID quake_cast = new PrefabGUID(-597709516);
                FoundPrefabGuid foundPrefabGuid = new(quake_cast);
                CastCommand(ctx, foundPrefabGuid, null);
            }
            */

            [Command(name: "batwhirlwind", shortHand: "1", adminOnly: false, usage: "", description: "")]
            public static void BatWhirlwindCast(ChatCommandContext ctx)
            {
                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;

                if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData rankData))
                {
                    if (rankData.Rank < 1)
                    {
                        ctx.Reply("You must be at least rank 1 to use this ability.");
                        return;
                    }

                    if (DateTime.UtcNow - rankData.LastAbilityUse >= TimeSpan.FromHours(cd))
                    {
                        // Ability can be cast
                        rankData.LastAbilityUse = DateTime.UtcNow; // Update the last use time
                        DataStructures.playerRanks[SteamID] = rankData; // Ensure rank data is updated

                        PrefabGUID batwhirlwind_cast = new PrefabGUID(-1698981316);
                        FoundPrefabGuid foundPrefabGuid = new(batwhirlwind_cast);
                        CastCommand(ctx, foundPrefabGuid, null);
                        ChatCommands.SavePlayerRanks();
                    }
                    else
                    {
                        var waited = DateTime.UtcNow - rankData.LastAbilityUse;
                        var cooldown = TimeSpan.FromHours(cd) - waited;
                        ctx.Reply($"Ability is on cooldown. {((int)cooldown.TotalMinutes)} minutes remaining.");
                    }
                }
                else
                {
                    ctx.Reply("Your rank data could not be found.");
                }
            }

            [Command(name: "lightnova", shortHand: "2", adminOnly: false, usage: "", description: "")]
            public static void LightNovaCast(ChatCommandContext ctx)
            {
                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;

                if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData rankData))
                {
                    if (rankData.Rank < 2)
                    {
                        ctx.Reply("You must be at least rank 2 to use this ability.");
                        return;
                    }

                    if (DateTime.UtcNow - rankData.LastAbilityUse >= TimeSpan.FromHours(cd))
                    {
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        DataStructures.playerRanks[SteamID] = rankData;
                        PrefabGUID lightnova_cast = new PrefabGUID(114484622);
                        FoundPrefabGuid foundPrefabGuid = new(lightnova_cast);
                        CastCommand(ctx, foundPrefabGuid, null);
                        ChatCommands.SavePlayerRanks();
                    }
                    else
                    {
                        var waited = DateTime.UtcNow - rankData.LastAbilityUse;
                        var cooldown = TimeSpan.FromHours(cd) - waited;
                        ctx.Reply($"Ability is on cooldown. {((int)cooldown.TotalMinutes)} minutes remaining.");
                    }
                }
                else
                {
                    ctx.Reply("Your rank data could not be found.");
                }
            }

            [Command(name: "wispdance", shortHand: "5", adminOnly: false, usage: "", description: "")]
            public static void WispDanceCast(ChatCommandContext ctx)
            {
                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;

                if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData rankData))
                {
                    if (rankData.Rank < 5)
                    {
                        ctx.Reply("You must be at least rank 5 to use this ability.");
                        return;
                    }

                    if (DateTime.UtcNow - rankData.LastAbilityUse >= TimeSpan.FromHours(cd))
                    {
                        rankData.LastAbilityUse = DateTime.UtcNow;
                        DataStructures.playerRanks[SteamID] = rankData;
                        PrefabGUID wispdance_cast = new PrefabGUID(-1574537639);
                        FoundPrefabGuid foundPrefabGuid = new(wispdance_cast);
                        CastCommand(ctx, foundPrefabGuid, null);
                        ChatCommands.SavePlayerRanks();
                    }
                    else
                    {
                        var waited = DateTime.UtcNow - rankData.LastAbilityUse;
                        var cooldown = TimeSpan.FromHours(cd) - waited;
                        ctx.Reply($"Ability is on cooldown. {((int)cooldown.TotalMinutes)} minutes remaining.");
                    }
                }
                else
                {
                    ctx.Reply("Your rank data could not be found.");
                }
            }
        }
    }
}