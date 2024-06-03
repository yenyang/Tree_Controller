// <copyright file="LocaleEN.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using Colossal.IO.AssetDatabase.Internal;

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
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.UseDeadModelDuringWinter)), "Deciduous trees use Dead Model during Winter" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.UseDeadModelDuringWinter)), "Will temporarily make all non-lumber industry deciduous trees use the dead model and pause growth during winter." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.DisableTreeGrowth)), "Disable Tree Growth" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.DisableTreeGrowth)), "Disable tree growth for the entire map except for lumber industry." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.ColorVariationSet)), "Color Variation Set" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.ColorVariationSet)), "Sets of seasonal colors for Trees and Wild bushes. Vanilla is the base game. Yenyang's is my curated colors. Spring is green year round. Autumn is fall colors year round. Custom has been deprecated and forked over to a new mod called Recolor. Custom is still left here so you can switch to something else if you still have it enabled." },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.SafelyRemoveButton)), "Safely Remove" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.SafelyRemoveButton)), "Removes Tree Controller mod components and resets tree and bush model states. Only necessary during Winter and very end of Autumn. Must use reset button to undo setting change." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.SafelyRemoveButton)), "Remove Tree Controller mod components and reset tree and bush model states?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.DestroyFoliageSettings)), "Delete All Foliage" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.DestroyFoliageSettings)), "Permanently removes all trees and plants from the map. It keeps any foliage owned by buildings, parks, or roads. This action cannot be undone." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.DestroyFoliageSettings)), "Remove all trees and plants not attached to existing objects?" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.ResetModSettings)), "Reset Tree Controller Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.ResetModSettings)), "After confirmation this will reset Tree Controller Settings." },
                { m_Setting.GetOptionWarningLocaleID(nameof(TreeControllerSettings.ResetModSettings)), "Reset Tree Controller Settings?" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Yenyangs), "Yenyang's" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Vanilla), "Vanilla" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Spring), "Spring" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Autumn), "Autumn" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.ColorVariationSetYYTC.Custom), "Custom (Deprecated)" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.AgeSelectionOptions.RandomEqualWeight), "Equal Distribution" },
                { m_Setting.GetEnumValueLocaleID(TreeControllerSettings.AgeSelectionOptions.RandomWeighted), "Forest Distribution" },
                { m_Setting.GetOptionLabelLocaleID(nameof(TreeControllerSettings.AgeSelectionTechnique)), "Age Selection Technique" },
                { m_Setting.GetOptionDescLocaleID(nameof(TreeControllerSettings.AgeSelectionTechnique)), "When multiple Tree Ages are selected, one will be selected using this option. Equal Distribution is just a random selection. Forest Distribution randomly selects using the editor's approximation for a forest." },
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
                { "YY_TREE_CONTROLLER[child]", "Child" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[child]", "Essentially a Sapling. First stage of tree growth." },
                { "YY_TREE_CONTROLLER[teen]", "Teen" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[teen]", "A young tree. Second stage of tree growth." },
                { "YY_TREE_CONTROLLER[adult]", "Adult" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[adult]", "A mature tree but not the largest size. Third stage of tree growth." },
                { "YY_TREE_CONTROLLER[elderly]", "Elderly" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[elderly]", "The oldest and largest sized trees. Fourth stage of tree growth." },
                { "YY_TREE_CONTROLLER[dead]", "Dead" },
                { "YY_TREE_CONTROLLER_DESCRIPTION[dead]", "A bare, leafless tree. Final stage of tree growth. Will eventually cycle back into a child (sapling) tree." },
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
                { SectionLabel("InfoRowTitle"), "Tree Controller" },
                { SectionLabel("InfoRowSubTitle"), "Custom Color Variations" },
                { TooltipDescriptionKey("InfoRowTooltip"), "For choosing custom seasonal foliage colors." },
                { SectionLabel("Channel0"), "Channel0" },
                { SectionLabel("Channel1"), "Channel1" },
                { SectionLabel("Channel2"), "Channel2" },
                { SectionLabel("ResetAndSave"), "Reset / Save" },
                { TooltipTitleKey("Reset"), "Reset Seasonal Colors" },
                { TooltipDescriptionKey("Reset"), "Resets and saves the colors back to the original colors for this season and vegetation asset." },
                { TooltipTitleKey("Save"), "Save Seasonal Colors" },
                { TooltipDescriptionKey("Save"), "Saves the colors for this season and vegetation asset to an XML file located in a folder at %AppData%\\LocalLow\\Colossal Order\\Cities Skylines II \\ModsData\\Mods_Yenyang_Tree_Controller \\FoliageColorData\\Custom. Triggers a color refresh on all vegetation of the same type." },
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
