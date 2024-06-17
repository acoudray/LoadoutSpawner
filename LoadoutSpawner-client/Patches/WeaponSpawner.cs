using Aki.Common.Http;
using Aki.Common.Utils;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.BackendInventoryInteraction;
using EFT.UI;
using EFTApi;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace LoadoutSpawner.Patches
{
    internal class WeaponSpawner : ModulePatch
    {
        private static JsonConverter[] _defaultJsonConverters;
        private readonly Dictionary<string, GInterface142> dictionary_0 = new Dictionary<string, GInterface142>();

        protected override MethodBase GetTargetMethod()
        {
            var converterClass = typeof(AbstractGame).Assembly.GetTypes()
                .First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);
            _defaultJsonConverters = Traverse.Create(converterClass).Field<JsonConverter[]>("Converters").Value;
            return AccessTools.Method(typeof(EditBuildScreen), nameof(EditBuildScreen.Awake));
        }

        [PatchPostfix]
        public static void PatchPostfix(EditBuildScreen __instance)
        {
            var publishButton = Traverse.Create(__instance).Field("_publishButton").GetValue<Button>();
            var publishCanvasGroup = GameObject.Find("PublishBuildtButton").GetComponent<CanvasGroup>();
            if (publishButton != null)
            {
                publishButton.enabled = true;
                publishButton.interactable = true;
                publishButton.GetComponentInChildren<LocalizedText>().LocalizationKey = "ADD TO INVENTORY";
                publishCanvasGroup.alpha = 1.0f;
                publishCanvasGroup.interactable = true;
                publishButton.onClick.RemoveAllListeners();
                publishButton.onClick.AddListener(delegate { BuildWeapon(__instance); });
            }
        }

        public static void BuildWeapon(EditBuildScreen instance)
        {
            Item item = null;
            var traverse = Traverse.Create(instance);
            if (traverse.Property("Item").PropertyExists())
                item = (Item)AccessTools.Property(typeof(EditBuildScreen), "Item").GetValue(instance);
            if (traverse.Field("Item").FieldExists())
                item = (Item)AccessTools.Field(typeof(EditBuildScreen), "Item").GetValue(instance);
            if (item == null)
            {
                NotificationManagerClass.DisplayWarningNotification("No item selected", EFT.Communications.ENotificationDurationType.Default);
            }
            var tree = Singleton<ItemFactory>.Instance.TreeToFlatItems(item);
            var profile = (Profile)AccessTools.Field(typeof(EditBuildScreen), "profile_0").GetValue(instance);
            var inventoryController = (InventoryControllerClass)AccessTools.Property(typeof(EditBuildScreen), "InventoryController").GetValue(instance);
            var questController = EFTHelpers._QuestControllerHelper.QuestController;
            var res = RequestHandler.PostJson("/loadout-spawner/weapon", new
            {
                items = tree
            }.ToJson(_defaultJsonConverters));
            if (res != null)
            {
                ProfileChangesPocoClass profileChangesPocoClass = JsonConvert.DeserializeObject<ProfileChangesPocoClass>(res);
                GInterface142 profileUpdateClass = new GClass1841(profile, (GClass2764)inventoryController, (GClass3206)questController, EFTHelpers._SessionHelper.Session.RagFair);
                profileUpdateClass.UpdateProfile(profileChangesPocoClass);
                NotificationManagerClass.DisplayMessageNotification("Weapon successfully added to your inventory", EFT.Communications.ENotificationDurationType.Default, EFT.Communications.ENotificationIconType.Default);
            } else
            {
                NotificationManagerClass.DisplayWarningNotification("Error while trying to add item", EFT.Communications.ENotificationDurationType.Default);
            }
            return;
        }

        public static void DebugOnClick()
        {
            Plugin.LogSource.LogInfo("Button clicked");
        }
    }

}
