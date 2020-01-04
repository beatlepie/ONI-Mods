using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace testing
{
    public class CondensationVentConfig : IBuildingConfig
    {
        public static void Setup()
        {
            AddBuilding.AddStrings("CondensationVent", "Condensation Vent", "Allows gases to condense out of the vent!", "Allows gases to condense out of the vent!!!");
            AddBuilding.AddBuildingToPlanScreen("Utilities", "CondensationVent", "ThermalBlock");
            AddBuilding.IntoTechTree("TemperatureModulation", "CondensationVent");
        }

        //putting [f] at the end of the number makes the computer treat the number as [float] instead of defaulting to [double]
        public override BuildingDef CreateBuildingDef()
        {
            string id = "CondensationVent";
            int width = 1, height = 1;
            string anim = "ventgas_kanim";
            int hitpoints = 10;
            float construction_time = 10f;
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2; //100kg
            string[] RAW_METALS = MATERIALS.RAW_METALS;
            float melting_point = 1600f;
            //THIS MIGHT CAUSE PROBLEMS!!!
            BuildLocationRule build_location_rule = BuildLocationRule.NotInTiles; //Anywhere that is not blocked by tiles
            EffectorValues tier2 = BUILDINGS.DECOR.PENALTY.TIER0;
            EffectorValues tier3 = NOISE_POLLUTION.NOISY.TIER0;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, RAW_METALS, melting_point, build_location_rule, tier2, tier3, 0.2f);
            buildingDef.ThermalConductivity = 2f;
            buildingDef.Overheatable = false;
            buildingDef.Entombable = true;
//            buildingDef.ViewMode = OverlayModes.GasConduits.ID;
            //Layer the building is stored in, if it is [GasConduit], [GasConduit] objects cannot be place over them.
            buildingDef.ObjectLayer = ObjectLayer.Backwall;
            buildingDef.AudioCategory = "Metal";
            buildingDef.BaseTimeUntilRepair = -1f;
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
            //adds the output sprite on the building!
            buildingDef.InputConduitType = ConduitType.Gas;
            buildingDef.OutputConduitType = ConduitType.Liquid;
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
            conduitConsumer.conduitType = def.InputConduitType;
            conduitConsumer.consumptionRate = 10f;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.ignoreMinMassCheck = true;
            conduitConsumer.capacityKG = 10f;

            Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
            storage.showDescriptor = true;
            storage.capacityKg = 10f;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KPrefabID>().prefabSpawnFn += delegate (GameObject game_object)
            {
                HandleVector<int>.Handle handle = GameComps.StructureTemperatures.GetHandle(game_object);
                StructureTemperaturePayload payload = GameComps.StructureTemperatures.GetPayload(handle);
                int cell = Grid.PosToCell(game_object);
                GameComps.StructureTemperatures.SetPayload(handle, ref payload);
            };
        }
        public const string ID = "CondensationVent";
    }
}