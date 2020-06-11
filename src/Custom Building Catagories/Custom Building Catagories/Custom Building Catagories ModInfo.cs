// This mod was made referencing [EnableAllModsButton] mod made by Cairath: https://github.com/Cairath/ONI-Mods/tree/master/src/EnableAllModsButton
// This mod was also referenced to make the mod screen: https://github.com/Cairath/ONI-Mods/blob/master/src/AchievementProgress/AchievementProgressPatches.cs
// This library mod was used to make the screens, made by Peter Han: https://github.com/peterhaneve/ONIMods/tree/master/PLib

namespace Custom_Building_Categories
{
    static class Custom_Building_Catagories_ModInfo
    {
        public static string Name = "Custom Building Catagories";
        public static int Version = 3;
        public static string Date = "2020/05/26";

        // This mod was requested by [charkko#2413] in discord server [Oxygen Not Included].

        // Version 1 (2020/05/08):
        // Originally, thought of adding a button to the [entryParent] in [ModScreen], however due to the way mod display screen is coded, I could not find an easy way to add a "customize" button
        // Found that the [OptionsMenuScreen] is much easier to modify and added the button there. (In game, press escape, then options to find the menu!)
        // Below error has been encountered...
        /*
            NullReferenceException
          at (wrapper managed-to-native) UnityEngine.Component.get_gameObject(UnityEngine.Component)
          at UnityEngine.Component.GetComponentInParent (System.Type t) [0x00001] in <a35d771e78bd4d75a6f3aedeaad4d1ed>:0 
          at UnityEngine.Component.GetComponentInParent[T] () [0x00001] in <a35d771e78bd4d75a6f3aedeaad4d1ed>:0 
          at KScreen.OnSpawn () [0x00006] in <e3c6a70dbf804766b5f7ed03fef8497e>:0 
          at Custom_Building_Catagories.CustomizeBuildingMenu.OnSpawn () [0x0000c] in <3633bb8052064997bc80d361a172fa46>:0 
          at Custom_Building_Catagories.CustomizeBuildingMenu.OpenCuztomizeBuildingMenu () [0x00001] in <3633bb8052064997bc80d361a172fa46>:0 
          at KButtonMenu+<>c__DisplayClass11_1.<RefreshButtons>b__1 () [0x00000] in <e38a27e719b642cba3ce0b7a77b47055>:0 
          at KButton.SignalClick (KKeyCode btn) [0x00010] in <e3c6a70dbf804766b5f7ed03fef8497e>:0 
          at KButton.OnPointerClick (UnityEngine.EventSystems.PointerEventData eventData) [0x00069] in <e3c6a70dbf804766b5f7ed03fef8497e>:0 
          at UnityEngine.EventSystems.ExecuteEvents.Execute (UnityEngine.EventSystems.IPointerClickHandler handler, UnityEngine.EventSystems.BaseEventData eventData) [0x00008] in <cfe2f64a3dc9415eb325f322bb4ecd6a>:0 
          at UnityEngine.EventSystems.ExecuteEvents.Execute[T] (UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.ExecuteEvents+EventFunction`1[T1] functor) [0x00070] in <cfe2f64a3dc9415eb325f322bb4ecd6a>:0 
        UnityEngine.DebugLogHandler:Internal_LogException(Exception, Object)
        UnityEngine.DebugLogHandler:LogException(Exception, Object)
        UnityEngine.Logger:LogException(Exception, Object)
        UnityEngine.Debug:LogException(Exception)
        UnityEngine.EventSystems.ExecuteEvents:Execute(GameObject, BaseEventData, EventFunction`1)
        UnityEngine.EventSystems.StandaloneInputModule:ProcessMousePress(MouseButtonEventData)
        UnityEngine.EventSystems.StandaloneInputModule:ProcessMouseEvent(Int32)
        UnityEngine.EventSystems.StandaloneInputModule:ProcessMouseEvent()
        UnityEngine.EventSystems.StandaloneInputModule:Process()
        UnityEngine.EventSystems.EventSystem:Update()
        */
        // ********************************************************************************************************************************************************************************
        // caused by attempting to use [gameObject] when it does not exist
        // This was because the game uses [Prefab]s which are predefined templates of [class]es
        // As my [CustomizeBuildingMenuScreen] is just a [class] and no [Prefab] exists, predefined [gameObject] does not exist either
        // According to the modding discord server, I could [Instantiate] (clone) the [Prefab] and change that as [Prefab]s are templates used elsewhere and must not be altered
        // Or I could technically make a new screen and just instanciate that one...?
        // The [Prefabs] are stored in a [class] called [Assets], where they are loaded and kept in a [dictionary]
        // ********************************************************************************************************************************************************************************

        // Version 2 (2020/05/__):
        // Seems like replicating the game settings are too difficult, could not extracting game presets 
        // HOWEVER, Peter Han made the [PLib] library which allows modders to easily make screens and add mod menus
        // The mod remade buttons and klei looking windows 
        // ********************************************************************************************************************************************************************************
        // The PLib can be added to the libraries by Tools ==> NuGet package control ==> Package Console ==> copy and paste packet manager command
        // This may not work with cmd if the project is set to allow "external interference"
        // The [PLib.dll] must be included in the mod files in order to launch properly!
        // PLib also requires [PeterHan.PLib.PUtil.InitLibrary()] in [OnLoad] method of patching to use as well

        // The [CustomizeBuildingMenuScreen] is made using PLib, and all the catagory names had to be matched using a [Dictionary<HashedString, String>] as it could not be de-hashed...
        // [HashCache] class is just a database that added values from [BuildMenu], not from [TUNING.BUILDINGS], cannot reverse the hash...
        // [PPanel] is a container; [PDialog] is the actual screen class!******************************************************************************************************************

        // Version 3 (2020/05/26):
        // Figured out how buttons and delegates work. Delegates must have the same parameters and return as the predefined delegate linked to the button
        // Made a screen in options that function as it should, but have not found a way to change in game building menu yet

        // POTENTIAL IMPROVEMENT: I could make a struct containing button, [PlanScreen.PlanInfo] to make the location of the button and plan identical without switching both in confusing ways
        // but I am like 1000 lines in and I don't want to change it T_T

        // Version 4 (2020/06/10):
        // Instead of writing everything in string and changing it back, found that making a [json] file using serialize and deserializing them is much easier (Thanks to all the people in Oxygen Not Included Discord server!)
        // When reading the save file, [iconNameMap.Add(HashCache.Get().Add(categoryNames[plan.category.ToString()]), "icon_category_base")] is necessary because [HashCache] is used to call the correct naming for all the values
        // that is why I could not get the correct values from the [HashCache] yet as the [iconNameMap] has not been initialized yet...

        // I also had to make a [struct] for [PlanScreen.PlanInfo] because the [data] inside of it is just [Object] instead of [List<string>]
        // The above causes the [Read()] to read the [data] as if it is just an [object] and cannot be called as a [List<string>]
        // SOLUTION: Made a new [struct] in [GameBuildingMenuData] which makes a list in the [SaveFileIO.Save()] and puts it inside another [struct] which makes it really easy to read afterwards

        // ANOTHER POTENTIAL IMPROVEMENT: after looking through NightingGale's [AddBuilding], [categoryIndex = BUILDINGS.PLANORDER.FindIndex((PlanScreen.PlanInfo x) => x.category == category)]
        // can be used to find the index of the category...
    }
}


// This code was originally used to attempt to add the customize building menu button into the mod screen
/*
using Harmony;
using UnityEngine;

namespace Custom_Building_Catagories
{
// [typeof] gets the type of the [class] while the code is running, [method] of that class to patch
[HarmonyPatch(typeof(ModsScreen))]
[HarmonyPatch("BuildDisplay")]
public static class Custom_Building_Catagories_Patch
{
// Run this code after the above [method] runs
// This [method] will take the [GameOptionsScreen] instance and modify it
// It also takes [workshopButton] variable in the original game code to modify it (The [___] portion is for [HarmonyPatch] not the actual variable name!)
// https:// github.com/pardeike/Harmony/wiki/Patching
// To simplify: [__instance] is used to get the instance of the class; [___VARIABLE NAME] allows [HarmonyPatch] to read and write to private variables
public static void Postfix(ModsScreen __instance, KButton ___workshopButton, Transform ___entryParent)
{
    string CustomizeButtonName = "customizeButton";
    // The buttons have a [transform] which stores its position and relationship with its [parent] page (the mods page)
    // This method is used to access the list of all buttons on the [GameOptionScreen __instance] because [___workshopButton] is a defined variable linked to the parent
    // I think the list of buttons can be referenced in different methods but this was the simplest way of handling it
    var buttons = ___entryParent.GetComponentsInChildren<KButton>();
    var hasCustomize = false;

    // THIS IS NECESSARY
    // The [ModScreen.OnToggleAllClicked()] enables all mods and reruns [ModScreen.BuildDisplay()] to update all mod toggle buttons, without this, the game will countinuously make more copies of this button!
    // Or whenever [ModScreen.BuildDisplay()] function is run without clearing the instance in that matter
    foreach (var button in buttons)
    {
        if (button.name == CustomizeButtonName)
        {
            hasCustomize = true;
        }
    }

    if (!hasCustomize)
    {
        // To display the button on the UI, we take the shape and properties of [___workshopButton.gameObject], and locate the button on [___worshopButton.transform.parent.gameObject]
        var customizeButton = Util.KInstantiateUI<KButton>(___workshopButton.gameObject, ___entryParent.gameObject);
        customizeButton.name = CustomizeButtonName;
        customizeButton.transform.GetComponentInChildren<LocText>().text = "Customize Building Menu";
        // The command below will locate the mod at the top of the mod list
        // The button's [transform] (position) is set as first in the list of children
        // As the game displays the mods based on the order in the children list from the parent, this will locate the mod at the top
        customizeButton.transform.SetAsFirstSibling();
        // This activates the button ([gameObject]), which makes it visible and interactable
        customizeButton.gameObject.SetActive(true);
        // customizeButton.onClick += (() => ToggleAllMods(__instance, false));
    }
}
}
}
*/
