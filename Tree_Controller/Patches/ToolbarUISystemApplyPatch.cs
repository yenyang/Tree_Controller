﻿// <copyright file="ToolbarUISystemApplyPatch.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Patches
{
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI.InGame;
    using HarmonyLib;
    using Tree_Controller.Tools;
    using Unity.Entities;

    /// <summary>
    /// Patches ToolbarUISystemApplyPatch so that additionally selected prefabs can also show as selected.
    /// </summary>
    [HarmonyPatch(typeof(ToolbarUISystem), "Apply")]
    public class ToolbarUISystemApplyPatch
    {
        /// <summary>
        /// Patches ToolbarUISystemApplyPatch so that additionally selected prefabs can also show as selected.
        /// </summary>
        /// <param name="themeEntity">Theme entity for prefab selection.</param>
        /// <param name="assetMenuEntity">Not needed assetMenuEntity.</param>
        /// <param name="assetCategoryEntity">Not needed assetCategoryEntity.</param>
        /// <param name="assetEntity">Not needed assetEntity.</param>
        public static void Postfix(Entity themeEntity, Entity assetMenuEntity, Entity assetCategoryEntity, Entity assetEntity)
        {
            TreeControllerTool treeControllerTool = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeControllerTool>();
            ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            ObjectToolSystem objectToolSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ObjectToolSystem>();
            TreeControllerUISystem treeControllerUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeControllerUISystem>();
            if (toolSystem.activeTool != treeControllerTool && toolSystem.activeTool != objectToolSystem && toolSystem.activeTool.toolID != "Line Tool")
            {
                return;
            }

            if (toolSystem.activeTool == objectToolSystem && objectToolSystem.mode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            if (toolSystem.activeTool == objectToolSystem || toolSystem.activeTool.toolID == "Line Tool")
            {
                PrefabBase prefab = objectToolSystem.GetPrefab();
                if (prefab != null)
                {
                    PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
                    if (!prefabSystem.TryGetEntity(prefab, out Entity prefabEntity))
                    {
                        return;
                    }

                    if (prefabSystem.EntityManager.HasComponent<Vegetation>(prefabEntity) && treeControllerUISystem.ThemeEntity != themeEntity)
                    {
                        if (treeControllerUISystem.ThemeEntity != Entity.Null)
                        {
                            treeControllerUISystem.UpdateSelectionSet = true;
                        }

                        treeControllerUISystem.ThemeEntity = themeEntity;
                        TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} Setting UpdateSelectionSet to true while using {toolSystem.activeTool.toolID}.");
                    }
                }
            }
            else if (treeControllerUISystem.ThemeEntity != themeEntity)
            {
                if (treeControllerUISystem.ThemeEntity != Entity.Null)
                {
                    treeControllerUISystem.UpdateSelectionSet = true;
                }

                treeControllerUISystem.ThemeEntity = themeEntity;
                TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} Setting UpdateSelectionSet to true while using {toolSystem.activeTool.toolID}.");
            }
        }
    }
}
