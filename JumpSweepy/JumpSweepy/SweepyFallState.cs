using System;
using STRINGS;

namespace JumpSweepy
{
	public class SweepyFallStates : GameStateMachine<SweepyFallStates, SweepyFallStates.Instance, IStateMachineTarget, SweepyFallStates.Def>
	{
		public override void InitializeStates(out StateMachine.BaseState default_state)
		{
			default_state = loop;
			root.ToggleStatusItem(CREATURES.STATUSITEMS.FALLING.NAME, CREATURES.STATUSITEMS.FALLING.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Neutral, false, default(HashedString), 129022, null, null, Db.Get().StatusItemCategories.Main);
			loop.PlayAnim((SweepyFallStates.Instance smi) => smi.GetSMI<SweepyFallMonitor.Instance>().anim, KAnim.PlayMode.Loop).ToggleGravity().EventTransition(GameHashes.Landed, this.snaptoground, null).Transition(pst, (SweepyFallStates.Instance smi) => smi.GetSMI<SweepyFallMonitor.Instance>().CanSwimAtCurrentLocation(true), UpdateRate.SIM_33ms);
			snaptoground.Enter(delegate (SweepyFallStates.Instance smi)
			{
				smi.GetSMI<SweepyFallMonitor.Instance>().SnapToGround();
			}).GoTo(pst);
			pst.Enter(new StateMachine<SweepyFallStates, SweepyFallStates.Instance, IStateMachineTarget, SweepyFallStates.Def>.State.Callback(SweepyFallStates.PlayLandAnim)).BehaviourComplete(GameTags.Creatures.Falling, false);
		}

		private static void PlayLandAnim(SweepyFallStates.Instance smi)
		{
			smi.GetComponent<KBatchedAnimController>().Queue(smi.def.getLandAnim(smi), KAnim.PlayMode.Loop, 1f, 0f);
		}

		private GameStateMachine<SweepyFallStates, SweepyFallStates.Instance, IStateMachineTarget, SweepyFallStates.Def>.State loop;

		private GameStateMachine<SweepyFallStates, SweepyFallStates.Instance, IStateMachineTarget, SweepyFallStates.Def>.State snaptoground;

		private GameStateMachine<SweepyFallStates, SweepyFallStates.Instance, IStateMachineTarget, SweepyFallStates.Def>.State pst;

		public class Def : StateMachine.BaseDef
		{
			public Func<SweepyFallStates.Instance, string> getLandAnim = (SweepyFallStates.Instance smi) => "idle_loop";
		}

		public new class Instance : GameStateMachine<SweepyFallStates, SweepyFallStates.Instance, IStateMachineTarget, SweepyFallStates.Def>.GameInstance
		{
			public Instance(Chore<SweepyFallStates.Instance> chore, SweepyFallStates.Def def) : base(chore, def)
			{
				chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Falling);
			}
		}
	}
}
