using System.Collections.Generic;
using ProjectM;

namespace WorldBuild.Data;

public static class JewelData
{
    public static float RANDOM_POWER = -1f;

    public static readonly Dictionary<string, PrefabGUID> abilityToPrefabDictionary = new Dictionary<string, PrefabGUID>
    {
        {
            "bloodfountain",
            Prefabs.AB_Blood_BloodFountain_AbilityGroup
        },
        {
            "bloodrage",
            Prefabs.AB_Blood_BloodRage_AbilityGroup
        },
        {
            "bloodrite",
            Prefabs.AB_Blood_BloodRite_AbilityGroup
        },
        {
            "sanguinecoil",
            Prefabs.AB_Blood_SanguineCoil_AbilityGroup
        },
        {
            "shadowbolt",
            Prefabs.AB_Blood_Shadowbolt_AbilityGroup
        },
        {
            "aftershock",
            Prefabs.AB_Chaos_Aftershock_Group
        },
        {
            "chaosbarrier",
            Prefabs.AB_Chaos_Barrier_AbilityGroup
        },
        {
            "powersurge",
            Prefabs.AB_Chaos_PowerSurge_AbilityGroup
        },
        {
            "void",
            Prefabs.AB_Chaos_Void_AbilityGroup
        },
        {
            "chaosvolley",
            Prefabs.AB_Chaos_Volley_AbilityGroup
        },
        {
            "crystallance",
            Prefabs.AB_Frost_CrystalLance_AbilityGroup
        },
        {
            "frostbat",
            Prefabs.AB_Frost_FrostBat_AbilityGroup
        },
        {
            "iceblock",
            Prefabs.AB_Frost_IceBlock_AbilityGroup
        },
        {
            "icenova",
            Prefabs.AB_Frost_IceNova_AbilityGroup
        },
        {
            "frostbarrier",
            Prefabs.AB_FrostBarrier_AbilityGroup
        },
        {
            "misttrance",
            Prefabs.AB_Illusion_MistTrance_AbilityGroup
        },
        {
            "mosquito",
            Prefabs.AB_Illusion_Mosquito_AbilityGroup
        },
        {
            "phantomaegis",
            Prefabs.AB_Illusion_PhantomAegis_AbilityGroup
        },
        {
            "spectralwolf",
            Prefabs.AB_Illusion_SpectralWolf_AbilityGroup
        },
        {
            "wraithspear",
            Prefabs.AB_Illusion_WraithSpear_AbilityGroup
        },
        {
            "balllightning",
            Prefabs.AB_Storm_BallLightning_AbilityGroup
        },
        {
            "cyclone",
            Prefabs.AB_Storm_Cyclone_AbilityGroup
        },
        {
            "discharge",
            Prefabs.AB_Storm_Discharge_AbilityGroup
        },
        {
            "lightningcurtain",
            Prefabs.AB_Storm_LightningWall_AbilityGroup
        },
        {
            "polarityshift",
            Prefabs.AB_Storm_PolarityShift_AbilityGroup
        },
        {
            "boneexplosion",
            Prefabs.AB_Unholy_CorpseExplosion_AbilityGroup
        },
        {
            "corruptedskull",
            Prefabs.AB_Unholy_CorruptedSkull_AbilityGroup
        },
        {
            "deathknight",
            Prefabs.AB_Unholy_DeathKnight_AbilityGroup
        },
        {
            "soulburn",
            Prefabs.AB_Unholy_Soulburn_AbilityGroup
        },
        {
            "wardofthedamned",
            Prefabs.AB_Unholy_WardOfTheDamned_AbilityGroup
        },
        {
            "veilofblood",
            Prefabs.AB_Vampire_VeilOfBlood_Group
        },
        {
            "veilofbones",
            Prefabs.AB_Vampire_VeilOfBones_AbilityGroup
        },
        {
            "veilofchaos",
            Prefabs.AB_Vampire_VeilOfChaos_Group
        },
        {
            "veiloffrost",
            Prefabs.AB_Vampire_VeilOfFrost_Group
        },
        {
            "veilofillusion",
            Prefabs.AB_Vampire_VeilOfIllusion_AbilityGroup
        },
        {
            "veilofstorm",
            Prefabs.AB_Vampire_VeilOfStorm_Group
        }
    };

    public static Dictionary<string, List<KeyValuePair<PrefabGUID, string>>> SpellMods = new Dictionary<string, List<KeyValuePair<PrefabGUID, string>>>
    {
        {
            "bloodfountain",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_RecastLesser, "Recast to conjure an AoE that deals damage to enemies and heals allies (17 - 25%) on hit and then explodes (26 - 33%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactApplyLeech, "Hit applies Leech"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactFadingSnare, "Hit applies fading Snare (0.6 - 1.2s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactDispell, "Hit removes negative effects from caster and allies"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_SecondImpactKnockback, "Explosion pushes enemies back (2 - 3m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_ConsumeLeechBonusDamage, "Explosion consumes Leech to deal damage (27 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_FirstImpactHealIncrease, "Increase hit healing (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_SecondImpactDamageIncrease, "Increase explosion damage (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodFountain_SecondImpactHealIncrease, "Increase explosion healing (25 - 50%)")
            }
        },
        {
            "bloodrage",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_IncreaseMoveSpeed, "Explosion increases ally MS (11 - 18%) for 3s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_HealOnKill, "Kill an enemy to heal (1.2 - 2.5% max HP)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs, "Cast removes all negative effects from caster and allies"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_Shield, "Cast grants a shield (65 - 90%) to caster and allies"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium, "Cast applies a fading Snare (0.8 - 1.5s) on enemies"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_DamageBoost, "Increases physical damage (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_IncreaseLifetime, "Increase effect duration (17 - 30%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRage_IncreaseMoveSpeed, "Increase MS (3.6 - 10%)")
            }
        },
        {
            "bloodrite",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_Stealth, "Turn invisible while Immaterial"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium, "Trigger applies a fading Snare (0.8 - 1.5s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_DamageOnAttack, "Trigger for first primary attack within 5s to deal bonus damage (29 - 50%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High, "Increase MS (22 - 35%) during channel"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_IncreaseLifetime, "Increase Immaterial duration (17 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_BonusDamage, "Increase damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_ConsumeLeechReduceCooldownXTimes, "Trigger consumes Leech to decrease spell CD (0.6 - 1s) per target (3 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BloodRite_ConsumeLeechHealXTimes, "Trigger consumes Leech to heal (1.7 - 2.5% max HP) per target (3 max)")
            }
        },
        {
            "sanguinecoil",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_KillRecharge, "Lethal attacks restore 1 charge"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_AddBounces, "Hit bounces projectile to an additional target or the caster to deal damage or heal (35 - 60%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_AddCharges, "Increase charges by 1"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_BonusDamage, "Increase damage (9 - 15%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_BonusHealing, "Increase healing (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Blood_ConsumeLeechSelfHeal, "Hit consumes Leech to heal (3.5 - 6% max HP)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SanguineCoil_ConsumeLeechBonusDamage, "Hit consumes Leech to deal damage (22 - 35%)")
            }
        },
        {
            "shadowbolt",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shadowbolt_ExplodeOnHit, "Hit conjures an AoE that deals damage (23 - 40%) and applies Leech"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shadowbolt_ForkOnHit, "Hit forks into 2 projectiles that deal damage (55 - 80%) and apply Leech"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium, "Hit pushes enemies back (1.7 - 3m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease CD (7 - 12%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_CastRate, "Decrease cast time (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shadowbolt_ConsumeLeechBonusDamage, "Hit consumes Leech to deal damage (35 - 60%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Blood_ConsumeLeechSelfHeal, "Hit consumes Leech to heal (3.5 - 6% max HP)")
            }
        },
        {
            "veilofblood",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_DashInflictLeech, "Dashing through an enemy applies Leech"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_BloodNova, "Next primary attack within 3s conjurs an AoE that deals damage (17 - 25%) and applies Leech"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_Empower, "Next primary attack within 3s consumes Leech to increase physical power (17 - 30%) for 4s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_AttackInflictFadingSnare, "Next primary attack within 3s applies a fading Snare (1.3 - 2s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration, "Increase Elude duration (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary, "Increase damage (12 - 25%) of next primary attack within 3s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBlood_SelfHealing, "Increase healing (1.6 - 2.3% max HP)")
            }
        },
        {
            "aftershock",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Aftershock_KnockbackArea, "Cast knocks enemies back (1.7 - 3m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Aftershock_InflictSlowOnProjectile, "Cast applies a fading Snare (1.1 - 1.8s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Aftershock_BonusDamage, "Increase damage (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium, "Increase projectile range (14 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease CD (7 - 12%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion, "Explosion consumes Ignite to conjure an AoE that deals damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated, "Explosion consumes Ignite to increase MS (7 - 10%) for 5s (2 max)")
            }
        },
        {
            "chaosbarrier",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_ArcProjectiles, "Recast launches 3 projectiles instead of 1 that deal damage (19 - 25%) and apply Ignite"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_StunOnAbsorbMelee, "Barrier hit applies Stun (0.8 - 1.2s) to the attacker"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion, "Projectile hit consumes Ignite to conjure an AoE that deals damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_ForkOnHit, "Projectile hit forks into 2 projectiles that deal damage (32 - 45%) and apply Ignite"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_BonusDamage, "Increase damage (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low, "Increase MS (7 - 12%) during channel"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_IncreasePullRange, "Increase pull distance (45 - 70%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Barrier_ConsumeAttackReduceCooldownXTimes, "Decrease CD (0.5 - 0.8s) on absorbed hit (3 max)")
            }
        },
        {
            "chaosvolley",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Volley_SecondProjectileBonusDamage, "Hitting a different enemy with the second projectile deals damage (29 - 50%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Light, "Hit pushes enemies back (0.9 - 1.5m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Volley_BonusDamage, "Increase damage (12 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease CD (7 - 12%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion, "Hit consumes Ignite to conjure an AoE that deals damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated, "Hit consumes Ignite to increase MS (7 - 10%) for 5s")
            }
        },
        {
            "powersurge",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_RecastDestonate, "Recast to remove the effect and conjure an AoE that deals damage (45 - 70%) and pulls enemies toward the target"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs, "Removes all negative effects"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_Shield, "Apply a shield (65 - 90%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_IncreaseDurationOnKill, "Lethal attacks during the effect reduce CD by 1s (1 - 4 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_AttackSpeed, "Increase AS (4 - 10%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_Haste, "Increase MS (4 - 8%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_Lifetime, "Increase effect duration (17 - 30%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PowerSurge_EmpowerPhysical, "Increase physical power (10 - 20%)")
            }
        },
        {
            "void",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_FragBomb, "Explosion conjures 3 AoEs that explode to deal damage (17 - 30%) and apply Ignite"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_BurnArea, "Explosion leaves behind an AoE that deals damage (9 - 15%) (3 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_BonusDamage, "Increase damage (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium, "Increase range (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Chaos_Void_ReduceChargeCD, "Increase recharge rate (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion, "Explosion consumes Ignite to conjure an AoE that deals damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated, "Explosion consume Ignite to increase MS (7 - 10%) for 5s (2 max)")
            }
        },
        {
            "veilofchaos",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfChaos_BonusIllusion, "Recast conjurs a second illusion that deals damage (23 - 40%) in an AoE after 2.2s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfChaos_ApplySnareOnExplode, "Explosion applies a fading Snare (0.8 - 1.5s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration, "Increase Elude duration (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfChaos_BonusDamageOnExplode, "Increase explosion damage (12 - 25%) of any illusion"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoCombustion_OnAttack, "Next primary attack within 3s consumes Ignite to conjure an AoE that deals damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Chaos_ConsumeIgniteIntoHeated_OnAttack, "Next primary attack within 3s consumes Ignite to increase MS (7 - 10%) for 5s (2 max)")
            }
        },
        {
            "crystallance",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CrystalLance_PierceEnemies, "Projectile pierces enemies (1 - 3 max) dealing 50% reduced damage"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_SplinterNovaOnFrosty, "Hit on an enemy affected by Chill or Freeze launches 8 projectiles that deal damage (17 - 30%) and apply Chill"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CrystalLance_BonusDamageToFrosty, "Increase damage (35 - 60%) to enemies affected by Chill or Freeze"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CrystalLance_IncreaseFreeze, "Increase Freeze duration (0.5 - 1.2s) to enemies affected by Chill"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_CastRate, "Decrease cast time (12 - 25%)")
            }
        },
        {
            "frostbarrier",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_ConsumeAttackReduceCooldownXTimes, "Barrier hits (3 max) decrease CD (0.5 - 0.8s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast, "Recast consumes Chill and applies Freeze (2.3 - 4s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_KnockbackOnRecast, "Recast pushes enemies back (1.7 - 3m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_ShieldOnFrostyRecast, "Recast shields caster (36 - 70%) when hitting an enemy affected by Chill or Freeze"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_BonusDamage, "Increase damage (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBarrier_BonusSpellPowerOnAbsorb, "Increase spell power (5 - 10%) for 6s (3 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low, "Increase MS (7 - 12%) during channel")
            }
        },
        {
            "frostbat",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_SplinterNovaOnFrosty, "Hit on an enemy affected by Chill or Freeze launches 8 projectiles that deal damage (17 - 30%) and apply Chill"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ShieldOnFrosty, "Hit on an enemy affected by Chill or Freeze shields caster (36 - 70%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBat_AreaDamage, "Hit conjures an AoE that deals damage (27 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_FrostBat_BonusDamageToFrosty, "Increase damage (17 - 30%) to enemies affected by Chill or Freeze"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_CastRate, "Decrease cast time (12 - 25%)")
            }
        },
        {
            "iceblock",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_InflictChillOnAttackers, "Apply Chill to the attacker if hit by physical damage"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_FrostWeapon, "Next primary attack for 6s after the spell ends deals damage (27 - 40%) and inflicts Chill"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Nova, "Conjure a caster-centred AoE that applies Chill or consumes Chill to apply Freeze (2.3 - 4s) once the spell ends"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_BonusAbsorb, "Increase shield (29 - 50%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceBlock_BonusHealing, "Increase healing (14 - 25%)")
            }
        },
        {
            "icenova",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceNova_RecastLesserNova, "Recast to conjure an AoE that explodes to deal damage (40 - 70%) and apply Chill"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze, "Explosion consumes Chill to apply Freeze (2.3 - 4s) (even with recast)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceNova_ApplyShield, "Explosion shields (36 - 70%) caster and allies"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium, "Increase range (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_IceNova_BonusDamageToFrosty, "Increase damage (27 - 40%) to enemies affected by Chill or Freeze"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease CD (7 - 12%)")
            }
        },
        {
            "veiloffrost",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack, "Next primary attack within 3s consumes Chill and applies Freeze (2.3 - 4s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Frost_ShieldOnFrosty, "Next primary attack within 3s on an enemy affected by Chill or Freeze shields caster (36 - 70%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfFrost_IllusionFrostBlast, "Illusion explodes in an AoE that deals damage (23 - 40%) and applies Chill"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration, "Increase Elude duration (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfFrost_BonusDamage, "Increase damage (12 - 20%) of next primary attack within 3s")
            }
        },
        {
            "misttrance",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_ReduceSecondaryWeaponCD, "Trigger reduces secondary weapon skill CD (46 - 80%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_PhantasmOnTrigger, "Trigger grants Phantasm (1 - 4)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_HasteOnTrigger, "Trigger increases MS (13 - 21%) for 3s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MIstTrance_DamageOnAttack, "Trigger increases first primary attack damage (29 - 50%) for 5s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_MistTrance_FearOnTrigger, "Trigger applies Fear to enemies (1.3 - 2s) in a caster-centred AoE"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium, "Trigger pushes enemies back (1.7 - 3m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield_Counter, "Teleport consumes Weaken to grant a shield (33 - 50%) (3 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High, "Increase MS (22 - 35%) during channel"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TravelBuff_IncreaseRange_Medium, "Increase distance travelled (10 - 20%)")
            }
        },
        {
            "mosquito",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_ShieldOnSpawn, "Cast shields (56 - 90%) caster and allies"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenReduceCooldown, "Explosion consumes Weaken to reduce CD (0.9 - 1.4s) per target (3 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_WispsOnDestroy, "Explosion summons 3 Wisps that heal the caster and allies (23 - 40%) when walked over"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_BonusDamage, "Increase damage (12 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_BonusFearDuration, "Increase Fear duration (0.4 - 0.7s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Mosquito_BonusHealthAndSpeed, "Increase summon max HP (35 - 60%) and MS (12 - 25%)")
            }
        },
        {
            "phantomaegis",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_ConsumeShieldAndPullAlly, "Recast to remove the effect and pull the target towards the caster"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs, "Cast removes all negative effects"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium, "Cast knocks enemies back (1.7 - 3m) from target"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_ExplodeOnDestroy, "Expiration conjurs a target-centred AoE that deals damage (29 - 50%) and inflicts Weaken"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_IncreaseLifetime, "Increase duration (18 - 35%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_MovementSpeed_Normal, "Increase MS (9 - 14%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_IncreaseSpellPower, "Increase spell power (14 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_PhantomAegis_ConsumeWeakenIntoFear, "Cast consumes Weaken to apply Fear (1.3 - 1.6s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenReduceCooldown, "Cast consumes Weaken to reduce CD (0.9 - 1.4s) (3 max)")
            }
        },
        {
            "spectralwolf",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_FirstBounceInflictFadingSnare, "First hit applies a fading Snare (1.1 - 1.8s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_ReturnToOwner, "Hit returns the projectile to the caster on last bounce to heal (63 - 80%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_AddBounces, "Increase max bounces by 1"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_DecreaseBounceDamageReduction, "Decrease damage penalty per bounce (9 - 15%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield, "Hit consumes Weaken to grant a shield (33 - 50%) per target (3 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_SpectralWolf_ConsumeWeakenApplyXPhantasm, "Hit consumes Weaken to grant Phantasm (3 - 6)")
            }
        },
        {
            "wraithspear",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WraithSpear_ShieldAlly, "Hit grants allies a shield (56 - 90%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium, "Hit applies a fading Snare (0.8 - 1.5s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WraithSpear_BonusDamage, "Increase damage (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium, "Increase projectile range (14 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WraithSpear_ReducedDamageReduction, "Decrease damage penalty per hit (6 - 10%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield, "Hit consumes Weaken to shield caster (33 - 50%) per target (3 max)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenSpawnWisp, "Hit consumes Weaken to summon a Wisp that heals the caster and allies (23 - 40%) when walked over")
            }
        },
        {
            "veilofillusion",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_RecastDetonate, "Recast to explode the Illusion to deal damage (22 - 35%) and apply Weaken"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_IllusionProjectileDamage, "Illusion projectiles deal damage (12 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_PhantasmOnHit, "Next primary attack within 3s grants Phantasm (2 - 4)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary, "Next primary attack within 3s deals damage (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfIllusion_AttackInflictFadingSnare, "Next primary attack within 3s applies a fading Snare (1.3 - 2s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Illusion_ConsumeWeakenShield_OnAttack, "Next primary attack within 3s consumes Weaken to grant caster a shield (33 - 50%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration, "Increase Elude duration (12 - 25%)")
            }
        },
        {
            "balllightning",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_DetonateOnRecast, "Recast detonates the ball to deal damage (29 - 50%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun_Explode, "Explosion consumes Static to apply Stun (0.4 - 0.7s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_KnockbackOnExplode, "Explosion pushes enemies back (1.7 - 3m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_Haste, "Explosion increases caster and ally MS (9 - 15%) for 4s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_BallLightning_BonusDamage, "Increase tick damage (3 - 6%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_IncreaseRange_Medium, "Increase projectile range (14 - 25%)")
            }
        },
        {
            "cyclone",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Cyclone_BonusDamage, "Increase damage (12 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Cyclone_IncreaseLifetime, "Increase projectile duration (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge, "Hit consumes Static for the next 3 primary attacks within 6s to deal damage (23 - 40%) and apply Static"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun, "Hit consumes Static to apply Stun (0.4 - 0.7s)")
            }
        },
        {
            "discharge",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Discharge_DoubleDash, "Trigger to travel a second time after first travel"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge, "Passing through an enemy consumes Static for the next 3 primary attacks within 6s to deal damage (23 - 40%) and apply Static"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Discharge_BonusDamage, "Increase damage (12 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_High, "Increase MS (22 - 35%) during channel"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Discharge_IncreaseAirDuration, "Increase Airborne duration (0.4 - 0.7s)")
            }
        },
        {
            "lightningcurtain",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_FadingSnare, "Hit applies a fading Snare (0.8 - 1.5s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_ApplyShield, "Hit on caster or ally grants a shield (56 - 90%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun, "Hit to consume Static and apply Stun (0.4 - 0.7s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_ConsumeProjectileWeaponCharge, "Block projectiles (3 max) for the next 3 primary attacks within 6s to deal damage (23 - 40%) and apply Static"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_BonusDamage, "Increase tick damage (5 - 12%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_LightningWall_IncreaseMovementSpeed, "Increase MS (12 - 25%)")
            }
        },
        {
            "polarityshift",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Storm_PolarityShift_AreaImpactOrigin, "Hit conjures an AoE at the caster's location that deals damage (29 - 50%) and applies Static"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoWeaponCharge, "Hit consumes Static for the next 3 primary attacks within 6s to deal damage (23 - 40%) and apply Static"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_ApplyFadingSnare_Medium, "Hit applies a fading Snare (0.8 - 1.5s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Storm_PolarityShift_AreaImpactDestination, "Teleport conjures an AoE at the target's location that deals damage (29 - 50%) and applies Static"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)")
            }
        },
        {
            "veilofstorm",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfStorm_SparklingIllusion, "Illusion conjures an AoE that deals tick damage (9 - 20%) and applies Static"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary, "Next primary attack within 3s deals damage (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Storm_ConsumeStaticIntoStun, "Next primary attack within 3s consumes Static to apply Stun (0.4 - 0.7s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfStorm_AttackInflictFadingSnare, "Next primary attack within 3s applies a fading Snare (1.3 - 2s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration, "Increase Elude duration (12 - 25%)")
            }
        },
        {
            "boneexplosion",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony, "Hit on an enemy affected by Condemn applies Agony"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_KillingBlow, "Hit on an enemy under 30% max HP deals damage (17 - 25%). Lethal attacks reduce CD (1.6 - 2.3s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_SkullNova, "Explosion conjures 8 projectiles that deal damage (23 - 40%) and apply Condemn"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_DoubleImpact, "Explosion conjures an AoE that deals damage (29 - 50%) and applies Condemn"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_HealMinions, "Explosion heals allied skeletons (55 - 80% summon max HP) and resets their uptime"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_SnareBonus, "Explosion applies a fading Snare (0.9 - 1.5s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorpseExplosion_BonusDamage, "Increase damage (12 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_TargetAoE_IncreaseRange_Medium, "Increase range (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Cooldown_Medium, "Decrease CD (7 - 12%)")
            }
        },
        {
            "corruptedskull",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_LesserProjectiles, "Launch 2 projectiles that deal damage (26 - 40%) and apply Condemn"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_DetonateSkeleton, "Hit on allied skeleton causes it to explode in an AoE that deals damage (45 - 70%) and applies Condemn"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony, "Hit on an enemy affected by Condemn applies Agony"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyBane, "Hit on an enemy affected by Condemn applies Bane"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_KnockbackOnHit_Medium, "Hit pushes enemies back (1.7 - 3m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_BoneSpirit, "Hit conjures a projectile that circles around the enemy that deals damage (9 - 15%) and applies Condemn"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_CorruptedSkull_BonusDamage, "Increase damage (10 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Projectile_RangeAndVelocity, "Increase projectile range and speed (10 - 24%)")
            }
        },
        {
            "deathknight",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_SnareEnemiesOnSummon, "Cast applies a fading Snare (1.1 - 1.7s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony, "Hit on an enemy affected by Condemn applies Agony"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyBane, "Hit on an enemy affected by Condemn applies Bane"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_SkeletonMageOnDeath, "Hit summons a Skeleton Mage with increased damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_BonusDamage, "Increase damage (17 - 30%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_DeathKnight_BonusDamageBelowTreshhold, "Increase damage (29 - 50%) to enemies below 30% max HP")
            }
        },
        {
            "soulburn",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_DispellDebuffs_Self, "Cast removes all negative effects from caster"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony, "Hit on an enemy affected by Condemn applies Agony"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_IncreaseTriggerCount, "Increase targets hit by 1"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_IncreasedSilenceDuration, "Increase Silence duration (0.5 - 0.8s)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_BonusDamage, "Increase damage (9 - 16%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_BonusLifeDrain, "Increase life drain (7 - 15%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_ReduceCooldownOnSilence, "Decrease CD (0.5 - 0.8) for each enemy affected by Silence"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_ConsumeSkeletonHeal, "Cast consumes skeletons (3 max) to heal (46 - 80%) per skeleton"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Soulburn_ConsumeSkeletonEmpower, "Cast consumes skeletons (3 max) to increase spell and physical power (6 - 12%) per skeleton for 8s")
            }
        },
        {
            "wardofthedamned",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_MightSpawnMageSkeleton, "Barrier hit has a chance (23 - 40%) to summon a Skeleton Mage"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_DamageMeleeAttackers, "Barrier hit (melee) deals damage (29 - 50%) to the attacker"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_HealOnAbsorbProjectile, "Barrier hit (projectile) heals (22 - 35%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_KnockbackOnRecast, "Recast pushes enemies back (1.7 - 2m)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_EmpowerSkeletonsOnRecast, "Recast increases allied skeleton damage (17 - 30%) for 8s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_ShieldSkeletonsOnRecast, "Recast shields (69 - 120%) allied skeletons for 4s"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_WardOfTheDamned_BonusDamageOnRecast, "Increase recast damage (12 - 20%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_IncreaseMoveSpeedDuringChannel_Low, "Increase MS (7 - 12%) during channel")
            }
        },
        {
            "veilofbones",
            new List<KeyValuePair<PrefabGUID, string>>
            {
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBones_DashInflictCondemn, "Dashing through an enemy applies Condemn"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBones_DashHealMinions, "Dashing through an allied skeletons heals them (55 - 80% summon max HP) and resets their uptime"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyAgony_OnAttack, "Next primary attack within 3s on an enemy affected by Condemn applies Agony"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Unholy_ApplyBane_OnAttack, "Next primary attack within 3s on an enemy affected by Condemn applies Bane"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BonusDamageOnPrimary, "Next primary attack within 3s deals damage (12 - 25%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_VeilOfBones_BonusDamageBelowTreshhold, "Next primary attack within 3s to enemies below 30% max HP deals damage (23 - 40%)"),
                new KeyValuePair<PrefabGUID, string>(Prefabs.SpellMod_Shared_Veil_BuffAndIllusionDuration, "Increase Elude duration (12 - 25%)")
            }
        }
    };
}

#if false // Decompilation log
'342' items in cache
------------------
Resolve: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Runtime.dll'
------------------
Resolve: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Collections.dll'
------------------
Resolve: 'Unity.Entities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Entities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Entities.dll'
------------------
Resolve: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\UnityEngine.CoreModule.dll'
------------------
Resolve: 'ProjectM, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.dll'
------------------
Resolve: '0Harmony, Version=2.10.1.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: '0Harmony, Version=2.10.1.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\harmonyx\2.10.1\lib\netstandard2.0\0Harmony.dll'
------------------
Resolve: 'BepInEx.Unity.IL2CPP, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'BepInEx.Unity.IL2CPP, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\bepinex.unity.il2cpp\6.0.0-be.668\lib\net6.0\BepInEx.Unity.IL2CPP.dll'
------------------
Resolve: 'Il2CppInterop.Runtime, Version=1.4.5.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Il2CppInterop.Runtime, Version=1.4.5.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Il2CppInterop.Runtime.dll'
------------------
Resolve: 'Stunlock.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Stunlock.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Stunlock.Core.dll'
------------------
Resolve: 'ProjectM.Shared, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Shared, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Shared.dll'
------------------
Resolve: 'ProjectM.Gameplay.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Gameplay.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Gameplay.Systems.dll'
------------------
Resolve: 'ProjectM.Misc.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Misc.Systems, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Misc.Systems.dll'
------------------
Resolve: 'Unity.Collections, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Collections, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Collections.dll'
------------------
Resolve: 'ProjectM.Roofs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Roofs, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Roofs.dll'
------------------
Resolve: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Mathematics.dll'
------------------
Resolve: 'Il2Cppmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Il2Cppmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Il2Cppmscorlib.dll'
------------------
Resolve: 'Unity.Transforms, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Transforms, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\Unity.Transforms.dll'
------------------
Resolve: 'BepInEx.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'BepInEx.Core, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\bepinex.core\6.0.0-be.668\lib\netstandard2.0\BepInEx.Core.dll'
------------------
Resolve: 'Bloodstone, Version=0.1.6.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Bloodstone, Version=0.1.6.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.bloodstone\0.1.6\lib\net6.0\Bloodstone.dll'
------------------
Resolve: 'com.stunlock.network, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'com.stunlock.network, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\com.stunlock.network.dll'
------------------
Resolve: 'ProjectM.Terrain, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Terrain, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Terrain.dll'
------------------
Resolve: 'ProjectM.Gameplay.Scripting, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.Gameplay.Scripting, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.Gameplay.Scripting.dll'
------------------
Resolve: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Text.Json, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Text.Json.dll'
------------------
Resolve: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ObjectModel, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.ObjectModel.dll'
------------------
Resolve: 'VampireCommandFramework, Version=0.8.2.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'VampireCommandFramework, Version=0.8.2.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.vampirecommandframework\0.8.2\lib\net6.0\VampireCommandFramework.dll'
------------------
Resolve: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Linq.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime.InteropServices, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'ProjectM.CodeGeneration, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'ProjectM.CodeGeneration, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\mitch\.nuget\packages\vrising.unhollowed.client\0.6.5.57575090\lib\net6.0\ProjectM.CodeGeneration.dll'
------------------
Resolve: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Text.RegularExpressions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Text.RegularExpressions.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Users\mitch\.nuget\packages\microsoft.netcore.app.ref\6.0.27\ref\net6.0\System.Runtime.CompilerServices.Unsafe.dll'
#endif