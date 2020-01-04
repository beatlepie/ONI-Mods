using System;
using KSerialization;
using UnityEngine;
/*
//THIS CODE WAS COPIED FROM [IceMachine] file and changed for this MOD.
public class EvaporationPipe : Storage
{
    public void SetStorages(Storage liquid, Storage output)
    {
        this.inputStorage = liquid;
        this.outputStorage = output;
    }

    private void Vaporize()
    {
        for (int i = this.inputStorage.items.Count; i > 0; i--)
        {
            GameObject gameObject = this.inputStorage.items[i - 1];
            if (gameObject && gameObject.GetComponent<PrimaryElement>().Temperature > 90)//gameObject.GetComponent<PrimaryElement>().Element.highTemp)
            {
                PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
                this.inputStorage.AddOre(component.Element.highTempTransitionTarget, component.Mass, component.Temperature, component.DiseaseIdx, component.DiseaseCount, false, true);
                this.inputStorage.ConsumeIgnoringDisease(gameObject);
            }
        }
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        this.Vaporize();
        this.DoTransfer();
    }

    //private Operational operational;
    public Storage inputStorage;
    public Storage outputStorage;

    private void DoTransfer()
        {
            for (int i = this.inputStorage.items.Count - 1; i >= 0; i--)
            {
                GameObject gameObject = this.inputStorage.items[i];
                if (gameObject && gameObject.GetComponent<PrimaryElement>().Temperature >= gameObject.GetComponent<PrimaryElement>().Element.highTemp)
                {
                    this.inputStorage.Transfer(gameObject, this.outputStorage, false, true);
                }
            }
        }
}
*/

namespace testing
{
    public class EvaporationPipe : StateMachineComponent<EvaporationPipe.StatesInstance>
    {
        public void SetStorages(Storage inputStorage, Storage outputStorage)
        {
            this.inputStorage = inputStorage;
            this.outputStorage = outputStorage;
        }

        private bool CanMakeIce()
        {
            return true;
        }

        private void MakeIce(EvaporationPipe.StatesInstance smi, float dt)
        {
            for (int i = this.inputStorage.items.Count; i > 0; i--)
            {
                GameObject gameObject2 = this.inputStorage.items[i - 1];
                if (gameObject2 && gameObject2.GetComponent<PrimaryElement>().Temperature > gameObject2.GetComponent<PrimaryElement>().Element.highTemp)
                {
                    PrimaryElement component2 = gameObject2.GetComponent<PrimaryElement>();
                    this.inputStorage.AddOre(component2.Element.lowTempTransitionTarget, component2.Mass, component2.Temperature, component2.DiseaseIdx, component2.DiseaseCount, false, true);
                    this.inputStorage.ConsumeIgnoringDisease(gameObject2);
                }
            }
            smi.UpdateIceState();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            base.smi.StartSM();
        }

        private Operational operational;

        public Storage inputStorage;
        public Storage outputStorage;
        private static StatusItem outputStorageFullStatusItem;

        public class StatesInstance : GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.GameInstance
        {
            public StatesInstance(EvaporationPipe smi) : base(smi)
            {
                this.meter = new MeterController(base.gameObject.GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, new string[]
                {
                "meter_OL",
                "meter_frame",
                "meter_fill"
                });
                this.UpdateMeter();
                base.Subscribe(-1697596308, new Action<object>(this.OnStorageChange));
            }

            private void OnStorageChange(object data)
            {
                this.UpdateMeter();
            }

            public void UpdateMeter()
            {
                this.meter.SetPositionPercent(Mathf.Clamp01(base.smi.master.outputStorage.MassStored() / base.smi.master.outputStorage.Capacity()));
            }

            public void UpdateIceState()
            {
                bool value = false;
                for (int i = base.smi.master.inputStorage.items.Count; i > 0; i--)
                {
                    GameObject gameObject = base.smi.master.inputStorage.items[i - 1];
                    if (gameObject && gameObject.GetComponent<PrimaryElement>().Temperature <= base.smi.master.gameObject.GetComponent<PrimaryElement>().Element.highTemp)
                    {
                        value = true;
                    }
                }
                base.sm.doneFreezingIce.Set(value, this);
            }

            private MeterController meter;

            public Chore emptyChore;
        }

        public class States : GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe>
        {
            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.off;
                base.serializable = true;
                this.off.PlayAnim("off").EventTransition(GameHashes.OperationalChanged, this.on, (EvaporationPipe.StatesInstance smi) => smi.master.operational.IsOperational);
                this.on.PlayAnim("on").EventTransition(GameHashes.OperationalChanged, this.off, (EvaporationPipe.StatesInstance smi) => !smi.master.operational.IsOperational).DefaultState(this.on.waiting);
                this.on.waiting.EventTransition(GameHashes.OnStorageChange, this.on.working_pre, (EvaporationPipe.StatesInstance smi) => smi.master.CanMakeIce());
                this.on.working_pre.Enter(delegate (EvaporationPipe.StatesInstance smi)
                {
                    smi.UpdateIceState();
                }).PlayAnim("working_pre").OnAnimQueueComplete(this.on.working);
                this.on.working.QueueAnim("working_loop", true, null).Update("UpdateWorking", delegate (EvaporationPipe.StatesInstance smi, float dt)
                {
                    smi.master.MakeIce(smi, dt);
                }, UpdateRate.SIM_200ms, false).ParamTransition<bool>(this.doneFreezingIce, this.on.working_pst, GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.IsTrue).Enter(delegate (EvaporationPipe.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                    smi.master.gameObject.GetComponent<ManualDeliveryKG>().Pause(true, "Working");
                }).Exit(delegate (EvaporationPipe.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                    smi.master.gameObject.GetComponent<ManualDeliveryKG>().Pause(false, "Done Working");
                });
                this.on.working_pst.Exit(new StateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.State.Callback(this.DoTransfer)).PlayAnim("working_pst").OnAnimQueueComplete(this.on);
            }

            private void DoTransfer(EvaporationPipe.StatesInstance smi)
            {
                for (int i = smi.master.inputStorage.items.Count - 1; i >= 0; i--)
                {
                    GameObject gameObject = smi.master.inputStorage.items[i];
                    if (gameObject && gameObject.GetComponent<PrimaryElement>().Temperature >= smi.master.gameObject.GetComponent<PrimaryElement>().Element.highTemp)
                    {
                        smi.master.inputStorage.Transfer(gameObject, smi.master.outputStorage, false, true);
                    }
                }
                smi.UpdateMeter();
            }

            public StateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.BoolParameter doneFreezingIce;

            public GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.State off;

            public EvaporationPipe.States.OnStates on;

            public class OnStates : GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.State
            {
                public GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.State waiting;

                public GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.State working_pre;

                public GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.State working;

                public GameStateMachine<EvaporationPipe.States, EvaporationPipe.StatesInstance, EvaporationPipe, object>.State working_pst;
            }
        }
    }
}