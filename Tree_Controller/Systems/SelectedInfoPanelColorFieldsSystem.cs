// <copyright file="SelectedInfoPanelColorFieldsSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>
namespace Tree_Controller.Systems
{
    using Colossal.Logging;
    using Colossal.UI.Binding;
    using Game.Tools;
    using Tree_Controller.Utils;
    using Unity.Entities;

    /// <summary>
    /// Addes toggles to selected info panel for entites that can receive Anarchy mod components.
    /// </summary>
    public partial class SelectedInfoPanelColorFieldsSystem : ExtendedInfoSectionBase
    {
        private ILog m_Log;
        private ToolSystem m_ToolSystem;

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

        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();
            visible = EntityManager.HasComponent<Game.Objects.Plant>(selectedEntity) && TreeControllerMod.Instance.Settings.ColorVariationSet == Settings.TreeControllerSettings.ColorVariationSetYYTC.Custom;
            if (visible)
            {
            }
        }

    }
}
