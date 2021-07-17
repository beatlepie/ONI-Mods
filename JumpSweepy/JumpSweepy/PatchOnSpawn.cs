using HarmonyLib;
using UnityEngine;

namespace JumpSweepy
{
	[HarmonyPatch(typeof(SweepBotConfig), "OnSpawn")]
	public static class PatchOnSpawn
	{
		private static bool Prefix(GameObject inst)
		{
			StorageUnloadMonitor.Instance smi = inst.GetSMI<StorageUnloadMonitor.Instance>();
			smi.sm.internalStorage.Set(inst.GetComponents<Storage>()[1], smi);
			inst.GetComponent<OrnamentReceptacle>();
			inst.GetSMI<SweepyFallMonitor.Instance>().anim = "idle_loop";

			// Get its naviator and add a transition layer, this allows them to go through doors
			if (inst != null)
			{
				Navigator navigator = inst.AddOrGet<Navigator>();
				navigator.transitionDriver.overrideLayers.Add(new DoorTransitionLayer(navigator));
			}

			return false;
		}
	}
}
