// <copyright file="NoTreeGrowth.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller
{
    using Colossal.Serialization.Entities;
    using Unity.Entities;

    /// <summary>
    /// A component that is used to disable tree growth globally by adding to query.
    /// </summary>
    public struct NoTreeGrowth : IComponentData, IQueryTypeParameter, IEmptySerializable
    {
    }
}