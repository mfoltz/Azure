using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VBuild.Core.Commands;
using VBuild.Core.Toolbox;
using VBuild.Core.Commands;
using VBuild.Core.Toolbox;
using VBuild.Core;

[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public static class BuffSystem_Spawn_ServerPatch
{
    #region GodMode & Other Buff
    private static ModifyUnitStatBuff_DOTS Cooldown = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.CooldownModifier,
        Value = 0,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SunCharge = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SunChargeTime,
        Value = 50000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS Hazard = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ImmuneToHazards,
        Value = 1,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SunResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SunResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS Speed = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.MovementSpeed,
        Value = 15,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(4)
    };

    private static ModifyUnitStatBuff_DOTS PResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PhysicalResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS FResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.FireResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS HResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.HolyResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SilverResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS GResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.GarlicResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SPResist = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SpellResistance,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PhysicalPower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SiegePower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SiegePower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS RPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ResourcePower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS SPPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SpellPower,
        Value = 10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS PHRegen = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PassiveHealthRegen,
        Value = 100000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS HRecovery = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.HealthRecovery,
        Value = 100000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS MaxHP = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.MaxHealth,
        Value = 100000,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(5)
    };

    private static ModifyUnitStatBuff_DOTS MaxYield = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ResourceYield,
        Value = 10,
        ModificationType = ModificationType.Multiply,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS DurabilityLoss = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ReducedResourceDurabilityLoss,
        Value = -10000,
        ModificationType = ModificationType.Add,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS AttackSpeed = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.AttackSpeed,
        Value = 5,
        ModificationType = ModificationType.Multiply,
        Id = ModificationId.NewId(0)
    };
    #endregion

    #region TrollMode & Other Buff

    private static ModifyUnitStatBuff_DOTS TrollPPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.PhysicalPower,
        Value = -100,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS TrollRPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.ResourcePower,
        Value = -100,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS TrollSPPower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SpellPower,
        Value = -100,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };

    private static ModifyUnitStatBuff_DOTS TrollSiegePower = new ModifyUnitStatBuff_DOTS()
    {
        StatType = UnitStatType.SiegePower,
        Value = -100,
        ModificationType = ModificationType.Set,
        Id = ModificationId.NewId(0)
    };
    #endregion

    static readonly List<ModifyUnitStatBuff_DOTS> immortal = new List<ModifyUnitStatBuff_DOTS>()
    {
        PResist,
        FResist,
        HResist,
        SResist,
        SunResist,
        GResist,
        SPResist,
        Hazard,
        SunCharge,
    };

    static readonly List<ModifyUnitStatBuff_DOTS> nocd = new List<ModifyUnitStatBuff_DOTS>()
    {
        Cooldown
    };

    static readonly List<ModifyUnitStatBuff_DOTS> speed = new List<ModifyUnitStatBuff_DOTS>()
    {
        Speed
    };

    static readonly List<ModifyUnitStatBuff_DOTS> hp = new List<ModifyUnitStatBuff_DOTS>()
    {
        MaxHP
    };

    static readonly List<ModifyUnitStatBuff_DOTS> attackSpeed = new List<ModifyUnitStatBuff_DOTS>()
    {
        AttackSpeed
    };

    static readonly List<ModifyUnitStatBuff_DOTS> damage = new List<ModifyUnitStatBuff_DOTS>()
    {
        PPower,
        RPower,
        SPPower,
        SiegePower
    };

    static readonly List<ModifyUnitStatBuff_DOTS> trollDamage = new List<ModifyUnitStatBuff_DOTS>()
    {
        TrollPPower,
        TrollRPower,
        TrollSPPower,
        TrollSiegePower
    };

    static readonly Dictionary<string, List<ModifyUnitStatBuff_DOTS>> buffGroups = new Dictionary<string, List<ModifyUnitStatBuff_DOTS>>()
    {
        {"immortal", immortal},
        {"speed", speed},
        {"attackSpeed", attackSpeed},
        {"nocd", nocd},
        {"trollDamage", trollDamage},
        {"damage", damage},
        {"hp", hp}
    };

    public static void Prefix(BuffSystem_Spawn_Server __instance)
    {
        EntityManager entityManager = __instance.EntityManager;
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            PrefabGUID GUID = entity.Read<PrefabGUID>();
            Entity Owner = entity.Read<EntityOwner>().Owner;
            if (!Owner.Has<PlayerCharacter>()) continue;
            if (GUID == VBuild.Data.Buff.CustomBuff)
            {
                var Buffer = entityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(entity);
                Buffer.Clear();
                if (GodCommands.PlayerBuffDictionary.ContainsKey(Owner))
                {
                    foreach (var key in GodCommands.PlayerBuffDictionary[Owner].Keys)
                    {
                        if (GodCommands.PlayerBuffDictionary[Owner][key])
                        {
                            foreach (var buff in buffGroups[key])
                            {
                                if (buff.Id.Id == Speed.Id.Id && GodCommands.PlayerSpeeds.ContainsKey(Owner))
                                {
                                    var modifiedBuff = buff;
                                    modifiedBuff.Value = GodCommands.PlayerSpeeds[Owner];
                                    Buffer.Add(modifiedBuff);
                                }
                                else if (buff.Id.Id == MaxHP.Id.Id && GodCommands.PlayerHps.ContainsKey(Owner) && GodCommands.PlayerHps[Owner] != 0)
                                {
                                    var modifiedBuff = buff;
                                    modifiedBuff.Value = GodCommands.PlayerHps[Owner];
                                    Buffer.Add(modifiedBuff);
                                }
                                else
                                {
                                    Buffer.Add(buff);
                                }
                            }
                        }
                    }
                    if (GodCommands.isBuffEnabled(Owner, "immortal"))
                    {
                        entity.Add<ModifyBloodDrainBuff>();
                        var modifyBloodDrainBuff = new ModifyBloodDrainBuff()
                        {
                            AffectBloodValue = true,
                            AffectIdleBloodValue = true,
                            BloodValue = 0,
                            BloodIdleValue = 0,

                            ModificationId = new ModificationId(),
                            ModificationIdleId = new ModificationId(),
                            IgnoreIdleDrainModId = new ModificationId(),

                            ModificationPriority = 1000,
                            ModificationIdlePriority = 1000,

                            ModificationType = ModificationType.Set,
                            ModificationIdleType = ModificationType.Set,

                            IgnoreIdleDrainWhileActive = true,
                        };
                        entity.Write(modifyBloodDrainBuff);
                        entity.Add<DisableAggroBuff>();
                        entity.Write(new DisableAggroBuff
                        {
                            Mode = DisableAggroBuffMode.OthersDontAttackTarget
                        });

                        entity.Add<BuffModificationFlagData>();
                        var buffModificationFlagData = new BuffModificationFlagData()
                        {
                            ModificationTypes = (long)(BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision | BuffModificationTypes.Invulnerable | BuffModificationTypes.ImmuneToHazards | BuffModificationTypes.ImmuneToSun | BuffModificationTypes.CannotBeDisconnectDragged | BuffModificationTypes.DisableUnitVisibility | BuffModificationTypes.IsVbloodGhost),
                            ModificationId = ModificationId.NewId(0),
                        };
                        entity.Write(buffModificationFlagData);
                    }
                }
            }
            else if (GUID == VBuild.Data.Buff.Buff_InCombat_PvPVampire)
            {
                if (GodCommands.isBuffEnabled(Owner, "immortal") || GodCommands.isBuffEnabled(Owner, "immortal"))
                {
                    Helper.UnbuffCharacter(Owner, VBuild.Data.Buff.Buff_InCombat_PvPVampire);
                }
            }
        }
    }
}
