using HarmonyLib;
using UnityEngine;

namespace Reapy
{
    // This code was found from [NightingGale]'s [PipedPressureValve] mod!
    [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
    class PatchOnSpawn
    {
        /// <summary>
        /// This will change the color of the building when they are completed!
        /// The color change for [reapy] is locationed in [OnSpawn] of the [ReapBotConfig]
        /// </summary>
        /// <param name="__instance"> The instance which triggered [BuildingComplete]! </param>
        public static void Postfix(BuildingComplete __instance)
        {
            if (string.Compare(__instance.name, "ReapBotStationComplete") == 0)
                __instance.GetComponent<KAnimControllerBase>().TintColour = new Color32(50, 250, 50, byte.MaxValue);
        }
    }
}
