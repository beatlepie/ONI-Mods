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

        public ReapyOptions()
        {
            warningsOff = false;
        }
    }
}
