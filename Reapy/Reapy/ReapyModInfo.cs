namespace Reapy
{
    class ReapyModInfo : KMod.UserMod2
    {
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
         */
    }
}
