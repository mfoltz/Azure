using ProjectM;
using VPlus.Core.Toolbox;
using VPlus.Data;
using VampireCommandFramework;

namespace VPlus.Core.Converters
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
