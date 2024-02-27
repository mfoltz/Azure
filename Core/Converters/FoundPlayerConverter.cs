using VampireCommandFramework;
using WorldBuild.Core.Services;

namespace WorldBuild.Core.Converters
{
    internal class FoundPlayerConverter : CommandArgumentConverter<FoundPlayer>
    {
        public override FoundPlayer Parse(ICommandContext ctx, string input)
        {
            return new FoundPlayer(FoundPlayerConverter.HandleFindPlayerData(ctx, input, false));
        }

        public static PlayerService.Player HandleFindPlayerData(
          ICommandContext ctx,
          string input,
          bool requireOnline)
        {
            PlayerService.Player player;
            if (PlayerService.TryGetPlayerFromString(input, out player) && (!requireOnline || player.IsOnline))
                return player;
            throw ctx.Error("Player " + input + " not found.");
        }
    }
}