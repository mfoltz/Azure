using HarmonyLib;
using ProjectM;
using Unity.Collections;
using RPGMods.Utils;
using ProjectM.Shared.Systems;
using Unity.Entities;

/*
namespace RPGAddOnsEx.Hooks_TBD
{
    [HarmonyPatch(typeof(ServantMissionActionSystem), nameof(ServantMissionActionSystem.OnUpdate))]
    public class EquipServantItemSystem_Patch
    {
        public static void Prefix(ServantMissionActionSystem __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery != null)
            {
                var Servantcoffinentities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                int i = 0;
                foreach (var Servantcoffinentity in Servantcoffinentities)
                {
                    Cache.ServantcoffinCache[i++] = Servantcoffinentity;
                    var ServantCoffinstation = Plugin.Server.EntityManager.GetComponentData<ServantCoffinstation>(Servantcoffinentity);//里面的state可找到活着的仆人
                    if (ServantCoffinstation.State == ServantCoffinState.Converting)
                    {
                        ServantCoffinstation.State = ServantCoffinState.WakeUpReady;
                        Plugin.Server.EntityManager.SetComponentData(Servantcoffinentity, ServantCoffinstation);
                    }
                    var Servantentity = ServantCoffinstation.ConnectedServant._Entity;

                    var Follower = Plugin.Server.EntityManager.GetComponentData<Follower>(Servantentity);//跟随组件
                                                                                                         //var SBehaviourTreeState = Plugin.Server.EntityManager.GetComponentData<BehaviourTreeState>(Servantentity);//行为书组件

                    //var BoolModificationBuffer = Plugin.Server.EntityManager.GetComponentData<BoolModificationBuffer>(Servantentity);//状态输入
                    var fe = new Entity();
                    var Play = new Entity();

                    if (Cache.FServantcoffinCache.TryGetValue(0, out fe))
                    {
                        if (Cache.FServantcoffinCache.TryGetValue(1, out Play))
                        {
                            // var hp = Plugin.Server.EntityManager.GetComponentData<ProjectM.Health>(fe);//HP组件
                            var f = Plugin.Server.EntityManager.GetComponentData<Follower>(fe);//状态输入

                            //if (SBehaviourTreeState.Value == GenericEnemyState.Combat)
                            //{
                            //    var ServantLastTranslation = Plugin.Server.EntityManager.GetComponentData<LastTranslation>(Servantentity);//仆人最后位置
                            //    var PlayTranslation = Plugin.Server.EntityManager.GetComponentData<Translation>(Play);//仆人位置

                            //    ServantLastTranslation.Value = PlayTranslation.Value;
                            //    Plugin.Server.EntityManager.SetComponentData<LastTranslation>(Servantentity, ServantLastTranslation);

                            //}
                            //var FollowerBuffer = Plugin.Server.EntityManager.GetBuffer<FollowerBuffer>(fe);//状态输入
                            //speed.Variation = 0.001f;

                            Follower.Followed = f.Followed;
                            Follower.Followed.Value = Play;
                            Follower.Followed._Value = Play;
                            Follower.Followed._BaseValue = Play;

                            Follower.Offset = 0;
                            Follower.LastOffsetUpdateTime = 1.7976931348623157E+308;
                            Follower.ModeModifiable.Value = 1;
                            Follower.ModeModifiable._Value = 1;
                            Follower.ModeModifiable._BaseValue = 1;

                            //Follower.Stationary.Value = true;
                            //Follower.Stationary._Value = true;
                            //Follower.Stationary._BaseValue = true;
                            //Follower.InheritRotationWhenStationary = f.InheritRotationWhenStationary;
                            Plugin.Server.EntityManager.SetComponentData<Follower>(Servantentity, Follower);

                            //Cache.AiMoveCache[0] = "HP" + hp.Value + "LastOffsetUpdateTime:" + f.LastOffsetUpdateTime + "Offset:" + f.Offset;
                        }
                    }

                    FixedString64 Servantentityname = ServantCoffinstation.ServantName;
                    Cache.ServanCache[Servantentityname] = Servantentity;
                    //var BehaviourTreeStateData = Plugin.Server.EntityManager.GetComponentData<BehaviourTreeState>(Servantentity);//状态输入
                    //BehaviourTreeStateData.Value = GenericEnemyState.Follow;

                    //Plugin.Server.EntityManager.SetComponentData<BehaviourTreeState>(Servantentity, BehaviourTreeStateData);
                }
            }

            // var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
        }
    }
}
*/