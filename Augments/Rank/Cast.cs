
using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using V.Core;
using RPGMods.Utils;
using Unity.Entities;
using Unity.Mathematics;
using VampireCommandFramework;
using DateTime = System.DateTime;
using Plugin = V.Core.Plugin;
using TimeSpan = System.TimeSpan;
using static V.Core.Services.PlayerService;
using V.Data;
using V.Core.Tools;
using V.Core.Commands;
using V.Core.Services;
using static V.Augments.Rank.CastCommands;
using Type = System.Type;

#nullable disable

namespace V.Augments.Rank
{
    internal class CastCommands
    {
        private static readonly double cd = Plugin.rankCommandsCooldown; //cd in hours

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
        public class RankSpellConstructor
        {
            public string Name { get; set; }
            public PrefabGUID SpellGUID { get; set; }
            public int RequiredRank { get; set; }

            public RankSpellConstructor(string name, PrefabGUID spellGUID, int requiredRank)
            {
                Name = name;
                SpellGUID = spellGUID;
                RequiredRank = requiredRank;
            }
        }
        public class Nightmarshal
        {
            public Dictionary<int, RankSpellConstructor> Spells = new Dictionary<int, RankSpellConstructor>();

            public Nightmarshal()
            {
                Spells.Add(5, new RankSpellConstructor("Batswarm", V.Data.Prefabs.AB_BatVampire_BatSwarm_AbilityGroup, 5));
                Spells.Add(4, new RankSpellConstructor("Nightdash", V.Data.Prefabs.AB_BatVampire_NightDash_AbilityGroup, 4));
                Spells.Add(3, new RankSpellConstructor("Batstorm", V.Data.Prefabs.AB_BatVampire_BatStorm_AbilityGroup, 3));
                Spells.Add(2, new RankSpellConstructor("Batleap", V.Data.Prefabs.AB_BatVampire_SummonMinions_AbilityGroup, 2));
                Spells.Add(1, new RankSpellConstructor("Batwhirlwind", V.Data.Prefabs.AB_BatVampire_Whirlwind_AbilityGroup, 1));
            }
        }
        public static class CommandHandler
        {
            private static Dictionary<string, System.Func<object>> classFactories = new Dictionary<string, System.Func<object>>
            {
                { "nightmarshal", () => new Nightmarshal() },
                // Add other class factories here
            };
            [Command(name: "chooseClass", shortHand: "cc", adminOnly: false, usage: ".cs <name>", description: "Sets class to use spells from.")]
            public static void ClassChoice(ChatCommandContext ctx, string choice)
            {
                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;

                if (Databases.playerRanks.TryGetValue(SteamID, out RankData rankData))
                {
                    if (classFactories.ContainsKey(choice))
                    {
                        rankData.ClassChoice = choice;
                    }
                }
                else
                {
                    ctx.Reply("Your rank data could not be found.");
                }
            }
            [Command(name: "chooseSpell", shortHand: "cs", adminOnly: false, usage: ".cs <#>", description: "Sets class spell to shift.")]
            public static void SpellChoice(ChatCommandContext ctx, int choice)
            {
                Entity character = ctx.Event.SenderCharacterEntity;
                ulong SteamID = ctx.Event.User.PlatformId;

                if (Databases.playerRanks.TryGetValue(SteamID, out RankData rankData))
                {
                    // Attempt to get the class instance from the class choice
                    if (classFactories.TryGetValue(rankData.ClassChoice, out var classFactory))
                    {
                        var classInstance = classFactory.Invoke();

                        // Check if the class instance is of expected type and contains the spell
                        if (classInstance is Nightmarshal nightmarshal && nightmarshal.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor))
                        {
                            // Now you have the spellConstructor, you can check the player's rank
                            if (rankData.Rank >= spellConstructor.RequiredRank)
                            {
                                // Here you would typically cast the spell or apply its effects
                                PrefabGUID newSpell = spellConstructor.SpellGUID;
                                V.Data.FoundPrefabGuid foundPrefabGuid = new(newSpell);
                                rankData.RankSpell = newSpell.GuidHash;
                                //CastCommand(ctx, foundPrefabGuid, null); // Assuming this is how you cast the spell

                                ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                                ChatCommands.SavePlayerRanks();
                            }
                            else
                            {
                                ctx.Reply($"You must be at least rank {spellConstructor.RequiredRank} to use this ability.");
                            }
                        }
                        else
                        {
                            ctx.Reply("Invalid spell choice.");
                        }
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

        }


    }
}