using Bloodstone.API;
using ProjectM.Network;
using Unity.Entities;

namespace VCreate.Core.Toolbox;


public static class NetworkedEntityUtil
{

    private static readonly NetworkIdSystem _NetworkIdSystem = VWorld.Server.GetExistingSystem<NetworkIdSystem>();

    public static bool TryFindEntity(NetworkId networkId, out Entity entity)
    {
        return _NetworkIdSystem._NetworkIdToEntityMap.TryGetValue(networkId, out entity);
    }

}
