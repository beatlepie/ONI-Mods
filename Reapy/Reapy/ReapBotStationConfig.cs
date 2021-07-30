using TUNING;
using UnityEngine;

namespace Reapy
{
    class ReapBotStationConfig : IBuildingConfig
    {
        public static void AddConfig()
        {
            AddBuilding.AddBuildingToPlanScreen("Utilities", "ReapBotStation", "SweepBotStation");
            AddBuilding.IntoTechTree("RoboticTools", "ReapBotStation");
            AddBuilding.AddStrings("ReapBotStation", "Reapy's Dock", "An automated reaping/harvesting robot. \n\nReap/Harvest fully grown crops and takes the produce. As this is designed for automation than cultivation, it will not produce seeds!", "Harvests and collects the produce!");
        }

        public override BuildingDef CreateBuildingDef()
        {
            string id = "ReapBotStation";
            int width = 2;
            int height = 2;
            string anim = "sweep_bot_base_station_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] construction_mass = new float[]
            {
            BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0] - SweepBotConfig.MASS
            };
            string[] refined_METALS = MATERIALS.REFINED_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues none = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, construction_mass, refined_METALS, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, none, 0.2f);
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.Floodable = false;
            buildingDef.AudioCategory = "Metal";
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 240f;
            buildingDef.ExhaustKilowattsWhenActive = 0f;
            buildingDef.SelfHeatKilowattsWhenActive = 1f;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            Prioritizable.AddRef(go);

            // This storage is for reapy!
            Storage storage = go.AddComponent<Storage>();
            storage.showInUI = true;
            storage.allowItemRemoval = false;
            storage.ignoreSourcePriority = true;
            storage.showDescriptor = false;
            storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS;
            storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            storage.fetchCategory = Storage.FetchCategory.Building;
            storage.capacityKg = 25f;
            storage.allowClearable = false;

            // This is for the harvested crops!
            Storage storage2 = go.AddComponent<Storage>();
            storage2.showInUI = true;
            storage2.allowItemRemoval = true;
            storage2.ignoreSourcePriority = true;
            storage2.showDescriptor = true;
            storage2.storageFilters = STORAGEFILTERS.FOOD;
            // The value is 0.5f...
            storage2.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            // This might be an issue...
            storage2.fetchCategory = Storage.FetchCategory.StorageSweepOnly;
            storage2.capacityKg = 1000f;
            storage2.allowClearable = true;
            storage2.showCapacityStatusItem = true;

            go.AddOrGet<CharacterOverlay>().shouldShowName = true;
            go.AddOrGet<ReapBotStation>().SetStorages(storage, storage2);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }
}
