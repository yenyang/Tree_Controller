// <copyright file="SelectedInfoPanelColorFieldsSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>
namespace Tree_Controller.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Colossal.UI.Binding;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Prefabs.Climate;
    using Game.Rendering;
    using Game.Simulation;
    using Game.Tools;
    using System;
    using Tree_Controller.Domain;
    using Tree_Controller.Utils;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Addes toggles to selected info panel for entites that can receive Anarchy mod components.
    /// </summary>
    public partial class SelectedInfoPanelColorFieldsSystem : ExtendedInfoSectionBase
    {
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private Entity m_PreviouslySelectedEntity = Entity.Null;
        private ClimateSystem m_ClimateSystem;
        private ValueBindingHelper<TCColorSet> m_CurrentColorSet;
        private ValueBindingHelper<bool> m_MatchesSavedColorSet;
        private ClimatePrefab m_ClimatePrefab;
        private ReloadFoliageColorDataSystem m_ReloadFoliageColorDataSystem;

        /// <inheritdoc/>
        protected override string group => TreeControllerMod.Id;

        /// <inheritdoc/>
        public override void OnWriteProperties(IJsonWriter writer)
        {
        }

        /// <inheritdoc/>
        protected override void OnProcess()
        {
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_InfoUISystem.AddMiddleSection(this);
            m_Log = TreeControllerMod.Instance.Logger;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_Log.Info($"{nameof(SelectedInfoPanelColorFieldsSystem)}.{nameof(OnCreate)}");
            m_ReloadFoliageColorDataSystem = World.GetOrCreateSystemManaged<ReloadFoliageColorDataSystem>();
            m_ClimateSystem = World.GetOrCreateSystemManaged<ClimateSystem>();
            m_CurrentColorSet = CreateBinding("CurrentColorSet", new TCColorSet(default, default, default));
            m_MatchesSavedColorSet = CreateBinding("MatchesSavedColorSet", true);
            CreateTrigger<int, UnityEngine.Color>("ChangeColor", ChangeColor);
            CreateTrigger("SaveColorSet", SaveColorSet);
            CreateTrigger("ResetColorSet", ResetColorSet);
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (mode.IsGame())
            {
                GetClimatePrefab();
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
            visible = TreeControllerMod.Instance.Settings.ColorVariationSet == Settings.TreeControllerSettings.ColorVariationSetYYTC.Custom && EntityManager.HasComponent<Game.Objects.Plant>(selectedEntity);
            if (!visible)
            {
                return;
            }

            if (m_ClimatePrefab is null)
            {
                if (GetClimatePrefab() == false)
                {
                    visible = false;
                    return;
                }
            }

            if (m_PreviouslySelectedEntity != selectedEntity && selectedEntity != Entity.Null)
            {
                if (!EntityManager.TryGetComponent(selectedEntity, out PrefabRef prefabRef))
                {
                    return;
                }

                if (EntityManager.HasComponent<Evergreen>(prefabRef.m_Prefab))
                {
                    visible = false;
                    return;
                }

                if (!EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer))
                {
                    return;
                }

                FoliageUtils.Season currentSeason = FoliageUtils.GetSeasonFromSeasonID(m_ClimatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID);

                if (!EntityManager.TryGetBuffer(subMeshBuffer[0].m_SubMesh, isReadOnly: true, out DynamicBuffer<ColorVariation> colorVariationBuffer) || colorVariationBuffer.Length < 4)
                {
                    return;
                }

                ColorSet colorSet = colorVariationBuffer[(int)currentSeason].m_ColorSet;

                m_CurrentColorSet.Value = new TCColorSet(colorSet);

                m_PreviouslySelectedEntity = selectedEntity;

                if (!m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out PrefabBase prefabBase))
                {
                    return;
                }

                m_MatchesSavedColorSet.Value = m_ReloadFoliageColorDataSystem.MatchesSavedColorSet(colorSet, prefabBase.GetPrefabID(), currentSeason);
            }
        }

        private bool GetClimatePrefab()
        {
            Entity currentClimate = m_ClimateSystem.currentClimate;
            if (currentClimate == Entity.Null)
            {
                m_Log.Warn($"{nameof(SelectedInfoPanelColorFieldsSystem)}.{nameof(GetClimatePrefab)} couldn't find climate entity.");
                return false;
            }

            if (!m_PrefabSystem.TryGetPrefab(m_ClimateSystem.currentClimate, out m_ClimatePrefab))
            {
                m_Log.Warn($"{nameof(SelectedInfoPanelColorFieldsSystem)}.{nameof(GetClimatePrefab)} couldn't find climate prefab.");
                return false;
            }

            return true;
        }

        private void ChangeColor(int channel, UnityEngine.Color color)
        {
            if (!EntityManager.TryGetComponent(selectedEntity, out PrefabRef prefabRef))
            {
                return;
            }

            if (!EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer))
            {
                return;
            }

            FoliageUtils.Season currentSeason = FoliageUtils.GetSeasonFromSeasonID(m_ClimatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID);

            for (int i = 0; i < Math.Min(4, subMeshBuffer.Length); i++)
            {
                if (!EntityManager.TryGetBuffer(subMeshBuffer[i].m_SubMesh, isReadOnly: false, out DynamicBuffer<ColorVariation> colorVariationBuffer) || colorVariationBuffer.Length < 4)
                {
                    continue;
                }

                ColorVariation colorVariation = colorVariationBuffer[(int)currentSeason];

                if (channel == 0)
                {
                    colorVariation.m_ColorSet.m_Channel0 = color;
                }
                else if (channel == 1)
                {
                    colorVariation.m_ColorSet.m_Channel1 = color;
                }
                else if (channel == 2)
                {
                    colorVariation.m_ColorSet.m_Channel2 = color;
                }

                colorVariationBuffer[(int)currentSeason] = colorVariation;
            }

            m_Log.Debug($"Changed Color {channel} {color}");
            m_PreviouslySelectedEntity = Entity.Null;
            EntityManager.AddComponent<BatchesUpdated>(selectedEntity);
        }

        private void SaveColorSet()
        {
            if (!EntityManager.TryGetComponent(selectedEntity, out PrefabRef prefabRef))
            {
                return;
            }

            if (!m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out PrefabBase prefabBase))
            {
                return;
            }

            if (!EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer))
            {
                return;
            }

            FoliageUtils.Season currentSeason = FoliageUtils.GetSeasonFromSeasonID(m_ClimatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID);

            if (!EntityManager.TryGetBuffer(subMeshBuffer[0].m_SubMesh, isReadOnly: true, out DynamicBuffer<ColorVariation> colorVariationBuffer) || colorVariationBuffer.Length < 4)
            {
                return;
            }

            ColorSet colorSet = colorVariationBuffer[(int)currentSeason].m_ColorSet;

            m_ReloadFoliageColorDataSystem.TrySaveCustomColorSet(colorSet, prefabBase.GetPrefabID(), currentSeason);
            m_PreviouslySelectedEntity = Entity.Null;

            EntityQuery plantQuery = SystemAPI.QueryBuilder()
               .WithAll<Game.Objects.Plant>()
               .WithNone<Deleted, Game.Common.Overridden, Game.Tools.Temp, Evergreen>()
               .Build();

            NativeArray<Entity> entities = plantQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity e in entities)
            {
                if (EntityManager.TryGetComponent(e, out PrefabRef currentPrefabRef) && currentPrefabRef.m_Prefab == prefabRef.m_Prefab)
                {
                    EntityManager.AddComponent<BatchesUpdated>(e);
                }
            }
        }

        private void ResetColorSet()
        {
            if (!EntityManager.TryGetComponent(selectedEntity, out PrefabRef prefabRef))
            {
                return;
            }

            if (!m_PrefabSystem.TryGetPrefab(prefabRef.m_Prefab, out PrefabBase prefabBase))
            {
                return;
            }

            FoliageUtils.Season currentSeason = FoliageUtils.GetSeasonFromSeasonID(m_ClimatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID);

            if (!m_ReloadFoliageColorDataSystem.TryGetVanillaColorSet(prefabBase.GetPrefabID(), currentSeason, out ColorSet vanillaColorSet))
            {
                return;
            }

            ChangeColor(0, vanillaColorSet.m_Channel0);
            ChangeColor(1, vanillaColorSet.m_Channel1);
            ChangeColor(2, vanillaColorSet.m_Channel2);
            m_PreviouslySelectedEntity = Entity.Null;

            m_ReloadFoliageColorDataSystem.TrySaveCustomColorSet(vanillaColorSet, prefabBase.GetPrefabID(), currentSeason);

            EntityQuery plantQuery = SystemAPI.QueryBuilder()
               .WithAll<Game.Objects.Plant>()
               .WithNone<Deleted, Game.Common.Overridden, Game.Tools.Temp, Evergreen>()
               .Build();

            NativeArray<Entity> entities = plantQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity e in entities)
            {
                if (EntityManager.TryGetComponent(e, out PrefabRef currentPrefabRef) && currentPrefabRef.m_Prefab == prefabRef.m_Prefab)
                {
                    EntityManager.AddComponent<BatchesUpdated>(e);
                }
            }
        }
    }
}
