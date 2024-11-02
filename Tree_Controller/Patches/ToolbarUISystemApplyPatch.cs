// <copyright file="ToolbarUISystemApplyPatch.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Patches
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI.InGame;
    using HarmonyLib;
    using System.Collections.Generic;
    using Tree_Controller.Tools;
    using Tree_Controller.Utils;
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
        /// <param name="themes">list of themes</param>
        /// <param name="packs">list of pcaks</param>=
        /// <param name="assetMenuEntity">Not needed assetMenuEntity.</param>
        /// <param name="assetCategoryEntity">Not needed assetCategoryEntity.</param>
        /// <param name="assetEntity">Not needed assetEntity.</param>
        public static void Postfix(List<Entity> themes, List<Entity> packs, Entity assetMenuEntity, Entity assetCategoryEntity, Entity assetEntity)
        {
            TreeControllerTool treeControllerTool = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeControllerTool>();
            ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            ObjectToolSystem objectToolSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ObjectToolSystem>();
            TreeControllerUISystem treeControllerUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeControllerUISystem>();
            ToolbarUISystem toolbarUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolbarUISystem>();
            if (toolSystem.activeTool != treeControllerTool && toolSystem.activeTool != objectToolSystem && toolSystem.activeTool.toolID != null && toolSystem.activeTool.toolID != "Line Tool")
            {
                return;
            }

            /*
            if (toolSystem.activeTool == objectToolSystem && objectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }*/

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

                    if (prefabSystem.EntityManager.HasComponent<Vegetation>(prefabEntity) && treeControllerUISystem.ThemeEntities.Count != themes.Count)
                    {
                        if (treeControllerUISystem.ThemeEntities.Count != 0)
                        {
                            treeControllerUISystem.UpdateSelectionSet = true;
                        }

                        treeControllerUISystem.ThemeEntities = themes;
                        TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} Setting UpdateSelectionSet to true while using {toolSystem.activeTool.toolID}.");
                    }

                    if (toolSystem.activeTool == objectToolSystem && prefabSystem.EntityManager.HasComponent<TreeData>(prefabEntity))
                    {
                        TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} checking age mask binding");
                        var ageMaskBindingVar = toolbarUISystem.GetMemberValue("m_AgeMaskBinding");
                        if (ageMaskBindingVar is ValueBinding<int>)
                        {
                            TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} setting age mask binding");
                            ValueBinding<int> ageMaskBinding = ageMaskBindingVar as ValueBinding<int>;
                            ageMaskBinding.Update(0);

                            TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} finished age mask binding");
                        }
                    }
                }
            }
            else if (treeControllerUISystem.ThemeEntities.Count != themes.Count)
            {
                if (treeControllerUISystem.ThemeEntities.Count != 0)
                {
                    treeControllerUISystem.UpdateSelectionSet = true;
                }

                treeControllerUISystem.ThemeEntities = themes;
                TreeControllerMod.Instance.Logger.Debug($"{nameof(ToolbarUISystemApplyPatch)}.{nameof(Postfix)} Setting UpdateSelectionSet to true while using {toolSystem.activeTool.toolID}.");
            }
        }
    }
}
