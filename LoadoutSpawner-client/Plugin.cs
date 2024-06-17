﻿using BepInEx;
using BepInEx.Logging;
using LoadoutSpawner.Patches;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadoutSpawner
{
    [BepInPlugin("org.rameleu.loadoutspawner", "Loadout Spawner", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        private void Awake() //Awake() will run once when your plugin loads
        {
            //we save the Logger to our LogSource variable so we can use it anywhere, such as in our patches via Plugin.LogSource.LogInfo(), etc.
            LogSource = Logger;
            LogSource.LogInfo("plugin loaded!");

            //uncomment the line below and replace "PatchClassName" with the class name you gave your patch. Patches must be enabled like this to work.
            new WeaponSpawner().Enable();
        }

    }
}