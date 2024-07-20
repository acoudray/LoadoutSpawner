using Aki.Reflection.Patching;
using EFT.UI;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Comfort.Common;
using EFT.InventoryLogic;
using Aki.Common.Http;
using Aki.Common.Utils;
using Diz.LanguageExtensions;
using EFT.UI.Matchmaker;
using EFT.UI.Screens;
using UnityEngine.Profiling;


namespace LoadoutSpawner.Patches
{
    internal class RandomLoadout : ModulePatch
    {
        private static JsonConverter[] _defaultJsonConverters;
        private MatchmakerOfflineRaidScreen instance;
        protected override MethodBase GetTargetMethod()
        {
            var converterClass = typeof(AbstractGame).Assembly.GetTypes()
                .First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);
            _defaultJsonConverters = Traverse.Create(converterClass).Field<JsonConverter[]>("Converters").Value;
            return AccessTools.Method(typeof(MatchmakerOfflineRaidScreen), nameof(MatchmakerOfflineRaidScreen.Awake));
        }

        [PatchPostfix]
        public static void PatchPostfix(MatchmakerOfflineRaidScreen __instance)
        {
            var readyButton = __instance.transform.Find("ReadyButton");
            var randomReadyButton = GameObject.Instantiate(readyButton);
            randomReadyButton.SetParent(readyButton.parent);
            randomReadyButton.localPosition = new Vector3 (301.347f, 111.6571f, 0f);
            randomReadyButton.GetComponent<DefaultUIButton>().OnClick.RemoveAllListeners();
            randomReadyButton.GetComponent<DefaultUIButton>().SetRawText("START WITH\r\nRANDOM LOADOUT", 36);
            randomReadyButton.transform.Find("SizeLabel/Label").GetComponent<TextMeshProUGUI>().color = new Color(0.2525f, 0.7047f, 0.118f, 1);
            //randomReadyButton.transform.Find("SizeLabel").GetComponent<TextMeshProUGUI>().color = new Color(0.2525f, 0.7047f, 0.118f, 1);
            //randomReadyButton.transform.Find("SizeLabel/Label").GetComponent<TextMeshProUGUI>().text = "START WITH\r\nRANDOM LOADOUT";
            //randomReadyButton.transform.Find("SizeLabel").GetComponent<TextMeshProUGUI>().text = "START WITH\r\nRANDOM LOADOUT";
            randomReadyButton.GetComponent<DefaultUIButton>().OnClick.AddListener(() => StartWithRandomLoadout(__instance));
        }

        private static void StartWithRandomLoadout(MatchmakerOfflineRaidScreen instance)
        {
            MatchmakerOfflineRaidScreen.GClass3155 ScreenController = (MatchmakerOfflineRaidScreen.GClass3155)AccessTools.Field(typeof(MatchmakerOfflineRaidScreen), "ScreenController").GetValue(instance);
            var mainMenuControllerAction = (Action)AccessTools.Field(typeof(MatchmakerOfflineRaidScreen.GClass3155), "action_2").GetValue(ScreenController);
            var mainMenuController = (MainMenuController)AccessTools.Property(typeof(Action), "Target").GetValue(mainMenuControllerAction);
            mainMenuController.method_43();
            Traverse mainMenuControllerTraverse = Traverse.Create(mainMenuController);
            if (!mainMenuController.method_45() || !mainMenuController.method_49() || !mainMenuController.method_46())
            {
                return;
            }
            //if (mainMenuControllerTraverse.Field("raidSettings_0").Field<bool>("Local").Value && mainMenuControllerTraverse.Field("gclass3167_0").Field("GroupPlayers").Property<int>("Count").Value != 1)
            //{
            //    mainMenuControllerTraverse.Field("raidSettings_0").Field<ERaidMode>("RaidMode").Value = ERaidMode.Online;
            //}
            UpdateMatchmakerSettings(mainMenuControllerTraverse.Field<MatchmakerPlayerControllerClass>("gclass3167_0").Value);
            if (mainMenuControllerTraverse.Field("gclass3167_0").Field<bool>("IsLeader").Value && (!mainMenuControllerTraverse.Method("method_47").GetValue<bool>() || !mainMenuControllerTraverse.Method("method_48").GetValue<bool>()))
            {
                return;
            }
            var raidSettings = mainMenuControllerTraverse.Field<RaidSettings>("raidSettings_0").Value;
            MatchMakerAcceptScreen.GClass3150 gclass = new MatchMakerAcceptScreen.GClass3150(mainMenuControllerTraverse.Field<ISession>("ginterface145_0").Value, ref raidSettings, mainMenuControllerTraverse.Field<MatchmakerPlayerControllerClass>("gclass3167_0").Value);
            mainMenuControllerTraverse.Field<RaidSettings>("raidSettings_0").Value = raidSettings;
            gclass.OnReadyToStartRaid += () => StartMatching(instance, mainMenuController);
            gclass.ShowScreen(EScreenState.Queued);
        }
        private static void UpdateMatchmakerSettings(MatchmakerPlayerControllerClass _this)
        {
            Traverse traverse = Traverse.Create(_this);
            ESideType side = traverse.Field("gparam_1").Field<ESideType>("Side").Value;
            traverse.Field("CurrentPlayer").Field<PlayerVisualRepresentation>("PlayerVisualRepresentation").Value = null;
            var pmcProfile = traverse.Field("ginterface147_0").Property<Profile>("Profile").Value;
            var scavProfile = traverse.Field("ginterface147_0").Property<Profile>("ProfileOfPet").Value;
            traverse.Field("CurrentPlayer").Field<GClass1208>("Info").Value = traverse.Method("CreateRaidPlayerInfo", pmcProfile, scavProfile).GetValue<GClass1208>();
        }

        private static void StartMatching(MatchmakerOfflineRaidScreen instance, MainMenuController mainMenuController)
        {
            mainMenuController.method_74();
            //mainMenuController.method_20();
        }

        private static void GetRandomLoadout(MatchmakerOfflineRaidScreen instance)
        {
            var inventoryController = Traverse.Create(instance).Property<GClass2764>("InventoryController").Value;
            ISession session = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession();
            Profile profile = Traverse.Create(instance).Field("gclass2764_0").Property<Profile>("Profile").Value;
            GClass3206 questController = new GClass3206(profile, inventoryController, session, false);
            RagFairClass ragFair = session.RagFair;
            var res = RequestHandler.GetJson("/loadout-spawner/random-loadout");
            if (res != null)
            {
                ProfileChangesPocoClass profileChangesPocoClass = Json.Deserialize<ProfileChangesPocoClass>(res);
                GInterface142 profileUpdateClass = new GClass1841(profile, (GClass2764)inventoryController, (GClass3206)questController, ragFair);
                profileUpdateClass.UpdateProfile(profileChangesPocoClass);
                Inventory inventory = profile.Inventory;
                GStruct416<IEnumerable<GInterface323>> gstruct = InteractionsHandlerClass.TransferContent(build.Equipment, inventory.Equipment, inventory.SortingTable.ToEnumerable<SortingTableClass>(), this.gclass2763_0, null, false, true);

            }
        }

        private static void EquipLoadout()
        {

        }
    }
}
