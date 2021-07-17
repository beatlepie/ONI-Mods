using UnityEngine;

namespace JumpSweepy
{
	public class SweepyFallMonitor : GameStateMachine<SweepyFallMonitor, SweepyFallMonitor.Instance, IStateMachineTarget, SweepyFallMonitor.Def>
	{
		public override void InitializeStates(out StateMachine.BaseState default_state)
		{
			default_state = this.grounded;
			this.grounded.ToggleBehaviour(GameTags.Creatures.Falling, (SweepyFallMonitor.Instance smi) => smi.ShouldFall(), null);
		}

		public static float FLOOR_DISTANCE = -0.065f;

		public GameStateMachine<SweepyFallMonitor, SweepyFallMonitor.Instance, IStateMachineTarget, SweepyFallMonitor.Def>.State grounded;

		public GameStateMachine<SweepyFallMonitor, SweepyFallMonitor.Instance, IStateMachineTarget, SweepyFallMonitor.Def>.State falling;

		public class Def : StateMachine.BaseDef
		{
			public bool canSwim;
		}

		public new class Instance : GameStateMachine<SweepyFallMonitor, SweepyFallMonitor.Instance, IStateMachineTarget, SweepyFallMonitor.Def>.GameInstance
		{
			public Instance(IStateMachineTarget master, SweepyFallMonitor.Def def) : base(master, def)
			{
				this.navigator = master.GetComponent<Navigator>();
			}

			public void SnapToGround()
			{
				Vector3 position = base.smi.transform.GetPosition();
				Vector3 position2 = Grid.CellToPosCBC(Grid.PosToCell(position), Grid.SceneLayer.Creatures);
				position2.x = position.x;
				base.smi.transform.SetPosition(position2);
				if (this.navigator.IsValidNavType(NavType.Floor))
				{
					this.navigator.SetCurrentNavType(NavType.Floor);
					return;
				}
				if (this.navigator.IsValidNavType(NavType.Hover))
				{
					this.navigator.SetCurrentNavType(NavType.Hover);
				}
			}

			public bool ShouldFall()
			{
				if (base.gameObject.HasTag(GameTags.Stored))
				{
					return false;
				}
				Vector3 position = base.smi.transform.GetPosition();
				int num = Grid.PosToCell(position);
				if (Grid.IsValidCell(num) && Grid.Solid[num])
				{
					return false;
				}
				if (this.navigator.IsMoving())
				{
					return false;
				}
				// *************************************************************************************************************************************************************************************
				// This is the only part changed from [SweepyFallMonitor]!
				if (Grid.FakeFloor[Grid.CellBelow(num)])
                {
					return false;
                }
				// *************************************************************************************************************************************************************************************
				if (this.CanSwimAtCurrentLocation(false))
				{
					return false;
				}
				if (this.navigator.CurrentNavType != NavType.Swim)
				{
					if (this.navigator.NavGrid.NavTable.IsValid(num, this.navigator.CurrentNavType))
					{
						return false;
					}
					if (this.navigator.CurrentNavType == NavType.Ceiling)
					{
						return true;
					}
					if (this.navigator.CurrentNavType == NavType.LeftWall)
					{
						return true;
					}
					if (this.navigator.CurrentNavType == NavType.RightWall)
					{
						return true;
					}
				}
				Vector3 vector = position;
				vector.y += SweepyFallMonitor.FLOOR_DISTANCE;
				int num2 = Grid.PosToCell(vector);
				return !Grid.IsValidCell(num2) || !Grid.Solid[num2];
			}

			public bool CanSwimAtCurrentLocation(bool check_head)
			{
				if (base.def.canSwim)
				{
					Vector3 position = base.transform.GetPosition();
					float num = 1f;
					if (!check_head)
					{
						num = 0.5f;
					}
					position.y += base.transform.GetComponent<KBoxCollider2D>().size.y * num;
					if (Grid.IsSubstantialLiquid(Grid.PosToCell(position), 0.35f))
					{
						if (!GameComps.Gravities.Has(base.gameObject))
						{
							return true;
						}
						if (GameComps.Gravities.GetData(GameComps.Gravities.GetHandle(base.gameObject)).velocity.magnitude < 2f)
						{
							return true;
						}
					}
				}
				return false;
			}

			public string anim = "fall";

			private Navigator navigator;
		}
	}
}
