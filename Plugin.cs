using System;
using System.Collections.Generic;

using BepInEx;

using HarmonyLib;

using UnityEngine;

namespace RcHud;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    public void Awake() {
        RcHud.Config.Init(this.Config);
        Harmony.CreateAndPatchAll(this.GetType());
    }

    public void Start() {
        RacecarHud.SetLogger(this.Logger);
    }

    public void Update() {
        _ = RacecarHud.Instance;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Crosshair), nameof(Crosshair.CheckCrossHair))]
    static void UpdateCrosshairSettings() {
        RacecarHud.Instance.ApplyHiVisOverhealSettings();
    }
}
