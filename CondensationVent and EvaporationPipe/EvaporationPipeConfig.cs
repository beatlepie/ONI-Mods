using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace testing
{
    public class EvaporationPipeConfig : IBuildingConfig
    {
        public static void Setup()
        {
            AddBuilding.AddStrings("EvaporationPipe", "Evaporation Pipe", "Allows liquids to evaporate out of the pipe!", "Allows liquid to evaporate out of the pipe!!!");
            AddBuilding.AddBuildingToPlanScreen("Utilities", "EvaporationPipe", "ThermalBlock");
            AddBuilding.IntoTechTree("TemperatureModulation", "EvaporationPipe");
        }

        //putting [f] at the end of the number makes the computer treat the number as [float] instead of defaulting to [double]
        public override BuildingDef CreateBuildingDef()
        {
            string id = "EvaporationPipe";
            int width = 1, height = 1;
            string anim = "ventliquid_kanim";
            int hitpoints = 10;
            float construction_time = 10f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2; //100kg
            string[] REFINED_METALS = MATERIALS.REFINED_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.NotInTiles; //Anywhere that is not blocked by tiles
            EffectorValues tier2 = BUILDINGS.DECOR.PENALTY.TIER0;
            EffectorValues tier3 = NOISE_POLLUTION.NOISY.TIER0;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, REFINED_METALS, melting_point, build_location_rule, tier2, tier3, 0.2f);
            buildingDef.ThermalConductivity = 2f;
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

            buildingDef.AudioCategory = "Metal";
            buildingDef.BaseTimeUntilRepair = -1f;
            //Location of input and output
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            //Layer the building will be displayed in
            buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            AnimTileable animTileable = go.AddOrGet<AnimTileable>();
            animTileable.objectLayer = ObjectLayer.Backwall;
            go.AddComponent<ZoneTile>();
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);

            //Required for buildings with input
            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            BuildingDef def = go.GetComponent<Building>().Def;
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.capacityTag = GameTags.Liquid;
            conduitConsumer.consumptionRate = 10000f;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.ignoreMinMassCheck = true;
            conduitConsumer.capacityKG = 10f;

            //Without this, there wil only be a output image, will not actually output things
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Gas;

            Storage inputStorage = BuildingTemplates.CreateDefaultStorage(go, false);
            inputStorage.showInUI = true;
            inputStorage.capacityKg = 10f;

            Storage outputStorage = BuildingTemplates.CreateDefaultStorage(go, false);
            outputStorage.showInUI = true;

            EvaporationPipe evaporationPipe = go.AddOrGet<EvaporationPipe>();
            evaporationPipe.SetStorages(inputStorage, outputStorage);
        }


        public override void DoPostConfigureComplete(GameObject go)
        {
            /**
            go.GetComponent<KPrefabID>().prefabSpawnFn += delegate (GameObject game_object)
            {
                HandleVector<int>.Handle handle = GameComps.StructureTemperatures.GetHandle(game_object);
                StructureTemperaturePayload payload = GameComps.StructureTemperatures.GetPayload(handle);
                int cell = Grid.PosToCell(game_object);
                GameComps.StructureTemperatures.SetPayload(handle, ref payload);
            };
            */
        }
        public const string ID = "EvaporationPipe";
    }
}