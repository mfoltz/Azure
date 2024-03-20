using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using System.Runtime.CompilerServices;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Converters;
using VCreate.Core.Toolbox;
using static VCreate.Core.Services.PlayerService;
using VCreate.Data;
using UnityEngine;
using VCreate.Systems;
using VRising.GameData.Models;

namespace VCreate.Core.Commands
{
    

    internal class PetCommands
    {
        [Command(name: "summonfamiliar", shortHand: "summon", adminOnly: false, usage: ".summon", description: "?")]
        public static void MethodOne(ChatCommandContext ctx)
        {
            OnHover.SpawnCopy(ctx.Event.SenderUserEntity);
        }
        
        
    }
}