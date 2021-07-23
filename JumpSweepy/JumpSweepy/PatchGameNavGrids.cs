using HarmonyLib;
using System.Collections.Generic;

namespace JumpSweepy
{
    /// <summary>
    /// Class for patching [CreateDuplicantNavigation], it adds [CreateSweepyNavigation] at the end of it
    /// Contains all code necessary to do above
    /// </summary>
    [HarmonyPatch(typeof(GameNavGrids), "CreateDuplicantNavigation")]
    public static class PatchGameNavGrids
    {
        // The value is assigned at the end of [CreateSweepyNavigation]
        public static NavGrid SweepyGrid;

        /// <summary>
        /// Adds [CreateSweepyNavigation] at the end of [CreateDuplicantNavigation]
        /// </summary>
        /// <param name="pathfinding"> The [pathfinding] instance that saves all the [NavGrid]s </param>
        private static void Postfix(Pathfinding pathfinding)
        {
            // As this is a single tile creature, its [bounding_offsets] is only 0, 0!
            CreateSweepyNavigation(pathfinding, "SweepyNavGrid", new CellOffset[] { new CellOffset(0, 0) });
        }

        /// <summary>
        /// Initializes the [SweepyNavGrid]
        /// </summary>
        /// <param name="pathfinding"> The [pathfinding] instance that saves all the [NavGrid]s </param>
        /// <param name="id"> The name/id used to reference this [NavGrid] </param>
        /// <param name="bounding_offsets"> The locations this creature occupies relative to the anchor cell. </param>
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
                new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
                // Jump forward, over a single tile gap
                new NavGrid.Transition(NavType.Floor, NavType.Floor, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front (next cell) must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                new NavGrid.Transition(NavType.Floor, NavType.Floor, -2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front (next cell) must be free
                    new CellOffset(-1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(-1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk foward one tile on to a ladder, cannot hop onto ladder if there is a floor
                new NavGrid.Transition(NavType.Floor, NavType.Ladder, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[]
                {
                    // If there is a floor in front, it cannot use this transition!
                    new NavOffset(NavType.Floor, 1, 0)
                }, true, 1f),
                // Jump forward to a ladder
                new NavGrid.Transition(NavType.Floor, NavType.Ladder, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk forward to a ladder from a ladder
                new NavGrid.Transition(NavType.Ladder, NavType.Ladder, 1, 0, NavAxis.NA, true, true, true, 2, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
                // Jump forward to a ladder from a ladder
                new NavGrid.Transition(NavType.Ladder, NavType.Ladder, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk off a ladder to floor
                new NavGrid.Transition(NavType.Ladder, NavType.Floor, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
                // Jump forward to a floor from a ladder
                new NavGrid.Transition(NavType.Ladder, NavType.Floor, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk forward to a pole from a floor
                new NavGrid.Transition(NavType.Floor, NavType.Pole, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[]
                {
                    // If there is a floor in front, it cannot use this transition!
                    new NavOffset(NavType.Floor, 1, 0)
                }, true, 1f),
                // Jump forward to a pole
                new NavGrid.Transition(NavType.Floor, NavType.Pole, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk forward to a pole from a pole
                new NavGrid.Transition(NavType.Pole, NavType.Pole, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
                // Jump forward to a pole
                new NavGrid.Transition(NavType.Pole, NavType.Pole, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk forward to a floor from a pole
                new NavGrid.Transition(NavType.Pole, NavType.Floor, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f),
                // Jump forward to a floor from a pole
                new NavGrid.Transition(NavType.Pole, NavType.Floor, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk forward to a pole from a ladder
                new NavGrid.Transition(NavType.Ladder, NavType.Pole, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[]
                {
                    // If there is a floor in front, it cannot use this transition!
                    new NavOffset(NavType.Floor, 1, 0)
                }, true, 1f),
                // Jump forward to a pole from a ladder
                new NavGrid.Transition(NavType.Ladder, NavType.Pole, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f),
                // Walk forward to a ladder from a pole
                new NavGrid.Transition(NavType.Pole, NavType.Ladder, 1, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[]
                {
                    // If there is a floor in front, it cannot use this transition!
                    new NavOffset(NavType.Floor, 1, 0)
                }, true, 1f),
                // Jump forward to a ladder from a pole
                new NavGrid.Transition(NavType.Pole, NavType.Ladder, 2, 0, NavAxis.NA, true, true, true, 1, "floor_floor_1_0", new CellOffset[]
                {
                    // The cell in front must be free
                    new CellOffset(1, 0),
                    // The cell below the next cell must be free
                    new CellOffset(1, -1)
                }, new CellOffset[0], new NavOffset[0], invalid_nav_offsets, true, 1f)
            };

            // Mirroring direction process
            NavGrid.Transition[] completeTransition = MirrorTransitions(transitions);

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
                }
            };

            // not exactly sure what the floor validator does...
            SweepyGrid = new NavGrid(id, completeTransition, nav_type_data, bounding_offsets, new NavTableValidator[]
            {
                new GameNavGrids.FloorValidator(true),
                new GameNavGrids.LadderValidator(),
                new GameNavGrids.PoleValidator(),
                // FOR SOME REASON THIS IS NECESSARY!!!!!
                // This allows sweepy to walk on pitcher pumps, not exactly sure why...even though [FloorValidator] is supposed to allow it too...right?
                new GameNavGrids.HoverValidator()
            }, 2, 1, 32); 
            pathfinding.AddNavGrid(SweepyGrid);
        }

        /// <summary>
        /// This is basically the same as original (mirrors [NavGrid.Transition]s), just copied over and made static and changed a few things
        /// </summary>
        /// <param name="transitions"> The transitions that need to be mirrored </param>
        /// <returns> The original transitions and the mirrored transitions </returns>
        public static NavGrid.Transition[] MirrorTransitions(NavGrid.Transition[] transitions)
        {
            List<NavGrid.Transition> list = new List<NavGrid.Transition>();
            foreach (NavGrid.Transition transition in transitions)
            {
                list.Add(transition);
                if (transition.x != 0 || transition.start == NavType.RightWall || transition.end == NavType.RightWall || transition.start == NavType.LeftWall || transition.end == NavType.LeftWall)
                {
                    NavGrid.Transition transition2 = transition;
                    transition2.x = -transition2.x;
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
            return list.ToArray();
        }

        /// <summary>
        /// This is basically the same as original (mirrors [NavOffset]s), just copied over and made static and changed a few things
        /// Had to make a new field, [reversed] as modifying [navOffset] is not allowed due to its iterative usage in code!
        /// </summary>
        /// <param name="offsets"> The offsets that need to be mirrored </param>
        /// <returns> The original offsets and the mirrored offsets </returns>
        private static NavOffset[] MirrorNavOffsets(NavOffset[] offsets)
        {
            List<NavOffset> list = new List<NavOffset>();
            NavOffset reversed;
            foreach (NavOffset navOffset in offsets)
            {
                reversed = navOffset;
                // Below code is unnecessary for this specific case, as sweepy cannot climb walls.
                //reversed.navType = NavGrid.MirrorNavType(navOffset.navType);
                reversed.offset.x = -navOffset.offset.x;
                list.Add(reversed);
            }
            return list.ToArray();
        }

        /// <summary>
        /// This is basically the same as original (mirrors [CellOffset]s), just copied over and made static and changed a few things
        /// </summary>
        /// <param name="offsets"> The offsets that need to be mirrored </param>
        /// <returns> The original offsets and the mirrored offsets </returns>
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