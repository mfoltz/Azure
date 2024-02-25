using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using V.Core;
using V.Core.Commands;
using V.Core.Services;
using V.Core.Tools;
using V.Data;
using VampireCommandFramework;
using Plugin = V.Core.Plugin;

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

        public class Deus
        {
            public Dictionary<int, RankSpellConstructor> Spells = new Dictionary<int, RankSpellConstructor>();

            public Deus()
            {
                Spells.Add(5, new RankSpellConstructor("NukeAll", V.Data.Prefabs.AB_Debug_NukeAll_Group, 5));
                Spells.Add(4, new RankSpellConstructor("DivineShield", V.Data.Prefabs.AB_ChurchOfLight_Paladin_HolyBubble_Beam_AbilityGroup, 4));
                Spells.Add(3, new RankSpellConstructor("LightningStorm", V.Data.Prefabs.AB_Monster_LightningStorm_AbilityGroup, 3));
                Spells.Add(2, new RankSpellConstructor("ChaosBreath", V.Data.Prefabs.AB_Manticore_ChaosBreath_AbilityGroup, 2));
                Spells.Add(1, new RankSpellConstructor("Leapattack", V.Data.Prefabs.AB_Cursed_MountainBeast_LeapAttack_Travel_AbilityGroup, 1));
            }
        }

        public static class CommandHandler
        {
            private static readonly Dictionary<string, System.Func<object>> classFactories = new Dictionary<string, System.Func<object>>
            {
                { "nightmarshal", () => new Nightmarshal() },
                { "deus", () => new Deus() },
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
                        if (choice == "deus" || choice == "Deus")
                        {
                            if (!ctx.Event.User.IsAdmin)
                            {
                                ctx.Reply("You must be an admin to use this class.");
                                return;
                            }
                            else
                            {
                                rankData.ClassChoice = choice;
                                ctx.Reply($"Class set to {choice}.");
                            }
                        }
                        else
                        {
                            rankData.ClassChoice = choice;
                            ctx.Reply($"Class set to {choice}.");
                        }
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
                        // classInstance is as
                        // Check if the class instance is of expected type and contains the spell
                        if (classInstance is Nightmarshal nightmarshal && nightmarshal.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructorNightmarshal))
                        {
                            // Now you have the spellConstructor, you can check the player's rank
                            if (rankData.Rank >= spellConstructorNightmarshal.RequiredRank)
                            {
                                // Here you would typically cast the spell or apply its effects
                                PrefabGUID newSpell = spellConstructorNightmarshal.SpellGUID;
                                V.Data.FoundPrefabGuid foundPrefabGuid = new(newSpell);
                                rankData.RankSpell = newSpell.GuidHash;
                                //CastCommand(ctx, foundPrefabGuid, null); // Assuming this is how you cast the spell
                                V.Core.Tools.Helper.BuffCharacter(character, V.Data.Prefabs.AllowJumpFromCliffsBuff, 0, false);
                                ctx.Reply($"Rank spell set to {spellConstructorNightmarshal.Name}.");
                                ChatCommands.SavePlayerRanks();
                            }
                            else
                            {
                                ctx.Reply($"You must be at least rank {spellConstructorNightmarshal.RequiredRank} to use this ability.");
                            }
                        }
                        else
                        {
                            if (classInstance is Deus deus && deus.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructorDeus))
                            {
                                if (rankData.Rank >= spellConstructorDeus.RequiredRank)
                                {
                                    // Here you would typically cast the spell or apply its effects
                                    PrefabGUID newSpell = spellConstructorDeus.SpellGUID;
                                    V.Data.FoundPrefabGuid foundPrefabGuid = new(newSpell);
                                    rankData.RankSpell = newSpell.GuidHash;
                                    //CastCommand(ctx, foundPrefabGuid, null); // Assuming this is how you cast the spell
                                    V.Core.Tools.Helper.BuffCharacter(character, V.Data.Prefabs.AllowJumpFromCliffsBuff, 0, false);
                                    ctx.Reply($"Rank spell set to {spellConstructorDeus.Name}.");
                                    ChatCommands.SavePlayerRanks();
                                }
                                else
                                {
                                    ctx.Reply($"You must be at least rank {spellConstructorDeus.RequiredRank} to use this ability.");
                                }
                            }
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