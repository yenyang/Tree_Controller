// <copyright file="TreeControllerData.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Tools
{
    /// <summary>
    /// A enum of Ages that are selected.
    /// </summary>
    public enum Ages
    {
        /// <summary>
        /// No selection will default to adult.
        /// </summary>
        None = 0,

        /// <summary>
        /// Sapling trees.
        /// </summary>
        Child = 1,

        /// <summary>
        /// Young trees.
        /// </summary>
        Teen = 2,

        /// <summary>
        /// Mature trees.
        /// </summary>
        Adult = 4,

        /// <summary>
        /// Old trees.
        /// </summary>
        Elderly = 8,

        /// <summary>
        ///  Dead model.
        /// </summary>
        Dead = 16,

        /// <summary>
        /// Random selection of all ages.
        /// </summary>
        All = 32,
    }

    /// <summary>
    ///  An enum for tool mode.
    /// </summary>
    public enum ToolMode
    {
        /// <summary>
        /// Object tool plopping single tree.
        /// </summary>
        Plop = 0,

        /// <summary>
        /// Object tool brushing multiple trees.
        /// </summary>
        Brush = 1,

        /// <summary>
        /// Tree controller change age mode.
        /// </summary>
        ChangeAge = 2,

        /// <summary>
        /// Tree controller change type mode.
        /// </summary>
        ChangeType = 3,
    }

    /// <summary>
    /// An enum for selection mode.
    /// </summary>
    public enum Selection
    {
        /// <summary>
        /// A single tree.
        /// </summary>
        Single = 0,

        /// <summary>
        /// Whole building or network.
        /// </summary>
        BuildingOrNet = 1,

        /// <summary>
        /// Radius around cursor.
        /// </summary>
        Radius = 2,

        /// <summary>
        /// Whole map.
        /// </summary>
        Map = 3,
    }

    /*
    /// <summary>
    /// A JsonWritable binding for tree controller tool data.
    /// </summary>
    public struct TreeControllerData : IJsonWritable
    {
        /// <summary>
        /// the currently selected ages in an enum.
        /// </summary>
        public Ages selectedAges;

        /// <summary>
        /// The currently selected tool mode for object tool or tree controller tool.
        /// </summary>
        public ToolMode toolMode;

        /// <summary>
        /// The current selection mode.
        /// </summary>
        public Selection selection;

        /// <inheritdoc/>
        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(GetType().FullName);
            writer.PropertyName(nameof(selectedAges));
            writer.Write(Enum.GetName(typeof(Ages), selectedAges));
            writer.PropertyName(nameof(toolMode));
            writer.Write(Enum.GetName(typeof(ToolMode), toolMode));
            writer.PropertyName(nameof(selection));
            writer.Write(Enum.GetName(typeof(Selection), selection));
            writer.TypeEnd();
        }
    }
    */
}
