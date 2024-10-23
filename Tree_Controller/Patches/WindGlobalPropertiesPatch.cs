using Game.Rendering;
using HarmonyLib;
using TreeWindsController;
using UnityEngine.Rendering;

[HarmonyPatch(typeof(WindControl), "SetGlobalProperties")]
public static class WindGlobalPropertiesPatch
{
    static void Prefix(CommandBuffer cmd, WindVolumeComponent wind)
    {
        // Disable all wind if the wind system is disabled
        if (!WindControlSystem.Instance.windEnabled)
        {
            wind.windGlobalStrengthScale.Override(0);
            wind.windGlobalStrengthScale2.Override(0);
            return; // Skip further updates if wind is disabled
        }

        // Apply global wind settings directly from WindControlSystem
        var globalSettings = WindControlSystem.Instance.globalSettings;

        // Override the wind volume properties with the user-controlled values
        wind.windGlobalStrengthScale.Override(globalSettings.globalStrengthScale.value);
        wind.windGlobalStrengthScale2.Override(globalSettings.globalStrengthScale2.value);
        wind.windDirection.Override(globalSettings.windDirection.value);
        wind.windDirectionVariance.Override(globalSettings.windDirectionVariance.value);
        wind.windDirectionVariancePeriod.Override(globalSettings.windDirectionVariancePeriod.value);
        wind.windParameterInterpolationDuration.Override(globalSettings.interpolationDuration.value);

        // Additional wind settings, like gust control, could be added here if necessary
        // Example: Apply gust strength settings from grass or tree wind settings
        var grassSettings = WindControlSystem.Instance.grassSettings;
        var treeSettings = WindControlSystem.Instance.treeSettings;

        wind.windGustStrengthControl.value = grassSettings.gustStrengthControl.value;
        wind.windTreeGustStrengthControl.value = treeSettings.gustControl.value;
    }
}
