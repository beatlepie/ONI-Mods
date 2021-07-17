using HarmonyLib;
using Klei.AI;
using STRINGS;
using System.Collections.Generic;
using UnityEngine;

namespace JumpSweepy
{
    [HarmonyPatch(typeof(SweepBotConfig), "CreatePrefab")]
    public static class PatchCreatePrefab
    {
		/// <summary>
		/// This was done to change [CreatureFallMonitor] into [SweepyFallMonitor] and [WalkerBabyNavGrid] to [SweepyNavGrid]!
		/// </summary>
		/// <param name="__result"></param>
		/// <returns></returns>
        private static bool Prefix(GameObject __result)
        {
			string id = "SweepBot";
			string name = ROBOTS.MODELS.SWEEPBOT.NAME;
			string desc = ROBOTS.MODELS.SWEEPBOT.DESC;
			float mass = SweepBotConfig.MASS;
			EffectorValues none = TUNING.BUILDINGS.DECOR.NONE;
			GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, mass, Assets.GetAnim("sweep_bot_kanim"), "idle", Grid.SceneLayer.Creatures, 1, 1, none, default(EffectorValues), SimHashes.Creature, null, 293f);
			gameObject.AddOrGet<LoopingSounds>();
			gameObject.GetComponent<KBatchedAnimController>().isMovable = true;
			KPrefabID kprefabID = gameObject.AddOrGet<KPrefabID>();
			kprefabID.AddTag(GameTags.Creature, false);
			gameObject.AddComponent<Pickupable>();
			gameObject.AddOrGet<Clearable>().isClearable = false;
			Trait trait = Db.Get().CreateTrait("SweepBotBaseTrait", name, name, null, false, null, true, true);
			trait.Add(new AttributeModifier(Db.Get().Amounts.InternalBattery.maxAttribute.Id, 21000f, name, false, false, true));
			trait.Add(new AttributeModifier(Db.Get().Amounts.InternalBattery.deltaAttribute.Id, -40f, name, false, false, true));
			Modifiers modifiers = gameObject.AddOrGet<Modifiers>();
			modifiers.initialTraits.Add("SweepBotBaseTrait");
			modifiers.initialAmounts.Add(Db.Get().Amounts.HitPoints.Id);
			modifiers.initialAmounts.Add(Db.Get().Amounts.InternalBattery.Id);
			gameObject.AddOrGet<KBatchedAnimController>().SetSymbolVisiblity("snapto_pivot", false);
			gameObject.AddOrGet<Traits>();
			gameObject.AddOrGet<CharacterOverlay>();
			gameObject.AddOrGet<Effects>();
			gameObject.AddOrGetDef<AnimInterruptMonitor.Def>();
			gameObject.AddOrGetDef<StorageUnloadMonitor.Def>();
			RobotBatteryMonitor.Def def = gameObject.AddOrGetDef<RobotBatteryMonitor.Def>();
			def.batteryAmountId = Db.Get().Amounts.InternalBattery.Id;
			def.canCharge = true;
			def.lowBatteryWarningPercent = 0.5f;
			gameObject.AddOrGetDef<SweetBotReactMonitor.Def>();

			Debug.Log("1");

			gameObject.AddOrGetDef<SweepyFallMonitor.Def>();

			Debug.Log("2");

			gameObject.AddOrGetDef<SweepBotTrappedMonitor.Def>();
			gameObject.AddOrGet<AnimEventHandler>();
			gameObject.AddOrGet<SnapOn>().snapPoints = new List<SnapOn.SnapPoint>(new SnapOn.SnapPoint[]
			{
			new SnapOn.SnapPoint
			{
				pointName = "carry",
				automatic = false,
				context = "",
				buildFile = null,
				overrideSymbol = "snapTo_ornament"
			}
			});
			SymbolOverrideControllerUtil.AddToPrefab(gameObject);
			gameObject.AddComponent<Storage>();
			gameObject.AddComponent<Storage>().capacityKg = 500f;
			gameObject.AddOrGet<OrnamentReceptacle>().AddDepositTag(GameTags.PedestalDisplayable);
			gameObject.AddOrGet<DecorProvider>();
			gameObject.AddOrGet<UserNameable>();
			gameObject.AddOrGet<CharacterOverlay>();
			gameObject.AddOrGet<ItemPedestal>();
			Navigator navigator = gameObject.AddOrGet<Navigator>();
			// changed the navigation grid from [WalkerBabyNavGrid] to [SweepyNavGrid] (modified from adult hatch movement)
			navigator.NavGridName = "SweepyNavGrid";
			navigator.CurrentNavType = NavType.Floor;
			navigator.defaultSpeed = 1f;
			navigator.updateProber = true;
			navigator.maxProbingRadius = 32;
			navigator.sceneLayer = Grid.SceneLayer.Creatures;
			kprefabID.AddTag(GameTags.Creatures.Walker, false);

			Debug.Log("3");

			ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new SweepyFallStates.Def(), true).Add(new AnimInterruptStates.Def(), true).Add(new SweepBotTrappedStates.Def(), true).Add(new DeliverToSweepLockerStates.Def(), true).Add(new ReturnToChargeStationStates.Def(), true).Add(new SweepStates.Def(), true).Add(new IdleStates.Def(), true);
			gameObject.AddOrGet<LoopingSounds>();

			Debug.Log("4");

			EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Robots.Models.SweepBot, null);
			__result = gameObject;
            return false;
        }
    }
}
