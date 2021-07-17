using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace JumpSweepy
{
    /*	//this is what failed!
        [HarmonyPatch(typeof(EntityConfigManager), "LoadGeneratedEntities")]
        class PatchTESTING
        {
            public static string SAVE_FILE = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Save.txt";
            public static void Prefix(List<Type> types)
            {
                types.Remove(typeof(STRINGS.CODEX.SWEEPY));
                types.Remove(typeof(STRINGS.CODEX.SWEEPY.BODY));
                types.Add(typeof(STRINGS.CODEX.SWEEPY));
                types.Add(typeof(STRINGS.CODEX.SWEEPY.BODY));

                using (StreamWriter file = File.CreateText(SAVE_FILE))
                {

                    file.WriteLine();
                    file.WriteLine();
                    foreach (Type type in types)
                        file.WriteLine(type);
                }

                Debug.Log("SUCCESS");
            }
            public static void Postfix()
            {
                Debug.Log("LoadGeneratedEntities finished!");
            }
        }
    */

    [HarmonyPatch(typeof(EntityConfigManager), "RegisterEntity")]
    class PatchTESTING
    {
        public static bool Prefix(IEntityConfig config)
        {
            Debug.Log(config);

            KPrefabID component = config.CreatePrefab().GetComponent<KPrefabID>();
            Debug.Log(component.name);

            component.prefabInitFn += config.OnPrefabInit;
            component.prefabSpawnFn += config.OnSpawn;
            Assets.AddPrefab(component);

            return false;
        }
    }
}