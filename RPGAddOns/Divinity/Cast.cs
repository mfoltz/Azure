using AdminCommands;
using AdminCommands.Commands.Converters;
using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using RPGAddOns.Core;
using RPGAddOns.PvERank;
using Unity.Entities;
using Unity.Mathematics;
using VampireCommandFramework;

#nullable disable

namespace RPGAddOns.Divinity
{
    [CommandGroup(name: "casting", shortHand: "c")]
    internal class CastCommands
    {
        //[Command("cast", null, null, "Cast any ability", null, true)]
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
            [Command(name: "risenangel", shortHand: "ra", adminOnly: false, usage: "", description: "Summon the divine angel that once aided Solarus. It serves a new master now...")]
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

            [Command(name: "chaosquake", shortHand: "cq", adminOnly: false, usage: "", description: "Unleash the devastating power of chaos. Only the worthy can control it.")]
            public static void ChaosQuakeCast(ChatCommandContext ctx)
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
                        PrefabGUID quake_cast = AdminCommands.Data.Prefabs.AB_Purifier_ChaosQuake_AbilityGroup;
                        FoundPrefabGuid foundPrefabGuid = new(quake_cast);
                        CastCommand(ctx, foundPrefabGuid, null);
                        // give player virtual point once per X that caps at Y that the casting costs to simulate cooldown
                    }
                }
            }
        }
    }
}