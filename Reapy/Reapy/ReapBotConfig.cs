using Klei.AI;
using System.Collections.Generic;
using UnityEngine;

namespace Reapy
{
    public class ReapBotConfig : IEntityConfig
    {
        /// <summary>
        /// Defining values of the object
        /// </summary>
        string id = "ReapBot";
        public static string name = "Reapy";
        string desc = "An automated reaping/harvesting robot. \n\nReap/Harvest fully grown crops and takes the produce. As this is designed for automation than cultivation, it will not produce seeds!";
        public static float mass = 25f;
        EffectorValues none = TUNING.BUILDINGS.DECOR.NONE;

        public GameObject CreatePrefab()
        {
            // This is an entity!
            GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, mass, Assets.GetAnim("sweep_bot_kanim"), "idle", Grid.SceneLayer.Creatures, 1, 1, none, default(EffectorValues), SimHashes.Creature, null, 293f);
            // Used for anything that requires looping sound
            gameObject.AddOrGet<LoopingSounds>();
            // Used for anything that requires animation of the object
            gameObject.GetComponent<KBatchedAnimController>().isMovable = true;
            // Used to identify and differentiate the object...?
            KPrefabID kprefabID = gameObject.AddOrGet<KPrefabID>();
            kprefabID.AddTag(GameTags.Creature, false);
            // This is required as Reapy may get stuck, allows dupes to pick it up
            gameObject.AddComponent<Pickupable>();
            // Prevents Chores from getting removed on reset...?
            gameObject.AddOrGet<Clearable>().isClearable = false;

            // Creating a trait just for reapy, same as sweepy
            Trait trait = Db.Get().CreateTrait("ReapBotBaseTrait", name, name, null, false, null, true, true);
            trait.Add(new AttributeModifier(Db.Get().Amounts.InternalBattery.maxAttribute.Id, 9000f, name, false, false, true));
            trait.Add(new AttributeModifier(Db.Get().Amounts.InternalBattery.deltaAttribute.Id, -17.1428566f, name, false, false, true));

            Modifiers modifiers = gameObject.AddOrGet<Modifiers>();
            modifiers.initialTraits.Add("ReapBotBaseTrait");
            modifiers.initialAmounts.Add(Db.Get().Amounts.HitPoints.Id);
            modifiers.initialAmounts.Add(Db.Get().Amounts.InternalBattery.Id);

            gameObject.AddOrGet<KBatchedAnimController>().SetSymbolVisiblity("snapto_pivot", false);
            gameObject.AddOrGet<Traits>();
            gameObject.AddOrGet<Effects>();
            gameObject.AddOrGetDef<AnimInterruptMonitor.Def>();
            gameObject.AddOrGetDef<StorageUnloadMonitor.Def>();

            RobotBatteryMonitor.Def def = gameObject.AddOrGetDef<RobotBatteryMonitor.Def>();
            def.batteryAmountId = Db.Get().Amounts.InternalBattery.Id;
            def.canCharge = true;
            def.lowBatteryWarningPercent = 0.5f;

            // Reapy will not react when a dupe is nearby
            //gameObject.AddOrGetDef<SweetBotReactMonitor.Def>();
            gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
            gameObject.AddOrGetDef<SweepBotTrappedMonitor.Def>();
            gameObject.AddOrGet<AnimEventHandler>();

            // This seems to be the location where the item needs to go!
            // when this is left as it is, it seems to cause a "red dot" on reapy
            //gameObject.AddOrGet<SnapOn>().snapPoints = new List<SnapOn.SnapPoint>(new SnapOn.SnapPoint[]
            //{
            //new SnapOn.SnapPoint
            //{
            //    pointName = "carry",
            //    automatic = false,
            //    context = "",
            //    buildFile = null,
            //    overrideSymbol = "snapTo_ornament"
            //}
            //});

            SymbolOverrideControllerUtil.AddToPrefab(gameObject);
            gameObject.AddComponent<Storage>();
            gameObject.AddComponent<Storage>().capacityKg = 500f;

            // Existed for sweepy, shouldn't for reapy...?
            //gameObject.AddOrGet<OrnamentReceptacle>().AddDepositTag(GameTags.PedestalDisplayable);
            //gameObject.AddOrGet<DecorProvider>();
            //gameObject.AddOrGet<ItemPedestal>();

            gameObject.AddOrGet<UserNameable>();
            gameObject.AddOrGet<CharacterOverlay>();
            Navigator navigator = gameObject.AddOrGet<Navigator>();
            navigator.NavGridName = "ReapyNavGrid";
            navigator.CurrentNavType = NavType.Floor;
            navigator.defaultSpeed = 1f;
            navigator.updateProber = true;
            navigator.maxProbingRadius = 32;
            navigator.sceneLayer = Grid.SceneLayer.Creatures;
            kprefabID.AddTag(GameTags.Creatures.Walker, false);
            // Did not finish this yet! issue
            ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new FallStates.Def(), true, -1).Add(new AnimInterruptStates.Def(), true, -1).Add(new SweepBotTrappedStates.Def(), true, -1).Add(new DeliverToSweepLockerStates.Def(), true, -1).Add(new ReapyReturnToChargeStationStates.Def(), true, -1).Add(new ReapStates.Def(), true, -1).Add(new IdleStates.Def(), true, -1);
            gameObject.AddOrGet<LoopingSounds>();
            EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Robots.Models.SweepBot, null);
            return gameObject;
        }

        /// <summary>
        /// Required for [IEntityConfig], allows its use on all DLC
        /// </summary>
        public string[] GetDlcIds()
        {
            return DlcManager.AVAILABLE_ALL_VERSIONS;
        }

        public void OnPrefabInit(GameObject inst)
        {
        }

        public void OnSpawn(GameObject inst)
        {
            StorageUnloadMonitor.Instance smi = inst.GetSMI<StorageUnloadMonitor.Instance>();
            smi.sm.internalStorage.Set(inst.GetComponents<Storage>()[1], smi);
            inst.GetSMI<CreatureFallMonitor.Instance>().anim = "idle_loop";

            // Get its naviator and add a transition layer, this allows them to go through doors
            if (inst != null)
            {
                Navigator navigator = inst.AddOrGet<Navigator>();
                navigator.transitionDriver.overrideLayers.Add(new DoorTransitionLayer(navigator));
            }

            inst.GetComponent<KAnimControllerBase>().TintColour = new Color32(50, 250, 50, byte.MaxValue);
        }
    }
}
