
namespace ChooseNeuralVacillator
{
    public class ChooseNeuralVacillatorModInfo : KMod.UserMod2
    {
        /*
         * 2021/07/21: Starting the Mod at the request of [SquidCake#0420]!
         *
         * Originally wanted to modify the side screen that it has to add the options. 
         * Could not find the initialization for the side screen [contents], and seemed like adding [PLib] elements may cause issues.
         * 
         * Found the name [GeneShuffler] from a different mod that allowed the building to be built!
         *
         * Found the [ApplyRandomTrait] is very easy to modify, gets traits that the duplicant does not have yet, and allows the user to choose which the duplicant will get!
         * 
         * When removing file, remove from *****solution explorer***** as well!
         * 
         * It seems that the [TextStyle] is only initialized at [Build()], which will cause a crash if you modify its values without setting a value to it!
         * 
         * ****************************************************************************************************************************************************************************
         * From Peter Han (peterhaneve#5420) (2021/07/22, 11:54pm, ONI discord): 
         * Also you changed UILightStyle!
         * Please do not do this, use PUIUtils.DeriveStyle
         * You will get away with it if you ilmerge
         * If you do not ilmerge, you could break other mods
         * If you ilmerge the assembly, each one gets its own copy, and they will be unaffected
         * If you do not, any mods that do not merge like so will pull from one copy, which could cause issues
         * ****************************************************************************************************************************************************************************
         * From Aze (Aze#0066) (2021/07/22, 11:55pm, ONI discord):
         * Also, unsolicited suggestion: It looks like your RandomSelected could probably be a Reverse Patch
         * ****************************************************************************************************************************************************************************
         * 
         * I added a [myStyle] for Peter Han's advice, could not figure out how to do [HarmonyReversePatch], but it works so I will leave it be :P
         * 
         * There was an issue where the machine will stay in the wrong state after the Neural Vacillation, this seems to have been caused due to the delay
         * in [SetConsumed], as the state machine seeks to change state immediately after the animation!
         * SOLUTION1: tried relocating [SetConsumed] so it is executed before building the menu instead of after selecting the trait
         * Above did not work, though will leave it as it is just in case, as it should not change anything
         * SOLUTION2: found that [IsConsumed] is public instead of private, Harmony could not find the correct value, and was not altering them
         * When the value was accessed and changed correctly, it worked!
         * ****************************************************************************************************************************************************************************
         * Harmony CANNOT access public values using [___VARIABLE_NAME]!!!!!
         * ****************************************************************************************************************************************************************************
         * There still is a bug where trying to use and recharge the vacillator will cause the vacillator to get stuck until reload.
         * This is fixed by Stock Bug Fix, though I should be able to fix this on my own...right?
         * I think it can be fixed by forcing the device back to idle state (return to root/default) after delivery is complete?
         */
    }
}
