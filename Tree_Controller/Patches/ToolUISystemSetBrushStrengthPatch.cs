// <copyright file="ToolUISystemSetBrushStrengthPatch.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Patches
{
    using Game.Modding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI.InGame;
    using HarmonyLib;
    using Tree_Controller.Tools;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Patches ToolUISystemSetBrushStrengthPatch to set a different maximum brush strength.
    /// </summary>
    [HarmonyPatch(typeof(ToolUISystem), "SetBrushStrength")]
    public class ToolUISystemSetBrushStrengthPatch
    {
        /// <summary>
        /// Patches ToolUISystemSetBrushStrengthPatch to set a different maximum brush strength.
        /// </summary>
        /// <param name="strength">brush strength.</param>
        /// <returns>True if skip vanilla method. False if not.</returns>
        public static bool Prefix(float strength)
        {
            if (!TreeControllerMod.Instance.Settings.FasterFullBrushStrength)
            {
                return true;
            }

            ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolUISystem)}.{nameof(ToolUISystemSetBrushStrengthPatch)} toolSystem.activeTool.brushStrength = {toolSystem.activeTool.brushStrength}");
            TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolUISystem)}.{nameof(ToolUISystemSetBrushStrengthPatch)} strength = {strength}");
            PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();

            if (toolSystem.activePrefab is null)
            {
                return true;
            }

            if (!prefabSystem.TryGetEntity(toolSystem.activePrefab, out Entity prefabEntity) || !toolSystem.EntityManager.HasComponent<Vegetation>(prefabEntity))
            {
                return true;
            }

            // If brush strength is being set to 100%, then override strength according to settings and skip vanilla method.
            if (toolSystem.activeTool.brushStrength < strength && strength == 1f)
            {
                toolSystem.activeTool.brushStrength = TreeControllerUISystem.MaxBrushStrength;
                TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolUISystem)}.{nameof(ToolUISystemSetBrushStrengthPatch)} set brush strength to {toolSystem.activeTool.brushStrength}");
                return false;
            }
            else if (toolSystem.activeTool.brushStrength == TreeControllerUISystem.MaxBrushStrength && strength == 1f)
            {
                toolSystem.activeTool.brushStrength = 0.95f;
                TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolUISystem)}.{nameof(ToolUISystemSetBrushStrengthPatch)} set brush strength to {toolSystem.activeTool.brushStrength}");
                return false;
            }

            // If not then run vanilla method.
            return true;
        }
    }
}
