﻿// <copyright file="TreeControllerSettings.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Settings
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Game.UI;
    using Tree_Controller.Systems;
    using Unity.Entities;

    /// <summary>
    /// The mod settings for the Tree Controller Mod.
    /// </summary>
    [FileLocation("Mods_Yenyang_Tree_Controller")]
    [SettingsUITabOrder(General, WindTab)]
    [SettingsUIGroupOrder(Stable, DisableWinds, GlobalWindGroup, Remove, Reset, Info)]
    [SettingsUIMouseAction(TreeControllerMod.ApplyMimicAction, "TreeControllerTool")]
    [SettingsUIMouseAction(TreeControllerMod.SecondaryApplyMimicAction, "TreeControllerTool")]
    public class TreeControllerSettings : ModSetting
    {
        /// <summary>
        /// General Tree Controller Settings.
        /// </summary>
        public const string General = "General";

        /// <summary>
        /// Tree Wind controller settings.
        /// </summary>
        public const string WindTab = "Wind";

        // Groups
        private const string GlobalWindGroup = "Global Wind Settings";
        private const string DisableWinds = "DisableWinds";
        private const string Reset = "Reset";
        private const string Remove = "Remove";
        private const string Stable = "Stable";
        private const string Info = "Version";

        private ReloadFoliageColorDataSystem m_ReloadFoliageColorDataSystem;
        private DestroyFoliageSystem m_DestroyFoliageSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeControllerSettings"/> class.
        /// </summary>
        /// <param name="mod">TreeControllerMod.</param>
        public TreeControllerSettings(IMod mod)
            : base(mod)
        {
            SetDefaults();
            RegisterEventListener();
        }

        /// <summary>
        /// An enum for choosing a set of ColorVariations for seasonal Tree Foliage.
        /// </summary>
        public enum ColorVariationSetYYTC
        {
            /// <summary>
            /// Use the game's vanilla seasonal tree foliage.
            /// </summary>
            Vanilla,

            /// <summary>
            /// Use Yenyangs researched seasonal tree foliage.
            /// </summary>
            Yenyangs,

            /// <summary>
            /// Use Spring colors for every season.
            /// </summary>
            Spring,

            /// <summary>
            /// Use Autumn colors for every season.
            /// </summary>
            Autumn,
        }

        /// <summary>
        /// An enum for choosing the age selection
        /// </summary>
        public enum AgeSelectionOptions
        {
            /// <summary>
            /// random selection with equal weight.
            /// </summary>
            RandomEqualWeight,

            /// <summary>
            /// Uses vanilla weight for randomly selecting trees.
            /// </summary>
            RandomWeighted,
        }

        /// <summary>
        /// Gets or sets a value indicating whether Deciduous trees use Dead model during winter.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public bool UseDeadModelDuringWinter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tree growth is disabled globally.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public bool DisableTreeGrowth { get; set; }

        /// <summary>
        /// Gets or sets a enum that defines the selection for Age Selection.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public AgeSelectionOptions AgeSelectionTechnique { get; set; }

        /// <summary>
        /// Gets or sets a enum that defines the type of Seasonal foliage color set preference.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public ColorVariationSetYYTC ColorVariationSet { get; set; }

        /// <summary>
        /// Sets a value indicating whether the mod needs to safely remove components and reset models.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(General, Remove)]
        public bool SafelyRemoveButton
        {
            set
            {
                UseDeadModelDuringWinter = false;
            }
        }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the Mod.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(General, Reset)]
        public bool ResetGeneralSettings
        {
            set
            {
                DisableTreeGrowth = false;
                ColorVariationSet = ColorVariationSetYYTC.Vanilla;
                UseDeadModelDuringWinter = false;
                AgeSelectionTechnique = AgeSelectionOptions.RandomWeighted;
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Sets a value indicating whether: a button for destroying all foliage in the current world.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(General, Remove)]
        public bool DestroyFoliageSettings
        {
            set
            {
                m_DestroyFoliageSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<DestroyFoliageSystem>();
                m_DestroyFoliageSystem.Enabled = true;
            }
        }

        /// <summary>
        /// Gets a value indicating the version.
        /// </summary>
        [SettingsUISection(General, Info)]
        public string Version => TreeControllerMod.Instance.Version;

        /// <summary>
        /// Gets or sets hidden keybinding for apply action.
        /// </summary>
        [SettingsUIMouseBinding(TreeControllerMod.ApplyMimicAction)]
        [SettingsUIBindingMimic(InputManager.kToolMap, "Apply")]
        [SettingsUIHidden]
        public ProxyBinding ApplyMimic { get; set; }

        /// <summary>
        /// Gets or sets hidden keybinding for secondary apply action.
        /// </summary>
        [SettingsUIMouseBinding(TreeControllerMod.SecondaryApplyMimicAction)]
        [SettingsUIBindingMimic(InputManager.kToolMap, "Secondary Apply")]
        [SettingsUIHidden]
        public ProxyBinding SecondaryApplyMimic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether wind is enabled.
        /// </summary>
        [SettingsUISection(WindTab, DisableWinds)]
        public bool WindEnabled { get; set; }

        // Global Wind Settings

        /// <summary>
        /// Gets or sets a value indicating the wind global strength.
        /// </summary>
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 3f, step = 0.1f, unit = Unit.kPercentage)]
        public float WindGlobalStrength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind gloabal strength 2.
        /// </summary>
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 3f, step = 0.1f, unit = Unit.kPercentage)]
        public float WindGlobalStrength2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind direction.
        /// </summary>
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 360f, step = 1f, unit = Unit.kAngle)]
        public float WindDirection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind direction variance.
        /// </summary>
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 90f, step = 1f, unit = Unit.kAngle)]
        public float WindDirectionVariance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind direction variance period.
        /// </summary>
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0.01f, max = 120f, step = 0.1f, unit = Unit.kPercentage)]
        public float WindDirectionVariancePeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind interpolation duration.
        /// </summary>
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0.0001f, max = 5f, step = 0.01f, unit = Unit.kPercentage)]
        public float WindInterpolationDuration { get; set; }

        /// <summary>
        /// Gets a value indicating the author of Tree Wind Controller.
        /// </summary>
        [SettingsUISection(WindTab, Info)]
        [SettingsUIMultilineText]
        public string Author { get; }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the wind tab.
        /// </summary>
        [SettingsUIButton]
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUIConfirmation]
        public bool ResetWindSettings
        {
            set
            {
                WindEnabled = true;
                WindGlobalStrength = 1f;
                WindGlobalStrength2 = 1f;
                WindDirection = 65f;
                WindDirectionVariance = 25f;
                WindDirectionVariancePeriod = 15f;
                WindInterpolationDuration = 0.5f;
                ApplyAndSave();
            }
        }

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            DisableTreeGrowth = false;
            ColorVariationSet = ColorVariationSetYYTC.Vanilla;
            UseDeadModelDuringWinter = false;
            AgeSelectionTechnique = AgeSelectionOptions.RandomWeighted;
            WindEnabled = true;
            WindGlobalStrength = 1f;
            WindGlobalStrength2 = 1f;
            WindDirection = 65f;
            WindDirectionVariance = 25f;
            WindInterpolationDuration = 0.5f;
        }

        /// <summary>
        /// Attach event listener method to inherited event delegate.
        /// </summary>
        public void RegisterEventListener()
        {
            // Listen to event when any settings are applied.
            onSettingsApplied += Setting_SettingsAppliedListener;
        }

        /// <summary>
        /// Method delegate for handling onSettingsApplied events.
        /// </summary>
        /// <param name="setting">Setting object.</param>
        public void Setting_SettingsAppliedListener(Setting setting)
        {
            TreeControllerMod.Instance.Logger.Debug($"Setting triggered or updated: {setting}.");
        }
    }
}
