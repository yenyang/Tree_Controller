// <copyright file="TreeControllerUISystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Tree_Controller.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Serialization;
    using Colossal.Logging;
    using Colossal.PSI.Environment;
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Tree_Controller.Settings;
    using Tree_Controller.Systems;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine.InputSystem;
    using static Colossal.AssetPipeline.Diagnostic.Report;

    /// <summary>
    /// UI system for Object Tool while using tree prefabs.
    /// </summary>
    public partial class TreeControllerUISystem : UISystemBase
    {
        private const string ModId = "Tree_Controller";

        private readonly List<PrefabID> m_VanillaDeciduousPrefabIDs = new()
        {
            { new PrefabID("StaticObjectPrefab", "EU_AlderTree01") },
            { new PrefabID("StaticObjectPrefab", "BirchTree01") },
            { new PrefabID("StaticObjectPrefab", "NA_LondonPlaneTree01") },
            { new PrefabID("StaticObjectPrefab", "NA_LindenTree01") },
            { new PrefabID("StaticObjectPrefab", "NA_HickoryTree01") },
            { new PrefabID("StaticObjectPrefab", "EU_ChestnutTree01") },
            { new PrefabID("StaticObjectPrefab", "OakTree01") },
        };

        private readonly List<PrefabID> m_VanillaEvergreenPrefabIDs = new()
        {
            { new PrefabID("StaticObjectPrefab", "PineTree01") },
            { new PrefabID("StaticObjectPrefab", "SpruceTree01") },
        };

        private readonly List<PrefabID> m_VanillaWildBushPrefabs = new()
        {
            { new PrefabID("StaticObjectPrefab", "GreenBushWild01") },
            { new PrefabID("StaticObjectPrefab", "GreenBushWild02") },
            { new PrefabID("StaticObjectPrefab", "FlowerBushWild01") },
            { new PrefabID("StaticObjectPrefab", "FlowerBushWild02") },
        };

        private readonly List<PrefabID> m_DefaultCustomSet1Prefabs = new()
        {
            { new PrefabID("StaticObjectPrefab", "NA_LondonPlaneTree01") },
            { new PrefabID("StaticObjectPrefab", "NA_LindenTree01") },
            { new PrefabID("StaticObjectPrefab", "NA_HickoryTree01") },
        };

        private readonly List<PrefabID> m_DefaultCustomSet2Prefabs = new()
        {
            { new PrefabID("StaticObjectPrefab", "EU_AlderTree01") },
            { new PrefabID("StaticObjectPrefab", "EU_ChestnutTree01") },
            { new PrefabID("StaticObjectPrefab", "EU_PoplarTree01") },
        };

        private readonly List<PrefabID> m_DefaultCustomSet3Prefabs = new()
        {
            { new PrefabID("StaticObjectPrefab", "BirchTree01") },
            { new PrefabID("StaticObjectPrefab", "OakTree01") },
            { new PrefabID("StaticObjectPrefab", "AppleTree01") },
        };

        private readonly List<PrefabID> m_DefaultCustomSet4Prefabs = new()
        {
            { new PrefabID("StaticObjectPrefab", "BirchTree01") },
            { new PrefabID("StaticObjectPrefab", "EU_PoplarTree01") },
        };

        private readonly List<PrefabID> m_DefaultCustomSet5Prefabs = new()
        {
            { new PrefabID("StaticObjectPrefab", "EU_ChestnutTree01") },
            { new PrefabID("StaticObjectPrefab", "NA_LondonPlaneTree01") },
            { new PrefabID("StaticObjectPrefab", "NA_LindenTree01") },
        };

        private ToolSystem m_ToolSystem;
        private PrefabSystem m_PrefabSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private TreeObjectDefinitionSystem m_TreeObjectDefinitionSystem;
        private TreeControllerTool m_TreeControllerTool;
        private string m_InjectedJS = string.Empty;
        private ILog m_Log;
        private PrefabBase m_LastObjectToolPrefab;
        private Dictionary<string, List<PrefabID>> m_PrefabSetsLookup;
        private bool m_ObjectToolPlacingTree = false;
        private string m_SelectedPrefabSet = string.Empty;
        private string m_ContentFolder;
        private EntityQuery m_VegetationQuery;
        private Entity m_ThemeEntity = Entity.Null;
        private bool m_LastToolWasLineTool = false;
        private ValueBinding<int> m_ToolMode;
        private ValueBinding<int> m_SelectionMode;
        private ValueBinding<int> m_SelectedAges;
        private ValueBinding<float> m_Radius;
        private ValueBinding<bool> m_IsVegetation;
        private ValueBinding<bool> m_IsTree;

        /// <summary>
        /// Gets or sets a value indicating the theme entity.
        /// </summary>
        public Entity ThemeEntity
        {
            get => m_ThemeEntity;
            set => m_ThemeEntity = value;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            m_ObjectToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_TreeObjectDefinitionSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeObjectDefinitionSystem>();
            m_TreeControllerTool = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TreeControllerTool>();
            m_ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "Mods_Yenyang_Tree_Controller", "CustomSets");
            System.IO.Directory.CreateDirectory(m_ContentFolder);
            ToolSystem toolSystem = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            ToolSystem toolSystem2 = m_ToolSystem;
            toolSystem2.EventPrefabChanged = (Action<PrefabBase>)Delegate.Combine(toolSystem2.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));
            m_PrefabSetsLookup = new Dictionary<string, List<PrefabID>>()
            {
                { "YYTC-wild-deciduous-trees", m_VanillaDeciduousPrefabIDs },
                { "YYTC-evergreen-trees", m_VanillaEvergreenPrefabIDs },
                { "YYTC-wild-bushes", m_VanillaWildBushPrefabs },
                { "YYTC-custom-set-1", m_DefaultCustomSet1Prefabs },
                { "YYTC-custom-set-2", m_DefaultCustomSet2Prefabs },
                { "YYTC-custom-set-3", m_DefaultCustomSet3Prefabs },
                { "YYTC-custom-set-4", m_DefaultCustomSet4Prefabs },
                { "YYTC-custom-set-5", m_DefaultCustomSet5Prefabs },
            };

            for (int i = 1; i <= 5; i++)
            {
                TryLoadCustomPrefabSet($"YYTC-custom-set-{i}");
            }

            // This section handles binding couples between C# and UI.
            AddBinding(m_ToolMode = new ValueBinding<int>(ModId, "ToolMode", (int)ToolMode.Plop));
            AddBinding(m_SelectedAges = new ValueBinding<int>(ModId, "SelectedAges", (int)Ages.Adult));
            AddBinding(m_SelectionMode = new ValueBinding<int>(ModId, "SelectionMode", (int)Selection.Radius));
            AddBinding(m_IsVegetation = new ValueBinding<bool>(ModId, "IsVegetation", false));
            AddBinding(m_IsTree = new ValueBinding<bool>(ModId, "IsTree", false));
            AddBinding(m_Radius = new ValueBinding<float>(ModId, "Radius", 100f));
            AddBinding(new TriggerBinding(ModId, "radius-up-arrow", IncreaseRadius));
            AddBinding(new TriggerBinding(ModId, "radius-down-arrow", DecreaseRadius));

            // This section handles trigger bindings which listen for triggers from UI and then start an event.
            AddBinding(new TriggerBinding<int>(ModId, "ChangeToolMode", ChangeToolMode));
            AddBinding(new TriggerBinding<int>(ModId, "ChangeSelectedAge", ChangeSelectedAge));
            AddBinding(new TriggerBinding<int>(ModId, "ChangeSelectionMode", ChangeSelectionMode));
            AddBinding(new TriggerBinding<int>(ModId, "ChangeSelectedAge", ChangeSelectedAge));
            AddBinding(new TriggerBinding<int>(ModId, "ChangeSelectionMode", ChangeSelectionMode));

            m_VegetationQuery = GetEntityQuery(ComponentType.ReadOnly<Vegetation>());

            m_Log.Info($"{nameof(TreeControllerUISystem)}.{nameof(OnCreate)}");
            Enabled = false;
        }

        private void ChangeToolMode(int mode)
        {
            ToolMode toolMode = (ToolMode)mode;
            switch (toolMode)
            {
                case ToolMode.Plop:
                    m_ToolMode.Update((int)ToolMode.Plop);
                    ActivatePlopTrees();
                    break;
                case ToolMode.Brush:
                    m_ToolMode.Update((int)ToolMode.Brush);
                    ActivateBrushTrees();
                    break;
                case ToolMode.ChangeAge:
                    m_ToolMode.Update((int)ToolMode.ChangeAge);
                    ActivateTreeControllerTool();
                    break;
                case ToolMode.ChangeType:
                    m_ToolMode.Update((int)ToolMode.ChangeType);
                    ActivatePrefabChange();
                    break;
            }
        }

        private void ChangeSelectedAge(int age)
        {
            Ages selectedAges = (Ages)m_SelectedAges.value;
            Ages toggledAge = (Ages)age;
            if (toggledAge != Ages.All && (selectedAges & Ages.All) == Ages.All)
            {
                selectedAges &= ~Ages.All;
            }
            else if (toggledAge == Ages.All && selectedAges != Ages.None)
            {
                m_SelectedAges.Update((int)Ages.None);
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

            if ((int)selectedAges == 31)
            {
                selectedAges |= Ages.All;
            }

            m_SelectedAges.Update((int)selectedAges);
        }

        private void ChangeSelectionMode(int selectionMode)
        {
            m_SelectionMode.Update(selectionMode);
        }


        /// <summary>
        /// C# event handler for event callback from UI JavaScript.
        /// </summary>
        private void ActivatePrefabChange()
        {
            m_Log.Debug("Enable Tool with Prefab Change please.");
            m_ToolSystem.selected = Entity.Null;

            if (m_SelectedPrefabSet == string.Empty && m_ObjectToolSystem.prefab != null && m_TreeControllerTool.GetSelectedPrefabs().Count <= 1)
            {
                UnselectPrefabs();
                m_TreeControllerTool.ClearSelectedTreePrefabs();
                m_TreeControllerTool.SelectTreePrefab(m_ObjectToolSystem.prefab);
            }
            else if (m_SelectedPrefabSet != string.Empty && m_PrefabSetsLookup.ContainsKey(m_SelectedPrefabSet))
            {
                ChangePrefabSet(m_SelectedPrefabSet);
            }

            m_ToolSystem.activeTool = m_TreeControllerTool;
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript.
        /// </summary>
        /// <param name="ages">An array of bools for whether that age is selected.</param>
        private void ChangeSelectedAges(bool[] ages)
        {
            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ChangeSelectedAges)}");
            m_TreeControllerTool.ApplySelectedAges(ages);
        }

        /// <summary>
        /// activates tree controller tool.
        /// </summary>
        private void ActivateTreeControllerTool()
        {
            m_Log.Debug("Enable Tool please.");
            m_SelectedPrefabSet = string.Empty;
            UnselectPrefabs();
            m_TreeControllerTool.ClearSelectedTreePrefabs();
            m_ToolSystem.selected = Entity.Null;
            m_ToolSystem.activeTool = m_TreeControllerTool;
        }

        /// <summary>
        /// activates object tool plopping trees
        /// </summary>
        private void ActivatePlopTrees()
        {
            m_Log.Debug("Enable Object Tool plopping please.");
            m_ToolSystem.selected = Entity.Null;
            m_ObjectToolSystem.mode = ObjectToolSystem.Mode.Create;
            m_ToolSystem.activeTool = m_ObjectToolSystem;
        }


        /// <summary>
        /// activates tree controller tool.
        /// </summary>
        private void ActivateBrushTrees()
        {
            m_Log.Debug("Enable brushing trees please.");
            m_ToolSystem.selected = Entity.Null;
            m_ObjectToolSystem.mode = ObjectToolSystem.Mode.Brush;
            m_ToolSystem.activeTool = m_ObjectToolSystem;
        }

        /// <summary>
        /// Lots a string from JS.
        /// </summary>
        /// <param name="prefabSetID">ID from button for changing Prefab Set from JS.</param>
        private void ChangePrefabSet(string prefabSetID)
        {
            PrefabBase originallySelectedPrefab = m_TreeControllerTool.GetPrefab();
            List<PrefabBase> selectedPrefabs = m_TreeControllerTool.GetSelectedPrefabs();
            if (!m_PrefabSetsLookup.ContainsKey(prefabSetID))
            {
                UnselectPrefabs();
                m_TreeControllerTool.ClearSelectedTreePrefabs();
                m_SelectedPrefabSet = string.Empty;
                /*
                foreach (PrefabBase prefab in selectedPrefabs)
                {
                    if (m_ToolSystem.ActivatePrefabTool(prefab))
                    {
                        SelectPrefab(prefab);
                        break;
                    }
                }*/

                return;
            }

            bool ctrlKeyPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
            if (prefabSetID.Contains("custom") && selectedPrefabs.Count > 1 && ctrlKeyPressed)
            {
                m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ChangePrefabSet)} trying to add prefab ids to set lookup.");
                TrySaveCustomPrefabSet(prefabSetID, selectedPrefabs);
            }

            if (m_PrefabSetsLookup[prefabSetID].Count == 0)
            {
                m_SelectedPrefabSet = string.Empty;
                m_TreeControllerTool.SelectTreePrefab(originallySelectedPrefab);
                m_Log.Warn($"{nameof(TreeControllerUISystem)}.{nameof(ChangePrefabSet)} could not select empty set");
                return;
            }

            UnselectPrefabs();
            m_TreeControllerTool.ClearSelectedTreePrefabs();
            m_SelectedPrefabSet = prefabSetID;

            /*
            int i = 0;
            foreach (PrefabID id in m_PrefabSetsLookup[prefabSetID])
            {
                if (m_PrefabSystem.TryGetPrefab(id, out PrefabBase prefab))
                {
                    m_TreeControllerTool.SelectTreePrefab(prefab);
                    SelectPrefab(prefab);
                    i++;
                }
            }*/

            selectedPrefabs = m_TreeControllerTool.GetSelectedPrefabs();
            if (!selectedPrefabs.Contains(m_ToolSystem.activePrefab))
            {
                foreach (PrefabBase prefab in selectedPrefabs)
                {
                    if (m_ToolSystem.ActivatePrefabTool(prefab))
                    {
                        break;
                    }
                }
            }
        }

        private void UnselectPrefabs()
        {
            // This script creates the Tree Controller object if it doesn't exist.
            NativeList<Entity> m_VegetationPrefabEntities = m_VegetationQuery.ToEntityListAsync(Allocator.Temp, out JobHandle jobHandle);
            jobHandle.Complete();
            /*
            foreach (Entity e in m_VegetationPrefabEntities)
            {
                if (m_PrefabSystem.TryGetPrefab(e, out PrefabBase prefab))
                {
                    // This script searches through all img and adds removes selected if the src of that image contains the name of the prefab and is not the active prefab.
                    UIFileUtils.ExecuteScript(m_UiView, $"yyTreeController.tagElements = document.getElementsByTagName(\"img\"); for (yyTreeController.i = 0; yyTreeController.i < yyTreeController.tagElements.length; yyTreeController.i++) {{ if (yyTreeController.tagElements[yyTreeController.i].src.includes(\"{prefab.name}\")) {{ yyTreeController.tagElements[yyTreeController.i].parentNode.classList.remove(\"selected\");  }} }} ");
                }
            }*/

            m_VegetationPrefabEntities.Dispose();
            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(UnselectPrefabs)}");
        }

        /// <summary>
        /// Logs a string from JS.
        /// </summary>
        /// <param name="log">A string from JS to log.</param>
        private void LogFromJS(string log)
        {
            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(LogFromJS)} {log}");
        }

        /// <summary>
        /// Increases the radius.
        /// </summary>
        private void IncreaseRadius()
        {
            float radius = m_Radius.value;
            if (radius >= 500 && radius < 1000)
            {
                radius += 100;
            }
            else if (radius >= 100 && radius < 500)
            {
                radius += 50;
            }
            else if (radius < 1000)
            {
                radius += 10;
            }

            m_Radius.Update(radius);
        }

        /// <summary>
        /// Decreases the radius.
        /// </summary>
        private void DecreaseRadius()
        {
            float radius = m_Radius.value;
            if (radius <= 100 && radius > 10)
            {
                radius -= 10;
            }
            else if (radius <= 500 && radius > 100)
            {
                radius -= 50;
            }
            else if (radius > 500)
            {
                radius -= 100;
            }

            m_Radius.Update(radius);
        }

        /// <summary>
        /// Method implemented by event triggered by tool changing.
        /// </summary>
        /// <param name="tool">The new tool.</param>
        private void OnToolChanged(ToolBaseSystem tool)
        {
            if (m_ToolSystem.activePrefab != null)
            {
                if (m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity prefabEntity))
                {
                    if (EntityManager.HasComponent<Vegetation>(prefabEntity))
                    {
                        m_IsVegetation.Update(true);
                    }
                    else
                    {
                        m_IsVegetation.Update(false);
                    }

                    if (EntityManager.HasComponent<TreeData>(prefabEntity))
                    {
                        m_IsTree.Update(true);
                    }
                    else
                    {
                        m_IsTree.Update(false);
                    }
                }
            }
            else
            {
                m_IsTree.Update(false);
                m_IsVegetation.Update(false);
            }
        }

        /// <summary>
        /// Method implemented by event triggered by prefab changing.
        /// </summary>
        /// <param name="prefab">The new prefab.</param>
        private void OnPrefabChanged(PrefabBase prefab)
        {
            if (prefab != null)
            {
                m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(OnPrefabChanged)} {prefab.name} {prefab.uiTag}");
            }
            else
            {
                m_IsTree.Update(false);
                m_IsVegetation.Update(false);
            }

            if (m_PrefabSystem.TryGetEntity(prefab, out Entity prefabEntity))
            {
                if (EntityManager.HasComponent<Vegetation>(prefabEntity))
                {
                    m_IsVegetation.Update(true);
                }
                else
                {
                    m_IsVegetation.Update(false);
                }

                if (EntityManager.HasComponent<TreeData>(prefabEntity))
                {
                    m_IsTree.Update(true);
                }
                else
                {
                    m_IsTree.Update(false);
                }
            }

            if (m_ObjectToolSystem.prefab != null)
            {
                m_LastObjectToolPrefab = m_ObjectToolSystem.prefab;
            }
        }

        private bool TrySaveCustomPrefabSet(string prefabSetID, List<PrefabBase> prefabBases)
        {
            List<PrefabID> prefabIDs = new List<PrefabID>();
            foreach (PrefabBase prefab in prefabBases)
            {
                prefabIDs.Add(prefab.GetPrefabID());
            }

            return TrySaveCustomPrefabSet(prefabSetID, prefabIDs);
        }

        private bool TrySaveCustomPrefabSet(string prefabSetID, List<PrefabID> prefabIDs)
        {
            string fileName = Path.Combine(m_ContentFolder, $"{prefabSetID}.xml");
            CustomSetRepository repository = new (prefabIDs);

            m_PrefabSetsLookup[prefabSetID].Clear();
            foreach (PrefabID prefab in prefabIDs)
            {
                m_PrefabSetsLookup[prefabSetID].Add(prefab);
            }

            try
            {
                XmlSerializer serTool = new XmlSerializer(typeof(CustomSetRepository)); // Create serializer
                using (System.IO.FileStream file = System.IO.File.Create(fileName)) // Create file
                {
                    serTool.Serialize(file, repository); // Serialize whole properties
                }

                return true;
            }
            catch (Exception ex)
            {
                m_Log.Warn($"{nameof(TreeControllerUISystem)}.{nameof(TrySaveCustomPrefabSet)} Could not save values for {prefabSetID}. Encountered exception {ex}");
                return false;
            }
        }

        private bool TryLoadCustomPrefabSet(string prefabSetID)
        {
            string fileName = Path.Combine(m_ContentFolder, $"{prefabSetID}.xml");
            if (File.Exists(fileName))
            {
                try
                {
                    XmlSerializer serTool = new XmlSerializer(typeof(CustomSetRepository)); // Create serializer
                    using System.IO.FileStream readStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open); // Open file
                    CustomSetRepository result = (CustomSetRepository)serTool.Deserialize(readStream); // Des-serialize to new Properties

                    if (m_PrefabSetsLookup.ContainsKey(prefabSetID) && result.GetPrefabIDs().Count > 0)
                    {
                        m_PrefabSetsLookup[prefabSetID] = result.GetPrefabIDs();
                    }

                    m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(TryLoadCustomPrefabSet)} loaded repository for {prefabSetID}.");
                    return true;
                }
                catch (Exception ex)
                {
                    m_Log.Warn($"{nameof(TreeControllerUISystem)}.{nameof(TryLoadCustomPrefabSet)} Could not get default values for Set {prefabSetID}. Encountered exception {ex}");
                    return false;
                }
            }

            return false;
        }
    }
}
