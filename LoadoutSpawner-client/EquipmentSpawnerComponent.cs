using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFT.UI.Screens;
using HarmonyLib;
using LoadoutSpawner.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoadoutSpawner.Components
{
    public class EquipmentSpawnerComponent : MonoBehaviour
    {
        public static EquipmentSpawnerComponent Instance { get; private set; }

        private CommonUI _commonUI => Singleton<CommonUI>.Instance;
        private GameObject _spawnObject;
        private void Awake()
        {
        }
    }
}
