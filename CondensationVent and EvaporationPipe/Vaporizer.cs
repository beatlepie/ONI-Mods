using System;
using KSerialization;
using UnityEngine;

namespace testing
{
    public class Vaporizer : StateMachineComponent<Vaporizer.StatesInstance>
    {
        public void SetStorages(Storage inputStorage, Storage outputStorage)
        {
            this.inputStorage = inputStorage;
            this.outputStorage = outputStorage;
        }

        private bool storageCheck()
        {
            bool flag = this.inputStorage != null;
            bool flag2 = this.outputStorage != null && this.outputStorage.IsFull();
            return flag && !flag2;
        }

        private void MakeIce(Vaporizer.StatesInstance smi, float dt)
        {
            float num = this.heatRemovalRate * dt / (float)this.inputStorage.items.Count;
            foreach (GameObject gameObject in this.inputStorage.items)
            {
                PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
                targetTemperature = component.Element.highTemp + 20f;
                GameUtil.DeltaThermalEnergy(component, -num, smi.master.targetTemperature);
            }
            for (int i = this.inputStorage.items.Count; i > 0; i--)
            {
                GameObject gameObject2 = this.inputStorage.items[i - 1];
                if (gameObject2 && gameObject2.GetComponent<PrimaryElement>().Temperature > gameObject2.GetComponent<PrimaryElement>().Element.highTemp)
                {
                    PrimaryElement component2 = gameObject2.GetComponent<PrimaryElement>();
                    this.inputStorage.AddOre(component2.Element.highTempTransitionTarget, component2.Mass, component2.Temperature, component2.DiseaseIdx, component2.DiseaseCount, false, true);
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

        public float heatRemovalRate = 20f;
        public float targetTemperature;
        private Operational operational;
        public Storage inputStorage;
        public Storage outputStorage;

        public class StatesInstance : GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.GameInstance
        {
            public StatesInstance(Vaporizer smi) : base(smi)
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
                    if (gameObject && gameObject.GetComponent<PrimaryElement>().Temperature >= base.smi.master.gameObject.GetComponent<PrimaryElement>().Element.highTemp)
                    {
                        value = true;
                    }
                }
                base.sm.doneVaporizing.Set(value, this);
            }

            private MeterController meter;
        }

        public class States : GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer>
        {
            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.off;
                base.serializable = true;
                //Changed GameHashes to OnStorageChange from OperationalChanged
                this.off.PlayAnim("liquidconditioner_kanim").EventTransition(GameHashes.OnStorageChange, this.on, (Vaporizer.StatesInstance smi) => smi.master.operational.IsOperational);
                this.on.PlayAnim("liquidconditioner_kanim").EventTransition(GameHashes.OnStorageChange, this.off, (Vaporizer.StatesInstance smi) => !smi.master.operational.IsOperational).DefaultState(this.on.waiting);
                this.on.waiting.EventTransition(GameHashes.OnStorageChange, this.on.working_pre, (Vaporizer.StatesInstance smi) => smi.master.storageCheck());
                this.on.working_pre.Enter(delegate (Vaporizer.StatesInstance smi)
                {
                    smi.UpdateIceState();
                }).PlayAnim("liquidconditioner_kanim").OnAnimQueueComplete(this.on.working);
                this.on.working.QueueAnim("liquidconditioner_kanim", true, null).Update("liquidconditioner_kanim", delegate (Vaporizer.StatesInstance smi, float dt)
                {
                    smi.master.MakeIce(smi, dt);
                }, UpdateRate.SIM_200ms, false).ParamTransition<bool>(this.doneVaporizing, this.on.working_pst, GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.IsTrue).Enter(delegate (Vaporizer.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Exit(delegate (Vaporizer.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                });
                this.on.working_pst.Exit(new StateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.State.Callback(this.DoTransfer)).PlayAnim("liquidconditioner_kanim").OnAnimQueueComplete(this.on);
            }

            private void DoTransfer(Vaporizer.StatesInstance smi)
            {
                for (int i = smi.master.inputStorage.items.Count - 1; i >= 0; i--)
                {
                    GameObject gameObject = smi.master.inputStorage.items[i];
                    if (gameObject && gameObject.GetComponent<PrimaryElement>().Temperature <= gameObject.GetComponent<PrimaryElement>().Element.highTemp + 20f)
                    {
                        smi.master.inputStorage.Transfer(gameObject, smi.master.outputStorage, false, true);
                    }
                }
            }

            public StateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.BoolParameter doneVaporizing;

            public GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.State off;

            public Vaporizer.States.OnStates on;

            public class OnStates : GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.State
            {
                public GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.State waiting;
                public GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.State working_pre;
                public GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.State working;
                public GameStateMachine<Vaporizer.States, Vaporizer.StatesInstance, Vaporizer, object>.State working_pst;
            }
        }
    }
}