using BepInEx.Configuration;

namespace RcHud;

#nullable disable

public enum StaminaColorMode { Vanilla, Static, AllRed, AllDark, RedDark }

public static class Config {
    private static ConfigEntry<bool>
        hiVisOverheal,
        persistentHp;

    public static bool HiVisOverheal => hiVisOverheal.Value;
    public static bool PersistentHp => persistentHp.Value;

    private static ConfigEntry<StaminaColorMode> staminaColorMode;
    public static StaminaColorMode StaminaColorMode => staminaColorMode.Value;

    private static ConfigEntry<float>
        wheelFade;

    public static float WheelFadeTime => wheelFade.Value;

    public static void Init(ConfigFile cfg) {
        string[] scmDescriptions = [
            "Vanilla: vanilla behavior (first segment red while charging)",
            "Static: all segments blue, all the time",
            "AllRed: all segments red while charging",
            "AllDark: all segments dark blue while charging",
            "RedDark: first segment red while charging, other segments dark blue",
            "(the actual colors reflect game color settings)",
        ];

        staminaColorMode = cfg.Bind("Tweaks", "StaminaColorMode", StaminaColorMode.AllRed, string.Join("\n", scmDescriptions));
        hiVisOverheal = cfg.Bind("Tweaks", "HiVisOverheal", true, "Display overheal as a dark green ring with a different thickness");
        persistentHp = cfg.Bind("Tweaks", "PersistentHp", true, "Prevent HP wheel from fading if damaged or overhealed");

        wheelFade = cfg.Bind("FadeTime", "RailcannonMeter", 5.0f, AcceptableRange(0f, 30f));
    }

    private static ConfigDescription AcceptableRange<T>(T min, T max) where T : System.IComparable {
        return new("", new AcceptableValueRange<T>(min, max));
    }
}
