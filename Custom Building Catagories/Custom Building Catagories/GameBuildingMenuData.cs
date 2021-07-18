using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Custom_Building_Categories
{
    class GameBuildingMenuData
    {
        // The location of the save file
        public static string SAVE_FILE = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Save.txt";

        /// <summary>
        /// [struct] to replace [PlanScreen.PlanInfo] as [object] cannot be converted to [List<string>] when read from json
        /// </summary>
        public struct CategoryData
        {
            public HashedString category;
            public List<string> buildings;

            public CategoryData(HashedString category, List<string> buildings)
            {
                this.category = category;
                this.buildings = buildings;
            }
        }

        public static void ADD_STRINGS()
        {
            // This is to add all the pre-existing categories that may have different names due to being swapped
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "VENTILATION" + ".TOOLTIP", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "HVAC" + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "REFINEMENT" + ".TOOLTIP", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Refining".ToUpper() + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "Medicine".ToUpper() + ".TOOLTIP", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Medical".ToUpper() + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "Stations".ToUpper() + ".TOOLTIP", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Equipment".ToUpper() + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "Shipping".ToUpper() + ".TOOLTIP", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Conveyance".ToUpper() + ".NAME"));

            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "VENTILATION" + ".NAME", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "HVAC" + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "REFINEMENT" + ".NAME", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Refining".ToUpper() + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "Medicine".ToUpper() + ".NAME", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Medical".ToUpper() + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "Stations".ToUpper() + ".NAME", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Equipment".ToUpper() + ".NAME"));
            Strings.Add("STRINGS.UI.BUILDCATEGORIES." + "Shipping".ToUpper() + ".NAME", Strings.Get("STRINGS.UI.BUILDCATEGORIES." + "Conveyance".ToUpper() + ".NAME"));
        }

        [JsonProperty]
        public static Dictionary<HashedString, string> iconNameMap = new Dictionary<HashedString, string>
        {
            {
                HashCache.Get().Add("Base"),
                "icon_category_base"
            },
            {
                HashCache.Get().Add("Oxygen"),
                "icon_category_oxygen"
            },
            {
                HashCache.Get().Add("Power"),
                "icon_category_electrical"
            },
            {
                HashCache.Get().Add("Food"),
                "icon_category_food"
            },
            {
                HashCache.Get().Add("Plumbing"),
                "icon_category_plumbing"
            },
            {
                HashCache.Get().Add("HVAC"),
                "icon_category_ventilation"
            },
            {
                HashCache.Get().Add("Refining"),
                "icon_category_refinery"
            },
            {
                HashCache.Get().Add("Medical"),
                "icon_category_medical"
            },
            {
                HashCache.Get().Add("Furniture"),
                "icon_category_furniture"
            },
            {
                HashCache.Get().Add("Equipment"),
                "icon_category_misc"
            },
            {
                HashCache.Get().Add("Utilities"),
                "icon_category_utilities"
            },
            {
                HashCache.Get().Add("Automation"),
                "icon_category_automation"
            },
            {
                HashCache.Get().Add("Conveyance"),
                "icon_category_shipping"
            },
            {
                HashCache.Get().Add("Rocketry"),
                "icon_category_rocketry"
            },
            // These are added to ensure that when categories with different ID and screen name are swapped, the game can still recognize them for what they are
            {
                HashCache.Get().Add("Ventilation"),
                "icon_category_ventilation"
            },
            {
                HashCache.Get().Add("Refinement"),
                "icon_category_refinery"
            },
            {
                HashCache.Get().Add("Medicine"),
                "icon_category_medical"
            },
            {
                HashCache.Get().Add("Stations"),
                "icon_category_misc"
            },
            {
                HashCache.Get().Add("Shipping"),
                "icon_category_shipping"
            }
        };

        // The access to the building menu names were too complicated...they were stored all over the place and could not access them as I'd like...
        [JsonProperty]
        public static Dictionary<string, string> categoryNames = new Dictionary<string, string>()
        {
            { new HashedString("Base").ToString(), "Base" },
            { new HashedString("Oxygen").ToString(), "Oxygen" },
            { new HashedString("Power").ToString(), "Power"},
            { new HashedString("Food").ToString(), "Food" },
            { new HashedString("Plumbing").ToString(), "Plumbing" },
            { new HashedString("HVAC").ToString(), "Ventilation" },
            { new HashedString("Refining").ToString(), "Refinement" },
            { new HashedString("Medical").ToString(), "Medicine" },
            { new HashedString("Furniture").ToString(), "Furniture" },
            { new HashedString("Equipment").ToString(), "Stations" },
            { new HashedString("Utilities").ToString(), "Utilities" },
            { new HashedString("Automation").ToString(), "Automation" },
            { new HashedString("Conveyance").ToString(), "Shipping" },
            { new HashedString("Rocketry").ToString(), "Rocketry" },

            // These are required to switch categories, as their HashedString and actual names are different when originally called
            { new HashedString("Ventilation").ToString(), "Ventilation" },
            { new HashedString("Refinement").ToString(), "Refinement" },
            { new HashedString("Medicine").ToString(), "Medicine" },
            { new HashedString("Stations").ToString(), "Stations" },
            { new HashedString("Shipping").ToString(), "Shipping" },
        };

    }
}
