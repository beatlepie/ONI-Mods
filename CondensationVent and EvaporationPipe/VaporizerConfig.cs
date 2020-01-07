using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace testing
{
    public class VaporizerConfig : IBuildingConfig
    {
        public static void Setup()
        {
            AddBuilding.AddStrings("Vaporizer", "Vaporizer", "Allows liquid contents to vaporize!", "Allows liquid to vaporize!!!");
            AddBuilding.AddBuildingToPlanScreen("Utilities", "Vaporizer", "ThermalBlock");
            AddBuilding.IntoTechTree("TemperatureModulation", "Vaporizer");
        }

        //putting [f] at the end of the number makes the computer treat the number as [float] instead of defaulting to [double]
        public override BuildingDef CreateBuildingDef()
        {
            string id = "Vaporizer";
            int width = 2;
            int height = 2;
            string anim = "liquidconditioner_kanim";
            int hitpoints = 10;
            float construction_time = 120f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER6; //1200kg
            string[] REFINED_METALS = MATERIALS.ALL_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor; 
            EffectorValues tier2 = BUILDINGS.DECOR.PENALTY.TIER0;
            EffectorValues noise = NOISE_POLLUTION.NOISY.TIER2;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, REFINED_METALS, melting_point, build_location_rule, tier2, noise, 0.2f);
            BuildingTemplates.CreateElectricalBuildingDef(buildingDef);
            buildingDef.EnergyConsumptionWhenActive = 1200f;
            buildingDef.SelfHeatKilowattsWhenActive = 0f;
            buildingDef.OverheatTemperature = 398.15f;
            buildingDef.PermittedRotations = PermittedRotations.FlipH;

            //what types of input and outputs the building has
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Gas;
            //buildingDef.Floodable = true;
            buildingDef.Overheatable = false;
            buildingDef.Entombable = true;

            //Which [OverlayModes] the building will be shown with color.
            //buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;

            //Layer the building is stored in, if it is [GasConduit], [GasConduit] objects cannot be place over them.
            //Nothing is default object layer
            //buildingDef.ObjectLayer = ObjectLayer.;

            //wtf does this do lul
            //buildingDef.TileLayer = ObjectLayer.Backwall;

            buildingDef.BaseTimeUntilRepair = -1f;
            //Location of input and output
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            buildingDef.PowerInputOffset = new CellOffset(1, 0);

            //Layer the building will be displayed in
            buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();

            //Required for buildings with input
            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            BuildingDef def = go.GetComponent<Building>().Def;
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.capacityTag = GameTags.Liquid;
            conduitConsumer.consumptionRate = 10f;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.capacityKG = 10f;

            //Without this, there wil only be a output image, will not actually output things
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Gas;

            Storage inputStorage = BuildingTemplates.CreateDefaultStorage(go, false);
            inputStorage.showInUI = true;
            inputStorage.capacityKg = 10f;

            Storage outputStorage = BuildingTemplates.CreateDefaultStorage(go, false);
            outputStorage.showInUI = true;
            outputStorage.capacityKg = 1000f;

            Vaporizer vaporizer = go.AddOrGet<Vaporizer>();
            vaporizer.SetStorages(inputStorage, outputStorage);
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
        public const string ID = "Vaporizer";
    }
}