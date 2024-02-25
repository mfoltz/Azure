using Bloodstone.API;
using ProjectM;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using V.Core.Tools;
using VampireCommandFramework;

namespace V.Core.Commands
{
    internal class GiveItemCommands
    {
        [Command(name: "give", shortHand: "g", adminOnly: true, usage: ".g <ItemName> <Quantity>", description: "Gives the specified item.")]
        public static void GiveItem(
          ChatCommandContext ctx,
          GivenItem item,
          int quantity = 1)
        {
            if (!Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, item.Value, quantity, out Entity _))
                return;
            string str;
            VWorld.Server.GetExistingSystem<PrefabCollectionSystem>().PrefabGuidToNameDictionary.TryGetValue(item.Value, out str);
            ChatCommandContext chatCommandContext = ctx;
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 2);
            interpolatedStringHandler.AppendLiteral("Gave ");
            interpolatedStringHandler.AppendFormatted(quantity);
            interpolatedStringHandler.AppendLiteral(" ");
            interpolatedStringHandler.AppendFormatted(str);
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            chatCommandContext.Reply(stringAndClear);
        }

        public record struct GivenItem(PrefabGUID Value);

        internal class GiveItemConverter : CommandArgumentConverter<GivenItem>
        {
            public override GivenItem Parse(ICommandContext ctx, string input)
            {
                PrefabGUID prefabGUID;
                if (Helper.TryGetItemPrefabGUIDFromString(input, out prefabGUID))
                    return new GivenItem(prefabGUID);
                throw ctx.Error("Could not find item: " + input);
            }


        }
    }
}