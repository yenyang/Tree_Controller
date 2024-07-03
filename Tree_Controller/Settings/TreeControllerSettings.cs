// <copyright file="TreeControllerSettings.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Settings
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Tree_Controller.Systems;
    using Unity.Entities;

    /// <summary>
    /// The mod settings for the Anarchy Mod.
    /// </summary>
    [FileLocation("Mods_Yenyang_Tree_Controller")]
    [SettingsUIMouseAction(TreeControllerMod.ApplyMimicAction, "TreeControllerTool")]
    [SettingsUIMouseAction(TreeControllerMod.SecondaryApplyMimicAction, "TreeControllerTool")]
    public class TreeControllerSettings : ModSetting
    {
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
        public bool UseDeadModelDuringWinter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tree growth is disabled globally.
        /// </summary>
        public bool DisableTreeGrowth { get; set; }

        /// <summary>
        /// Gets or sets a enum that defines the selection for Age Selection.
        /// </summary>
        public AgeSelectionOptions AgeSelectionTechnique { get; set; }

        /// <summary>
        /// Gets or sets a enum that defines the type of Seasonal foliage color set preference.
        /// </summary>
        public ColorVariationSetYYTC ColorVariationSet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use random rotation while plopping trees.
        /// </summary>
        [SettingsUIHidden]
        public bool RandomRotation { get; set; }

        /// <summary>
        /// Sets a value indicating whether the mod needs to safely remove components and reset models.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
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
        public bool ResetModSettings
        {
            set
            {
                bool rotation = RandomRotation;
                SetDefaults();
                RandomRotation = rotation;
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Sets a value indicating whether: a button for destroying all foliage in the current world.
        /// </summary>
        [SettingsUIButton]
        [SettingsUIConfirmation]
        public bool DestroyFoliageSettings
        {
            set
            {
                m_DestroyFoliageSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<DestroyFoliageSystem>();
                m_DestroyFoliageSystem.Enabled = true;
            }
        }

        /// <summary>
        /// Gets or sets hidden keybinding for apply action.
        /// </summary>
        [SettingsUIMouseBinding(TreeControllerMod.ApplyMimicAction)]
        [SettingsUIHidden]
        public ProxyBinding ApplyMimic { get; set; }

        /// <summary>
        /// Gets or sets hidden keybinding for secondary apply action.
        /// </summary>
        [SettingsUIMouseBinding(TreeControllerMod.SecondaryApplyMimicAction)]
        [SettingsUIHidden]
        public ProxyBinding SecondaryApplyMimic { get; set; }

        /// <inheritdoc/>
        public override void SetDefaults()
        {
            RandomRotation = true;
            DisableTreeGrowth = false;
            ColorVariationSet = ColorVariationSetYYTC.Vanilla;
            UseDeadModelDuringWinter = false;
            AgeSelectionTechnique = AgeSelectionOptions.RandomWeighted;
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
