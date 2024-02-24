using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.Core.Tools;
using V.Data;
using VampireCommandFramework;

namespace V.Core.Converters
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
