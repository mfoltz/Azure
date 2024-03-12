using ProjectM;
using VampireCommandFramework;
using VCreate.Core.Converters;
using VCreate.Core.Toolbox;
using Buff = ProjectM.Buff;

namespace VCreate.Core.Commands
{

    internal class BuffCommands
    {

        [Command(name: "buff", shortHand: "b", adminOnly: true, usage: ".v b <PrefabGUID> <Player> <Duration> <Persists>", description: "Buff a player with a prefab name or guid.")]
        public void BuffCommand(ChatCommandContext ctx, FoundPrefabGuid buffGuid, FoundPlayer player = null, int duration = Helper.DEFAULT_DURATION, bool persistsThroughDeath = false)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            try
            {
                Helper.BuffPlayer(Character, User, buffGuid.Value, duration, persistsThroughDeath);
                ctx.Reply("Added buff");
            }
            catch (Exception e)
            {
                throw ctx.Error(e.ToString());
            }
        }
        [Command(name: "unbuff", shortHand: "ub", adminOnly: true, usage: ".v ub <PrefabGUID> <Player>", description: "Unbuff a player with a prefab name or guid.")]
        public void UnbuffCommand(ChatCommandContext ctx, FoundPrefabGuid buffGuid, FoundPlayer player = null)
        {
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            Helper.UnbuffCharacter(Character, buffGuid.Value);
            ctx.Reply("Removed buff");
        }
        [Command(name: "listbuffs", shortHand: "lb", adminOnly: true, usage: ".v lb <Player>", description: "Lists the buffs a player has.")]
        public void ListBuffsCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            var buffEntities = Helper.GetEntitiesByComponentTypes<Buff, PrefabGUID>();
            foreach (var buffEntity in buffEntities)
            {
                if (buffEntity.Read<EntityOwner>().Owner == Character)
                {
                    ctx.Reply(buffEntity.Read<PrefabGUID>().LookupName());
                }
            }
        }
        [Command(name: "clearbuffs", shortHand: "cb", adminOnly: true, usage: ".v cb <Player>", description: "Attempts to remove any extra buffs on a player.")]
        public void ClearBuffs(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            Helper.ClearExtraBuffs(Character);
        }
    }
}


