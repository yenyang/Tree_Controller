// <copyright file="Vegetation.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller
{
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// A component used to find Vegetation prefabs quickly with a query.
    /// </summary>
    public struct Vegetation : IComponentData, IQueryTypeParameter
    {
        /// <summary>
        /// Records the original size from ObjectGeometryData Component.
        /// </summary>
        public float3 m_Size;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vegetation"/> struct.
        /// </summary>
        /// <param name="size">Records the original size from ObjectGeometryData component.</param>
        public Vegetation(float3 size)
        {
            m_Size = size;
        }
    }
}