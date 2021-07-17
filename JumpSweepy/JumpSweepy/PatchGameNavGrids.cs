using HarmonyLib;
using System.Collections.Generic;

namespace JumpSweepy
{
    [HarmonyPatch(typeof(GameNavGrids), "CreateDuplicantNavigation")]
    public static class PatchGameNavGrids
    {
        // The value is assigned at the end of [CreateSweepyNavigation]
        public static NavGrid SweepyGrid;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathfinding"></param>
        private static void Postfix(Pathfinding pathfinding)
        {
            // As this is a single tile creature, its [bounding_offsets] is only 0, 0!
            CreateSweepyNavigation(pathfinding, "SweepyNavGrid", new CellOffset[] { new CellOffset(0, 0) });
        }

        private static void CreateSweepyNavigation(Pathfinding pathfinding, string id, CellOffset[] bounding_offsets)
        {
            // When jumping, if these are in front, do not jump!
            NavOffset[] invalid_nav_offsets = new NavOffset[]
            {
            new NavOffset(NavType.Floor, 1, 0),
            new NavOffset(NavType.Ladder, 1, 0),
            new NavOffset(NavType.Pole, 1, 0)
            };

            // All movements' cost are 1 as the bot will only move left or right with no vertical movement
            NavGrid.Transition[] transitions = new NavGrid.Transition[]
            {
                // Walk forward one tile
                new NavGrid.Transition(NavType.Hover, NavType.Hover, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], false, 1f),
                // Jump forward, over a single tile gap
                new NavGrid.Transition(NavType.Floor, NavType.Floor, 2, 0, NavAxis.NA, false, false, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front (next cell) must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, false, 1f),
                // Walk foward one tile on to a ladder, cannot hop onto ladder if there is a floor
                new NavGrid.Transition(NavType.Floor, NavType.Ladder, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[]
                {
                    // If there is a floor in front, it cannot use this transition!
                    new NavOffset(NavType.Floor, 1, 0)
                }, false, 1f),
                // Jump forward to a ladder
                new NavGrid.Transition(NavType.Floor, NavType.Ladder, 2, 0, NavAxis.NA, false, false, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, false, 1f),
                // Jump forward to a ladder from a ladder
                new NavGrid.Transition(NavType.Ladder, NavType.Ladder, 2, 0, NavAxis.NA, false, false, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, false, 1f),
                // Walk forward to a ladder from a ladder
                new NavGrid.Transition(NavType.Ladder, NavType.Ladder, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], false, 1f),
                // Walk forward to a pole from a floor
                new NavGrid.Transition(NavType.Floor, NavType.Pole, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[]
                {
                    // If there is a floor in front, it cannot use this transition!
                    new NavOffset(NavType.Floor, 1, 0)
                }, false, 1f),
                // Jump forward to a ladder
                new NavGrid.Transition(NavType.Floor, NavType.Pole, 2, 0, NavAxis.NA, false, false, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, false, 1f),
                // Walk forward to a ladder from a ladder
                new NavGrid.Transition(NavType.Pole, NavType.Pole, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], false, 1f)
            };

            // *************************************************************************************************************************************************************************************
            // Mirroring direction process
            List<NavGrid.Transition> list = new List<NavGrid.Transition>();
            foreach (NavGrid.Transition transition in transitions)
            {
                list.Add(transition);
                if (transition.x != 0 || transition.start == NavType.RightWall || transition.end == NavType.RightWall || transition.start == NavType.LeftWall || transition.end == NavType.LeftWall)
                {
                    NavGrid.Transition transition2 = transition;
                    transition2.x = (sbyte)-transition2.x;
                    transition2.voidOffsets = MirrorOffsets(transition.voidOffsets);
                    transition2.solidOffsets = MirrorOffsets(transition.solidOffsets);
                    transition2.validNavOffsets = MirrorNavOffsets(transition.validNavOffsets);
                    transition2.invalidNavOffsets = MirrorNavOffsets(transition.invalidNavOffsets);
                    transition2.start = NavGrid.MirrorNavType(transition2.start);
                    transition2.end = NavGrid.MirrorNavType(transition2.end);
                    list.Add(transition2);
                }
            }
            list.Sort((NavGrid.Transition x, NavGrid.Transition y) => x.cost.CompareTo(y.cost));
            NavGrid.Transition[] completeTransition = list.ToArray();
            // *************************************************************************************************************************************************************************************

            // This sets the default animation for the tiles they are on
            NavGrid.NavTypeData[] nav_type_data = new NavGrid.NavTypeData[]
            {
                new NavGrid.NavTypeData
                {
                    navType = NavType.Floor,
                    idleAnim = "idle_loop"
                },
                new NavGrid.NavTypeData
                {
                    navType = NavType.Ladder,
                    idleAnim = "idle_loop"
                },
                new NavGrid.NavTypeData
                {
                    navType = NavType.Pole,
                    idleAnim = "idle_loop"
                },
                new NavGrid.NavTypeData
                {
                    navType = NavType.Hover,
                    idleAnim = "idle_loop"
                }
            };

            // not exactly sure what the floor validator does...
            SweepyGrid = new NavGrid(id, completeTransition, nav_type_data, bounding_offsets, new NavTableValidator[]
            {
                new GameNavGrids.FloorValidator(true),
                new GameNavGrids.LadderValidator(),
                new GameNavGrids.PoleValidator(),
                new GameNavGrids.HoverValidator()
            }, 2, 1, 32); 
            pathfinding.AddNavGrid(SweepyGrid);
        }

        // This is basically the same as original, just copied over and made static and changed a few things
        // Had to make a new field, [reversed] as modifying [navOffset] is not allowed due to its iterative usage in code!
        private static NavOffset[] MirrorNavOffsets(NavOffset[] offsets)
        {
            List<NavOffset> list = new List<NavOffset>();
            NavOffset reversed;
            foreach (NavOffset navOffset in offsets)
            {
                reversed = navOffset;
                reversed.navType = NavGrid.MirrorNavType(navOffset.navType);
                reversed.offset.x = -navOffset.offset.x;
                list.Add(reversed);
            }
            return list.ToArray();
        }

        private static CellOffset[] MirrorOffsets(CellOffset[] offsets)
        {
            List<CellOffset> list = new List<CellOffset>();
            CellOffset reversed;
            foreach (CellOffset cellOffset in offsets)
            {
                reversed = cellOffset;
                reversed.x = -cellOffset.x;
                list.Add(cellOffset);
            }
            return list.ToArray();
        }
    }
}