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

        public class StatesInstance : GameStateMachine<States, StatesInstance, Vaporizer, object>.GameInstance
        {
            public StatesInstance(Vaporizer smi) : base(smi)
            {
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
        }

        public class States : GameStateMachine<States, StatesInstance, Vaporizer>
        {
            public State working;
            public State output;
            public State off;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = off;

                off
                    .PlayAnim("liquidconditioner_kanim")
                    .EventTransition(GameHashes.OperationalChanged, working, smi => smi.master.operational.IsOperational);

                working
                    .PlayAnim("liquidconditioner_kanim")
                    .Enter(smi => smi.master.operational.SetActive(true))
                    .Exit(smi => smi.master.operational.SetActive(false))
                    .EventTransition(GameHashes.OnStorageChange, off, smi => smi.master.operational.IsOperational)
                    .Update("liquidconditioner_kanim", (smi, dt) => { if (smi.master.storageCheck()) smi.GoTo(off); }, UpdateRate.SIM_200ms);

                //output
                //    .PlayAnim("liquidconditioner_kanim")
                //    .EventTransition()
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
        }
    }
}