using StunShared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StunShared.UI;

using HarmonyLib;
using ProjectM;
using ProjectM.UI;
using UnityEngine;
using FMOD.Studio;

namespace RPGAddOns.Core
{
    [HarmonyPatch(typeof(StunGUIBehaviour))]
    public static class StunGUIBehaviourPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("set__CachedGameObject_k__BackingField")]
        public static void Postfix(StunGUIBehaviour __instance, GameObject __0)
        {
            try
            {
                // Your existing logging code...

                // Retrieve and activate the InventoryBackground object
                GameObject inventoryBackground = FindInventoryBackground(__instance);
                Plugin.Logger.LogInfo("Object found, proceeding");

                if (inventoryBackground != null)
                {
                    inventoryBackground.SetActive(true);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogInfo(ex);
                // Your existing exception handling code...
            }
        }

        private static GameObject FindInventoryBackground(StunGUIBehaviour instance)
        {
            // Example: Assuming InventoryBackground is a direct member of StunGUIBehaviour
            // return instance.InventoryBackground;

            // OR, if you need to find it as a child of the CachedGameObject:
            // return instance.CachedGameObject.transform.Find("InventoryBackground").gameObject;

            // Adjust the method based on how InventoryBackground is actually referenced
            return instance.CachedGameObject.transform.Find("InventoryBackground").gameObject;
        }
    }
}