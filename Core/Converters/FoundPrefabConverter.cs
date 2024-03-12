using ProjectM;
using VampireCommandFramework;
using VBuild.Core.Toolbox;

namespace VBuild.Core.Converters
{
    public record struct FoundPrefabGuid(PrefabGUID Value);
    public class FoundPrefabConverter : CommandArgumentConverter<FoundPrefabGuid>
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
