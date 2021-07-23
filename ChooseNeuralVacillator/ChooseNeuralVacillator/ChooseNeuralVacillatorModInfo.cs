
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
        */
    }
}
