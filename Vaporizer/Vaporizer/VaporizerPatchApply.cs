using HarmonyLib;

namespace Vaporiser
{
    //this function will patch the game to add this [testing] building
    public static class VaporizerPatchApply
    {
        //[typeof] gets the type of the (class) while the code is running, "method" of that class to patch
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                VaporizerConfig.Setup();
            }
        }
    }
}
