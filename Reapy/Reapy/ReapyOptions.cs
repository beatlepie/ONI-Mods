using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using Newtonsoft.Json;

namespace Reapy
{

    /// <summary>
    /// Options for Reapy!
    /// Maybe we can add more so that it is more customizable functions!
    /// </summary>
    // Displays the [preview.png] from mod folder and gives a button that links to the [github page]
    [ModInfo("https://github.com/beatlepie/ONI-Mods", "preview.png")]
    // Must [OptIn] to serialize, only save explicitly indicated fields!
    [JsonObject(MemberSerialization.OptIn)]
    [RestartRequired]
    public sealed class ReapyOptions
    {
        internal static ReapyOptions Options { get; set; }

        [Option("Removes warnings when farm tile plants are set to not harvest!")]
        [JsonProperty]
        public bool warningsOff { get; set; }

        [Option("How far above Reapy should it harvest? (Blocked by tiles!)")]
        [JsonProperty]
        public int harvestRange { get; set; }

        [Option("How far should Reapy be allowed to go?")]
        [JsonProperty]
        public int leashRange { get; set; }

        [Option("How fast should Reapy move?")]
        [JsonProperty]
        public float speed { get; set; }


        public ReapyOptions()
        {
            warningsOff = false;
            harvestRange = 3;
            leashRange = 32;
            speed = 1;
        }
    }
}
