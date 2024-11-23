// <copyright file="AdvancedForestBrushEntry.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Domain
{
    using Game.Prefabs;
    using Game.UI;
    using Tree_Controller.Tools;
    using Unity.Entities;

    /// <summary>
    /// A class to use for UI binding for advance forest brushes.
    /// </summary>
    public class AdvancedForestBrushEntry
    {
        private string m_LocaleKey;
        private string m_Src;
        private Ages m_Ages;
        private int m_ProbabilityWeight;
        private int m_MinimumElevation;
        private int m_MaximumElevation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedForestBrushEntry"/> class.
        /// </summary>
        public AdvancedForestBrushEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedForestBrushEntry"/> class.
        /// </summary>
        /// <param name="prefabID">Prefab ID for the asset entry.</param>
        /// <param name="ages">Selected ages.</param>
        /// <param name="probablity">Probablity weight.</param>
        /// <param name="minElev">Minimum elevation.</param>
        /// <param name="maxElev">Maximum elevation.</param>
        public AdvancedForestBrushEntry(PrefabID prefabID, Ages ages, int probablity, int minElev, int maxElev)
        {
            m_Ages = ages;
            m_ProbabilityWeight = probablity;
            m_MinimumElevation = minElev;
            m_MaximumElevation = maxElev;
            m_LocaleKey = $"Assets.NAME[{prefabID.GetName()}]";

            PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            ImageSystem imageSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ImageSystem>();
            if (prefabSystem.TryGetPrefab(prefabID, out PrefabBase prefabBase)
                && prefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity))
            {
                m_Src = imageSystem.GetThumbnail(prefabEntity);
            }
            else
            {
                m_Src = imageSystem.placeholderIcon;
            }
        }

        /// <summary>
        /// Gets or sets selected ages.
        /// </summary>
        public Ages SelectedAges
        {
            get { return m_Ages; }
            set { m_Ages = value; }
        }

        /// <summary>
        /// Gets or sets the locale key.
        /// </summary>
        public string LocaleKey 
        {
            get { return m_LocaleKey; }
            set { m_LocaleKey = value; }
        }

        /// <summary>
        /// Gets or sets the minimum elevation.
        /// </summary>
        public int MinimumElevation
        {
            get { return m_MinimumElevation; }
            set { m_MinimumElevation = value; }
        }

        /// <summary>
        /// Gets or sets the maximum elevation.
        /// </summary>
        public int MaximumElevation
        {
            get { return m_MaximumElevation; }
            set { m_MaximumElevation = value; }
        }

        /// <summary>
        /// Gets or sets the image source string.
        /// </summary>
        public string Src
        {
            get { return m_Src; }
            set { m_Src = value; }
        }

        /// <summary>
        /// Gets or sets the probability weight.
        /// </summary>
        public int ProbabilityWeight
        {
            get { return m_ProbabilityWeight; }
            set { m_ProbabilityWeight = value; }
        }
    }
}
