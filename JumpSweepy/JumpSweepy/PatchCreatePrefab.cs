using HarmonyLib;
using Klei.AI;
using System.Collections.Generic;
using UnityEngine;

namespace JumpSweepy
{
    [HarmonyPatch(typeof(SweepBotConfig), "CreatePrefab")]
    public static class PatchCreatePrefab
    {
        //private static bool Prefix(ref GameObject __result)
        //      {
        //	string id = "SweepBot";
        //	string name = STRINGS.ROBOTS.MODELS.SWEEPBOT.NAME;
        //	string desc = STRINGS.ROBOTS.MODELS.SWEEPBOT.DESC;
        //	float mass = SweepBotConfig.MASS;
        //	EffectorValues none = TUNING.BUILDINGS.DECOR.NONE;
        //	GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, mass, Assets.GetAnim("sweep_bot_kanim"), "idle", Grid.SceneLayer.Creatures, 1, 1, none, default(EffectorValues), SimHashes.Creature, null, 293f);
        //	gameObject.AddOrGet<LoopingSounds>();
        //	gameObject.GetComponent<KBatchedAnimController>().isMovable = true;
        //	KPrefabID kprefabID = gameObject.AddOrGet<KPrefabID>();
        //	kprefabID.AddTag(GameTags.Creature, false);
        //	gameObject.AddComponent<Pickupable>();
        //	gameObject.AddOrGet<Clearable>().isClearable = false;
        //	Trait trait = Db.Get().CreateTrait("SweepBotBaseTrait", name, name, null, false, null, true, true);
        //	trait.Add(new AttributeModifier(Db.Get().Amounts.InternalBattery.maxAttribute.Id, 9000f, name, false, false, true));
        //	trait.Add(new AttributeModifier(Db.Get().Amounts.InternalBattery.deltaAttribute.Id, -17.1428566f, name, false, false, true));
        //	Modifiers modifiers = gameObject.AddOrGet<Modifiers>();
        //	modifiers.initialTraits.Add("SweepBotBaseTrait");
        //	modifiers.initialAmounts.Add(Db.Get().Amounts.HitPoints.Id);
        //	modifiers.initialAmounts.Add(Db.Get().Amounts.InternalBattery.Id);
        //	gameObject.AddOrGet<KBatchedAnimController>().SetSymbolVisiblity("snapto_pivot", false);
        //	gameObject.AddOrGet<Traits>();
        //	gameObject.AddOrGet<Effects>();
        //	gameObject.AddOrGetDef<AnimInterruptMonitor.Def>();
        //	gameObject.AddOrGetDef<StorageUnloadMonitor.Def>();
        //	RobotBatteryMonitor.Def def = gameObject.AddOrGetDef<RobotBatteryMonitor.Def>();
        //	def.batteryAmountId = Db.Get().Amounts.InternalBattery.Id;
        //	def.canCharge = true;
        //	def.lowBatteryWarningPercent = 0.5f;
        //	gameObject.AddOrGetDef<SweetBotReactMonitor.Def>();
        //	gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
        //	gameObject.AddOrGetDef<SweepBotTrappedMonitor.Def>();
        //	gameObject.AddOrGet<AnimEventHandler>();
        //	gameObject.AddOrGet<SnapOn>().snapPoints = new List<SnapOn.SnapPoint>(new SnapOn.SnapPoint[]
        //	{
        //	new SnapOn.SnapPoint
        //	{
        //		pointName = "carry",
        //		automatic = false,
        //		context = "",
        //		buildFile = null,
        //		overrideSymbol = "snapTo_ornament"
        //	}
        //	});
        //	SymbolOverrideControllerUtil.AddToPrefab(gameObject);
        //	gameObject.AddComponent<Storage>();
        //	gameObject.AddComponent<Storage>().capacityKg = 500f;
        //	gameObject.AddOrGet<OrnamentReceptacle>().AddDepositTag(GameTags.PedestalDisplayable);
        //	gameObject.AddOrGet<DecorProvider>();
        //	gameObject.AddOrGet<UserNameable>();
        //	gameObject.AddOrGet<CharacterOverlay>();
        //	gameObject.AddOrGet<ItemPedestal>();
        //	Navigator navigator = gameObject.AddOrGet<Navigator>();
        //	navigator.NavGridName = "SweepyNavGrid";
        //	navigator.CurrentNavType = NavType.Floor;
        //	navigator.defaultSpeed = 1f;
        //	navigator.updateProber = true;
        //	navigator.maxProbingRadius = 32;
        //	navigator.sceneLayer = Grid.SceneLayer.Creatures;
        //	kprefabID.AddTag(GameTags.Creatures.Walker, false);
        //	ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new FallStates.Def(), true, -1).Add(new AnimInterruptStates.Def(), true, -1).Add(new SweepBotTrappedStates.Def(), true, -1).Add(new DeliverToSweepLockerStates.Def(), true, -1).Add(new ReturnToChargeStationStates.Def(), true, -1).Add(new SweepStates.Def(), true, -1).Add(new IdleStates.Def(), true, -1);
        //	gameObject.AddOrGet<LoopingSounds>();
        //	EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Robots.Models.SweepBot, null);

        //	__result = gameObject;
        //	return false;
        //      }

        private static void Postfix(ref GameObject __result)
        {
            __result.GetComponent<Navigator>().NavGridName = "SweepyNavGrid";

            // This was in [SmartSweepy], but I don't see why this was done...
            ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new AnimInterruptStates.Def(), true).Add(new SweepBotTrappedStates.Def(), true).Add(new DeliverToSweepLockerStates.Def(), true).Add(new ReturnToChargeStationStates.Def(), true).Add(new SweepStates.Def(), true).Add(new IdleStates.Def(), true);
            EntityTemplates.AddCreatureBrain(__result, chore_table, GameTags.Robots.Models.SweepBot, null);
        }
    }
}
