// <copyright file="Lumber.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller
{
    using Colossal.Serialization.Entities;
    using Unity.Entities;

    /// <summary>
    /// A component that is used to ensure tree growth for lumber industry.
    /// </summary>
    public struct Lumber : IComponentData, IQueryTypeParameter, IEmptySerializable
    {
    }
}