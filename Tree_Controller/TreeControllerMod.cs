﻿// <copyright file="TreeControllerMod.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Tree_Controller
{
    using System;
    using System.IO;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;
    using HarmonyLib;
    using Tree_Controller.Settings;
    using Tree_Controller.Systems;
    using Tree_Controller.Tools;

    /// <summary>
    /// Mod entry point.
    /// </summary>
    public class TreeControllerMod : IMod
    {
        /// <summary>
        /// Fake keybind action for apply.
        /// </summary>
        public const string ApplyMimicAction = "ApplyMimic";

        /// <summary>
        /// Fake keybind action for secondary apply.
        /// </summary>
        public const string SecondaryApplyMimicAction = "SecondaryApplyMimic";

        /// <summary>
        /// An id used for bindings between UI and C#.
        /// </summary>
        public static readonly string Id = "Tree_Controller";

        /// <summary>
        /// Gets the install folder for the mod.
        /// </summary>
        private static string m_modInstallFolder;

        private Harmony m_Harmony;

        /// <summary>
        /// Gets the static reference to the mod instance.
        /// </summary>
        public static TreeControllerMod Instance
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the Install Folder for the mod as a string.
        /// </summary>
        public static string ModInstallFolder
        {
            get
            {
                if (m_modInstallFolder is null)
                {
                    var thisFullName = Instance.GetType().Assembly.FullName;
                    ExecutableAsset thisInfo = AssetDatabase.global.GetAsset(SearchFilter<ExecutableAsset>.ByCondition(x => x.definition?.FullName == thisFullName)) ?? throw new Exception("This mod info was not found!!!!");
                    m_modInstallFolder = Path.GetDirectoryName(thisInfo.GetMeta().path);
                }

                return m_modInstallFolder;
            }
        }

        /// <summary>
        ///  Gets or sets the Mod Settings.
        /// </summary>
        internal TreeControllerSettings Settings { get; set; }

        /// <summary>
        /// Gets ILog for mod.
        /// </summary>
        internal ILog Logger { get; private set; }

        /// <summary>
        /// Creates logger, settings, systems, and harmony instance.
        /// </summary>
        /// <param name="updateSystem">Update system to add new systems to.</param>
        public void OnLoad(UpdateSystem updateSystem)
        {
            Instance = this;
            Logger = LogManager.GetLogger("Mods_Yenyang_Tree_Controller").SetShowsErrorsInUI(false);
#if DEBUG
            Logger.effectivenessLevel = Level.Debug;
#elif VERBOSE
            Logger.effectivenessLevel = Level.Verbose;
#else
            Logger.effectivenessLevel = Level.Info;
#endif
            Logger.Info($"[{nameof(TreeControllerMod)}] {nameof(OnLoad)}");
            Settings = new (this);
            Settings.RegisterKeyBindings();
            Settings.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings(nameof(TreeControllerMod), Settings, new TreeControllerSettings(this));
            Logger.Info($"[{nameof(TreeControllerMod)}] {nameof(OnLoad)} finished loading settings.");
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));
            Logger.Info($"[{nameof(TreeControllerMod)}] {nameof(OnLoad)} loaded localization for en-US.");
            Logger.Info($"{nameof(TreeControllerMod)}.{nameof(OnLoad)} Injecting Harmony Patches.");
            m_Harmony = new Harmony("Mods_Yenyang_Tree_Controller");
            m_Harmony.PatchAll();
            Logger.Info($"{nameof(TreeControllerMod)}.{nameof(OnLoad)} Injecting systems.");
            updateSystem.UpdateAt<TreeControllerTool>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateBefore<TreeObjectDefinitionSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateAt<TreeControllerUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<TreeControllerTooltipSystem>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateAt<ClearTreeControllerTool>(SystemUpdatePhase.ClearTool);
            updateSystem.UpdateBefore<FindTreesAndBushesSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateBefore<DeciduousSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<ReloadFoliageColorDataSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<ModifyTreeGrowthSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateBefore<SafelyRemoveSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateBefore<LumberSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<DetectAreaChangeSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<DestroyFoliageSystem>(SystemUpdatePhase.ToolUpdate);
            Logger.Info($"[{nameof(TreeControllerMod)}] {nameof(OnLoad)} finished systems");
        }

        /// <inheritdoc/>
        public void OnDispose()
        {
            Logger.Info("Disposing..");
            m_Harmony.UnpatchAll();
            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }

    }
}
