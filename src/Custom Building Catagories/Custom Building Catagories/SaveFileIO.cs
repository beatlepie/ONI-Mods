using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using static Custom_Building_Categories.GameBuildingMenuData;
using static Custom_Building_Categories.CustomizeBuildingMenuScreen;
//IBW-493Z.

namespace Custom_Building_Categories
{
    class SaveFileIO
    {
        /// <summary>
        /// DEFAULT and SAVED building menu settings
        /// </summary>
        public static List<PlanScreen.PlanInfo> DEFAULT = new List<PlanScreen.PlanInfo>(TUNING.BUILDINGS.PLANORDER);
        public static List<PlanScreen.PlanInfo> SAVED = new List<PlanScreen.PlanInfo>();

        /// <summary>
        /// List of category data that contains the [HashedString] category and [List<string>] data instead of [object] data
        /// </summary>
        static List<CategoryData> categoryData;

        /// <summary>
        /// [struct] made to store settings and conveniently write and read as json
        /// </summary>
        public static Data data;

        /// <summary>
        /// Save all the settings and changes made by the user
        /// </summary>
        public static void Save()
        {
            categoryData = new List<CategoryData>();
            foreach(PlanScreen.PlanInfo plan in categoryMenu)
            {
                categoryData.Add(new CategoryData(plan.category, plan.data as IList<string>));
            }
            data = new Data(categoryNames, categoryData, uncategorizedBuildings);

            // The [@-quoting] or [@-quoted] line indicates that the string contains a literal [\] character, which is an escape character by default
            using (StreamWriter file = File.CreateText(SAVE_FILE))
            {
                var serialized = JsonConvert.SerializeObject(data, Formatting.Indented);
                file.Write(serialized);
            }
        }

        /// <summary>
        /// Read from the save file and apply those settings
        /// </summary>
        public static void Read()
        {
            using (StreamReader file = new StreamReader(SAVE_FILE))
                data = JsonConvert.DeserializeObject<Data>(file.ReadToEnd());

            // Sets all the values from the save file
            categoryNames = data.categoryNames;
            foreach (CategoryData category in data.categoryMenu)
                SAVED.Add(new PlanScreen.PlanInfo(category.category, false, category.buildings));
            categoryMenu = new List<PlanScreen.PlanInfo>(SAVED);
            uncategorizedBuildings = data.uncategorizedBuildings;

            // Check for any category that is not linked to tooltips and name
            foreach(PlanScreen.PlanInfo plan in SAVED)
            {
                if (!iconNameMap.ContainsKey(plan.category))
                {
                    Debug.Log("ADDED:" + categoryNames[plan.category.ToString()]);
                    iconNameMap.Add(HashCache.Get().Add(categoryNames[plan.category.ToString()]), "icon_category_base");
                    Strings.Add("STRINGS.UI.BUILDCATEGORIES." + categoryNames[plan.category.ToString()].ToUpper() + ".NAME", categoryNames[plan.category.ToString()]);
                    Strings.Add("STRINGS.UI.BUILDCATEGORIES." + categoryNames[plan.category.ToString()].ToUpper() + ".TOOLTIP", "Customized Building Category!");
                }
            }
        }

        /// <summary>
        /// The [struct] used to save and call the settings
        /// </summary>
        public struct Data
        {
            public Dictionary<string, string> categoryNames;
            public List<CategoryData> categoryMenu;
            public List<string> uncategorizedBuildings;

            public Data(Dictionary<string, string> categoryNames, List<CategoryData> categoryMenu, List<string> uncategorizedBuildings)
            {
                this.categoryNames = categoryNames;
                this.categoryMenu = categoryMenu;
                this.uncategorizedBuildings = uncategorizedBuildings;
            }
        }
    }
}