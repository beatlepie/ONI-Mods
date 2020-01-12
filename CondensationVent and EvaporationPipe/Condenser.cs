using System;
using KSerialization;
using UnityEngine;

namespace testing
{
    public class Condenser : StateMachineComponent<Condenser.StatesInstance>
    {
        //Initialize all the settings of the storages
        public void SetStorages(Storage inputStorage, Storage outputStorage)
        {
            this.inputStorage = inputStorage;
            this.outputStorage = outputStorage;
        }

        private bool storageCheck()
        {
            //if the [inputStorage] has more than or equal to 10kg of liquid and [outputStorage] is not full, work!
            return this.inputStorage.MassStored() >= 10f && !this.outputStorage.IsFull();
        }

        private void heat(Condenser.StatesInstance smi, float dt)
        {
            //If there is more than 1 
            float num = this.heatRemovalRate * dt / (float)this.inputStorage.items.Count;
            foreach (GameObject gameObject in this.inputStorage.items)
            {
                PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
                targetTemperature = component.Element.lowTemp - 5f;
                GameUtil.DeltaThermalEnergy(component, -num, smi.master.targetTemperature);
            }
            for (int i = this.inputStorage.items.Count; i > 0; i--)
            {
                GameObject gameObject2 = this.inputStorage.items[i - 1];
                if (gameObject2 && gameObject2.GetComponent<PrimaryElement>().Temperature < gameObject2.GetComponent<PrimaryElement>().Element.lowTemp)
                {
                    PrimaryElement component2 = gameObject2.GetComponent<PrimaryElement>();
                    //This code changes the element to the gasous form of the element, above if statement checks whether it should be converted or not
                    this.inputStorage.AddOre(component2.Element.highTempTransitionTarget, component2.Mass, component2.Temperature, component2.DiseaseIdx, component2.DiseaseCount, false, true);
                    this.inputStorage.ConsumeIgnoringDisease(gameObject2);
                }
            }
            smi.changeStorage();
        }

        protected override void OnSpawn()
        {
            //lol I have no idea what these do xD
            base.OnSpawn();
            base.smi.StartSM();
        }

        public float heatRemovalRate = 90f;
        public float targetTemperature;
        private Operational operational;
        public Storage inputStorage;
        public Storage outputStorage;

        public class StatesInstance : GameStateMachine<States, StatesInstance, Condenser, object>.GameInstance
        {
            public StatesInstance(Condenser smi) : base(smi)
            {
            }

            public void changeStorage()
            {
                for (int i = base.smi.master.inputStorage.items.Count; i > 0; i--)
                {
                    GameObject gameObject = base.smi.master.inputStorage.items[i - 1];
                    //1. [gameObject] must exist
                    //2. game object must be a gas
                    //3. game object's temperature must be 20C higher than [lowTemp], the consensation temperature of the element
                    if (gameObject && gameObject.GetComponent<PrimaryElement>().Element.IsLiquid && gameObject.GetComponent<PrimaryElement>().Temperature < gameObject.GetComponent<PrimaryElement>().Element.highTemp - 5f)
                    {
                        smi.master.inputStorage.Transfer(gameObject, smi.master.outputStorage, false, true);
                    }
                }

            }
        }

        public class States : GameStateMachine<States, StatesInstance, Condenser>
        {
            public State working;
            public State off;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = off;

                root
                    //When [GameHashes.OperationalChanged] changes, check [smi]'s [smi.master.operational.IsOperational]
                    //If the value is false, GOTO [.GoTo(off)] state, otherwise, stay.
                    .EventTransition(GameHashes.OperationalChanged, off, smi => !smi.master.operational)
                    .Enter(delegate (Condenser.StatesInstance smi) { Debug.Log("ROOT"); });

                off
                    //Play this animation. There are ["on"], ["off"], ["working_loop"], ["working_pst"], ["working_pre"]
                    .PlayAnim("off")
                    //The [GameHashes] flag is raised whenever the specified occurs, using that the state can be changed without checking the condition every time.
                    .EventTransition(GameHashes.OnStorageChange, working, smi => smi.master.storageCheck())
                    //You can use this to [Debug.Log()], since Enter is used to make a function when variables can only be determined during the game state
                    //By passing nothing to it, it does nothing while still printing things to the [output_log] file.
                    .Enter(delegate (Condenser.StatesInstance smi) { Debug.Log(smi.master.storageCheck()); });

                working
                    //Queue and loop the animation, not sure what that last part is...
                    .QueueAnim("working_loop", true, null)
                    //[Update] can do what [EventTransition] does as well, but instead of using [GameHashes] as interrupt, it checks every by 200ms default.
                    //[Update(state?????, action or conditional, rate at which action will be executed)]
                    //The lambda expression has 2 predefined values inside the update function, [state machine instance, smi] and [dt, delta time]
                    .Update(delegate (Condenser.StatesInstance smi, float dt) { smi.master.heat(smi, dt); smi.changeStorage(); })
                    //If the [outputStorage] is full OR [inputStorage] is empty, turn off
                    .EventTransition(GameHashes.OnStorageChange, off, smi => !smi.master.storageCheck())
                    .EventTransition(GameHashes.OperationalChanged, off, smi => smi.master.operational);
            }
        }
    }
}