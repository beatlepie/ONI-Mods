﻿using Klei.AI;
using System;
using UnityEngine;

namespace Reapy
{
    public class ReapStates : GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>
    {
        public override void InitializeStates(out StateMachine.BaseState default_state)
        {
            default_state = beginPatrol;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            beginPatrol.Enter(delegate (ReapStates.Instance smi)
            {
                smi.GoTo(moving);
                ReapStates.Instance smi2 = smi;
                smi2.OnStop = (Action<string, StateMachine.Status>)Delegate.Combine(smi2.OnStop, new Action<string, StateMachine.Status>(delegate (string data, StateMachine.Status status)
                {
                    StopMoveSound(smi);
                }));
            });
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            moving.Enter(delegate (ReapStates.Instance smi)
            {
            }).MoveTo((ReapStates.Instance smi) => GetNextCell(smi), harvest, redirected, false).Update(delegate (ReapStates.Instance smi, float dt)
            {
                StorageUnloadMonitor.Instance smi2 = smi.master.gameObject.GetSMI<StorageUnloadMonitor.Instance>();
                Storage storage = smi2.sm.sweepLocker.Get(smi2);
                if (storage != null && smi.sm.headingRight.Get(smi) == smi.master.transform.position.x > storage.transform.position.x)
                {
                    Navigator component = smi.master.gameObject.GetComponent<Navigator>();
                    if (component.GetNavigationCost(Grid.PosToCell(storage)) >= component.maxProbingRadius - 1)
                    {
                        smi.GoTo(smi.sm.emoteRedirected);
                    }
                }
            }, UpdateRate.SIM_1000ms, false);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            emoteRedirected.Enter(delegate (ReapStates.Instance smi)
            {
                StopMoveSound(smi);
                int cell = Grid.PosToCell(smi.master.gameObject);
                if (Grid.IsCellOffsetValid(cell, headingRight.Get(smi) ? 1 : -1, -1) && !Grid.Solid[Grid.OffsetCell(cell, headingRight.Get(smi) ? 1 : -1, -1)])
                {
                    smi.Play("gap", KAnim.PlayMode.Once);
                }
                else
                {
                    smi.Play("bump", KAnim.PlayMode.Once);
                }
                headingRight.Set(!headingRight.Get(smi), smi);
            }).OnAnimQueueComplete(harvest);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            redirected.StopMoving().GoTo(emoteRedirected);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            harvest.PlayAnim("pickup", KAnim.PlayMode.Loop).ToggleEffect("BotSweeping").Enter(delegate (ReapStates.Instance smi)
            {
                StopMoveSound(smi);
                TryHarvest(smi);
                smi.GoTo(moving);
            });
        }

        public void StopMoveSound(ReapStates.Instance smi)
        {
            LoopingSounds component = smi.gameObject.GetComponent<LoopingSounds>();
            component.StopSound(GlobalAssets.GetSound("SweepBot_mvmt_lp", false));
            component.StopAllSounds();
        }

        public void StartMoveSound(ReapStates.Instance smi)
        {
            LoopingSounds component = smi.gameObject.GetComponent<LoopingSounds>();
            if (!component.IsSoundPlaying(GlobalAssets.GetSound("SweepBot_mvmt_lp", false)))
            {
                component.StartSound(GlobalAssets.GetSound("SweepBot_mvmt_lp", false));
            }
        }

        public void TryHarvest(ReapStates.Instance smi)
        {
            int cell = Grid.PosToCell(smi);
            // [Grid.Objects[cell, 5]] is the [Plants] layer!
            GameObject gameObject = Grid.Objects[cell, 5];

            if (gameObject != null && gameObject.GetComponent<Harvestable>().CanBeHarvested)
                // This will drop the produce after harvest, so [TrySweep] the produce after harvest!
                gameObject.GetComponent<Harvestable>().Harvest();
            TrySweep(smi);
        }

        public void TrySweep(ReapStates.Instance smi)
        {
            int cell = Grid.PosToCell(smi);
            // [Grid.Objects[cell, 3]] is the [Pickupables] layer!
            GameObject gameObject = Grid.Objects[cell, 3];
            if (gameObject != null)
            {
                ObjectLayerListItem nextItem = gameObject.GetComponent<Pickupable>().objectLayerListItem.nextItem;
                Debug.Log(nextItem.gameObject.name);
                if(nextItem.gameObject.HasTag(GameTags.CookingIngredient) || nextItem.gameObject.name == "SwampLilyFlower")
                    TryStore(nextItem.gameObject, smi);
            }
        }

        public bool TryStore(GameObject go, ReapStates.Instance smi)
        {
            Pickupable pickupable = go.GetComponent<Pickupable>();
            if (pickupable == null)
            {
                return false;
            }
            Storage storage = smi.master.gameObject.GetComponents<Storage>()[1];
            if (storage.IsFull())
            {
                return false;
            }
            if (pickupable != null && pickupable.absorbable)
            {
                SingleEntityReceptacle component = smi.master.GetComponent<SingleEntityReceptacle>();
                if (pickupable.gameObject == component.Occupant)
                {
                    return false;
                }
                bool flag;
                if (pickupable.TotalAmount > 10f)
                {
                    pickupable.GetComponent<EntitySplitter>();
                    pickupable = EntitySplitter.Split(pickupable, Mathf.Min(10f, storage.RemainingCapacity()), null);
                    smi.gameObject.GetAmounts().GetValue(Db.Get().Amounts.InternalBattery.Id);
                    storage.Store(pickupable.gameObject, false, false, true, false);
                    flag = true;
                }
                else
                {
                    smi.gameObject.GetAmounts().GetValue(Db.Get().Amounts.InternalBattery.Id);
                    storage.Store(pickupable.gameObject, false, false, true, false);
                    flag = true;
                }
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetNextCell(ReapStates.Instance smi)
        {
            int i = 0;
            int num = Grid.PosToCell(smi);
            int num2 = Grid.InvalidCell;
            if (!Grid.Solid[Grid.CellBelow(num)])
            {
                return Grid.InvalidCell;
            }
            if (Grid.Solid[num])
            {
                return Grid.InvalidCell;
            }
            while (i < 1)
            {
                num2 = (smi.sm.headingRight.Get(smi) ? Grid.CellRight(num) : Grid.CellLeft(num));
                if (!Grid.IsValidCell(num2) || Grid.Solid[num2] || !Grid.IsValidCell(Grid.CellBelow(num2)) || !Grid.Solid[Grid.CellBelow(num2)])
                {
                    break;
                }
                num = num2;
                i++;
            }
            if (num == Grid.PosToCell(smi))
            {
                return Grid.InvalidCell;
            }
            return num;
        }

        public const string MOVE_LOOP_SOUND = "SweepBot_mvmt_lp";

        public StateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.BoolParameter headingRight;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State beginPatrol;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State moving;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State redirected;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State emoteRedirected;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State harvest;
        public class Def : StateMachine.BaseDef
        {
        }

        public new class Instance : GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.GameInstance
        {
            public Instance(Chore<ReapStates.Instance> chore, ReapStates.Def def) : base(chore, def)
            {
            }

            public override void StartSM()
            {
                base.StartSM();
                base.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().RobotStatusItems.Working, base.gameObject);
            }

            protected override void OnCleanUp()
            {
                base.OnCleanUp();
                base.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().RobotStatusItems.Working, false);
            }
        }

    }
}
