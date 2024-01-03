using ProjectM.UI;
using HarmonyLib;
using UnityEngine.Events;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

/*
namespace DropTeleportBound
{
    [HarmonyPatch]
    public class InventorySubMenu_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventorySubMenu), nameof(InventorySubMenu.StartRunning))]
        private static void StartRunning(InventorySubMenu __instance)
        {
            // dtb = Drop Teleport Bound
            // GO = GameObject
            string dtbButtonName = "DropTeleportBound";

            // Find the smart merge button (compulsively count)
            SimpleStunButton smartMergeButton = null;
            foreach (var stunButton in __instance.GetComponentsInChildren<SimpleStunButton>(true).Where(stunButton => stunButton.name == "SmartMergeButton"))
            {
                smartMergeButton = stunButton;
            }

            if (smartMergeButton == null)
            {
                return;
            }

            // Determine if button already exists
            LayoutGroup dtbButtonParent = __instance.BagsParent;
            var dtbButtonGO = dtbButtonParent.transform.Find(dtbButtonName)?.gameObject;
            if (dtbButtonGO == null)
            {
                // Copy the smart merge button GameObject to use as a template
                dtbButtonGO = GameObject.Instantiate(smartMergeButton.gameObject);

                // Set up button and some attributes
                SimpleStunButton dtbStunButton = dtbButtonGO.GetComponent<SimpleStunButton>();
                dtbButtonGO.name = dtbButtonName;
                dtbStunButton.name = "DropTeleportBound";
                dtbButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = "Drop Teleport\nBound";
                dtbStunButton.onClick.AddListener((UnityAction)Clicked_DropTeleportBoundItems);

                // Put the button on the parent
                dtbButtonGO.transform.SetParent(dtbButtonParent.transform);

                // Set the correct position on the parent
                RectTransform dtbRectTransform = dtbButtonGO.GetComponent<RectTransform>();
                dtbRectTransform.SetAsLastSibling();
                dtbRectTransform.SetPivot(PivotPresets.MiddleLeft);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventorySubMenu), nameof(InventorySubMenu.OnUpdateFromParent))]
        private static void OnUpdateFromParent(InventorySubMenu __instance)
        {
            // Find the DropTeleportBound button if it exists
            SimpleStunButton dtbButton = null;
            foreach (var stunButton in __instance.GetComponentsInChildren<SimpleStunButton>(true).Where(stunButton => stunButton.name == "DropTeleportBound"))
            {
                dtbButton = stunButton;
            }

            if (dtbButton != null)
            {
                // Change button text if right mouse button held down
                if (Input.GetMouseButton(1))
                {
                    dtbButton.GetComponentInChildren<TextMeshProUGUI>().text = "Drop Other\nItems";
                }
                else
                {
                    dtbButton.GetComponentInChildren<TextMeshProUGUI>().text = "Drop Teleport\nBound";
                }
            }
        }

        private static void Clicked_DropTeleportBoundItems()
        {
            // If right mouse button is held down, drop teleportable items
            // Otherwise, drop teleport bound items
            if (Input.GetMouseButton(1))
            {
                DropTeleportBoundClient.TryDropTeleportBoundItems(true);
            }
            else
            {
                DropTeleportBoundClient.TryDropTeleportBoundItems(false);
            }
        }
    }
}
*/