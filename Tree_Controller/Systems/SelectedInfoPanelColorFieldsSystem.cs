// <copyright file="SelectedInfoPanelColorFieldsSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>
namespace Tree_Controller.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game.Common;
    using Game.Prefabs;
    using Game.Prefabs.Climate;
    using Game.Rendering;
    using Game.Simulation;
    using Game.Tools;
    using Tree_Controller.Domain;
    using Tree_Controller.Utils;
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
            m_ClimateSystem = World.GetOrCreateSystemManaged<ClimateSystem>();
            m_CurrentColorSet = CreateBinding("CurrentColorSet", new TCColorSet(default, default, default));
            CreateTrigger<int, UnityEngine.Color>("ChangeColor", ChangeColor);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
            visible = TreeControllerMod.Instance.Settings.ColorVariationSet == Settings.TreeControllerSettings.ColorVariationSetYYTC.Custom && EntityManager.HasComponent<Game.Objects.Plant>(selectedEntity) && !EntityManager.HasComponent<Evergreen>(selectedEntity);
            if (!visible)
            {
                return;
            }

            Entity currentClimate = m_ClimateSystem.currentClimate;
            if (currentClimate == Entity.Null)
            {
                return;
            }

            if (!m_PrefabSystem.TryGetPrefab(m_ClimateSystem.currentClimate, out ClimatePrefab climatePrefab))
            {
                return;
            }

            if (m_PreviouslySelectedEntity != selectedEntity)
            {
                if (!EntityManager.TryGetComponent(selectedEntity, out PrefabRef prefabRef))
                {
                    return;
                }

                if (!EntityManager.TryGetComponent(selectedEntity, out Game.Objects.Tree tree))
                {
                    return;
                }

                if (!EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: false, out DynamicBuffer<SubMesh> subMeshBuffer))
                {
                    return;
                }

                FoliageUtils.Season currentSeason = FoliageUtils.GetSeasonFromSeasonID(climatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID);

                if (!EntityManager.TryGetBuffer(subMeshBuffer[0].m_SubMesh, isReadOnly: true, out DynamicBuffer<ColorVariation> colorVariationBuffer) || colorVariationBuffer.Length < 4)
                {
                    return;
                }

                ColorSet colorSet = colorVariationBuffer[(int)currentSeason].m_ColorSet;

                m_CurrentColorSet.Value = new TCColorSet(colorSet);

                m_PreviouslySelectedEntity = selectedEntity;
            }
        }

        private void ChangeColor(int channel, UnityEngine.Color color)
        {
            Entity currentClimate = m_ClimateSystem.currentClimate;
            if (currentClimate == Entity.Null)
            {
                return;
            }

            if (!m_PrefabSystem.TryGetPrefab(m_ClimateSystem.currentClimate, out ClimatePrefab climatePrefab))
            {
                return;
            }

            if (!EntityManager.TryGetComponent(selectedEntity, out PrefabRef prefabRef))
            {
                return;
            }

            if (!EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: false, out DynamicBuffer<SubMesh> subMeshBuffer))
            {
                return;
            }

            FoliageUtils.Season currentSeason = FoliageUtils.GetSeasonFromSeasonID(climatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID);

            for (int i = 0; i <= 3; i++)
            {
                if (!EntityManager.TryGetBuffer(subMeshBuffer[i].m_SubMesh, isReadOnly: false, out DynamicBuffer<ColorVariation> colorVariationBuffer) || colorVariationBuffer.Length < 4)
                {
                    return;
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

    }
}
