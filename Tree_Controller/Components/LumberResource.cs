// <copyright file="LumberResource.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Components
{
    using Colossal.Serialization.Entities;
    using Unity.Entities;

    /// <summary>
    /// An Enableable Component to record whether a tree is a lumber resource or not.
    /// </summary>
    public struct LumberResource : IComponentData, IEnableableComponent, IQueryTypeParameter, IEmptySerializable
    {
    }
}
