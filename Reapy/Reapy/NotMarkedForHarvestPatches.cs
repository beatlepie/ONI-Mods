using HarmonyLib;
using PeterHan.PLib.Options;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Reapy
{
    [HarmonyPatch(typeof(HarvestDesignatable), "SetHarvestWhenReady")]
    public class PatchSetHarvestWhenReady
    {
        /// <summary>
        /// Checks whether the warnings should be removed first.
        /// If the warning should be removed, we remove part of the code that causes the warning.
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ReapyOptions.Options = POptions.ReadSettings<ReapyOptions>();
            if (ReapyOptions.Options == null)
                ReapyOptions.Options = new ReapyOptions();

            if (!ReapyOptions.Options.warningsOff)
                return instructions;

            var codes = new List<CodeInstruction>(instructions);
            // Locations where the IL must be removed
            int start = 0, end = 0;

            // This should find where the code we want to remove is!
            // This nesting was done to make the code more efficient!
            for(int i = codes.Count - 1; i > 0; i--)
            {
                // The method only has one instance of [OpCodes.Ldc_I4]!
                if (codes[i].opcode == OpCodes.Ldc_I4)
                {
                    // This is [label_2], this must exist as there are jumps programmed to this command/address!
                    end = i - 1;
                    for(int j = i - 1; j > 0; j--)
                    {
                        // The method has multiple instances of [OpCodes.Call], but contains only one of [HarvestDesignatable.CanBeHarvested]!
                        if (codes[j].opcode == OpCodes.Call && (codes[j].operand as MethodInfo) == typeof(HarvestDesignatable).GetMethod("CanBeHarvested", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            // We must remove FROM [ldarg.0] to [pop]!
                            // This is because all the jumps references [IL_0068], removing this causes labeling issues!
                            start = j - 1;
                            break;
                        }
                    }
                    break;
                }
            }
            // This will remove the code starting from [start] to just before [end]!
            codes.RemoveRange(start, end - start);

            return codes;
        }
    }

    [HarmonyPatch(typeof(Harvestable), "SetCanBeHarvested")]
    public class PatchSetCanBeHarvested
    {
        /// <summary>
        /// Checks whether the warnings should be removed first.
        /// If the warnings should be removed, we SKIP the unnecessary code, as removing it will cause branching issues!
        /// Removing mean we have to fix the branching issue as well, where skipping ignore that branching issue!
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ReapyOptions.Options = POptions.ReadSettings<ReapyOptions>();
            if (ReapyOptions.Options == null)
                ReapyOptions.Options = new ReapyOptions();

            if (!ReapyOptions.Options.warningsOff)
                return instructions;

            var codes = new List<CodeInstruction>(instructions);
            int count = 2;

            // Ths nested for loop was done for efficiency!
            for(int i = 0; i < codes.Count; i++)
            {
                // Find the second [brfalse.s] operation!
                if(codes[i].opcode == OpCodes.Brfalse_S)
                {
                    count--;
                    if (count == 0)
                    {
                        count = i;

                        for(int j = i + 1; j < codes.Count; j++)
                        {
                            // Find the next [br.s] operation!
                            if (codes[j].opcode == OpCodes.Br_S)
                            {
                                // Originally tried [codes[count] = codes[j];], but this did not work because
                                // [br.s] does not pop the last value of the stack while [brfalse.s] does.
                                // That caused stack error, as [ret] could not pop the correct call!

                                // Replace the [brfalse.s] operand with [br.s] operand!
                                // This was done because I could not figure out how to make the opcode myself T_T
                                codes[count].operand = codes[j].operand;
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            return codes;
        }
    }
}
