using UnityEngine;

namespace Reapy
{
    public class ReapyReturnToChargeStationStates : GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>
    {
        public override void InitializeStates(out StateMachine.BaseState default_state)
        {
            default_state = this.emote;
            this.emote.ToggleStatusItem(Db.Get().RobotStatusItems.MovingToChargeStation, (ReapyReturnToChargeStationStates.Instance smi) => smi.gameObject, Db.Get().StatusItemCategories.Main).PlayAnim("react_lobatt", KAnim.PlayMode.Once).OnAnimQueueComplete(this.movingToChargingStation);
            this.idle.ToggleStatusItem(Db.Get().RobotStatusItems.MovingToChargeStation, (ReapyReturnToChargeStationStates.Instance smi) => smi.gameObject, Db.Get().StatusItemCategories.Main).ScheduleGoTo(1f, this.movingToChargingStation);
            this.movingToChargingStation.ToggleStatusItem(Db.Get().RobotStatusItems.MovingToChargeStation, (ReapyReturnToChargeStationStates.Instance smi) => smi.gameObject, Db.Get().StatusItemCategories.Main).MoveTo(delegate (ReapyReturnToChargeStationStates.Instance smi)
            {
                Storage sweepLocker = this.GetSweepLocker(smi);
                if (!(sweepLocker == null))
                {
                    return Grid.PosToCell(sweepLocker);
                }
                return Grid.InvalidCell;
            }, this.chargingstates.waitingForCharging, this.idle, false);
            this.chargingstates.Enter(delegate (ReapyReturnToChargeStationStates.Instance smi)
            {
                smi.master.GetComponent<Facing>().Face(this.GetSweepLocker(smi).gameObject.transform.position + Vector3.right);
                Vector3 position = smi.transform.GetPosition();
                position.z = Grid.GetLayerZ(Grid.SceneLayer.BuildingUse);
                smi.transform.SetPosition(position);
                KBatchedAnimController component = smi.GetComponent<KBatchedAnimController>();
                component.enabled = false;
                component.enabled = true;
            }).Exit(delegate (ReapyReturnToChargeStationStates.Instance smi)
            {
                Vector3 position = smi.transform.GetPosition();
                position.z = Grid.GetLayerZ(Grid.SceneLayer.Creatures);
                smi.transform.SetPosition(position);
                KBatchedAnimController component = smi.GetComponent<KBatchedAnimController>();
                component.enabled = false;
                component.enabled = true;
            }).Enter(delegate (ReapyReturnToChargeStationStates.Instance smi)
            {
                this.Station_DockRobot(smi, true);
            }).Exit(delegate (ReapyReturnToChargeStationStates.Instance smi)
            {
                this.Station_DockRobot(smi, false);
            });
            this.chargingstates.waitingForCharging.PlayAnim("react_base", KAnim.PlayMode.Loop).TagTransition(GameTags.Robots.Behaviours.RechargeBehaviour, this.chargingstates.completed, true).Transition(this.chargingstates.charging, (ReapyReturnToChargeStationStates.Instance smi) => smi.StationReadyToCharge(), UpdateRate.SIM_200ms);
            this.chargingstates.charging.TagTransition(GameTags.Robots.Behaviours.RechargeBehaviour, this.chargingstates.completed, true).Transition(this.chargingstates.interupted, (ReapyReturnToChargeStationStates.Instance smi) => !smi.StationReadyToCharge(), UpdateRate.SIM_200ms).ToggleEffect("Charging").PlayAnim("sleep_pre").QueueAnim("sleep_idle", true, null).Enter(new StateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State.Callback(this.Station_StartCharging)).Exit(new StateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State.Callback(this.Station_StopCharging));
            this.chargingstates.interupted.PlayAnim("sleep_pst").TagTransition(GameTags.Robots.Behaviours.RechargeBehaviour, this.chargingstates.completed, true).OnAnimQueueComplete(this.chargingstates.waitingForCharging);
            this.chargingstates.completed.PlayAnim("sleep_pst").OnAnimQueueComplete(this.behaviourcomplete);
            this.behaviourcomplete.BehaviourComplete(GameTags.Robots.Behaviours.RechargeBehaviour, false);
        }

        public Storage GetSweepLocker(ReapyReturnToChargeStationStates.Instance smi)
        {
            StorageUnloadMonitor.Instance smi2 = smi.master.gameObject.GetSMI<StorageUnloadMonitor.Instance>();
            if (smi2 == null)
            {
                return null;
            }
            return smi2.sm.sweepLocker.Get(smi2);
        }

        public void Station_StartCharging(ReapyReturnToChargeStationStates.Instance smi)
        {
            Storage sweepLocker = this.GetSweepLocker(smi);
            if (sweepLocker != null)
            {
                sweepLocker.GetComponent<ReapBotStation>().StartCharging();
            }
        }

        public void Station_StopCharging(ReapyReturnToChargeStationStates.Instance smi)
        {
            Storage sweepLocker = this.GetSweepLocker(smi);
            if (sweepLocker != null)
            {
                sweepLocker.GetComponent<ReapBotStation>().StopCharging();
            }
        }

        public void Station_DockRobot(ReapyReturnToChargeStationStates.Instance smi, bool dockState)
        {
            Storage sweepLocker = this.GetSweepLocker(smi);
            if (sweepLocker != null)
            {
                sweepLocker.GetComponent<ReapBotStation>().DockRobot(dockState);
            }
        }

        public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State emote;
        public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State idle;
        public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State movingToChargingStation;
        public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State behaviourcomplete;

        public ReapyReturnToChargeStationStates.ChargingStates chargingstates;

        public class Def : StateMachine.BaseDef
        {
        }

        public new class Instance : GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.GameInstance
        {
            public Instance(Chore<ReapyReturnToChargeStationStates.Instance> chore, ReapyReturnToChargeStationStates.Def def) : base(chore, def)
            {
                chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Robots.Behaviours.RechargeBehaviour);
            }

            public bool ChargeAborted()
            {
                return base.smi.sm.GetSweepLocker(base.smi) == null || !base.smi.sm.GetSweepLocker(base.smi).GetComponent<Operational>().IsActive;
            }

            public bool StationReadyToCharge()
            {
                return base.smi.sm.GetSweepLocker(base.smi) != null && base.smi.sm.GetSweepLocker(base.smi).GetComponent<Operational>().IsActive;
            }
        }

        public class ChargingStates : GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State
        {
            public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State waitingForCharging;
            public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State charging;
            public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State interupted;
            public GameStateMachine<ReapyReturnToChargeStationStates, ReapyReturnToChargeStationStates.Instance, IStateMachineTarget, ReapyReturnToChargeStationStates.Def>.State completed;
        }
    }
}