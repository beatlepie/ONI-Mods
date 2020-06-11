using TUNING;
using UnityEngine;

namespace Condenser
{
    public class CondenserConfig : IBuildingConfig
    {
        public static void Setup()
        {
            AddBuilding.AddStrings("Condenser", "Condenser", "Condenses its contents! Operates with 30kg of gas, cools it until 5 degree below the condensation temperature. It heats itself by 16kDTU while it is running.", "Condenses its contents.");
            AddBuilding.AddBuildingToPlanScreen("Utilities", "Condenser", "ThermalBlock");
            AddBuilding.IntoTechTree("TemperatureModulation", "Condenser");
        }

        //putting [f] at the end of the number makes the computer treat the number as [float] instead of defaulting to [double]
        public override BuildingDef CreateBuildingDef()
        {
            string id = "Condenser";
            int width = 2, height = 2;
            string anim = "liquidconditioner_kanim";
            int hitpoints = 10;
            float construction_time = 120f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER6; //1200kg
            string[] ALL_METALS = MATERIALS.ALL_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tier2 = BUILDINGS.DECOR.PENALTY.TIER0;
            EffectorValues noise = NOISE_POLLUTION.NOISY.TIER2;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, ALL_METALS, melting_point, build_location_rule, tier2, noise, 0.2f);
            BuildingTemplates.CreateElectricalBuildingDef(buildingDef);

            buildingDef.Floodable = false;
            buildingDef.Overheatable = true;
            buildingDef.Entombable = true;
            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
            buildingDef.PermittedRotations = PermittedRotations.FlipH;
            buildingDef.OverheatTemperature = 398.15f;
            buildingDef.EnergyConsumptionWhenActive = 1200f;
            buildingDef.SelfHeatKilowattsWhenActive = 96f;
            buildingDef.ExhaustKilowattsWhenActive = 24f;

            //Layer the building is stored in, if it is [GasConduit], [GasConduit] objects cannot be place over them. Apparently unnecessasry...
            //buildingDef.ObjectLayer = ObjectLayer.Backwall;
            buildingDef.AudioCategory = "Metal";
            buildingDef.BaseTimeUntilRepair = -20f;
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(1, 0);
            buildingDef.PowerInputOffset = new CellOffset(1, 0);
            buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(1, 1));
            buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
            //adds the output sprite on the building!
            buildingDef.InputConduitType = ConduitType.Gas;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();

            Storage inputStorage = go.AddOrGet<Storage>();
            inputStorage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
            inputStorage.showInUI = true;
            //seems like this value is for maximum the storage can hold...not take in...to change how much it can take in, change the [conduitConsumer.capacityKG]...
            inputStorage.capacityKg = 30f;

            Storage outputStorage = go.AddComponent<Storage>();
            outputStorage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
            outputStorage.showInUI = true;
            //???????????????????????????????????????????????
            //This seems to be ignored...
            outputStorage.capacityKg = 30f;

            Condenser condenser = go.AddOrGet<Condenser>();
            condenser.SetStorages(inputStorage, outputStorage);

            //Required for buildings with input
            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            BuildingDef def = go.GetComponent<Building>().Def;
            conduitConsumer.conduitType = ConduitType.Gas;
            conduitConsumer.storage = inputStorage;
            conduitConsumer.consumptionRate = 10f;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.capacityKG = 30f;

            //Without this, there wil only be a output image, will not actually output things
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.storage = outputStorage;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go) {  }

        public override void DoPostConfigureUnderConstruction(GameObject go) {  }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}