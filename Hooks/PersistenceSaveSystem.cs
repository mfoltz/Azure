﻿using HarmonyLib;
using ProjectM;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.Augments;
using VPlus.Core;
using VPlus.Core.Commands;
using VPlus.Data;

namespace VPlus.Hooks
{
  
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), "TriggerSave")]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Postfix() => Tokens.UpdateTokens();
    }

    public class Tokens
    {
        public static void UpdateTokens()
        {
            Plugin.Logger.LogInfo("Updating tokens");
            var playerDivinities = Databases.playerDivinity;
            foreach (var entry in playerDivinities)
            {
                ulong steamId = entry.Key;
                DivineData currentPlayerDivineData = entry.Value;

                // Safely execute the intended actions outside of the main game loop to avoid conflicts.
                // Consider adding locks or other concurrency control mechanisms if needed.
                currentPlayerDivineData.OnUserDisconnected(); // Simulate user disconnection
                currentPlayerDivineData.OnUserConnected();    // Simulate user reconnection
                ChatCommands.SavePlayerDivinity();            // Save changes if necessary
                Plugin.Logger.LogInfo($"Updated token data for player {steamId}");
            }
        }
    }
    
}
