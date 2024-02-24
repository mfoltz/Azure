using ProjectM;
using V.Core.Tools;
using VampireCommandFramework;

namespace AdminCommands.Commands.Converters;
public record struct FoundPrefabGuid(PrefabGUID Value);
internal class BuffConverter : CommandArgumentConverter<FoundPrefabGuid>
{
	public override FoundPrefabGuid Parse(ICommandContext ctx, string input)
	{
		if (Helper.TryGetPrefabGUIDFromString(input, out PrefabGUID prefab))
		{
			return new FoundPrefabGuid(prefab);
		}

		throw ctx.Error($"Could not find buff: {input}");
	}
}
