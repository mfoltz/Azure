using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using Unity.Collections;
using Unity.Entities;
using V.Core.Commands;
using V.Core.Tools;

namespace AdminCommands.Patches;

[HarmonyPatch(typeof(ProjectileSystem_Spawn_Server), nameof(ProjectileSystem_Spawn_Server.OnUpdate))]
public static class ProjectileSystem_Spawn_ServerPatch
{
    public static void Prefix(ProjectileSystem_Spawn_Server __instance)
    {
        try
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                PrefabGUID GUID = entity.Read<PrefabGUID>();
                Entity Character = entity.Read<EntityOwner>().Owner;
                if (!Character.Has<PlayerCharacter>()) continue;

                if (GodCommands.PlayerProjectileSpeeds.ContainsKey(Character) && GodCommands.PlayerProjectileSpeeds[Character] != 1f)
                {
                    var projectile = entity.Read<Projectile>();
                    projectile.Speed *= GodCommands.PlayerProjectileSpeeds[Character];
                    entity.Write(projectile);
                }
                if (GodCommands.PlayerProjectileRanges.ContainsKey(Character) && GodCommands.PlayerProjectileRanges[Character] != 1f)
                {
                    var projectile = entity.Read<Projectile>();
                    projectile.Range *= GodCommands.PlayerProjectileRanges[Character];
                    entity.Write(projectile);
                }
                if (GodCommands.PlayerProjectileBounces.ContainsKey(Character) && GodCommands.PlayerProjectileBounces[Character] != -1)
                {
                    if (entity.Has<Script_BouncingProjectile_DataServer>())
                    {
                        Script_BouncingProjectile_DataServer bouncingProjectile = entity.Read<Script_BouncingProjectile_DataServer>();
                        bouncingProjectile.Settings.MaxBounces = GodCommands.PlayerProjectileBounces[Character];
                        entity.Write(bouncingProjectile);
                    }
                }
                if (GodCommands.isBuffEnabled(Character, "attackSpeed") && (GodCommands.isBuffEnabled(Character, "trollDamage") || GodCommands.isBuffEnabled(Character, "damage"))) //way to check if god or troll mode enabled
                {
                    var hitColliderCastBuffer = VWorld.Server.EntityManager.GetBuffer<HitColliderCast>(entity);
                    for (var i = 0; i < hitColliderCastBuffer.Length; i++)
                    {
                        var hitColliderCast = hitColliderCastBuffer[i];
                        hitColliderCast.PrimaryFilterFlags = ProjectM.Physics.CollisionFilterFlags.Unit;
                        hitColliderCastBuffer[i] = hitColliderCast;
                    }
                }
            }
        }
        catch
        {

        }
    }
}
