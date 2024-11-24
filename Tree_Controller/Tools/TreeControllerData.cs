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
        /// Do not show ages.
        /// </summary>
        Hide = -2,

        /// <summary>
        /// Match global binding instead of a specific override.
        /// </summary>
        MatchGlobal = -1,

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
        /// Stump model.
        /// </summary>
        Stump = 32,

        /// <summary>
        /// Random selection of all ages.
        /// </summary>
        All = 64,
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
}
