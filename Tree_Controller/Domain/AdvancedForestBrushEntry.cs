// <copyright file="AdvancedForestBrushEntry.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Domain
{
    using Game.Prefabs;
    using Game.UI;
    using Tree_Controller.Tools;
    using Unity.Entities;
    using static Colossal.AssetPipeline.Diagnostic.Report;

    /// <summary>
    /// A class to use for UI binding for advance forest brushes.
    /// </summary>
    public class AdvancedForestBrushEntry
    {
        private string m_Name;
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
            m_Name = prefabID.GetName();
            m_Ages = ages;
            m_ProbabilityWeight = probablity;
            m_MinimumElevation = minElev;
            m_MaximumElevation = maxElev;
        }

        /// <summary>
        /// Gets or sets selected ages.
        /// </summary>
        public int SelectedAges
        {
            get { return (int)m_Ages; }
            set { m_Ages = (Ages)value; }
        }

        /// <summary>
        /// Gets or sets the locale key.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
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
        /// Gets the image source string.
        /// </summary>
        public string Src
        {
            get { return GetThumbnailOrPlaceholder(new PrefabID(nameof(StaticObjectPrefab), Name)); }
        }

        /// <summary>
        /// Gets or sets the probability weight.
        /// </summary>
        public int ProbabilityWeight
        {
            get { return m_ProbabilityWeight; }
            set { m_ProbabilityWeight = value; }
        }

        /// <summary>
        /// Gets the prefab entity for the forest brush entry.
        /// </summary>
        /// <returns>Prefab entity or Entity.Null.</returns>
        public Entity GetPrefabEntity()
        {
            PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            if (prefabSystem.TryGetPrefab(new PrefabID(nameof(StaticObjectPrefab), m_Name), out PrefabBase prefabBase) &&
                prefabSystem.TryGetEntity(prefabBase, out Entity entity))
            {
                return entity;
            }

            return Entity.Null;
        }

        private string GetThumbnailOrPlaceholder(PrefabID prefabID)
        {
            PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            ImageSystem imageSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ImageSystem>();
            if (prefabSystem.TryGetPrefab(prefabID, out PrefabBase prefabBase)
                && prefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity))
            {
                return imageSystem.GetThumbnail(prefabEntity);
            }
            else
            {
                return imageSystem.placeholderIcon;
            }
        }
    }
}
