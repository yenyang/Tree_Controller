// <copyright file="TreeControllerSettings.cs" company="Yenyangs Mods. MIT License">
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
    [SettingsUIGroupOrder(Stable, DisableWinds, Override, Remove, Reset, Info)]
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
        private const string Override = "Override";
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
        /// An enum for how mod affects wind on vegetatoin.
        /// </summary>
        public enum WindOptions
        {
            /// <summary>
            /// Use the game's vanilla wind shader values.
            /// </summary>
            Vanilla,

            /// <summary>
            /// Disable When completely.
            /// </summary>
            Disabled,

            /// <summary>
            /// Override Vanilla wind shader.
            /// </summary>
            Override,
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
        /// Gets or sets a value indicating whether to include stumps.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public bool IncludeStumps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to increase brush strength at 100%.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public bool FasterFullBrushStrength { get; set; }

        /// <summary>
        /// Gets or sets a enum that defines the type of Seasonal foliage color set preference.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public ColorVariationSetYYTC ColorVariationSet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to make vegetation costs free.
        /// </summary>
        [SettingsUISection(General, Stable)]
        [SettingsUISetter(typeof(TreeControllerSettings), nameof(ToggleVegetationCost))]
        public bool FreeVegetation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to constrain the brush.
        /// </summary>
        [SettingsUISection(General, Stable)]
        public bool ConstrainBrush { get; set; }

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
                FreeVegetation = false;
                ConstrainBrush = true;
                IncludeStumps = false;
                FasterFullBrushStrength = false;
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
        [SettingsUISection(WindTab, Stable)]
        public WindOptions SelectedWindOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether wind is enabled.
        /// </summary>
        [SettingsUISection(WindTab, Stable)]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(WindDisabled))]
        public bool DisableWindWhenPaused { get; set; }

        // Global Wind Settings

        /// <summary>
        /// Gets or sets a value indicating the wind global strength.
        /// </summary>
        [SettingsUISection(WindTab, Override)]
        [SettingsUISlider(min = 0f, max = 3f, step = 0.1f, unit = Unit.kFloatSingleFraction)]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(OverrideWind), true)]
        public float WindGlobalStrength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind gloabal strength 2.
        /// </summary>
        [SettingsUISection(WindTab, Override)]
        [SettingsUISlider(min = 0f, max = 3f, step = 0.1f, unit = Unit.kFloatSingleFraction)]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(OverrideWind), true)]
        public float WindGlobalStrength2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind direction.
        /// </summary>
        [SettingsUISection(WindTab, Override)]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(OverrideWind), true)]
        [SettingsUISlider(min = 0f, max = 360f, step = 1f, unit = Unit.kAngle)]
        public float WindDirection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind direction variance.
        /// </summary>
        [SettingsUISection(WindTab, Override)]
        [SettingsUISlider(min = 0f, max = 90f, step = 1f, unit = Unit.kAngle)]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(OverrideWind), true)]
        public float WindDirectionVariance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind direction variance period.
        /// </summary>
        [SettingsUISection(WindTab, Override)]
        [SettingsUISlider(min = 0.01f, max = 20f, step = 0.1f, unit = Unit.kFloatSingleFraction)]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(OverrideWind), true)]
        public float WindDirectionVariancePeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wind interpolation duration.
        /// </summary>
        [SettingsUISection(WindTab, Override)]
        [SettingsUISlider(min = 0.0001f, max = 5f, step = 0.01f, unit = Unit.kFloatSingleFraction)]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(OverrideWind), true)]
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
        [SettingsUISection(WindTab, Override)]
        [SettingsUIConfirmation]
        [SettingsUIDisableByCondition(typeof(TreeControllerSettings), nameof(OverrideWind), true)]
        public bool ResetWindSliders
        {
            set
            {
                WindGlobalStrength = 1f;
                WindGlobalStrength2 = 1f;
                WindDirection = 65f;
                WindDirectionVariance = 25f;
                WindDirectionVariancePeriod = 15f;
                WindInterpolationDuration = 0.5f;
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Sets a value indicating whether: a button for Resetting the settings for the wind tab.
        /// </summary>
        [SettingsUIButton]
        [SettingsUISection(WindTab, Reset)]
        [SettingsUIConfirmation]
        public bool ResetWindSettings
        {
            set
            {
                SelectedWindOption = WindOptions.Vanilla;
                DisableWindWhenPaused = false;
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
            SelectedWindOption = WindOptions.Vanilla;
            DisableWindWhenPaused = false;
            WindGlobalStrength = 1f;
            WindGlobalStrength2 = 1f;
            WindDirection = 65f;
            WindDirectionVariance = 25f;
            WindDirectionVariancePeriod = 15f;
            WindInterpolationDuration = 0.5f;
            FreeVegetation = false;
            IncludeStumps = false;
            ConstrainBrush = true;
            FasterFullBrushStrength = false;
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

        /// <summary>
        /// Checks whether selected wind option is override.
        /// </summary>
        /// <returns>True if override, false if not.</returns>
        public bool OverrideWind() => SelectedWindOption == WindOptions.Override;


        /// <summary>
        /// Checks whether selected wind option is disabled.
        /// </summary>
        /// <returns>True if disabled, false if not.</returns>
        public bool WindDisabled() => SelectedWindOption == WindOptions.Disabled;

        /// <summary>
        /// Toggles the vegetation cost by calling public methods from FreeVegetationSystem.
        /// </summary>
        /// <param name="free">Should vegeation be free or regular cost.</param>
        public void ToggleVegetationCost(bool free)
        {
            ModifyVegetationPrefabsSystem freeVegetationSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ModifyVegetationPrefabsSystem>();
            if (free)
            {
                freeVegetationSystem.SetVegetationCostsToZero();
            }
            else
            {
                freeVegetationSystem.ResetVegetationCosts();
            }
        }
    }
}
