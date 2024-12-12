// <copyright file="CustomSetRepository.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Colossal.Logging;
    using Game.Prefabs;
    using Tree_Controller.Tools;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// A class to use for XML serialization and deserialization for storing prefabs used in a custom set.
    /// </summary>
    public class CustomSetRepository
    {
        private readonly int DefaultProbabilityWeight = 100;
        private readonly int DefaultMinimumElevation = 0;
        private readonly int DefaultMaximumElevation = 4096;
        private readonly Tools.Ages DefaultAge = Tools.Ages.Adult;

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
            m_Version = 2;
            GenerateDefaultAdvancedForestBrushEntries(ConvertToArray(customSet));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSetRepository"/> class.
        /// </summary>
        /// <param name="customSet">list of prefab IDs for the custom set.</param>
        public CustomSetRepository(List<PrefabID> customSet)
        {
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

        /// <summary>
        /// Removes an entry from advanced forest brush entries by name.
        /// </summary>
        /// <param name="name">Name of prefab to remove.</param>
        public void RemoveEntry(string name)
        {
            bool foundEntryToRemove = false;
            AdvancedForestBrushEntry[] advancedForestBrushEntries = new AdvancedForestBrushEntry[m_AdvancedForestBrushEntries.Length - 1];
            int j = 0;
            for (int i = 0; i < m_AdvancedForestBrushEntries.Length; i++)
            {
                if (m_AdvancedForestBrushEntries[i].Name != name && j < advancedForestBrushEntries.Length)
                {
                    advancedForestBrushEntries[j] = m_AdvancedForestBrushEntries[i];
                    j++;
                }
                else
                {
                    foundEntryToRemove = true;
                }
            }

            if (foundEntryToRemove)
            {
                m_AdvancedForestBrushEntries = advancedForestBrushEntries;
            }
        }

        /// <summary>
        /// Sets the probability weight for an advanced forest brush entry based on name.
        /// </summary>
        /// <param name="name">The prefab's name.</param>
        /// <param name="probabilityWeight">New probability weight.</param>
        public void SetProbabilityWeight(string name, int probabilityWeight)
        {
            for (int i = 0; i <= m_AdvancedForestBrushEntries.Length; i++)
            {
                if (m_AdvancedForestBrushEntries[i].Name == name)
                {
                    m_AdvancedForestBrushEntries[i].ProbabilityWeight = probabilityWeight;
                    TreeControllerMod.Instance.Logger.Debug($"{nameof(CustomSetRepository)}.{nameof(SetProbabilityWeight)} set weight for {name} to {probabilityWeight}.");
                    return;
                }
            }

            TreeControllerMod.Instance.Logger.Debug($"{nameof(CustomSetRepository)}.{nameof(SetProbabilityWeight)} did not find {name}.");
        }

        /// <summary>
        /// Sets the minimum elevation for an advanced forest brush entry based on name.
        /// </summary>
        /// <param name="name">The prefab's name.</param>
        /// <param name="minimumElevation">New minimum elevation.</param>
        public void SetMinimumElevation(string name, int minimumElevation)
        {
            for (int i = 0; i < m_AdvancedForestBrushEntries.Length; i++)
            {
                if (m_AdvancedForestBrushEntries[i].Name == name)
                {
                    m_AdvancedForestBrushEntries[i].MinimumElevation = minimumElevation;
                    if (m_AdvancedForestBrushEntries[i].MaximumElevation <= minimumElevation)
                    {
                        m_AdvancedForestBrushEntries[i].MaximumElevation = Mathf.Clamp(minimumElevation + 1, DefaultMinimumElevation, DefaultMaximumElevation);
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// Sets the max elevation for an advanced forest brush entry based on name.
        /// </summary>
        /// <param name="name">The prefab's name.</param>
        /// <param name="maxElevation">New probability weight.</param>
        public void SetMaximumElevation(string name, int maxElevation)
        {
            for (int i = 0; i < m_AdvancedForestBrushEntries.Length; i++)
            {
                if (m_AdvancedForestBrushEntries[i].Name == name)
                {
                    m_AdvancedForestBrushEntries[i].MaximumElevation = maxElevation;
                    if (m_AdvancedForestBrushEntries[i].MinimumElevation >= maxElevation)
                    {
                        m_AdvancedForestBrushEntries[i].MinimumElevation = Mathf.Clamp(maxElevation - 1, DefaultMinimumElevation, DefaultMaximumElevation);
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// Sets the selected for an advanced forest brush entry based on name.
        /// </summary>
        /// <param name="name">the prefab's name.</param>
        /// <param name="toggledAge">Toggled age.</param>
        public void SetAges(string name, Ages toggledAge)
        {
            for (int i = 0; i < m_AdvancedForestBrushEntries.Length; i++)
            {
                if (m_AdvancedForestBrushEntries[i].Name == name)
                {
                    Ages selectedAges = (Ages)m_AdvancedForestBrushEntries[i].SelectedAges;
                    if (toggledAge == Ages.OverrideAge)
                    {
                        if ((selectedAges & Ages.OverrideAge) == Ages.OverrideAge)
                        {
                            selectedAges &= ~Ages.OverrideAge;
                        }
                        else
                        {
                            selectedAges |= Ages.OverrideAge;
                        }

                        m_AdvancedForestBrushEntries[i].SelectedAges = (int)selectedAges;
                        return;
                    }

                    if (toggledAge != Ages.All && (selectedAges & Ages.All) == Ages.All)
                    {
                        selectedAges &= ~Ages.All;
                    }
                    else if (toggledAge == Ages.All && selectedAges != Ages.OverrideAge)
                    {
                        selectedAges &= ~(Ages.Child | Ages.Teen | Ages.Adult | Ages.Elderly | Ages.Elderly | Ages.Dead | Ages.Stump | Ages.All);
                        m_AdvancedForestBrushEntries[i].SelectedAges = (int)selectedAges;
                        return;
                    }
                    else if (toggledAge == Ages.All && selectedAges == Ages.OverrideAge)
                    {
                        selectedAges |= Ages.Child | Ages.Teen | Ages.Adult | Ages.Elderly | Ages.Elderly | Ages.Dead | Ages.Stump | Ages.All;
                        m_AdvancedForestBrushEntries[i].SelectedAges = (int)selectedAges;
                        return;
                    }

                    if ((selectedAges & toggledAge) == toggledAge)
                    {
                        selectedAges &= ~toggledAge;
                    }
                    else
                    {
                        selectedAges |= toggledAge;
                    }

                    if (m_AdvancedForestBrushEntries[i].SelectedAges == ((int)Ages.All - 1))
                    {
                        selectedAges |= Ages.All;
                    }

                    m_AdvancedForestBrushEntries[i].SelectedAges = (int)selectedAges;

                    return;
                }
            }
        }

        /// <summary>
        /// Resets an entry based on the name of the prefab.
        /// </summary>
        /// <param name="name">Prefab name.</param>
        public void ResetEntry(string name)
        {
            for (int i = 0; i < m_AdvancedForestBrushEntries.Length; i++)
            {
                if (m_AdvancedForestBrushEntries[i].Name == name)
                {
                    m_AdvancedForestBrushEntries[i] = GetDefaultAdvancedForestBrushEntry(name);
                    return;
                }
            }
        }

        /// <summary>
        /// Saves the custom set while maintaining previous advanced forest brush entries and adding default ones if new.
        /// </summary>
        /// <param name="customSet">List of prefab bases.</param>
        public void SaveCustomSet(List<PrefabBase> customSet)
        {
            List<string> names = ConvertToArray(customSet).ToList();
            AdvancedForestBrushEntry[] newEntries = new AdvancedForestBrushEntry[customSet.Count];
            for (int i = 0; i < customSet.Count; i++)
            {
                if (i < m_AdvancedForestBrushEntries.Length && names.Contains(m_AdvancedForestBrushEntries[i].Name))
                {
                    newEntries[i] = m_AdvancedForestBrushEntries[i];
                }
                else
                {
                    newEntries[i] = GetDefaultAdvancedForestBrushEntry(customSet[i].name);
                }
            }

            m_AdvancedForestBrushEntries = newEntries;
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
            PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            PrefabID prefabID = new PrefabID("StaticObjectPrefab", name);
            Tools.Ages age = DefaultAge;
            if (prefabSystem.TryGetPrefab(prefabID, out PrefabBase prefabBase)
                && prefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity)
                && !prefabSystem.EntityManager.HasComponent<Game.Prefabs.TreeData>(prefabEntity))
            {
                age = Tools.Ages.Hide;
            }

            return new AdvancedForestBrushEntry(prefabID, age, DefaultProbabilityWeight, DefaultMinimumElevation, DefaultMaximumElevation);
        }
    }
}
