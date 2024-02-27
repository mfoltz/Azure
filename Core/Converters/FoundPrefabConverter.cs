using ProjectM;

using VampireCommandFramework;
using WorldBuild.Core.Toolbox;
using WorldBuild.Data;

namespace WorldBuild.Core.Converters
{
    internal class FoundPrefabConverter : CommandArgumentConverter<FoundPrefabGuid>
    {
        public override FoundPrefabGuid Parse(ICommandContext ctx, string input)
        {
            PrefabGUID prefabGUID;
            if (Helper.TryGetPrefabGUIDFromString(input, out prefabGUID))
                return new FoundPrefabGuid(prefabGUID);
            throw ctx.Error("Could not find buff: " + input);
        }
    }
}