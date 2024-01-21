/*
namespace RPGMods.Hooks
{
    public class AiMove_Patch
    {
        [HarmonyPatch(typeof(AiMoveSystem_Server), "OnUpdate")]
        [HarmonyPostfix]
        public static void Postfix(AiMoveSystem_Server __instance)
        {
            if (__instance._AiMoveQuery != null)
            {
                //Cache.AiMoveCache[0] = "找到了AiMoveSystem_Server...";

                var aientities = __instance._AiMoveQuery.ToEntityArray(Allocator.Temp);

                foreach (var ev in aientities)
                {
                    var servant = Plugin.Server.EntityManager.HasComponent<ServantConnectedCoffin>(ev);

                    if (servant)
                    {
                        //仆人
                        var hp = Plugin.Server.EntityManager.GetComponentData<ProjectM.Health>(ev);//HP组件
                                                                                                   //
                                                                                                   //
                                                                                                   //var Servantdatatgaeposition = Plugin.Server.EntityManager.GetComponentData<Translation>(ev);//仆人位置

                        var ServantConnectedCoffin = Plugin.Server.EntityManager.GetComponentData<ServantConnectedCoffin>(ev); //查找棺材实体

                        //var ServantCoffinstation = Plugin.Server.EntityManager.GetComponentData<ServantCoffinstation>(ServantConnectedCoffin.CoffinEntity._Entity); //仆人连接棺材

                        var CastleHeartConnection = Plugin.Server.EntityManager.GetComponentData<CastleHeartConnection>(ServantConnectedCoffin.CoffinEntity._Entity);//获得城堡之心实体

                        var UserOwner = Plugin.Server.EntityManager.GetComponentData<UserOwner>(CastleHeartConnection.CastleHeartEntity._Entity);//获得城堡之心创建者实体

                        //var UserOwnerposition = Plugin.Server.EntityManager.GetComponentData<Translation>(UserOwner.Owner._Entity);//获得城堡之心创建者实体位置

                        Cache.FServantcoffinCache[1] = UserOwner.Owner._Entity;
                    }

                    var BehaviourTreeStateData2 = Plugin.Server.EntityManager.GetComponentData<BehaviourTreeState>(ev);//状态输入
                    var CharmSource = Plugin.Server.EntityManager.HasComponent<CharmSource>(ev);//状态输入
                    var Follower = Plugin.Server.EntityManager.GetComponentData<Follower>(ev);//状态输入
                    var Play = new Entity();
                    var Aie = new Entity();
                    if (Cache.FServantcoffinCache.TryGetValue(0, out Aie))
                    {
                    }
                    else
                    {
                        if (Cache.FServantcoffinCache.TryGetValue(1, out Play))
                        {
                            //if (Follower.Followed.prop_Entity_1 == Play) {
                            if (BehaviourTreeStateData2.Value == GenericEnemyState.Follow)
                            {
                                if (CharmSource)
                                {
                                    Cache.FollowerCache[0] = Follower;
                                    Cache.FServantcoffinCache[0] = ev;
                                    //var CharmSourceComponent = Plugin.Server.EntityManager.GetBuffer<FollowerBuffer>(ev);//状态输入

                                    //ModifiableEntity_set = Follower.Followed;
                                    //Offset_set = Follower.Offset;
                                    //LastOffsetUpdateTime_set = Follower.LastOffsetUpdateTime;
                                    //ModeModifiable_set = Follower.ModeModifiable;
                                    //Stationary_set = Follower.Stationary;
                                    //InheritRotationWhenStationary_set = Follower.InheritRotationWhenStationary;

                                    //---------------------------------------------------------------------------------
                                    var hp = Plugin.Server.EntityManager.GetComponentData<ProjectM.Health>(ev);//HP组件
                                    hp.Value = 1;
                                    Plugin.Server.EntityManager.SetComponentData<ProjectM.Health>(ev, hp);

                                    //BehaviourTreeStateData2.Value = GenericEnemyState.Idle;

                                    //Plugin.Server.EntityManager.SetComponentData<BehaviourTreeState>(ev, BehaviourTreeStateData2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ServantMissionActionSystem), nameof(ServantMissionActionSystem.OnUpdate))]
    public class EquipServantItemSystem_Patch
    {
        public static void Postfix(ServantMissionActionSystem __instance)
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

                    var Follower = Plugin.Server.EntityManager.GetComponentData<Follower>(Servantentity);//状态输入
                    //var BoolModificationBuffer = Plugin.Server.EntityManager.GetComponentData<BoolModificationBuffer>(Servantentity);//状态输入
                    var f = new Follower();

                    if (Cache.FollowerCache.TryGetValue(0, out f))
                    {
                        Follower.Followed = f.Followed;
                        Follower.Offset = f.Offset;
                        Follower.LastOffsetUpdateTime = f.LastOffsetUpdateTime;
                        Follower.ModeModifiable = f.ModeModifiable;
                        Follower.Stationary = f.Stationary;
                        Follower.InheritRotationWhenStationary = f.InheritRotationWhenStationary;

                        Plugin.Server.EntityManager.SetComponentData<ProjectM.Follower>(Servantentity, Follower);

                        //Cache.AiMoveCache[0] = "01";
                    }

                    //FixedString64 Servantentityname = ServantCoffinstation.ServantName;
                    //Cache.ServanCache[Servantentityname] = Servantentity;
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