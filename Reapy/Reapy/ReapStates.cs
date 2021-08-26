using Klei.AI;
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
            // Seems like without the [Enter] it stops updating things such as [Navigator]...
            moving.Enter(delegate (ReapStates.Instance smi) { }).MoveTo((ReapStates.Instance smi) => GetNextCell(smi), pause, redirected, false).Update(delegate (ReapStates.Instance smi, float dt)
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

            //TRY:
            //.Enter(delegate (ReapStates.Instance smi){component.GoTo(GetNextCell(smi))})....not sure if this will work though...
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
            }).OnAnimQueueComplete(pause);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            redirected.StopMoving().GoTo(emoteRedirected);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            harvest1.PlayAnim("mop_pre", KAnim.PlayMode.Once).QueueAnim("mop_loop").QueueAnim("mop_loop").QueueAnim("mop_loop").QueueAnim("mop_loop").Enter(delegate (ReapStates.Instance smi)
            {
                // This is made to loop the sweep animation 4 times!
                // Above was done to "match" the harvesting duration of dupes!
                StopMoveSound(smi);
            }).OnAnimQueueComplete(harvest2);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            harvest2.Enter(delegate (ReapStates.Instance smi)
            {                
                // checking for [target != null] because if a dupe already harvested muckroot or something it no longer exists
                // Adding this will hopefully prevent issue where the plant gets harvested beforehand...
                if (target != null && target.CanBeHarvested)
                    target.Harvest();
                smi.GoTo(moving);
            });
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            sweep.PlayAnim("pickup").ToggleEffect("BotSweeping").Enter(delegate (ReapStates.Instance smi)
            {
                StopMoveSound(smi);
            }).OnAnimQueueComplete(moving);
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            pause.Enter(delegate (ReapStates.Instance smi)
            {
                if (TryHarvest(smi))
                {
                    smi.GoTo(harvest1);
                    return;
                }
                if (TrySweep(smi))
                {
                    smi.GoTo(sweep);
                    return;
                }
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

        public bool TryHarvest(ReapStates.Instance smi)
        {
            int cell = Grid.PosToCell(smi);

            // Found this piece from [HarvestTool.OnDragTool]!
            // This works...but seems too extensive to find one plant...
            foreach (Harvestable harvestable in Components.Harvestables.Items)
            {
                // This will check current location, and one & two cells above BY DEFAULT for Arbor Trees or Pincha plants!
                // This checks whether the plant is fully grown or not!
                if (harvestable.CanBeHarvested)
                {
                    for(int _ = 0; _ < ReapyOptions.Options.harvestRange; _++)
                    {
                        if(Grid.PosToCell(harvestable) == cell)
                        {
                            target = harvestable;
                            return true;
                        }

                        // Check one cell above until harvest range is reached!
                        cell = Grid.CellAbove(cell);
                        // If the above block is solid, stop checking!
                        if (Grid.Solid[cell])
                            break;
                    }
                }
            }
            return false;
        }

        public bool TrySweep(ReapStates.Instance smi)
        {
            int cell = Grid.PosToCell(smi);
            // [Grid.Objects[cell, 3]] is the [Pickupables] layer!
            GameObject gameObject = Grid.Objects[cell, 3];

            if (gameObject != null)
            {
                ObjectLayerListItem nextItem = gameObject.GetComponent<Pickupable>().objectLayerListItem.nextItem;
                //Debug.Log("HERE");
                while (nextItem != null)
                {
                    // Apparently this can also detect liquids and dupes...
                    //Debug.Log(nextItem.gameObject.GetComponent<KSelectable>().name);
                    if (nextItem.gameObject.HasAnyTags(new Tag[] { GameTags.CookingIngredient, GameTags.IndustrialIngredient, GameTags.Edible }))
                    {
                        TryStore(nextItem.gameObject, smi);
                        return true;
                    }
                    nextItem = nextItem.nextItem;
                }
            }
            return false;
        }

        public void TryStore(GameObject go, ReapStates.Instance smi)
        {
            Pickupable pickupable = go.GetComponent<Pickupable>();
            if (pickupable == null)
                return;

            Storage storage = smi.master.gameObject.GetComponents<Storage>()[1];
            if (storage.IsFull())
                return;

            if (pickupable != null && pickupable.absorbable)
            {
                // [SingleEntityReceptacle] returned null, removing for now, will see what happens! : )
                //SingleEntityReceptacle component = smi.master.GetComponent<SingleEntityReceptacle>();
                //Debug.Log(component == null);
                //if (pickupable.gameObject == component.Occupant)
                //    return;

                // [go.GetAmounts()] does not exist!
                // [go.GetProperName()] or [go.name] does not exist!

                // Removing pick-up limit from 10units to as much as the storage allows!
                pickupable.GetComponent<EntitySplitter>();
                pickupable = EntitySplitter.Split(pickupable, Mathf.Min(pickupable.TotalAmount, storage.RemainingCapacity()), null);
                smi.gameObject.GetAmounts().GetValue(Db.Get().Amounts.InternalBattery.Id);
                storage.Store(pickupable.gameObject, false, false, true, false);
            }
        }

        public int GetNextCell(ReapStates.Instance smi)
        {
            int currentCell = Grid.PosToCell(smi);
            int result;

            // 1. If the groud is not solid, INVALID (1 and 2 used De Morgan's laws)
            // 2. If a door is located on the cell, NOT INVALID
            // 3. If it is entomed, INVALID
            if (!Grid.Solid[Grid.CellBelow(currentCell)] || (!Grid.HasDoor[currentCell] && Grid.Solid[currentCell]))
                result = Grid.InvalidCell;
            else
            {
                // find the next cell to traverse, based on which direction it was going
                int nextCell = smi.sm.headingRight.Get(smi) ? Grid.CellRight(currentCell) : Grid.CellLeft(currentCell);
                // if the next cell is traversable, return [nextCell], otherwise, check next of [nextCell]
                result = canMoveToCell(nextCell) ? nextCell : Grid.InvalidCell;
                // This code was used to check if reapy can jump one tile, but will not be used
                //if (result == Grid.InvalidCell)
                //{
                //    nextCell = smi.sm.headingRight.Get(smi) ? Grid.CellRight(nextCell) : Grid.CellLeft(nextCell);
                //    result = canMoveToCell(nextCell) ? nextCell : Grid.InvalidCell;
                //}
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
                result = Grid.Solid[cellBelowNext];
            return result;
        }

        public const string MOVE_LOOP_SOUND = "SweepBot_mvmt_lp";

        public StateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.BoolParameter headingRight;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State beginPatrol;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State moving;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State redirected;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State emoteRedirected;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State harvest1;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State harvest2;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State sweep;
        private GameStateMachine<ReapStates, ReapStates.Instance, IStateMachineTarget, ReapStates.Def>.State pause;

        private Harvestable target = null;

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
