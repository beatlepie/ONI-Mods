using HarmonyLib;
using System.Collections.Generic;

namespace Reapy
{
    [HarmonyPatch(typeof(GameNavGrids), "CreateWalkerBabyNavigation")]
    public class PatchGameNavGrids
    {
		/// <summary>
		/// This changes the [FloorValidator] from [false] to [true].
		/// All other methods in this file is the same as the game!
		/// </summary>
        private static void Postfix(Pathfinding pathfinding, CellOffset[] bounding_offsets)
        {
			NavGrid.Transition[] transitions = new NavGrid.Transition[]
			{
			new NavGrid.Transition(NavType.Floor, NavType.Floor, 1, 0, NavAxis.NA, true, true, true, 1, "", new CellOffset[0], new CellOffset[0], new NavOffset[0], new NavOffset[0], false, 1f)
			};
			NavGrid.Transition[] array = MirrorTransitions(transitions);
			NavGrid.NavTypeData[] nav_type_data = new NavGrid.NavTypeData[]
			{
				new NavGrid.NavTypeData
				{
					navType = NavType.Floor,
					idleAnim = "idle_loop"
				}
			};
			NavGrid navGrid = new NavGrid("ReapyNavGrid", array, nav_type_data, bounding_offsets, new NavTableValidator[]
			{
				// This must be true for Reapy to pass doors! Thank you to [Romen] for finding that!
				new GameNavGrids.FloorValidator(true)
			}, 2, 0, array.Length);
			pathfinding.AddNavGrid(navGrid);
		}

		private static NavGrid.Transition[] MirrorTransitions(NavGrid.Transition[] transitions)
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

		private static CellOffset[] MirrorOffsets(CellOffset[] offsets)
		{
			List<CellOffset> list = new List<CellOffset>();
			foreach (CellOffset cellOffset in offsets)
				list.Add(new CellOffset(-cellOffset.x, cellOffset.y));
			return list.ToArray();
		}

		private static NavOffset[] MirrorNavOffsets(NavOffset[] offsets)
		{
			List<NavOffset> list = new List<NavOffset>();
			foreach (NavOffset navOffset in offsets)
				list.Add(new NavOffset(NavGrid.MirrorNavType(navOffset.navType), -navOffset.offset.x, navOffset.offset.y));
			return list.ToArray();
		}

	}
}
