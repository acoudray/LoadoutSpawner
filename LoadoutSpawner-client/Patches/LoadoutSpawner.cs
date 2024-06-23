using Aki.Reflection.Patching;
using EFT.UI;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Comfort.Common;
using EFT.InventoryLogic;
using Aki.Common.Http;
using Aki.Common.Utils;
using Diz.LanguageExtensions;

namespace LoadoutSpawner.Patches
{
    internal class LoadoutSpawner : ModulePatch
    {
        private static JsonConverter[] _defaultJsonConverters;
        protected override MethodBase GetTargetMethod()
        {
            var converterClass = typeof(AbstractGame).Assembly.GetTypes()
                .First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);
            _defaultJsonConverters = Traverse.Create(converterClass).Field<JsonConverter[]>("Converters").Value;
            return AccessTools.Method(typeof(EquipmentBuildsScreen), nameof(EquipmentBuildsScreen.Awake));
        }

        [PatchPostfix]
        public static void PatchPostfix(EquipmentBuildsScreen __instance)
        {
            var ButtonsPanel = __instance.transform.Find("Panels/Gear Panel/ButtonsPanel");
            var SpawnButton = GameObject.Instantiate(ButtonsPanel.Find("EquipButton"));
            SpawnButton.SetParent(ButtonsPanel);
            if (SpawnButton != null)
            {
                var text = SpawnButton.transform.Find("SizeLabel/Label").GetComponent<TextMeshProUGUI>();
                SpawnButton.GetComponent<DefaultUIButton>().SetRawText("SPAWN GEAR", 36);
                text.text = "SPAWN GEAR";
                var _spawnButton = SpawnButton.GetComponent<DefaultUIButton>();
                _spawnButton.OnClick.RemoveAllListeners();
                _spawnButton.OnClick.AddListener(delegate { SpawnLoadout(__instance); });
                return;
            }
            else
            {
                return;
            }
        }

        public static void SpawnLoadout(EquipmentBuildsScreen instance)
        {
            try
            {
                if (instance != null)
                {
                    List<EquipmentBuildsScreen.Class2415> EquipmentBuildsScreen = Traverse.Create(instance).Field("list_0").GetValue<List<EquipmentBuildsScreen.Class2415>>();
                    EquipmentClass equipment = Traverse.Create(EquipmentBuildsScreen[0].Build).Field("gclass2701_0").GetValue<EquipmentClass>();
                    GClass3182 build = Traverse.Create(instance).Field("gclass3182_0").GetValue<GClass3182>();
                    List<Item> missingItemsList;
                    Error error;
                    instance.method_13(out missingItemsList, out error);
                    List<GClass1189[]> tree = new List<GClass1189[]>();
                    if (missingItemsList.Count > 0)
                    {
                        foreach (Item item in missingItemsList)
                        {
                            if (item == null)
                                continue;
                            var _item = Singleton<ItemFactory>.Instance.TreeToFlatItems(item);
                            tree.Add(_item);
                        }
                    }

                    Profile profile = (Profile)AccessTools.Field(typeof(EquipmentBuildsScreen), "profile_0").GetValue(instance);
                    EquipmentBuildsScreen.GClass3115 ScreenController = (EquipmentBuildsScreen.GClass3115)AccessTools.Field(typeof(EquipmentBuildsScreen), "ScreenController").GetValue(instance);
                    var inventoryController = ScreenController.InventoryController;
                    ISession session = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession();
                    GClass3206 questController = new GClass3206(profile, inventoryController, session, false);
                    RagFairClass ragFair = session.RagFair;
                    var res = RequestHandler.PostJson("/loadout-spawner/equipment", new
                    {
                        items = tree
                    }.ToJson(_defaultJsonConverters));
                    if (res != null)
                    {
                        ProfileChangesPocoClass profileChangesPocoClass = Json.Deserialize<ProfileChangesPocoClass>(res);
                        GInterface142 profileUpdateClass = new GClass1841(profile, (GClass2764)inventoryController, (GClass3206)questController, ragFair);
                        profileUpdateClass.UpdateProfile(profileChangesPocoClass);
                        instance.method_15(build);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationManagerClass.DisplayMessageNotification("Error", EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.Default);
                throw ex;
            }
        }
    }
}
