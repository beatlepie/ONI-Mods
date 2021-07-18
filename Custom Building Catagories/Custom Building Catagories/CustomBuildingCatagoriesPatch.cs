using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using TUNING;
using UnityEngine.Events;

namespace Custom_Building_Categories
{
    // EVERYTHING for Harmony must be public and static, as harmony patch needs to reference and access the files properly

    // This is loaded when the mod is loaded at the start of the game
    public static class Start_PLib
    {
        public static void OnLoad()
        {
            // This is no longer necessary as [PLib] will automatically instanciate when called upon!
/*            PeterHan.PLib.PUtil.InitLibrary();
            PeterHan.PLib.UI.InitLibrary();
*/
            SaveFileIO.Read();
            // These must be added in case any swaps change their original names
            GameBuildingMenuData.ADD_STRINGS();
        }
    }

    [HarmonyPatch(typeof(OptionsMenuScreen), "OnPrefabInit")]
    public static class Custom_Building_Catagories_Patch
    {
        /// <summary>
        /// Apply this patch after the original game code is run
        /// Adds one button at the top of the list of buttons in the [OptionMenuScreen]
        /// </summary>
        /// <param name="___buttons">Gets the [buttons] variable from [OptionMenuScreen] to alter it. ___ allows alteration.</param>
        public static void Postfix(IList<KButtonMenu.ButtonInfo> ___buttons)
        {
            // This method can only send methods without any input parameters
            ___buttons.Insert(0, new KButtonMenu.ButtonInfo("Customize Building Menu", Action.NumActions, new UnityAction(CustomizeBuildingMenuScreen.OnClicked)));
            // The bottom method is used to pass a method with input parameters!
            // ___buttons.Insert(0, new KButtonMenu.ButtonInfo("Customize Building Menu", Action.NumActions, () => { CustomizeBuildingMenuScreen.CreateScreen(optionsMenuScreen); }));
        }
    }

    [HarmonyPatch(typeof(PlanScreen), "OnPrefabInit")]
    public static class Replace_iconNameMap_Patch
    {
        /// <summary>
        /// Replace the [iconNameMap] from [PlanScreen].
        /// This had to be done because [iconNameMap] is a [private] dictionary, which can only be accessed this way
        /// </summary>
        /// <param name="___iconNameMap">The [iconNameMap] in [PlanScreen]</param>
        public static void Postfix(ref Dictionary<HashedString, string> ___iconNameMap)
        {
            ___iconNameMap = GameBuildingMenuData.iconNameMap;
            CustomizeBuildingMenuScreen.CheckBuildings();
            BUILDINGS.PLANORDER = new List<PlanScreen.PlanInfo>(CustomizeBuildingMenuScreen.categoryMenu);
        }
    }
}