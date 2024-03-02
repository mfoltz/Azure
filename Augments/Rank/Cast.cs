using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using VPlus.Core;
using VPlus.Core.Commands;
using VPlus.Core.Services;
using VPlus.Core.Tools;
using VPlus.Data;
using VampireCommandFramework;
using Plugin = VPlus.Core.Plugin;
using StringComparer = System.StringComparer;
using static VPlus.Augments.Rank.CastCommands;

#nullable disable

namespace VPlus.Augments.Rank
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



        public class SpellRegistry
        {
            public static Dictionary<int, RankSpellConstructor> StaticSpells { get; private set; }

            static SpellRegistry()
            {
                StaticSpells = new Dictionary<int, RankSpellConstructor>
                {
                    Spells.Add(5, new RankSpellConstructor("Batswarm", VPlus.Data.Prefabs.AB_BatVampire_BatSwarm_AbilityGroup, 5));
                Spells.Add(4, new RankSpellConstructor("Nightdash", VPlus.Data.Prefabs.AB_BatVampire_NightDash_AbilityGroup, 4));
                Spells.Add(3, new RankSpellConstructor("Batstorm", VPlus.Data.Prefabs.AB_BatVampire_BatStorm_AbilityGroup, 3));
                Spells.Add(2, new RankSpellConstructor("Batleap", VPlus.Data.Prefabs.AB_BatVampire_SummonMinions_AbilityGroup, 2));
                Spells.Add(1, new RankSpellConstructor("Batwhirlwind", VPlus.Data.Prefabs.AB_BatVampire_Whirlwind_AbilityGroup, 1));
                // Add more spells as needed
            };
        }

        public Dictionary<int, RankSpellConstructor> Spells { get; private set; }

        public SpellRegistry()
        {
            Spells = SpellRegistry.StaticSpells;
        }


        public static Dictionary<int, RankSpellConstructor> GetSpellsBySet(string setName)
        {
            if (ModelRegistry.spellsBySet.TryGetValue(setName, out var setTiles))
            {
                return setTiles.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            return null;
        }

        public static readonly HashSet<string> adminSets = new(StringComparer.OrdinalIgnoreCase)
        {
            "T0",
        };

        public static class ModelRegistry
        {
            public static readonly Dictionary<string, Dictionary<int, RankSpellConstructor>> spellsBySet = new(StringComparer.OrdinalIgnoreCase);

            static ModelRegistry()
            {
                // Register spells similar to how tiles were registered
                RegisterSpells("BasicSpells", new Dictionary<int, RankSpellConstructor>
                {
                    { 1, new RankSpellConstructor("Fireball", new PrefabGUID(123456)) },
                    { 2, new RankSpellConstructor("Ice Spear", new PrefabGUID(234567)) },
                    // Add more spells
                });
            }

            public static void RegisterSpells(string setName, Dictionary<int, RankSpellConstructor> spells)
            {
                spellsBySet[setName] = spells;
            }


        }

    }
}