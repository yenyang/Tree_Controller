// <copyright file="LocaleEN.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using Game.Settings;

    /// <summary>
    /// Localization for <see cref="TreeControllerSettings"/> in English.
    /// </summary>
    public class LocaleEN : IDictionarySource
    {
        private readonly TreeControllerSettings m_Setting;

        private Dictionary<string, string> m_Localization;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleEN"/> class.
        /// </summary>
        /// <param name="setting">Settings class.</param>
        public LocaleEN(TreeControllerSettings setting)
        {
            m_Setting = setting;

            m_Localization = new Dictionary<string, string>()
            {
                { m_Setting.GetSettingsLocaleID(), "Tree Controller" },
                { m_Setting.GetOptionTabLocaleID(TreeControllerSettings.General), "General" },
                { m_Setting.GetOptionTabLocaleID(TreeControllerSettings.WindTab), "Wind" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.UseDeadModelDuringWinter)), "Deciduous trees use Dead Model during Winter" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.UseDeadModelDuringWinter)), "Will temporarily make all non-lumber industry deciduous trees use the dead model and pause growth during winter." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.DisableTreeGrowth)), "Disable Tree Growth" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.DisableTreeGrowth)), "Disable tree growth for the entire map except for lumber industry." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.FreeVegetation)), "Free Trees and Vegetation" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.FreeVegetation)), "Sets the cost of trees and vegetation to 0." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.IncludeStumps)), "Include Stumps" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.IncludeStumps)), "Age selection will include stumps for Trees." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.ConstrainBrush)), "Constrain Brush" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.ConstrainBrush)), "Prevents tree and vegetation applied with the circular brush from exceeding the visual limits of the brush. You may not always have a visual indicator of where the next one will be if the game wants to place the next one outside the limits." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.ColorVariationSet)), "Color Variation Set" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.ColorVariationSet)), "Sets of seasonal colors for Trees, bushes, and plants. Vanilla is the base game. Yenyang's is my curated colors. Spring is green year round. Autumn is fall colors year round. Custom has been moved to a new mod called Recolor." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.SafelyRemoveButton)), "Safely Remove" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.SafelyRemoveButton)), "Removes Tree Controller mod components and resets tree and bush model states. Only necessary during Winter and very end of Autumn. Must use reset button to undo setting change." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.SafelyRemoveButton)), "Remove Tree Controller mod components and reset tree and bush model states?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.DestroyFoliageSettings)), "Delete All Foliage" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.DestroyFoliageSettings)), "Permanently removes all trees and plants from the map. It keeps any foliage owned by buildings, parks, or roads. This action cannot be undone." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.DestroyFoliageSettings)), "Remove all trees and plants not attached to existing objects?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.ResetGeneralSettings)), "Reset Tree Controller General Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.ResetGeneralSettings)), "After confirmation this will reset Tree Controller General Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.ResetGeneralSettings)), "Reset Tree Controller General Settings?" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Yenyangs), "Yenyang's" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Vanilla), "Vanilla" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Spring), "Spring" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Autumn), "Autumn" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.AgeSelectionOptions.RandomEqualWeight), "Equal Distribution" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.AgeSelectionOptions.RandomWeighted), "Forest Distribution" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.AgeSelectionTechnique)), "Age Selection Technique" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.AgeSelectionTechnique)), "When multiple Tree Ages are selected, one will be selected using this option. Equal Distribution is just a random selection. Forest Distribution randomly selects using the editor's approximation for a forest." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerMod.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerMod.Version)), $"Version number for the Tree Controller mod installed." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.SelectedWindOption)), "Wind Control Type" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.SelectedWindOption)), "Vanilla option will not change the effects of wind on trees and vegetation. Disabled options prevents trees and vegetation from swaying due to wind. Override controls the values for the wind effects on trees and vegetation based on these settings." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.DisableWindWhenPaused)), "Disable Wind When Paused" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.DisableWindWhenPaused)), "Trees and vegetation will not sway from wind when the game is paused. Trees and vegetation will spring back abruptly to their static or swaying configurations when switching between paused and simulation running." },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.WindOptions.Vanilla), "Vanilla" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.WindOptions.Disabled), "Disabled" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.WindOptions.Override), "Override" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.WindGlobalStrength)), "Wind Strength" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.WindGlobalStrength)), "One of two factors used to determine the global wind strength." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.FasterFullBrushStrength)), "Faster Full Brush Strength" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.FasterFullBrushStrength)), "Instead of 100%, the brush strength while creating trees and vegetation will be 300%." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.WindGlobalStrength2)), "Wind Strength 2" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.WindGlobalStrength2)), "One of two factors used to determine the global wind strength." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.WindDirection)), "Wind Direction" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.WindDirection)), "Controls the wind direction in degrees." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.WindDirectionVariance)), "Wind Direction Variance" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.WindDirectionVariance)), "Controls random variance for wind direction." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.WindDirectionVariancePeriod)), "Direction Variance period" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.WindDirectionVariancePeriod)), "Controls a period of time related to wind directional variances." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.WindInterpolationDuration)), "Wind Interpolation Duration" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.WindInterpolationDuration)), "Controls a value that inversely affects the sampleing period for interpolating values." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.ResetWindSettings)), "Reset Tree Controller - Wind Tab Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.ResetWindSettings)), "After confirmation this will reset Tree Controller - Wind Tab Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.ResetWindSettings)), "Reset Tree Controller - Wind Tab Settings?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.ResetWindSliders)), "Reset Wind Override Sliders" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.ResetWindSliders)), "After confirmation this will reset wind override sliders." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.ResetWindSliders)), "Reset wind override sliders?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.Author)), "Tree Wind Controller co-authored with BruceyBoy, Originally by donutmonger." },
                { "Options.TOOLTIPYYTC[WholeMapApply]", "Right Click to Apply." },
                { "YY_TREE_CONTROLLER[Selection]", "Selection" },
                { "YY_TREE_CONTROLLER[Age]", "Age" },
                { "YY_TREE_CONTROLLER[Radius]", "Radius" },
                { "YY_TREE_CONTROLLER[Sets]", "Sets" },
                { "YY_TREE_CONTROLLER[Rotation]", "Rotation" },
                { "YY_TREE_CONTROLLER[building-or-net]", "Whole Building or Network" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[building-or-net]", "Selects every tree in a whole building or network. You cannot change type of trees in networks." },
                { "YY_TREE_CONTROLLER[single-tree]", "Single Tree" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[single-tree]", "Selects a single tree in a building, network, or on the map. You cannot change type of trees in networks." },
                { "YY_TREE_CONTROLLER[radius]", "Radius" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[radius]", "Selects every tree in a radius including those in buildings, networks, or on the map. You should avoid changing type of trees in networks." },
                { "YY_TREE_CONTROLLER[whole-map]", "Whole Map" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[whole-map]", "Selects all trees in every building, network, or on the map. Be careful how you use this. Right click to apply. You should avoid changing type of trees in networks." },
                { "YY_TREE_CONTROLLER[dead]", "Dead" },
                { "YY_TREE_CONTROLLER[clear-ages]", "Toggle all Ages on/off" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[clear-ages]", "Either selects all or none of the ages depending on your current selection. Having none selected will always default to adult." },
                { "YY_TREE_CONTROLLER_DESCRIPTION[radius-up-arrow]", "Increases the selection radius." },
                { "YY_TREE_CONTROLLER_DESCRIPTION[radius-down-arrow]", "Decreases the selection radius." },
                { "YY_TREE_CONTROLLER[change-age-tool]", "Change Tree Age Tool" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[change-age-tool]", "A custom tool for changing the ages of existing trees." },
                { "YY_TREE_CONTROLLER[change-prefab-tool]", "Change Tree Type Tool" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[change-prefab-tool]", "A custom tool for changing the type and/or age of existing trees." },
                { "YY_TREE_CONTROLLER[wild-deciduous-trees]", "Wild Deciduous" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[wild-deciduous-trees]", "Alder, Birch, London Plane, Linden, Hickory, Chestnut, and Oak. Does not include Apple and Poplar." },
                { "YY_TREE_CONTROLLER[evergreen-trees]", "Evergreen" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[evergreen-trees]", "Pine and spruce trees." },
                { "YY_TREE_CONTROLLER[wild-bushes]", "Wild Bushes" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[wild-bushes]", "Green and flowering wild bushes." },
                { "YY_TREE_CONTROLLER[random-rotation]", "Random Rotation" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[random-rotation]", "Will randomly rotate the tree or bush as the tree or bush is moved to different positions on the map." },
                { "YY_TREE_CONTROLLER[custom-set-1]", "Custom Set 1" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[custom-set-1]", "Hold Ctrl to select or unselect multiple types of trees using the toolbar menu. Then hold Ctrl and click this button to save a custom set. Once a set has been saved, click this button to select that set. Hold Ctrl while switching themes to maintain the custom set." },
                { "YY_TREE_CONTROLLER[custom-set-2]", "Custom Set 2" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[custom-set-2]", "Hold Ctrl to select or unselect multiple types of trees using the toolbar menu. Then hold Ctrl and click this button to save a custom set. Once a set has been saved, click this button to select that set. Hold Ctrl while switching themes to maintain the custom set." },
                { "YY_TREE_CONTROLLER[custom-set-3]", "Custom Set 3" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[custom-set-3]", "Hold Ctrl to select or unselect multiple types of trees using the toolbar menu. Then hold Ctrl and click this button to save a custom set. Once a set has been saved, click this button to select that set. Hold Ctrl while switching themes to maintain the custom set." },
                { "YY_TREE_CONTROLLER[custom-set-4]", "Custom Set 4" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[custom-set-4]", "Hold Ctrl to select or unselect multiple types of trees using the toolbar menu. Then hold Ctrl and click this button to save a custom set. Once a set has been saved, click this button to select that set. Hold Ctrl while switching themes to maintain the custom set." },
                { "YY_TREE_CONTROLLER[custom-set-5]", "Custom Set 5" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[custom-set-5]", "Hold Ctrl to select or unselect multiple types of trees using the toolbar menu. Then hold Ctrl and click this button to save a custom set. Once a set has been saved, click this button to select that set. Hold Ctrl while switching themes to maintain the custom set." },
                { "YY_TREE_CONTROLLER[change]", "Change" },
                { TooltipTitleKey("Stump"), "Stump" },
            };
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return m_Localization;
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }

        private string TooltipDescriptionKey(string key)
        {
            return $"{TreeControllerMod.Id}.TOOLTIP_DESCRIPTION[{key}]";
        }

        private string SectionLabel(string key)
        {
            return $"{TreeControllerMod.Id}.SECTION_TITLE[{key}]";
        }


        private string TooltipTitleKey(string key)
        {
            return $"{TreeControllerMod.Id}.TOOLTIP_TITLE[{key}]";
        }
    }
}
