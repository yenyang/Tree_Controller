// <copyright file="TreeControllerTooltipSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Tools
{
    using System.Collections.Generic;
    using Game.Tools;
    using Game.UI.Localization;
    using Game.UI.Tooltip;
    using Unity.Entities;

    /// <summary>
    /// Tooltip system for Tree Controller Tool.
    /// </summary>
    public partial class TreeControllerTooltipSystem : TooltipSystemBase
    {
        /// <summary>
        /// A dictionary of ToolMode Tooltips.
        /// </summary>
        private readonly Dictionary<Selection, StringTooltip> m_ToolModeToolTipsDictionary = new ()
        {
             { Selection.Map, new StringTooltip() { path = "Options.TOOLTIPYYTC[WholeMapApply]", value = LocalizedString.IdWithFallback("Options.TOOLTIPYYTC[WholeMapApply]", "Right Click to Apply.") } },
        };

        private ToolSystem m_ToolSystem;
        private TreeControllerTool m_TreeControllerTool;
        private TreeControllerUISystem m_TreeControllerUISystem;
        private StringTooltip m_ToolModeTooltip;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeControllerTooltipSystem"/> class.
        /// </summary>
        public TreeControllerTooltipSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_TreeControllerTool = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeControllerTool>();
            m_TreeControllerUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeControllerUISystem>();
            m_ToolModeTooltip = new StringTooltip();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_ToolSystem.activeTool != m_TreeControllerTool)
            {
                return;
            }

            if (m_ToolModeToolTipsDictionary.ContainsKey(m_TreeControllerUISystem.SelectionMode))
            {
                m_ToolModeTooltip = m_ToolModeToolTipsDictionary[m_TreeControllerUISystem.SelectionMode];
                AddMouseTooltip(m_ToolModeTooltip);
            }
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
