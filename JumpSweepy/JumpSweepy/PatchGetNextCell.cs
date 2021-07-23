using HarmonyLib;

namespace JumpSweepy
{
	[HarmonyPatch(typeof(SweepStates), "GetNextCell")]
	public class PatchGetNextCell
	{
		private static bool Prefix(SweepStates.Instance smi, ref int __result)
		{
			__result = GetNextCell(smi);
			// don't run the original function!
			return false;
		}

		public static int GetNextCell(SweepStates.Instance smi)
		{
			int currentCell = Grid.PosToCell(smi);
			int result;

			// is the current floor non-existant OR is the sweepy entombed?
			if (!floorType(Grid.CellBelow(currentCell), currentCell) || Grid.Solid[currentCell])
				result = Grid.InvalidCell;
			else
			{
				// find the next cell to traverse, based on which direction it was going
				int nextCell = smi.sm.headingRight.Get(smi) ? Grid.CellRight(currentCell) : Grid.CellLeft(currentCell);
				// if the next cell is traversable, return [nextCell], otherwise, check next of [nextCell]
				result = canMoveToCell(nextCell) ? nextCell : Grid.InvalidCell;
				if (result == Grid.InvalidCell)
				{
					nextCell = smi.sm.headingRight.Get(smi) ? Grid.CellRight(nextCell) : Grid.CellLeft(nextCell);
					result = canMoveToCell(nextCell) ? nextCell : Grid.InvalidCell;
				}
			}
			return result;
		}

		public static bool canMoveToCell(int cellNext)
		{
			bool result;
			int cellBelowNext = Grid.CellBelow(cellNext);

			// if the next cell or the floor of the next cell is non-existant, return false
			if (!Grid.IsValidCell(cellNext) || !Grid.IsValidCell(cellBelowNext))
				result = false;
			else
				result = floorType(cellBelowNext, cellNext);
			return result;
		}

		/// <summary>
		/// 1. Is the [floor] solid? (natural tile, tile)
		/// 2. Is the [floor] [FakeFloor]? (pitcher pump, open doors)
		/// 3. Is the [floor] [Foundation]? (uhh...tiles?)
		/// 4. Does the [cell] have a ladder? (standing IN ladder)
		/// 5. Does the [cell] have a pole? (standing IN pole)
		/// 6. Does the [floor] have a ladder? (standing ON ladder)
		/// </summary>
		/// <param name="floor"> The cell below the target location </param>
		/// <param name="cell"> The cell Sweepy will be at </param>
		/// <returns> Can Sweepy go to the [cell] specified? </returns>
		private static bool floorType(int floor, int cell)
		{
			return Grid.Solid[floor] || Grid.FakeFloor[floor] || Grid.Foundation[floor] || Grid.HasLadder[cell] || Grid.HasPole[cell] || Grid.HasLadder[floor];
		}
	}
}