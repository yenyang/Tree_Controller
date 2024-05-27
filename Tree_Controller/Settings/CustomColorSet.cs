// <copyright file="CustomColorSet.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Settings
{
    using Game.Prefabs;
    using Game.Rendering;
    using Tree_Controller.Utils;

    /// <summary>
    /// A class to use for XML Serialization and deserialization of custom foliage color sets.
    /// </summary>
    public class CustomColorSet
    {
        private ColorSet m_ColorSet;
        private string m_PrefabName;
        private FoliageUtils.Season m_Season;
        private int m_Version;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomColorSet"/> class.
        /// </summary>
        public CustomColorSet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomColorSet"/> class.
        /// </summary>
        /// <param name="channel0">One color.</param>
        /// <param name="channel1">2nd color.</param>
        /// <param name="channel2">3rd color.</param>
        /// <param name="prefabID">The prefab this applies to.</param>
        /// <param name="season">season this applies to.</param>
        public CustomColorSet(UnityEngine.Color channel0, UnityEngine.Color channel1, UnityEngine.Color channel2, PrefabID prefabID, FoliageUtils.Season season)
        {
            m_ColorSet = default;
            m_ColorSet.m_Channel0 = channel0;
            m_ColorSet.m_Channel1 = channel1;
            m_ColorSet.m_Channel2 = channel2;
            m_PrefabName = prefabID.GetName();
            m_Season = season;
            m_Version = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomColorSet"/> class.
        /// </summary>
        /// <param name="colorSet">set of colors</param>
        /// <param name="prefabID">The prefab this applies to.</param>
        /// <param name="season">season this applies to.</param>
        public CustomColorSet(ColorSet colorSet, PrefabID prefabID, FoliageUtils.Season season)
        {
            m_ColorSet = colorSet;
            m_PrefabName = prefabID.GetName();
            m_Season = season;
            m_Version = 1;
        }

        /// <summary>
        /// Gets or sets a value for the colorset.
        /// </summary>
        public ColorSet ColorSet
        {
            get { return m_ColorSet; }
            set { m_ColorSet = value; }
        }

        /// <summary>
        /// Gets or sets a value for the prefab name.
        /// </summary>
        public string PrefabName
        {
            get { return m_PrefabName; }
            set { m_PrefabName = value; }
        }

        /// <summary>
        /// Gets or sets a value for the season.
        /// </summary>
        public FoliageUtils.Season Season
        {
            get { return m_Season; }
            set { m_Season = value; }
        }

        /// <summary>
        /// Gets or sets a value for the version.
        /// </summary>
        public int Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        /// <summary>
        /// Sets Color set based on 3 colors.
        /// </summary>
        /// <param name="channel0">One color.</param>
        /// <param name="channel1">2nd color.</param>
        /// <param name="channel2">3rd color.</param>
        public void SetColorSet(UnityEngine.Color channel0, UnityEngine.Color channel1, UnityEngine.Color channel2)
        {
            m_ColorSet.m_Channel0 = channel0;
            m_ColorSet.m_Channel1 = channel1;
            m_ColorSet.m_Channel2 = channel2;
        }
    }
}
