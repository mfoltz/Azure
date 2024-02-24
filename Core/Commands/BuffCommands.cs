using ProjectM;
using V.Core.Services;
using V.Core.Tools;
using V.Data;
using VampireCommandFramework;
using Buff = ProjectM.Buff;

namespace V.Commands
{
    [CommandGroup(name: "V+(Rising)", shortHand: "v")]
    internal class BuffCommands
    {
        [Command("buff", description: "Buff a player with a prefab name or guid", adminOnly: true)]
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

        [Command("unbuff", description: "Removes a buff", adminOnly: true)]
        public void UnbuffCommand(ChatCommandContext ctx, FoundPrefabGuid buffGuid, FoundPlayer player = null)
        {
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            Helper.UnbuffCharacter(Character, buffGuid.Value);
            ctx.Reply("Removed buff");
        }

        [Command("listbuffs", description: "Lists the buffs a player has", adminOnly: true)]
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

        [Command("clearbuffs", description: "Removes any extra buffs on a player", adminOnly: true)]
        public void ClearBuffs(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            Helper.ClearExtraBuffs(Character);
        }
    }
}


