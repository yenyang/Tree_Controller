// <copyright file="TCColorSet.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Domain
{
    using Game.Rendering;
    using UnityEngine;

    /// <summary>
    /// A class for moving color data into and out of UI.
    /// </summary>
    public class TCColorSet
    {
        public Color Channel0;
        public Color Channel1;
        public Color Channel2;

        /// <summary>
        /// Initializes a new instance of the <see cref="TCColorSet"/> class.
        /// </summary>
        /// <param name="colorSet">Game.Rendering colorset.</param>
        public TCColorSet (ColorSet colorSet)
        {
            Channel0 = colorSet.m_Channel0;
            Channel1 = colorSet.m_Channel1;
            Channel2 = colorSet.m_Channel2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TCColorSet"/> class.
        /// </summary>
        /// <param name="colorSet">Game.Rendering colorset.</param>
        public TCColorSet(Color color0, Color color1, Color color2)
        {
            Channel0 = color0;
            Channel1 = color1;
            Channel2 = color2;
        }

        /// <summary>
        ///  Gets a vanilla color set.
        /// </summary>
        /// <returns>Vanilla Color set.</returns>
        public ColorSet GetColorSet()
        {
            ColorSet colorSet = new ()
            {
                m_Channel0 = Channel0,
                m_Channel1 = Channel1,
                m_Channel2 = Channel2,
            };
            return colorSet;
        }
    }
}
