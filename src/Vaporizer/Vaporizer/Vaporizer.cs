using UnityEngine;

namespace Vaporiser
{
    public class Vaporizer : StateMachineComponent<Vaporizer.StatesInstance>
    {
        //Initialize all the settings of the storages
        public void SetStorages(Storage inputStorage, Storage outputStorage)
        {
            this.inputStorage = inputStorage;
            this.outputStorage = outputStorage;
        }

        private bool storageCheck()
        {
            //if the [inputStorage] is full of liquid and [outputStorage] is not full, work!
            return this.inputStorage.IsFull() && !this.outputStorage.IsFull();
        }

        private void heat(Vaporizer.StatesInstance smi, float dt)
        {
            //If there is more than 1 
            float num = this.heatRate * dt / (float)this.inputStorage.items.Count;
            foreach (GameObject gameObject in this.inputStorage.items)
            {
                PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
                targetTemperature = component.Element.highTemp + 20f;
                GameUtil.DeltaThermalEnergy(component, num, smi.master.targetTemperature);
            }
            for (int i = this.inputStorage.items.Count; i > 0; i--)
            {
                GameObject gameObject2 = this.inputStorage.items[i - 1];
                //1. check if the object actually exists
                //3. check whether the object is ready to change states or not
                if (gameObject2 && gameObject2.GetComponent<PrimaryElement>().Temperature > gameObject2.GetComponent<PrimaryElement>().Element.highTemp)
                {
                    PrimaryElement component2 = gameObject2.GetComponent<PrimaryElement>();
                    //This will check if the object is polluted water, and produces dirt
                    if (component2.ElementID == SimHashes.DirtyWater)
                    {
                        this.outputStorage.AddOre(SimHashes.Dirt, component2.Mass * 0.0395f, component2.Temperature, component2.DiseaseIdx, component2.DiseaseCount, false, true);
                        this.outputStorage.AddOre(component2.Element.highTempTransitionTarget, component2.Mass * 0.9605f, component2.Temperature, component2.DiseaseIdx, component2.DiseaseCount, false, true);
                    }
                    else
                        //This code changes the element to the gasous form of the element, above if statement checks whether it should be converted or not
                        this.inputStorage.AddOre(component2.Element.highTempTransitionTarget, component2.Mass, component2.Temperature, component2.DiseaseIdx, component2.DiseaseCount, false, true);
                    
                    //This is originally used by [IceMachine] to delete the water, but it kept keeping 0kg masses, so I am replacing it with the 3 lines below
                    //this.inputStorage.ConsumeIgnoringDisease(gameObject2);

                    //Removes the object from the storage
                    inputStorage.items.Remove(gameObject2);
                    //Raise the [GameHashes.OnStorageChange] flag
                    //base.Trigger(-1697596308, gameObject2);
                    //Delete the [gameObject]
                    gameObject2.DeleteObject();
                }
            }
            smi.changeStorage();
        }

        protected override void OnSpawn()
        {
            //lol I have no idea what these do xD
            base.OnSpawn();
            smi.StartSM();
        }

        public float heatRate = 120f;
        public float targetTemperature;
        private Operational operational;
        public Storage inputStorage;
        public Storage outputStorage;


        public class StatesInstance : GameStateMachine<States, StatesInstance, Vaporizer, object>.GameInstance
        {
            public StatesInstance(Vaporizer smi) : base(smi)
            {
                //If this is not done here, [smi.master.operational] is null and all other values in the [Operational] class will return an error in the [States] section
                //Why must this be done here instead of [OnSpawn]??? not exactly sure...
                smi.operational = smi.GetComponent<Operational>();
            }

            public void changeStorage()
            {
                //This deletes 0kg mass objects stored in [outputStorage]
                foreach (GameObject gameObject in base.smi.master.outputStorage.items)
                    if (gameObject.GetComponent<PrimaryElement>().Mass == 0)
                        gameObject.DeleteObject();

                for (int i = base.smi.master.inputStorage.items.Count; i > 0; i--)
                {
                    GameObject gameObject = base.smi.master.inputStorage.items[i - 1];
                    //1. [gameObject] must exist
                    //2. game object must be a gas. This conditional is checked as the next conditional takes the [lowTemp] of the [gameObject], which if it is still liquid, it will take the freezing point instead of the vaporizing temperature!
                    //3. game object's temperature must be 20C higher than [lowTemp], the consensation temperature of the element
                    if (gameObject && gameObject.GetComponent<PrimaryElement>().Element.IsGas && gameObject.GetComponent<PrimaryElement>().Temperature > gameObject.GetComponent<PrimaryElement>().Element.lowTemp + 20f)
                        smi.master.inputStorage.Transfer(gameObject, smi.master.outputStorage, false, true);
                }
            }
        }

        public class States : GameStateMachine<States, StatesInstance, Vaporizer>
        {
            public State working;
            public State off;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = off;

                root
                    //When [GameHashes.OperationalChanged] changes, check [smi]'s [smi.master.operational.IsOperational]
                    //If the value is false, GOTO [.GoTo(off)] state, otherwise, stay.
                    .EventTransition(GameHashes.OperationalChanged, off, smi => !smi.master.operational.IsOperational);

                //This is used to output debug logs
                //.Enter(delegate (Vaporizer.StatesInstance smi) { Debug.Log(smi.master.operational); })

                off
                    //Play this animation. There are ["on"], ["off"], ["working_loop"], ["working_pst"], ["working_pre"]
                    .PlayAnim("off")
                    //The [GameHashes] flag is raised whenever the specified occurs, using that the state can be changed without checking the condition every time.
                    .EventTransition(GameHashes.OnStorageChange, working, smi => smi.master.storageCheck() && smi.master.operational.IsOperational)
                    .EventTransition(GameHashes.OperationalChanged, working, smi => smi.master.storageCheck() && smi.master.operational.IsOperational)
                    //You can use this to [Debug.Log()], since Enter is used to make a function when variables can only be determined during the game state
                    //By passing nothing to it, it does nothing while still printing things to the [output_log] file.
                    .Enter(smi => smi.master.operational.SetActive(false, false));

                working
                    //Queue and loop the animation, not sure what that last part is...
                    .QueueAnim("working_loop", true, null)
                    //[Update] can do what [EventTransition] does as well, but instead of using [GameHashes] as interrupt, it checks every by 200ms default.
                    //[Update(state?????, action or conditional, rate at which action will be executed)]
                    //The lambda expression has 2 predefined values inside the update function, [state machine instance, smi] and [dt, delta time]
                    .Update(delegate (Vaporizer.StatesInstance smi, float dt) { smi.master.heat(smi, dt); smi.changeStorage(); })
                    .Enter(smi => smi.master.operational.SetActive(true, false))
                    //If the [outputStorage] is full OR [inputStorage] is empty, turn off
                    .EventTransition(GameHashes.OnStorageChange, off, smi => !smi.master.storageCheck())
                    .EventTransition(GameHashes.OperationalChanged, off, smi => !smi.master.operational.IsOperational);
            }
        }
    }
}