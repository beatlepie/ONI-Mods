//namespace Reapy
//{
//    public class ReapBotTrappedStates : GameStateMachine<ReapBotTrappedStates, ReapBotTrappedStates.Instance, IStateMachineTarget, ReapBotTrappedStates.Def>
//    {
//        public override void InitializeStates(out StateMachine.BaseState default_state)
//        {
//            default_state = blockedStates.evaluating;
//            blockedStates.ToggleStatusItem(Db.Get().RobotStatusItems.CantReachStation, (ReapBotTrappedStates.Instance smi) => smi.gameObject, Db.Get().StatusItemCategories.Main).TagTransition(GameTags.Robots.Behaviours.TrappedBehaviour, behaviourcomplete, true);
//            blockedStates.evaluating.Enter(delegate (ReapBotTrappedStates.Instance smi)
//            {
//                if (smi.sm.GetReapLocker(smi) == null)
//                {
//                    smi.GoTo(blockedStates.noHome);
//                    return;
//                }
//                smi.GoTo(blockedStates.blocked);
//            });
//            // [RescueSweepBotChore] might cause an issue...
//            blockedStates.blocked.ToggleChore((ReapBotTrappedStates.Instance smi) => new RescueSweepBotChore(smi.master, smi.master.gameObject, smi.sm.GetReapLocker(smi).gameObject), behaviourcomplete, blockedStates.evaluating).PlayAnim("react_stuck", KAnim.PlayMode.Loop);
//            blockedStates.noHome.PlayAnim("react_stuck", KAnim.PlayMode.Once).OnAnimQueueComplete(blockedStates.evaluating);
//            behaviourcomplete.BehaviourComplete(GameTags.Robots.Behaviours.TrappedBehaviour, false);
//        }

//        public Storage GetReapLocker(ReapBotTrappedStates.Instance smi)
//        {
//            StorageUnloadMonitor.Instance smi2 = smi.master.gameObject.GetSMI<StorageUnloadMonitor.Instance>();
//            if (smi2 == null)
//            {
//                return null;
//            }
//            return smi2.sm.sweepLocker.Get(smi2);
//        }

//        // Token: 0x040006D9 RID: 1753
//        public ReapBotTrappedStates.BlockedStates blockedStates;

//        // Token: 0x040006DA RID: 1754
//        public GameStateMachine<ReapBotTrappedStates, ReapBotTrappedStates.Instance, IStateMachineTarget, ReapBotTrappedStates.Def>.State behaviourcomplete;

//        // Token: 0x02000D60 RID: 3424
//        public class Def : StateMachine.BaseDef
//        {
//        }

//        // Token: 0x02000D61 RID: 3425
//        public new class Instance : GameStateMachine<ReapBotTrappedStates, ReapBotTrappedStates.Instance, IStateMachineTarget, ReapBotTrappedStates.Def>.GameInstance
//        {
//            // Token: 0x0600609F RID: 24735 RVA: 0x00244D9A File Offset: 0x00242F9A
//            public Instance(Chore<ReapBotTrappedStates.Instance> chore, ReapBotTrappedStates.Def def) : base(chore, def)
//            {
//                chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Robots.Behaviours.TrappedBehaviour);
//            }
//        }

//        // Token: 0x02000D62 RID: 3426
//        public class BlockedStates : GameStateMachine<ReapBotTrappedStates, ReapBotTrappedStates.Instance, IStateMachineTarget, ReapBotTrappedStates.Def>.State
//        {
//            // Token: 0x0400495C RID: 18780
//            public GameStateMachine<ReapBotTrappedStates, ReapBotTrappedStates.Instance, IStateMachineTarget, ReapBotTrappedStates.Def>.State evaluating;

//            // Token: 0x0400495D RID: 18781
//            public GameStateMachine<ReapBotTrappedStates, ReapBotTrappedStates.Instance, IStateMachineTarget, ReapBotTrappedStates.Def>.State blocked;

//            // Token: 0x0400495E RID: 18782
//            public GameStateMachine<ReapBotTrappedStates, ReapBotTrappedStates.Instance, IStateMachineTarget, ReapBotTrappedStates.Def>.State noHome;
//        }
//    }
//}
