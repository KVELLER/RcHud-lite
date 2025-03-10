using System.Reflection;
using System.Collections.Generic;

using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RcHud;

public sealed class RacecarHud : MonoSingleton<RacecarHud> {
    private static ManualLogSource log = new("");
    internal static void SetLogger(ManualLogSource logger) => log = logger;

    private bool initialized = false;

    private GameObject wheel = new();

    private Crosshair crosshairReference = new();

    public void Update() {
        if (!this.TryInit()) {
            return;
        }

        var gunc = GunControl.Instance;
        // the information that this HUD shows is never relevant in these secret levels
        var inSecret = SceneManager.GetActiveScene().name is "Level 0-S" or "Level 1-S" or "Level 4-S";

        var showWheel = true;
        showWheel &= gunc.slot4.Count > 0;
        showWheel &= !inSecret;

        this.wheel.SetActive(showWheel);

        var weaponCharges = WeaponCharges.Instance;
        var cbs = ColorBlindSettings.Instance;

        var wheelSlider = this.wheel.GetComponent<Slider>();
        wheelSlider.value = weaponCharges.raicharge;

        var wheelImg = this.wheel.GetComponent<Image>();
        // TODO: flash white?
        wheelImg.color = wheelSlider.normalizedValue < 1 ? cbs.railcannonChargingColor : cbs.railcannonFullColor;

        this.UpdateStaminaColors();
        this.UpdatePersistentHp();
    }

    private void UpdateStaminaColors() {
        var cbs = ColorBlindSettings.Instance;
        var blue = cbs.staminaColor;
        var dark = cbs.staminaChargingColor * 0.6f; // the normal hud uses opacity, which doesn't work here for some reason
        var red = cbs.staminaEmptyColor;

        for (var i = 2; i < 5; i++) {
            var chud = this.crosshairReference.chuds[i];
            var stfa = chud.GetComponent<SliderToFillAmount>();
            stfa.copyColor = false;
            var full = chud.fillAmount == stfa.maxFill;
            var first = i == 2;

            chud.color = Config.StaminaColorMode switch {
                StaminaColorMode.Vanilla => full ? blue : (first ? red : blue),
                StaminaColorMode.Static => blue,
                StaminaColorMode.AllRed => full ? blue : red,
                StaminaColorMode.AllDark => full ? blue : dark,
                StaminaColorMode.RedDark => full ? blue : (first ? red : dark),
                _ => Color.magenta, // invalid
            };
        }
    }

    private void UpdatePersistentHp() {
        if (!Config.PersistentHp) {
            return;
        }

        var chuds = this.crosshairReference.chuds;
        var hp = chuds[1];
        var overheal = chuds[7];
        var stfa = hp.GetComponent<SliderToFillAmount>();
        if (hp.fillAmount < stfa.maxFill || overheal.fillAmount > 0) {
            // this value is shared between both HP sliders
            // it would take much more hackery to only persist overheal (it is possible, though)
            stfa.mama.fadeOutTime = 2f;
        }
    }

    public void ApplyHiVisOverhealSettings() {
        var chuds = this.crosshairReference.chuds;
        var circles = this.crosshairReference.circles;
        var overheal = chuds[7];
        var overhealDmg = chuds[6];

        if (!Config.HiVisOverheal) {
            overheal.color = new Color(0.703f, 1, 0.704f); // not sure where this is originally defined
            // everything else already got updated by Crosshair.CheckCrossHair
            return;
        }

        Sprite circle;
        switch (PrefsManager.Instance.GetInt("crossHairHud")) {
            case 1: circle = circles[2]; break; // thick covering thin
            case 2: circle = circles[3]; break; // extra thicc covering medium
            case 3: circle = circles[0]; break; // thin stripe on thick
            case 4: circle = circles[1]; break; // medium stripe on extra thicc
            default: return; // invalid value, or pref is zero (crosshair HUD disabled)
        }

        overhealDmg.sprite = overheal.sprite = circle;
        overheal.color = Color.green * 0.65f;
    }

    private bool TryInit() {
        if (this.initialized) {
            return true;
        }

        var powerUpMeter = PowerUpMeter.Instance;

        if (powerUpMeter == null) {
            return false;
        }

        this.crosshairReference = CanvasController.Instance.crosshair;

        this.InitWheel(powerUpMeter);

        log.LogInfo("hud initialized");
        this.initialized = true;
        return true;
    }

    private void InitWheel(PowerUpMeter powerUpMeter) {
        var wheel = new GameObject("Wheel");

        var img = wheel.AddComponent<Image>();
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillAmount = 0.5f;
        img.sprite = powerUpMeter.GetComponentInChildren<Image>().sprite;

        var rt = this.InitTransform(wheel);
        rt.sizeDelta = new(64, 64);
        rt.anchoredPosition = new(0, 0);
        rt.Rotate(0, 0, 180);

        // move the powerup meter so the two rings aren't fighting for space
        powerUpMeter.GetComponent<RectTransform>().sizeDelta = new(69, 69);

        var slider = wheel.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 4;

        var stfa = wheel.AddComponent<SliderToFillAmount>();
        stfa.targetSlider = slider;
        stfa.maxFill = 1;

        var fob = wheel.AddComponent<FadeOutBars>();
        fob.fadeOutTime = Config.WheelFadeTime;

        this.wheel = wheel;
    }

    private RectTransform InitTransform(GameObject obj) {
        var rt = obj.GetComponent<RectTransform>();
        rt.parent = this.crosshairReference.transform;
        rt.localScale = Vector3.one * 2 / 3;
        rt.anchorMin = rt.anchorMax = new(0.5f, 0.5f);
        return rt;
    }
}
