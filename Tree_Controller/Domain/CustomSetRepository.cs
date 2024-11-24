// <copyright file="CustomSetRepository.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Prefabs;
    using Unity.Entities;

    /// <summary>
    /// A class to use for XML serialization and deserialization for storing prefabs used in a custom set.
    /// </summary>
    public class CustomSetRepository
    {
        private readonly int DefaultProbabilityWeight = 100;
        private readonly int DefaultMinimumElevation = 0;
        private readonly int DefaultMaximumElevation = 4000;
        private readonly Tools.Ages DefaultAge = Tools.Ages.MatchGlobal;

        private int m_Version;
        private AdvancedForestBrushEntry[] m_AdvancedForestBrushEntries;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSetRepository"/> class.
        /// </summary>
        public CustomSetRepository()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSetRepository"/> class.
        /// </summary>
        /// <param name="customSet">List of prefab bases.</param>
        public CustomSetRepository(List<PrefabBase> customSet)
        {
            // m_PrefabNames = ConvertToArray(customSet);
            m_Version = 2;
            GenerateDefaultAdvancedForestBrushEntries(ConvertToArray(customSet));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSetRepository"/> class.
        /// </summary>
        /// <param name="customSet">list of prefab IDs for the custom set.</param>
        public CustomSetRepository(List<PrefabID> customSet)
        {
            // m_PrefabNames = ConvertToArray(customSet);
            m_Version = 2;
            GenerateDefaultAdvancedForestBrushEntries(ConvertToArray(customSet));
        }

        /// <summary>
        /// Gets or sets a value indicating the names of the prefabs in the set.
        /// </summary>
        public string[] PrefabNames
        {
            get
            {
                List<string> names = new List<string>();
                foreach (AdvancedForestBrushEntry entry in m_AdvancedForestBrushEntries)
                {
                    names.Add(entry.Name);
                }

                return names.ToArray();
            }

            set
            {
                if (value is not null)
                {
                    GenerateDefaultAdvancedForestBrushEntries(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the names of the prefabs in the set.
        /// </summary>
        public int Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        /// <summary>
        /// Gets the length of the array of prefab names.
        /// </summary>
        /// <returns>Lenght of array of prefab names.</returns>
        public int Count
        {
            get { return m_AdvancedForestBrushEntries.Length; }
        }

        /// <summary>
        /// Gets or sets a value indicating the advanced forest brush entries.
        /// </summary>
        public AdvancedForestBrushEntry[] AdvancedForestBrushEntries
        {
            get
            {
                return m_AdvancedForestBrushEntries;
            }

            set
            {
                if (value is not null)
                {
                    m_AdvancedForestBrushEntries = value;
                }
            }
        }

        /// <summary>
        /// Gets a list of PrefabBases from the array of prefab names.
        /// </summary>
        /// <returns>List of PrefabBases.</returns>
        public List<PrefabBase> GetPrefabBases()
        {
            PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            List<PrefabBase> prefabs = new List<PrefabBase>();
            foreach (string name in PrefabNames)
            {
                PrefabID prefabID = new PrefabID("StaticObjectPrefab", name);
                if (prefabSystem.TryGetPrefab(prefabID, out PrefabBase prefab))
                {
                    if (prefab != null)
                    {
                        prefabs.Add(prefab);
                    }
                }
            }

            return prefabs;
        }

        /// <summary>
        /// Gets a list of PrefabBases from the array of prefab names.
        /// </summary>
        /// <returns>List of PrefabBases.</returns>
        public List<PrefabID> GetPrefabIDs()
        {
            List<PrefabID> prefabIDs = new List<PrefabID>();
            foreach (string name in PrefabNames)
            {
                PrefabID prefabID = new PrefabID("StaticObjectPrefab", name);
                prefabIDs.Add(prefabID);
            }

            return prefabIDs;
        }

        /// <summary>
        /// Sets m_PrefabNames from a list of prefab IDs.
        /// </summary>
        /// <param name="prefabs">List of prefab IDs for the custom set.</param>
        public void SetPrefabsWithDefaultAdvancedOptions(List<PrefabBase> prefabs)
        {
            GenerateDefaultAdvancedForestBrushEntries(ConvertToArray(prefabs));
        }

        /// <summary>
        /// Sets m_PrefabNames from a list of prefab bases
        /// </summary>
        /// <param name="prefabIDs">List of prefab IDs for the custom set.</param>
        public void SetPrefabsWithDefaultAdvancedOptions(List<PrefabID> prefabIDs)
        {
            GenerateDefaultAdvancedForestBrushEntries(ConvertToArray(prefabIDs));
        }

        /// <summary>
        /// Adds a prefab ID to the custom set.
        /// </summary>
        /// <param name="prefabID">New Prefab ID to add.</param>
        public void Add(PrefabID prefabID)
        {
            List<PrefabID> prefabIDs = GetPrefabIDs();
            List<AdvancedForestBrushEntry> advancedForestBrushEntries = m_AdvancedForestBrushEntries.ToList();
            if (!prefabIDs.Contains(prefabID))
            {
                prefabIDs.Add(prefabID);
                advancedForestBrushEntries.Add(GetDefaultAdvancedForestBrushEntry(prefabID.GetName()));
                m_AdvancedForestBrushEntries = advancedForestBrushEntries.ToArray();
            }

        }


        private string[] ConvertToArray(List<PrefabBase> list)
        {
            string[] array = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                array[i] = list[i].name;
            }

            return array;
        }

        private string[] ConvertToArray(List<PrefabID> list)
        {
            string[] array = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                array[i] = list[i].GetName();
            }

            return array;
        }

        private void GenerateDefaultAdvancedForestBrushEntries(string[] names)
        {
            AdvancedForestBrushEntry[] advancedForestBrushEntries = new AdvancedForestBrushEntry[names.Length];
            int i = 0;
            foreach (string name in names)
            {
                advancedForestBrushEntries[i] = GetDefaultAdvancedForestBrushEntry(name);
                i++;
            }

            m_AdvancedForestBrushEntries = advancedForestBrushEntries;
        }

        private AdvancedForestBrushEntry GetDefaultAdvancedForestBrushEntry(string name)
        {
            return new AdvancedForestBrushEntry(new PrefabID("StaticObjectPrefab", name), DefaultAge, DefaultProbabilityWeight, DefaultMinimumElevation, DefaultMaximumElevation);
        }
    }
}
