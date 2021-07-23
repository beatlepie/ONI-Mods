namespace JumpSweepy
{
    public class ModInfo : KMod.UserMod2
    {
        /*
         * Made by: magequitpie, 2021/06/04
         * 
         * Huge thank you to GravyJones and their "SmartSweepy" mod for showing me where to start!
         * Huge thanks to the Oxygen Not Included modding community for helping out with all of my troubles!
         * 
         * I want the sweepy to move similar to Hatches, but only 1 tile jump, no horizontal jumps.
         * It will do a falling animation, then "spawn" at the lower cell
         * 
         * It seems that all the code in [PatchGetNextCell] for "SmartSweepy" was made to allow sweepy to walk on water pitcher
         * 
         * [Grid.InvalidCell] is -1!
         * 
         * [navigator.NavGridName = "WalkerNavGrid1x1";] makes it display navigation and movement similar to hatches, but did not move as displayed...
         * seems like it is using navigator, but still using [GetNextCell] to decide when and where to move...?
         */

        /* 2021/06/09 PROBLEM1: I want to make sweepy be able to jump 1 tile horizontally, no vertical jumps. I think vertical jumps does not make sense,
         * adding a "sweepy ramp" or "sweepy elevator" would be better for vertical movement...
         * SOLUTION: I think I can add a new Navigation in [GameNavGrids] class with relative ease, will add that pathing and modify [GetNextCell]!
         * Was trying to use original game code, there were some issues with [MirrorNavOffsets], fixed by adding a new field and making it static...not sure if that makes it work
         * 
         * 2021/06/14 PROBLEM2: DON'T FORGET [public] in front of classes! DON'T FORGET [static] in front of patching classes and functions!
         * USE [%appdata%\..\locallow\klei\Oxygen Not Included] to find the [Player.log] file, not the [Debug.log] file like before...
         * Harmony does not require the first two underscores in [__VARIABLE] if the VARIABLE is the method parameter. They are only for private variables in the function.
         * 
         * 2021/06/21 PROBLEM3: added [SweepyNavGrid], but sweepy cannot jump a tile for some reason. Might need to edit the state machine...
         * Tested by modifying hatch movement, (copy pasted code and added a 5 tile jump navigation) this showed that something else may be required!
         * First, checked the [NavGrid.Transition], as the animation seemed like the probable cause. Made the animation same as walking forward animation!
         * 
         * 2021/06/24 PROBLEM4: There was a huge mergedown for the game, significant issues arose. May have to do other fixes...
         * Inheriting [KMod.UserMod2] will allow the game to patch that class, this should be done on the class with [OnLoad] function!
         * For now the Mergedown doesn't seem to affect vanilla players...
         * 
         * 2021/06/29: Made [SweepyFallMonitor], but kept crashing. Found that crash occurs at [RegisterEntity();] of [EntityConfigManager]!
         * Error below was found when testing, the error occurs during [SweepBotConfig] call of [RegisterEntity();].
         * Attempted rearranging the [LoadGeneratedEntities] list to below the mod, but it did not work.
         * System.NullReferenceException: Object reference not set to an instance of an object
         * at JumpSweepy.PatchTESTING.Prefix (IEntityConfig config) [0x0000e] in <addee422da684a98a1a7941522bfa57a>:0 
         * at (wrapper dynamic-method) EntityConfigManager.RegisterEntity_Patch1(object,IEntityConfig)
         * at EntityConfigManager.LoadGeneratedEntities (System.Collections.Generic.List`1[T] types) [0x000e7] in <ab4bdc425be2489bb8a9b667082958c0>:0 
         * at LegacyModMain.LoadEntities (System.Collections.Generic.List`1[T] types) [0x00000] in <ab4bdc425be2489bb8a9b667082958c0>:0 
         * at LegacyModMain.Load () [0x0004d] in <ab4bdc425be2489bb8a9b667082958c0>:0 
         * at Assets.CreatePrefabs () [0x00006] in <ab4bdc425be2489bb8a9b667082958c0>:0 
         * at Assets.OnPrefabInit () [0x0031c] in <ab4bdc425be2489bb8a9b667082958c0>:0 
         * at KMonoBehaviour.InitializeComponent () [0x00068] in <7888ce3a87c24906b6c550663f653f46>:0 
         * 
         * Found that this crash was occuring due to not having [ref] in the [__result], which prevented harmony from editing the return.
         * 
         * 2021/07/15: The mergedown for everyone has occured, Harmony was change to HarmonyLib and other chnages were made as some game data were changed.
         * 
         * 2021/07/21: PROBLEM5: I believe I have understood what the [GameNavGrid] code does, however because sweepy uses a separate code for movement,
         * it seems that editing the [SweepyNavGrid] is not enough. There are significant inconsistancies in the game code for some reason...
         */
    }
}
