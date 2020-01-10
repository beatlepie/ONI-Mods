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
            //if the [inputStorage] has something, and [outputStorage] is not full, work!
            return this.inputStorage.items.Count != 0 && !this.outputStorage.IsFull();
        }

        private void heat(Vaporizer.StatesInstance smi, float dt)
        {
            //If there is more than 1 
            float num = this.heatRemovalRate * dt / (float)this.inputStorage.items.Count;
            foreach (GameObject gameObject in this.inputStorage.items)
            {
                PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
                targetTemperature = component.Element.highTemp + 20f;
                GameUtil.DeltaThermalEnergy(component, num, smi.master.targetTemperature);
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
            smi.UpdateState();
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

            public void UpdateState()
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
            public State working_past;
            public State output;
            public State off;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = off;
               
                root
                    //When [GameHashes.OperationalChanged] changes, check [smi]'s [smi.master.operational.IsOperational]
                    //If the value is false, GOTO [.GoTo(off)] state, otherwise, stay.
                    .EventTransition(GameHashes.OperationalChanged, off, smi => !smi.master.operational)
                    .Enter(delegate (Vaporizer.StatesInstance smi) { Debug.Log("ROOT"); });

                off
                    //Play this animation. There are ["on"], ["off"], ["working_loop"], ["working_pst"], ["working_pre"]
                    .PlayAnim("off")
                    //The [GameHashes] flag is raised whenever the specified occurs, using that the state can be changed without checking the condition every time.
                    .EventTransition(GameHashes.OnStorageChange, working, smi => smi.master.storageCheck())
                    //You can use this to [Debug.Log()], since Enter is used to make a function when variables can only be determined during the game state
                    //By passing nothing to it, it does nothing while still printing things to the [output_log] file.
                    .Enter(delegate (Vaporizer.StatesInstance smi) { Debug.Log(smi.master.storageCheck()); });

                working
                    //Queue and loop the animation, not sure what that last part is...
                    .QueueAnim("working_loop", true, null)
                    .Update(delegate (Vaporizer.StatesInstance smi, float dt) { smi.master.heat(smi, dt);  smi.UpdateState(); })
                    //[Update] can do what [EventTransition] does as well, but instead of using [GameHashes] as interrupt, it checks every by 200ms default.
                    //[Update(state?????, action or conditional, rate at which action will be executed)]
                    .Update("working_loop", (smi, dt) => { if (!smi.master.storageCheck()) smi.GoTo(off); })
                    .Enter(delegate (Vaporizer.StatesInstance smi) { Debug.Log(smi.master.storageCheck()); });

                output
                    .PlayAnim("working_pst")
                    .EventTransition(GameHashes.OnStorageChange, off, smi => smi.master.storageCheck());

                working_past
                    .PlayAnim("off")
                    .OnAnimQueueComplete(off);
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