//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;

//namespace JumpSweepy
//{
//    [HarmonyPatch(typeof(GameNavGrids), "CreateWalkerNavigation")]
//    class PatchTESTING
//    {
//        public static bool Prefix(Pathfinding pathfinding, string id, CellOffset[] bounding_offsets, ref NavGrid __result)
//        {
//            NavGrid.Transition[] transitions = new NavGrid.Transition[]
//            {
//            new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, 0, NavAxis.NA, true, true, true, 1, "", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),

//            // FOR SOME REASON THE GAME COULD NOT MIRROR SOME OF THESE CUSTOM TRANSITIONS!!!!!
//            new NavGrid.Transition(NavType.Floor, NavType.Ladder, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]{ new CellOffset(1, -1) }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//            new NavGrid.Transition(NavType.Floor, NavType.Ladder, -1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]{ new CellOffset(-1, -1) }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//            new NavGrid.Transition(NavType.Ladder, NavType.Floor, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//            new NavGrid.Transition(NavType.Ladder, NavType.Ladder, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]{ new CellOffset(1, -1) }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//            new NavGrid.Transition(NavType.Ladder, NavType.Ladder, -1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]{ new CellOffset(-1, -1) }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),

//            new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, 1, NavAxis.NA, false, false, true, 1, "", new CellOffset[]
//            {
//                new CellOffset(0, 1)
//            }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//            new NavGrid.Transition(NavType.Floor, NavType.Floor, 2, 0, NavAxis.NA, false, false, true, 1, "", new CellOffset[]
//            {
//                new CellOffset(1, 0),
//                new CellOffset(1, -1)
//            }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
///*            new NavGrid.Transition(NavType.Floor, NavType.Floor, 5, 0, NavAxis.NA, false, false, true, 1, "floor_floor_2_0", new CellOffset[]
//            {
//                new CellOffset(1, 0),
//                new CellOffset(1, -1),
//                new CellOffset(2, 0),
//                new CellOffset(2, -1),
//                new CellOffset(3, 0),
//                new CellOffset(3, -1),
//                new CellOffset(4, 0),
//                new CellOffset(4, -1),
//            }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//*/            new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, -2, NavAxis.NA, false, false, true, 1, "", new CellOffset[]
//            {
//                new CellOffset(1, 0),
//                new CellOffset(1, -1)
//            }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//            new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, -1, NavAxis.NA, false, false, true, 1, "", new CellOffset[]
//            {
//                new CellOffset(1, 0)
//            }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
//            new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, 2, NavAxis.NA, false, false, true, 1, "", new CellOffset[]
//            {
//                new CellOffset(0, 1),
//                new CellOffset(0, 2)
//            }, new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f)
//            };
//            NavGrid.Transition[] array = PatchGameNavGrids.MirrorTransitions(transitions);
//            NavGrid.NavTypeData[] nav_type_data = new NavGrid.NavTypeData[]
//            {
//                new NavGrid.NavTypeData
//                {
//                    navType = NavType.Floor,
//                    idleAnim = "idle_loop"
//                },
//                new NavGrid.NavTypeData
//                {
//                    navType = NavType.Ladder,
//                    idleAnim = "idle_loop"
//                }
//            };
//            NavGrid navGrid = new NavGrid(id, array, nav_type_data, bounding_offsets, new NavTableValidator[]
//            {
//                // Having [true] here will allow the critter to walk ON ladders, instead of IN it
//                new GameNavGrids.FloorValidator(false),
//                new GameNavGrids.LadderValidator()
//            }, 2, 3, array.Length);
//            pathfinding.AddNavGrid(navGrid);
//            __result = navGrid;
//            return false;
//        }
//    }


///*    [HarmonyPatch(typeof(EntityConfigManager), "RegisterEntity")]
//    class PatchTESTING
//    {
//        public static bool Prefix(IEntityConfig config)
//        {
//            Debug.Log(config);

//            KPrefabID component = config.CreatePrefab().GetComponent<KPrefabID>();
//            Debug.Log(component.name);

//            component.prefabInitFn += config.OnPrefabInit;
//            component.prefabSpawnFn += config.OnSpawn;
//            Assets.AddPrefab(component);

//            return false;
//        }
//    }
//*/
//}