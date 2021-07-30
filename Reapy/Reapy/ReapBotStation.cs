using System;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace Reapy
{
    // This might be an issue too...
    [AddComponentMenu("KMonoBehaviour/scripts/ReapBotStation")]
    public class ReapBotStation : KMonoBehaviour
    {
        public void SetStorages(Storage botMaterialStorage, Storage reapStorage)
        {
            this.botMaterialStorage = botMaterialStorage;
            this.reapStorage = reapStorage;
        }

        protected override void OnPrefabInit()
        {
            Initialize(false);
            Subscribe<ReapBotStation>(-592767678, ReapBotStation.OnOperationalChangedDelegate);
        }

        protected void Initialize(bool use_logic_meter)
        {
            base.OnPrefabInit();
            base.GetComponent<Operational>().SetFlag(dockedRobot, false);
        }

        protected override void OnSpawn()
        {
            base.Subscribe(-1697596308, new Action<object>(OnStorageChanged));
            meter = new MeterController(base.gameObject.GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, new string[]
            {
            "meter_frame",
            "meter_level"
            });
            if (ReapBot == null || ReapBot.Get() == null)
            {
                RequestNewReapBot(null);
            }
            else
            {
                StorageUnloadMonitor.Instance smi = ReapBot.Get().GetSMI<StorageUnloadMonitor.Instance>();
                smi.sm.sweepLocker.Set(reapStorage, smi);
                RefreshReapBotSubscription();
            }
            UpdateMeter();
            UpdateNameDisplay();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RequestNewReapBot(object data = null)
        {
            if (botMaterialStorage.FindFirstWithMass(GameTags.RefinedMetal, ReapBotConfig.mass) == null)
            {
                FetchList2 fetchList = new FetchList2(botMaterialStorage, Db.Get().ChoreTypes.Fetch);
                fetchList.Add(GameTags.RefinedMetal, null, null, ReapBotConfig.mass, FetchOrder2.OperationalRequirement.None);
                fetchList.Submit(null, true);
                return;
            }
            MakeNewReapBot(null);
        }

        private void MakeNewReapBot(object data = null)
        {
            if (newReapyHandle.IsValid)
            {
                return;
            }
            if (botMaterialStorage.GetAmountAvailable(GameTags.RefinedMetal) < ReapBotConfig.mass)
            {
                return;
            }
            PrimaryElement primaryElement = botMaterialStorage.FindFirstWithMass(GameTags.RefinedMetal, ReapBotConfig.mass);
            if (primaryElement == null)
            {
                return;
            }
            primaryElement.Mass -= ReapBotConfig.mass;
            UpdateMeter();
            // THIS MIGHT BE AN ISSUE!!! can schedule recognize ["MakeReapy"]
            newReapyHandle = GameScheduler.Instance.Schedule("MakeReapy", 2f, delegate (object obj)
            {
                // This will also be an issue...
                GameObject gameObject = GameUtil.KInstantiate(Assets.GetPrefab("ReapBot"), Grid.CellToPos(Grid.CellRight(Grid.PosToCell(this.gameObject))), Grid.SceneLayer.Creatures, null, 0);
                gameObject.SetActive(true);
                ReapBot = new Ref<KSelectable>(gameObject.GetComponent<KSelectable>());
                if (!string.IsNullOrEmpty(storedName))
                {
                    ReapBot.Get().GetComponent<UserNameable>().SetName(storedName);
                }
                UpdateNameDisplay();
                StorageUnloadMonitor.Instance smi = gameObject.GetSMI<StorageUnloadMonitor.Instance>();
                smi.sm.sweepLocker.Set(reapStorage, smi);
                ReapBot.Get().GetComponent<PrimaryElement>().ElementID = primaryElement.ElementID;
                RefreshReapBotSubscription();
                newReapyHandle.ClearScheduler();
            }, null, null);
            base.GetComponent<KBatchedAnimController>().Play("newreapy", KAnim.PlayMode.Once, 1f, 0f);
        }

        private void RefreshReapBotSubscription()
        {
            if (refreshReapBotHandle != -1)
            {
                ReapBot.Get().Unsubscribe(refreshReapBotHandle);
                ReapBot.Get().Unsubscribe(ReapBotNameChangeHandle);
            }
            // This might be an issue too...
            refreshReapBotHandle = ReapBot.Get().Subscribe(1969584890, new Action<object>(RequestNewReapBot));
            ReapBotNameChangeHandle = ReapBot.Get().Subscribe(1102426921, new Action<object>(UpdateStoredName));
        }

        private void UpdateStoredName(object data)
        {
            storedName = (string)data;
            UpdateNameDisplay();
        }

        private void UpdateNameDisplay()
        {
            if (string.IsNullOrEmpty(storedName))
            {
                base.GetComponent<KSelectable>().SetName(string.Format(BUILDINGS.PREFABS.SWEEPBOTSTATION.NAMEDSTATION, ReapBotConfig.name));
            }
            else
            {
                base.GetComponent<KSelectable>().SetName(string.Format(BUILDINGS.PREFABS.SWEEPBOTSTATION.NAMEDSTATION, storedName));
            }
            NameDisplayScreen.Instance.UpdateName(base.gameObject);
        }

        public void DockRobot(bool docked)
        {
            base.GetComponent<Operational>().SetFlag(dockedRobot, docked);
        }

        public void StartCharging()
        {
            base.GetComponent<KBatchedAnimController>().Queue("sleep_pre", KAnim.PlayMode.Once, 1f, 0f);
            base.GetComponent<KBatchedAnimController>().Queue("sleep_idle", KAnim.PlayMode.Loop, 1f, 0f);
        }

        public void StopCharging()
        {
            base.GetComponent<KBatchedAnimController>().Play("sleep_pst", KAnim.PlayMode.Once, 1f, 0f);
            this.UpdateNameDisplay();
        }

        protected override void OnCleanUp()
        {
            if (newReapyHandle.IsValid)
            {
                newReapyHandle.ClearScheduler();
            }
            if (refreshReapBotHandle != -1 && ReapBot.Get() != null)
            {
                ReapBot.Get().Unsubscribe(refreshReapBotHandle);
            }
        }

        private void UpdateMeter()
        {
            float maxCapacityMinusStorageMargin = GetMaxCapacityMinusStorageMargin();
            float positionPercent = Mathf.Clamp01(GetAmountStored() / maxCapacityMinusStorageMargin);
            if (meter != null)
            {
                meter.SetPositionPercent(positionPercent);
            }
        }

        private void OnStorageChanged(object data)
        {
            UpdateMeter();
            if (ReapBot == null || ReapBot.Get() == null)
            {
                RequestNewReapBot(null);
            }
            KBatchedAnimController component = base.GetComponent<KBatchedAnimController>();
            if (component.currentFrame >= component.GetCurrentNumFrames())
            {
                base.GetComponent<KBatchedAnimController>().Play("remove", KAnim.PlayMode.Once, 1f, 0f);
            }
            for (int i = 0; i < reapStorage.Count; i++)
            {
                reapStorage[i].GetComponent<Clearable>().MarkForClear(false, true);
            }
        }

        private void OnOperationalChanged(object data)
        {
            Operational component = base.GetComponent<Operational>();
            if (component.Flags.ContainsValue(false))
            {
                component.SetActive(false, false);
            }
            else
            {
                component.SetActive(true, false);
            }
            if (ReapBot == null || ReapBot.Get() == null)
            {
                RequestNewReapBot(null);
            }
        }

        private float GetMaxCapacityMinusStorageMargin()
        {
            return reapStorage.Capacity() - reapStorage.storageFullMargin;
        }

        private float GetAmountStored()
        {
            return reapStorage.MassStored();
        }

        [Serialize]
        public Ref<KSelectable> ReapBot;

        [Serialize]
        public string storedName;

        private Operational.Flag dockedRobot = new Operational.Flag("dockedRobot", Operational.Flag.Type.Functional);

        private MeterController meter;

        [SerializeField]
        private Storage botMaterialStorage;

        [SerializeField]
        private Storage reapStorage;

        private SchedulerHandle newReapyHandle;

        private static readonly EventSystem.IntraObjectHandler<ReapBotStation> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<ReapBotStation>(delegate (ReapBotStation component, object data)
        {
            component.OnOperationalChanged(data);
        });

        private int refreshReapBotHandle = -1;

        private int ReapBotNameChangeHandle = -1;
    }
}