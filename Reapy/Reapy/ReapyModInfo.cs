using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace Reapy
{
    public class ReapyModInfo : KMod.UserMod2
    {
        internal static POptions pOption { get; private set; }

        /// <summary>
        /// This is required to use PLib and add options easily for this mod!
        /// </summary>
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            PUtil.InitLibrary();
            pOption = new POptions();

            pOption.RegisterOptions(this, typeof(ReapyOptions));
        }
        /*
         * Thought that it might be easier to try making one first before trying to edit Sweepy!
         * 
         * Looked for all values that made sweepy work!
         * FOUND: [SweepBotConfig], [SweepBotStation], [SweepBotStationConfig], [GameNavGrid], [SweepStates], AND [SweepBotStationSideScreen], [SweepBotTrappedState]
         * Found that some of them can still be used as it is, might have to change them all.
         * With the current edit state, the navigation does not update even if there was a change in blocks.
         * 
         * Had to make [PatchOnSpawn] to change the color of [ReapyBotStation].
         * Added the color change to [ReapyBotConfig]!
         * 2021/08/01: The current iteration of [ReapStates] checks for [GameTags] to find what to pickup. 
         * This means that it will also pickup unrelated things, how would I fix that? It does not have [name] or [GetProperName()].
         * I don't really understand how harvesting works atm, need to find the harvest related mod that will help me figure everything out :)
         * 
         * The [ReturnToChargeStationStates] broke as it specifically addresses [SweepBotStation].
         * Changed above to [ReapyReturnToChargeStationStates] to prevent this issue!
         * 
         * enum [ObjectLayer] is used to find the pickupable
         * 
         * 2021/08/17: removing ReapyNavGrid, it seems too complicated to manipulate sweepy movement...
         * this means that farms need to be built in a linear fashion...
         * still using code from [HarvestTool.DragTool]...which seems to be extremely bad...
         * 
         * 2021/08/21: Made [harvest1] and [harvest2] where the first does the harvesting animation, harvest2 checks that 
         * the plant can still be uprooted, and exists (muckroot disappears after dupe uproots first!)
         * Leaving the tag checking at [TrySweep] because I don't want to compare strings and individual plant products...
         * Changed [FloorValidator] to [true]!
         * 
         * For deubugging uses, get the name from [KSelectable] instead!
         * 
         * 2021/08/22: Found bug related to arbor acorns, it seems that they are [HarvestDesignatable] but does not have [Harvestable]
         * This causes the game to crash immidiately the second one is planted!
         * I believe this can be fixed by simply iternating through [Components.Harvestables.Items] instead of [Components.HarvestDesignatables.Items]
         * Above issue was fixed!
         * 
         * Added [PatchSetHarvestWhenReady] so that error message will not pop up when harvest is disabled on farm tiles!
         * When using [Transpiler], remember to change all the jumps or NOT touch the labels!
         * As it does not recompile afterwards, if you remove the location where the jump (label) is supposed to occur, that causes issues!
         **************************************************************************************************************************************
         * If your mod is the only mod that patches that method, it runs once
         * If multiple mods patch the method, the transpiler gets rerun each time, but on the same method each time
         * Harmony starts over from the original method and applies all patches each time a mod patches the method
         **************************************************************************************************************************************
         * Above is from [Peter Han], indicates that transpiler only runs during patching, does not run again during game time!
         * 
         * 
         * Adding [ReapyOptions] and [Onload] for harmony for the above setting!
         * For [OnLoad], [public override void OnLoad(Harmony harmony)] is necessary, and must be placed under [KMod.UserMod2] inheriting class!
         */
    }
}
