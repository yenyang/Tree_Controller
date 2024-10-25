// <copyright file="WindGlobalPropertiesPatch.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

using Game.Rendering;
using HarmonyLib;
using Tree_Controller;
using UnityEngine.Rendering;

/// <summary>
/// Patches Wind Control Set Global Properties to control wind parameters.
/// </summary>
[HarmonyPatch(typeof(WindControl), "SetGlobalProperties")]
public static class WindGlobalPropertiesPatch
{
    /// <summary>
    /// Prefix Patch to control wind parameters.
    /// </summary>
    /// <param name="cmd">command buffer.</param>
    /// <param name="wind">wind volume component.</param>
    public static void Prefix(CommandBuffer cmd, WindVolumeComponent wind)
    {
        // Disable all wind if the wind system is disabled
        if (!TreeControllerMod.Instance.Settings.WindEnabled)
        {
            wind.windGlobalStrengthScale.Override(0);
            wind.windGlobalStrengthScale2.Override(0);
            return; // Skip further updates if wind is disabled
        }

        // Override the wind volume properties with the user-controlled values
        wind.windGlobalStrengthScale.Override(TreeControllerMod.Instance.Settings.WindGlobalStrength);
        wind.windGlobalStrengthScale2.Override(TreeControllerMod.Instance.Settings.WindGlobalStrength2);
        wind.windDirection.Override(TreeControllerMod.Instance.Settings.WindDirection);
        wind.windDirectionVariance.Override(TreeControllerMod.Instance.Settings.WindDirectionVariance);
        wind.windDirectionVariancePeriod.Override(TreeControllerMod.Instance.Settings.WindDirectionVariancePeriod);
        wind.windParameterInterpolationDuration.Override(TreeControllerMod.Instance.Settings.WindInterpolationDuration);
    }
}
