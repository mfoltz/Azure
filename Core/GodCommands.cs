using Bloodstone.API;
using ProjectM;
using Unity.Entities;
using Unity.Transforms;
using VBuild.Core.Services;
using VBuild.Data;
using VampireCommandFramework;
using VBuild.Core.Toolbox;
using VBuild.Core.Converters;

namespace VBuild.Core
{
    
    internal class GodCommands
    {
        public static Dictionary<Entity, Dictionary<string, bool>> PlayerBuffDictionary = new Dictionary<Entity, Dictionary<string, bool>>();
        public static Dictionary<Entity, float> PlayerSpeeds = new Dictionary<Entity, float>();
        public static Dictionary<Entity, int> PlayerHps = new Dictionary<Entity, int>();
        public static Dictionary<Entity, float> PlayerProjectileSpeeds = new Dictionary<Entity, float>();
        public static Dictionary<Entity, float> PlayerProjectileRanges = new Dictionary<Entity, float>();
        public static Dictionary<Entity, int> PlayerProjectileBounces = new Dictionary<Entity, int>();
        private const int DEFAULT_FAST_SPEED = 15;

        private static readonly List<string> GodFlags = new List<string>
    {
        "immortal", "nocd", "speed", "attackSpeed", "damage", "hp"
    };

        private static readonly Dictionary<string, List<string>> groupBuffTypes = new Dictionary<string, List<string>>()
    {
        {"god", GodFlags },
    };

        [Command("hp", adminOnly: true)]
        public void HpCommand(ChatCommandContext ctx, int hp = 0, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            PlayerHps[Character] = hp;
            if (hp != 0)
            {
                EnableBuff(ctx, User, Character, "hp");
                ctx.Reply($"hp set to {hp}");
            }
            else
            {
                if (ToggleBuff(User, Character, "hp"))
                {
                    ctx.Reply("high hp mode enabled");
                }
                else
                {
                    ctx.Reply("hp set to default");
                }
            }
        }

        [Command("projectilespeed", adminOnly: true)]
        public void ProjectileSpeedCommand(ChatCommandContext ctx, float speed = 1f, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            PlayerProjectileSpeeds[Character] = speed;
            if (speed != 1f)
            {
                ctx.Reply($"projectile speed multipled by {speed}");
            }
            else
            {
                ctx.Reply("projectile speed set to default");
            }
        }

        [Command("damage", adminOnly: true)]
        public void DamageCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            DisableBuffFlag(Character, "trollDamage");
            if (ToggleBuff(User, Character, "damage"))
            {
                ctx.Reply("damage mode enabled");
            }
            else
            {
                ctx.Reply("damage mode disabled");
            }
        }

        [Command("projectilerange", adminOnly: true)]
        public void ProjectileRangeCommand(ChatCommandContext ctx, float range = 1f, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            PlayerProjectileRanges[Character] = range;
            if (range != 1f)
            {
                ctx.Reply($"projectile range multipled by {range}");
            }
            else
            {
                ctx.Reply("projectile range set to default");
            }
        }

        [Command("projectilebounces", adminOnly: true)]
        public void ProjectileBouncesCommand(ChatCommandContext ctx, int bounces = -1, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            PlayerProjectileBounces[Character] = bounces;
            if (bounces != -1)
            {
                ctx.Reply($"projectile bounces set to {bounces}");
            }
            else
            {
                ctx.Reply("projectile bounces set to default");
            }
        }

        [Command("golem", adminOnly: true)]
        public void GolemCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            if (BuffUtility.HasBuff(VWorld.Server.EntityManager, Character, Prefabs.AB_Shapeshift_Golem_T02_Buff))
            {
                Helper.UnbuffCharacter(Character, Prefabs.AB_Shapeshift_Golem_T02_Buff);
                ctx.Reply("Ungolem'd");
            }
            else
            {
                Helper.BuffPlayer(Character, User, Prefabs.AB_Shapeshift_Golem_T02_Buff, Helper.NO_DURATION, true);
                ctx.Reply("Golem'd");
            }
        }

        [Command("speed", adminOnly: true)]
        public void SpeedCommand(ChatCommandContext ctx, float speed = DEFAULT_FAST_SPEED, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            PlayerSpeeds[Character] = speed;
            if (speed != DEFAULT_FAST_SPEED)
            {
                EnableBuff(ctx, User, Character, "speed");
                ctx.Reply($"speed set to {speed}");
            }
            else
            {
                if (ToggleBuff(User, Character, "speed"))
                {
                    ctx.Reply("speed mode enabled");
                }
                else
                {
                    ctx.Reply("speed mode disabled");
                }
            }
        }

        [Command("nocd", adminOnly: true)]
        public void NoCdCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            if (ToggleBuff(User, Character, "nocd"))
            {
                ctx.Reply("nocd mode enabled");
                Helper.ResetCooldown(Character);
            }
            else
            {
                ctx.Reply("nocd mode disabled");
            }
        }

        [Command("attackspeed", adminOnly: true)]
        public void AttackSpeedCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            if (ToggleBuff(User, Character, "attackSpeed"))
            {
                ctx.Reply("attack speed enabled");
            }
            else
            {
                ctx.Reply("attack speed disabled");
            }
        }

        [Command("immortal", adminOnly: true)]
        public void ImmortalCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            if (ToggleBuff(User, Character, "immortal"))
            {
                ctx.Reply("Made immortal");
            }
            else
            {
                ctx.Reply("Made mortal");
            }
        }

        public void SpectateCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            if (BuffUtility.HasBuff(VWorld.Server.EntityManager, Character, Prefabs.Admin_Observe_Invisible_Buff))
            {
                NormalizeCharacter(Character);
                Helper.TeleportPlayer(Character, User, ctx.Event.SenderUserEntity.Read<LocalToWorld>().Position);
                ctx.Reply("Removed spectator buff");
            }
            else
            {
                PlayerSpeeds[Character] = DEFAULT_FAST_SPEED;
                Helper.BuffPlayer(Character, User, Prefabs.Admin_Observe_Invisible_Buff, Helper.NO_DURATION, false);
                EnableBuff(ctx, User, Character, "speed");
                ctx.Reply("Set to spectator");
            }
        }

        [Command(name: "god", shortHand: "g", adminOnly: true, usage: ".v g <Player>", description: "Turns on godmode.")]
        public void GodCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            PlayerSpeeds[Character] = DEFAULT_FAST_SPEED;
            PlayerProjectileSpeeds[Character] = 10f;
            PlayerProjectileRanges[Character] = 10f;
            MakePlayerImmaterial(User, Character);
            Helper.BuffPlayer(Character, User, Prefabs.EquipBuff_ShroudOfTheForest, -1, true);
            EnableBuff(ctx, User, Character, "god");
            //Helper.ResetCharacter(Character);
            ctx.Reply("Set to god mode");
        }

        [Command(name: "normal", shortHand: "n", adminOnly: true, usage: ".v n <Player>", description: "Turns off godmode.")]
        public void NormalCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            NormalizeCharacter(Character);
            ctx.Reply("Set to normal mode");
        }

        [Command("immaterial", adminOnly: true)]
        public void ImmaterialCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            if (ToggleImmaterial(User, Character))
            {
                ctx.Reply("Made immaterial");
            }
            else
            {
                ctx.Reply("Made material");
            }
        }

        private static void NormalizeCharacter(Entity Character)
        {
            DisableAllBuffFlags(Character);
            PlayerSpeeds[Character] = DEFAULT_FAST_SPEED;
            PlayerProjectileBounces[Character] = -1;
            PlayerProjectileRanges[Character] = 1;
            PlayerProjectileSpeeds[Character] = 1;
            Helper.ClearExtraBuffs(Character);
            MakePlayerMaterial(Character);
        }

        public static bool isBuffEnabled(Entity Character, string buffType)
        {
            if (!PlayerBuffDictionary.TryGetValue(Character, out var buffEnabledDictionary))
            {
                PlayerBuffDictionary[Character] = new Dictionary<string, bool>()
            {
                { buffType, false }
            };
            }
            else
            {
                if (!buffEnabledDictionary.TryGetValue(buffType, out var buffEnabled))
                {
                    PlayerBuffDictionary[Character][buffType] = false;
                }
            }
            return PlayerBuffDictionary[Character][buffType];
        }

        private static void EnableBuff(ChatCommandContext ctx, Entity User, Entity Character, string buffType)
        {
            var buffTypeCapitalized = char.ToUpper(buffType[0]) + buffType.Substring(1);
            Helper.UnbuffCharacter(Character, Data.Buff.CustomBuff);
            isBuffEnabled(Character, buffType); //hacky way to build out the dictionary
            if (!EnableGroupBuffFlags(Character, buffType))
            {
                PlayerBuffDictionary[Character][buffType] = true;
            }
            var action = new ScheduledAction(Helper.BuffPlayer, new object[] { Character, User, Data.Buff.CustomBuff, Helper.NO_DURATION, true });
            ActionScheduler.ScheduleAction(action, 2);
        }

        private static bool ToggleBuff(Entity User, Entity Character, string buffType)
        {
            Helper.UnbuffCharacter(Character, Data.Buff.CustomBuff);
            var action = new ScheduledAction(Helper.BuffPlayer, new object[] { Character, User, Data.Buff.CustomBuff, Helper.NO_DURATION, true });
            if (isBuffEnabled(Character, buffType))
            {
                PlayerBuffDictionary[Character][buffType] = false;
                ActionScheduler.ScheduleAction(action, 2);
                return false;
            }
            else
            {
                PlayerBuffDictionary[Character][buffType] = true;
                ActionScheduler.ScheduleAction(action, 2);
                return true;
            }
        }

        private static bool EnableGroupBuffFlags(Entity Character, string group)
        {
            if (!groupBuffTypes.Keys.Contains(group))
            {
                return false;
            }
            DisableAllBuffFlags(Character);
            foreach (var buffType in groupBuffTypes[group])
            {
                isBuffEnabled(Character, buffType);
                PlayerBuffDictionary[Character][buffType] = true;
            }
            return true;
        }

        private static void DisableAllBuffFlags(Entity Character)
        {
            if (PlayerBuffDictionary.ContainsKey(Character))
            {
                foreach (var buffType in PlayerBuffDictionary[Character].Keys)
                {
                    isBuffEnabled(Character, buffType);
                    PlayerBuffDictionary[Character][buffType] = false;
                }
            }
        }

        private static void DisableBuffFlag(Entity Character, string buffType)
        {
            if (isBuffEnabled(Character, buffType))
            {
                PlayerBuffDictionary[Character][buffType] = false;
            }
        }

        private static void MakePlayerImmaterial(Entity User, Entity Character)
        {
            Helper.BuffPlayer(Character, User, Data.Buff.AB_Blood_BloodRite_Immaterial, Helper.NO_DURATION, true);
            if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, Character, Prefabs.AB_Blood_BloodRite_Immaterial, out Entity buffEntity))
            {
                var modifyMovementSpeedBuff = buffEntity.Read<ModifyMovementSpeedBuff>();
                modifyMovementSpeedBuff.MoveSpeed = 1; //bloodrite makes you accelerate forever, disable this
                buffEntity.Write(modifyMovementSpeedBuff);
            }
        }

        private static void MakePlayerMaterial(Entity Character)
        {
            Helper.UnbuffCharacter(Character, Data.Buff.AB_Blood_BloodRite_Immaterial);
        }

        private static bool ToggleInvisibility(Entity User, Entity Character)
        {
            if (!BuffUtility.HasBuff(VWorld.Server.EntityManager, Character, Prefabs.AB_InvisibilityAndImmaterial_Buff))
            {
                if (Helper.BuffPlayer(Character, User, Prefabs.AB_InvisibilityAndImmaterial_Buff, Helper.NO_DURATION, true))
                {
                    var action = new ScheduledAction(Helper.BuffPlayer, new object[] { Character, User, Prefabs.AB_Mantrap_Immaterial_Buff, Helper.NO_DURATION, true });
                    ActionScheduler.ScheduleAction(action, 2);
                    return true;
                }
            }
            else
            {
                Helper.UnbuffCharacter(Character, Prefabs.AB_InvisibilityAndImmaterial_Buff);
                Helper.UnbuffCharacter(Character, Prefabs.AB_Mantrap_Immaterial_Buff);
            }
            return false;
        }

        private static bool ToggleImmaterial(Entity User, Entity Character)
        {
            if (!BuffUtility.HasBuff(VWorld.Server.EntityManager, Character, Data.Buff.AB_Blood_BloodRite_Immaterial))
            {
                MakePlayerImmaterial(User, Character);
            }
            else
            {
                MakePlayerMaterial(Character);
            }
            return false;
        }
    }
}