using Unity.Entities;
using VampireCommandFramework;
using VBuild.Core.Converters;
using VBuild.Core.Toolbox;

namespace VCreate.Core.Commands;

internal class ReviveCommands
{
    public static Dictionary<Entity, bool> AutoReviveDictionary = new Dictionary<Entity, bool>();
    public static Dictionary<Entity, Entity> AutoReviveCharactersToUsers = new Dictionary<Entity, Entity>();

    [Command("revive", adminOnly: true)]
    public void ReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
    {
        var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
        var User = player?.Value.User ?? ctx.Event.SenderUserEntity;

        Helper.ReviveCharacter(Character, User);

        ctx.Reply("Revived");
    }

    [Command("autorevive", adminOnly: true)]
    public void AutoReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
    {
        var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
        var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

        if (AutoReviveDictionary.ContainsKey(Character))
        {
            AutoReviveDictionary[Character] = !AutoReviveDictionary[Character];
            if (AutoReviveDictionary[Character])
            {
                Helper.ReviveCharacter(Character, User);
                ctx.Reply($"Enabled autorevive for {player?.Value.Name ?? "you"}.");
            }
            else
            {
                ctx.Reply($"Disabled autorevive for {player?.Value.Name ?? "you"}.");
            }
        }
        else
        {
            Helper.ReviveCharacter(Character, User);
            AutoReviveDictionary[Character] = true;
            AutoReviveCharactersToUsers[Character] = User;
            ctx.Reply($"Enabled autorevive for {player?.Value.Name ?? "you"}.");
        }
    }
}
