using System;
using System.Collections.Generic;
using TUNING;
using HarmonyLib;

namespace Reapy
{
    //this function will patch the game to add this [testing] building
    public static class AddReapyPatch
    {
        //[typeof] gets the type of the (class) while the code is running, "method" of that class to patch
        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch("LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                ReapBotStationConfig.AddConfig();
            }
        }
    }

    //THIS CODE IS TAKEN FROM NIGHTINGGALE'S MOD
    public static class AddBuilding
    {
        //This adds the building to the "planning screen" so that it can be selected
        public static void AddBuildingToPlanScreen(HashedString category, string buildingId, string parentId)
        {
            int categoryIndex = GetCategoryIndex(category, buildingId);
            if (categoryIndex == -1)
            {
                //this is used to prevent an error!
                return;
            }
            int? num = null;
            if (!parentId.IsNullOrWhiteSpace())
            {
                //this gets the list of buildings under [category(such as plumbing)] building list in the game files
                IList<string> list = BUILDINGS.PLANORDER[categoryIndex].data as IList<string>;
                //this finds the location of the [parentId] building, and places MOD building after this building
                num = ((list != null) ? new int?(list.IndexOf(parentId)) : null);
                if (num != null)
                {
                    num++; //MOD building will be located after the [parentId] building!
                }
            }
            if (num == null)
            {
                Console.WriteLine(string.Concat(new object[]
                {
                    "ERROR: building \"",
                    parentId,
                    "\" not found in category ",
                    category,
                    ". Placing ",
                    buildingId,
                    " at the end of the list"
                }));
            }
            AddBuildingToPlanScreen(category, buildingId, num);
        }

        //seems like the repetition checks which screen the game is in...
        internal static void AddBuildingToPlanScreen(HashedString category, string buildingId, int? index = null)
        {
            int categoryIndex = AddBuilding.GetCategoryIndex(category, buildingId);
            if (categoryIndex == -1) return;
            if (index != null)
            {
                int? num = index;
                int num2 = 0;
                if (num.GetValueOrDefault() >= num2 & num != null)
                {
                    num = index;
                    IList<string> list = BUILDINGS.PLANORDER[categoryIndex].data as IList<string>;
                    int? num3 = (list != null) ? new int?(list.Count) : null;
                    if (num.GetValueOrDefault() < num3.GetValueOrDefault() & (num != null & num3 != null))
                    {
                        IList<string> list2 = BUILDINGS.PLANORDER[categoryIndex].data as IList<string>;
                        if (list2 == null)
                        {
                            return;
                        }
                        list2.Insert(index.Value, buildingId);
                        return;
                    }
                }
            }
            IList<string> list3 = BUILDINGS.PLANORDER[categoryIndex].data as IList<string>;
            if (list3 == null) return;

            list3.Add(buildingId);
        }

        private static int GetCategoryIndex(HashedString category, string buildingId)
        {
            //[BUILDINGS.PLANORDER.FindIndex((PlanScreen.PlanInfo x) => x.category] gets the category names of 
            //[PlanInfo] stored in [BUILDINGS.PLANORDER] list and checks where the MOD building should go into
            int num = BUILDINGS.PLANORDER.FindIndex((PlanScreen.PlanInfo x) => x.category == category);
            if (num == -1)
            {
                Console.WriteLine(string.Concat(new object[]
                {
                    "ERROR: can't add building ",
                    buildingId,
                    " to non-existing category ",
                    category
                }));
            }
            return num;
        }

        //This part was broken after the mergedown!
        public static void IntoTechTree(string tech, string BuildingID)
        {
/*            //this time, instead of bringing the [IList] with [as] it copies the array
            //both methods are valid!
            List<string> list = new List<string>(Techs.TECH_GROUPING[Tech]);
            list.Insert(1, BuildingID);
            //insert the MOD building, then copies it back to the game
            Techs.TECH_GROUPING[Tech] = list.ToArray();
*/

            //code used to test where and how the building will be added!
/*            Debug.Log(Db.Get().Techs.Name);
            foreach (Tech stuff in Db.Get().Techs.resources)
            {
                //remember those brackets...
                Debug.Log(stuff.Id + "     " + (stuff.Id == tech));
            }
*/            
            
            Tech target = Db.Get().Techs.resources.Find(x => x.Id == tech);
            target.unlockedItemIDs.Add(BuildingID);
        }

        //?????? is this even necessary??????
        private static int GetTechCategoryIndex(HashedString category, string buildingId)
        {
            int num = BUILDINGS.PLANORDER.FindIndex((PlanScreen.PlanInfo x) => x.category == category);
            if (num == -1)
            {
                Console.WriteLine(string.Concat(new object[]
                {
                    "ERROR: can't add building ",
                    buildingId,
                    " to non-existing category ",
                    category
                }));
            }
            return num;
        }

        public static void AddStrings(string ID, string Name, string Description, string Effect)
        {
            //so...[Strings] class has a dictionary function in it that can be used globally (because its static)
            //this function adds the MOD building descriptors and identifiers to the dictionary so the game can find it
            Strings.Add(new string[]
            {
                "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".NAME",
                string.Concat(new string[]
                {
                    "<link=\"",
                    ID,
                    "\">",
                    Name,
                    "</link>"
                })
            });
            Strings.Add(new string[]
            {
                "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".DESC",
                Description
            });
            Strings.Add(new string[]
            {
                "STRINGS.BUILDINGS.PREFABS." + ID.ToUpperInvariant() + ".EFFECT",
                Effect
            });
        }
    }
}
