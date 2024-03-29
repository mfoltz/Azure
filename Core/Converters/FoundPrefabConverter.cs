﻿using ProjectM;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Converters
{
    public record struct FoundPrefabGuid(PrefabGUID Value);
    public class FoundPrefabConverter : CommandArgumentConverter<FoundPrefabGuid>
    {
        public override FoundPrefabGuid Parse(ICommandContext ctx, string input)
        {
            PrefabGUID prefabGUID;
            if (Helper.TryGetPrefabGUIDFromString(input, out prefabGUID))
                return new FoundPrefabGuid(prefabGUID);
            throw ctx.Error("Could not find matching PrefabGUID: " + input);
        }
    }
}
