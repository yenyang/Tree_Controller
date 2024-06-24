// <copyright file="TreeControllerUISystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Tree_Controller.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using Colossal.Annotations;
    using Colossal.Logging;
    using Colossal.PSI.Environment;
    using Colossal.UI.Binding;
    using Game.Objects;
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI;
    using Tree_Controller.Settings;
    using Tree_Controller.Systems;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using UnityEngine.InputSystem;

    /// <summary>
    /// UI system for Object Tool while using tree prefabs.
    /// </summary>
    public partial class TreeControllerUISystem : UISystemBase
    {
        private const string ModId = "Tree_Controller";

        private readonly Dictionary<TreeState, float> AgeWeights = new ()
        {
            { 0, ObjectUtils.TREE_AGE_PHASE_CHILD },
            { TreeState.Teen,  ObjectUtils.TREE_AGE_PHASE_TEEN },
            { TreeState.Adult, ObjectUtils.TREE_AGE_PHASE_ADULT },
            { TreeState.Elderly, ObjectUtils.TREE_AGE_PHASE_ELDERLY },
            { TreeState.Dead, ObjectUtils.TREE_AGE_PHASE_DEAD },
        };

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


        private cohtml.Net.View m_UiView;
        private ToolSystem m_ToolSystem;
        private PrefabSystem m_PrefabSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private TreeObjectDefinitionSystem m_TreeObjectDefinitionSystem;
        private TreeControllerTool m_TreeControllerTool;
        private ILog m_Log;
        private Dictionary<string, List<PrefabID>> m_PrefabSetsLookup;
        private string m_ContentFolder;
        private EntityQuery m_VegetationQuery;
        private ValueBinding<int> m_ToolMode;
        private List<Entity> m_ThemeEntities;
        private ValueBinding<int> m_SelectionMode;
        private ValueBinding<int> m_SelectedAges;
        private ValueBinding<float> m_Radius;
        private ValueBinding<bool> m_IsVegetation;
        private ValueBinding<bool> m_IsTree;
        private ValueBinding<string> m_SelectedPrefabSet;
        private bool m_UpdateSelectionSet = false;
        private bool m_RecentlySelectedPrefabSet = false;
        private bool m_MultiplePrefabsSelected = false;
        private int m_FrameCount = 0;
        [CanBeNull]
        private PrefabBase m_TrySetPrefabNextFrame;

        /// <summary>
        /// Gets or sets a value indicating whether the selection set of buttons on the Toolbar UI needs to be updated.
        /// </summary>
        public bool UpdateSelectionSet
        {
            get => m_UpdateSelectionSet;
            set => m_UpdateSelectionSet = value;
        }

        /// <summary>
        /// Gets or sets a value indicating the list of theme entities selected.
        /// </summary>
        public List<Entity> ThemeEntities
        {
            get => m_ThemeEntities;
            set => m_ThemeEntities = value;
        }

        /// <summary>
        /// Gets a value indicating whether a prefab set was recently selected.
        /// </summary>
        public bool RecentlySelectedPrefabSet
        {
            get => m_RecentlySelectedPrefabSet;
        }

        /// <summary>
        /// Gets the current tool mode.
        /// </summary>
        public ToolMode CurrentToolMode { get => (ToolMode)m_ToolMode.value; }

        /// <summary>
        /// Gets the Selection mode for tree controller tool.
        /// </summary>
        public Selection SelectionMode { get => (Selection)m_SelectionMode.value; }

        /// <summary>
        /// Gets the radius for tree controller tool with radius tool mode.
        /// </summary>
        public float Radius { get => m_Radius.value; }

        /// <summary>
        /// Gets a value indicating whether gets a bool for whether there are any ages selected.
        /// </summary>
        public bool AtLeastOneAgeSelected { get => m_SelectedAges.value != 0; }

        /// <summary>
        /// Sets a value indicating what to try to set prefab to next frame.
        /// </summary>
        public PrefabBase TrySetPrefabNextFrame { set => m_TrySetPrefabNextFrame = value; }

        /// <summary>
        /// Resets the selected prefab set.
        /// </summary>
        public void ResetPrefabSets()
        {
            if (m_RecentlySelectedPrefabSet)
            {
                return;
            }

            m_SelectedPrefabSet.Update(string.Empty);
            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ResetPrefabSets)} Resetting prefab sets.");
        }

        /// <summary>
        /// Gets a native list of tree states from selected ages.
        /// </summary>
        /// <returns>Native List of tree states.</returns>
        public NativeList<TreeState> GetSelectedAges()
        {
            Ages seletedAges = (Ages)m_SelectedAges.value;

            NativeList<TreeState> treeState = new NativeList<TreeState>(Allocator.TempJob);

            if (seletedAges == Ages.None)
            {
                treeState.Add(TreeState.Adult);
                return treeState;
            }

            if ((seletedAges & Ages.Child) == Ages.Child)
            {
                treeState.Add(0);
            }

            if ((seletedAges & Ages.Teen) == Ages.Teen)
            {
                treeState.Add(TreeState.Teen);
            }

            if ((seletedAges & Ages.Adult) == Ages.Adult)
            {
                treeState.Add(TreeState.Adult);
            }

            if ((seletedAges & Ages.Elderly) == Ages.Elderly)
            {
                treeState.Add(TreeState.Elderly);
            }

            if ((seletedAges & Ages.Dead) == Ages.Dead)
            {
                treeState.Add(TreeState.Dead);
            }

            return treeState;
        }

        /// <summary>
        /// Gets a tree state from the selected tree state given a random parameter.
        /// </summary>
        /// <param name="random">A source of randomness.</param>
        /// <returns>A random tree state from selected or Adult if none are selected.</returns>
        public TreeState GetNextTreeState(ref Unity.Mathematics.Random random)
        {
            List<TreeState> selectedTreeStates = new List<TreeState>();
            Ages seletedAges = (Ages)m_SelectedAges.value;
            if (seletedAges == Ages.None)
            {
                return TreeState.Adult;
            }

            if ((seletedAges & Ages.Child) == Ages.Child)
            {
                selectedTreeStates.Add(0);
            }

            if ((seletedAges & Ages.Teen) == Ages.Teen)
            {
                selectedTreeStates.Add(TreeState.Teen);
            }

            if ((seletedAges & Ages.Adult) == Ages.Adult)
            {
                selectedTreeStates.Add(TreeState.Adult);
            }

            if ((seletedAges & Ages.Elderly) == Ages.Elderly)
            {
                selectedTreeStates.Add(TreeState.Elderly);
            }

            if ((seletedAges & Ages.Dead) == Ages.Dead)
            {
                selectedTreeStates.Add(TreeState.Dead);
            }

            if (selectedTreeStates.Count == 1)
            {
                return selectedTreeStates[0];
            }

            int iterations = random.NextInt(10);
            for (int i = 0; i < iterations; i++)
            {
                random.NextInt();
            }

            switch (TreeControllerMod.Instance.Settings.AgeSelectionTechnique)
            {
                case TreeControllerSettings.AgeSelectionOptions.RandomEqualWeight:
                    return selectedTreeStates[random.NextInt(selectedTreeStates.Count)];

                case TreeControllerSettings.AgeSelectionOptions.RandomWeighted:
                    float totalWeight = 0f;
                    for (int i = 0; i < selectedTreeStates.Count; i++)
                    {
                        if (AgeWeights.ContainsKey(selectedTreeStates[i]))
                        {
                            totalWeight += AgeWeights[selectedTreeStates[i]];
                        }
                    }

                    float randomWeight = random.NextFloat(totalWeight);
                    float currentWeight = 0f;
                    for (int i = 0; i < selectedTreeStates.Count; i++)
                    {
                        if (AgeWeights.ContainsKey(selectedTreeStates[i]))
                        {
                            currentWeight += AgeWeights[selectedTreeStates[i]];
                        }

                        if (randomWeight < currentWeight)
                        {
                            return selectedTreeStates[i];
                        }
                    }

                    return selectedTreeStates[selectedTreeStates.Count - 1];
            }

            return TreeState.Adult;
        }

        /// <summary>
        /// Adds selected to the selected prefab.
        /// </summary>
        /// <param name="prefab">The selected prefab.</param>
        public void SelectPrefab(PrefabBase prefab)
        {
            if (prefab == null)
            {
                return;
            }

            // This script creates the Tree Controller object if it doesn't exist.
            m_UiView.ExecuteScript("if (yyTreeController == null) var yyTreeController = {};");

            // This script searches through all img and adds selected if the src of that image contains the name of the prefab.
            m_UiView.ExecuteScript($"yyTreeController.tagElements = document.getElementsByTagName(\"img\"); for (yyTreeController.i = 0; yyTreeController.i < yyTreeController.tagElements.length; yyTreeController.i++) {{ if (yyTreeController.tagElements[yyTreeController.i].src.includes(\"{prefab.name}\")) {{ yyTreeController.tagElements[yyTreeController.i].parentNode.classList.add(\"selected\");  }} }} ");
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_TreeObjectDefinitionSystem = World.GetOrCreateSystemManaged<TreeObjectDefinitionSystem>();
            m_UiView = GameManager.instance.userInterface.view.View;
            m_ThemeEntities = new List<Entity>();
            m_TreeControllerTool = World.GetOrCreateSystemManaged<TreeControllerTool>();
            m_ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "Mods_Yenyang_Tree_Controller", "CustomSets");
            System.IO.Directory.CreateDirectory(m_ContentFolder);
            m_ToolSystem.EventToolChanged += OnToolChanged;
            m_ToolSystem.EventPrefabChanged += OnPrefabChanged;
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
            AddBinding(m_SelectedPrefabSet = new ValueBinding<string>(ModId, "PrefabSet", string.Empty));

            // This section handles trigger bindings which listen for triggers from UI and then start an event.
            AddBinding(new TriggerBinding<int>(ModId, "ChangeToolMode", ChangeToolMode));
            AddBinding(new TriggerBinding<int>(ModId, "ChangeSelectedAge", ChangeSelectedAge));
            AddBinding(new TriggerBinding<int>(ModId, "ChangeSelectionMode", ChangeSelectionMode));
            AddBinding(new TriggerBinding(ModId, "radius-up-arrow", IncreaseRadius));
            AddBinding(new TriggerBinding(ModId, "radius-down-arrow", DecreaseRadius));
            AddBinding(new TriggerBinding<string>(ModId, "ChangePrefabSet", ChangePrefabSet));

            m_VegetationQuery = GetEntityQuery(ComponentType.ReadOnly<Vegetation>());

            m_Log.Info($"{nameof(TreeControllerUISystem)}.{nameof(OnCreate)}");
            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            List<PrefabBase> selectedPrefabs = m_TreeControllerTool.GetSelectedPrefabs();

            if (m_TrySetPrefabNextFrame != null)
            {
                m_ToolSystem.ActivatePrefabTool(m_TrySetPrefabNextFrame);
                m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(OnUpdate)} ActivatedPrefabTool {m_TrySetPrefabNextFrame.name}.");
                m_TrySetPrefabNextFrame = null;
                return;
            }

            if (m_UiView is null)
            {
                m_Log.Info($"{nameof(TreeControllerUISystem)}.{nameof(OnUpdate)} m_UiView is null. Tried to reset it.");
                m_UiView = GameManager.instance.userInterface.view.View;
            }

            if ((m_ToolSystem.activeTool == m_TreeControllerTool || m_ToolSystem.activeTool.toolID == "Line Tool" || (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush)) && m_UiView != null)
            {
                // This script creates the Tree Controller object if it doesn't exist.
                m_UiView.ExecuteScript("if (yyTreeController == null) var yyTreeController = {};");

                if (m_MultiplePrefabsSelected == false && m_TreeControllerTool.GetSelectedPrefabs().Count > 1)
                {
                    m_UpdateSelectionSet = true;
                }

                if (m_UpdateSelectionSet && m_FrameCount <= 5)
                {
                    if (m_FrameCount < 5)
                    {
                        UnselectPrefabs();
                    }

                    foreach (PrefabBase prefab in selectedPrefabs)
                    {
                        SelectPrefab(prefab);
                    }

                    if (selectedPrefabs.Count > 1)
                    {
                        m_MultiplePrefabsSelected = true;
                    }
                    else
                    {
                        m_MultiplePrefabsSelected = false;
                    }

                    if (m_FrameCount == 5)
                    {
                        m_UpdateSelectionSet = false;
                        m_RecentlySelectedPrefabSet = false;
                        m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(OnUpdate)} finished frame set. selectedPrefabs.Count = {selectedPrefabs.Count}");
                        m_FrameCount = 6;
                    }
                    else
                    {
                        m_FrameCount++;
                    }
                }
                else if (m_UpdateSelectionSet)
                {
                    if (m_FrameCount == 6)
                    {
                        m_FrameCount = 0;
                    }

                    m_FrameCount++;
                }
            }
            else if (m_UiView != null && m_MultiplePrefabsSelected && m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create)
            {
                m_UiView.ExecuteScript("if (yyTreeController == null) var yyTreeController = {};");
                UnselectPrefabs();
                SelectPrefab(m_ToolSystem.activePrefab);
                m_MultiplePrefabsSelected = false;
            }

            if (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush && m_ToolMode.value != (int)ToolMode.Brush)
            {
                m_ToolMode.Update((int)ToolMode.Brush);
            }

            if (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create && m_ToolMode.value != (int)ToolMode.Plop)
            {
                m_ToolMode.Update((int)ToolMode.Plop);
            }

            base.OnUpdate();
            return;
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
            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ChangeSelectedAge)} selectedAges = {selectedAges}");
            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ChangeSelectedAge)} toggled = {toggledAge}");
            if (toggledAge != Ages.All && (selectedAges & Ages.All) == Ages.All)
            {
                m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ChangeSelectedAge)} removed all.");
                selectedAges &= ~Ages.All;
            }
            else if (toggledAge == Ages.All && selectedAges != Ages.None)
            {
                m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ChangeSelectedAge)} setting to none.");
                m_SelectedAges.Update((int)Ages.None);
                return;
            }
            else if (toggledAge == Ages.All && selectedAges == Ages.None)
            {
                selectedAges |= Ages.Child | Ages.Teen | Ages.Adult | Ages.Elderly | Ages.Elderly | Ages.Dead | Ages.All;
                m_SelectedAges.Update((int)selectedAges);
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

            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(ChangeSelectedAge)} selectedAges = {selectedAges}");
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

            if (m_SelectedPrefabSet.value == string.Empty && m_ObjectToolSystem.prefab != null && m_TreeControllerTool.GetSelectedPrefabs().Count <= 1)
            {
                UnselectPrefabs();
                m_TreeControllerTool.ClearSelectedTreePrefabs();
                m_TreeControllerTool.SelectTreePrefab(m_ObjectToolSystem.prefab);
            }
            else if (m_SelectedPrefabSet.value != string.Empty && m_PrefabSetsLookup.ContainsKey(m_SelectedPrefabSet.value))
            {
                ChangePrefabSet(m_SelectedPrefabSet.value);
            }

            m_UpdateSelectionSet = true;
            m_ToolSystem.activeTool = m_TreeControllerTool;
        }

        /// <summary>
        /// activates tree controller tool.
        /// </summary>
        private void ActivateTreeControllerTool()
        {
            m_Log.Debug("Enable Tool please.");
            m_SelectedPrefabSet.Update(string.Empty);
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
                m_SelectedPrefabSet.Update(string.Empty);

                foreach (PrefabBase prefab in selectedPrefabs)
                {
                    if (m_ToolSystem.ActivatePrefabTool(prefab))
                    {
                        SelectPrefab(prefab);
                        break;
                    }
                }

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
                m_SelectedPrefabSet.Update(string.Empty);
                m_TreeControllerTool.SelectTreePrefab(originallySelectedPrefab);
                m_Log.Warn($"{nameof(TreeControllerUISystem)}.{nameof(ChangePrefabSet)} could not select empty set");
                return;
            }

            UnselectPrefabs();
            m_TreeControllerTool.ClearSelectedTreePrefabs();
            m_RecentlySelectedPrefabSet = true;
            m_SelectedPrefabSet.Update(prefabSetID);
            int i = 0;
            foreach (PrefabID id in m_PrefabSetsLookup[prefabSetID])
            {
                if (m_PrefabSystem.TryGetPrefab(id, out PrefabBase prefab))
                {
                    m_TreeControllerTool.SelectTreePrefab(prefab);
                    SelectPrefab(prefab);
                    i++;
                }
            }

            m_UpdateSelectionSet = true;

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
            NativeList<Entity> m_VegetationPrefabEntities = m_VegetationQuery.ToEntityListAsync(Allocator.Temp, out JobHandle jobHandle);
            jobHandle.Complete();

            foreach (Entity e in m_VegetationPrefabEntities)
            {
                if (m_PrefabSystem.TryGetPrefab(e, out PrefabBase prefab))
                {
                    // This script creates the Tree Controller object if it doesn't exist.
                    m_UiView.ExecuteScript("if (yyTreeController == null) var yyTreeController = {};");

                    // This script searches through all img and adds removes selected if the src of that image contains the name of the prefab and is not the active prefab.
                    m_UiView.ExecuteScript($"yyTreeController.tagElements = document.getElementsByTagName(\"img\"); for (yyTreeController.i = 0; yyTreeController.i < yyTreeController.tagElements.length; yyTreeController.i++) {{ if (yyTreeController.tagElements[yyTreeController.i].src.includes(\"{prefab.name}\")) {{ yyTreeController.tagElements[yyTreeController.i].parentNode.classList.remove(\"selected\");  }} }} ");
                }
            }

            m_VegetationPrefabEntities.Dispose();
            m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(UnselectPrefabs)}");
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
            if (m_ToolSystem.activePrefab != null &&
                (tool == m_TreeControllerTool || tool.toolID == "Line Tool" || (tool == m_ObjectToolSystem && (m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create || m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush)))
                && m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity prefabEntity))
            {
                m_Log.Debug($"{nameof(TreeControllerUISystem)}.{nameof(OnToolChanged)} ");
                Enabled = true;
                m_IsVegetation.Update(EntityManager.HasComponent<Vegetation>(prefabEntity));
                List<PrefabBase> selectedPrefabs = m_TreeControllerTool.GetSelectedPrefabs();
                if (m_IsVegetation.value && selectedPrefabs.Count == 0)
                {
                    m_TreeControllerTool.SelectTreePrefab(m_ToolSystem.activePrefab);
                    selectedPrefabs = m_TreeControllerTool.GetSelectedPrefabs();
                    m_UpdateSelectionSet = true;
                }

                bool isTree = false;
                if (EntityManager.HasComponent<TreeData>(prefabEntity) && selectedPrefabs.Contains(m_ToolSystem.activePrefab))
                {
                    isTree = true;
                }
                else if (selectedPrefabs.Count > 1)
                {
                    foreach (PrefabBase prefabBase in selectedPrefabs)
                    {
                        if (m_PrefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity3) && EntityManager.HasComponent<TreeData>(prefabEntity3))
                        {
                            isTree = true;
                            break;
                        }
                    }
                }

                m_IsTree.Update(isTree);
                if (selectedPrefabs.Count > 1)
                {
                    m_UpdateSelectionSet = true;
                }

                if (tool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create)
                {
                    m_ToolMode.Update((int)ToolMode.Plop);
                }
                else if (tool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush)
                {
                    m_ToolMode.Update((int)ToolMode.Brush);
                }
            }
            else
            {
                Enabled = false;
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

            if (m_ToolSystem.activeTool != m_TreeControllerTool && m_ToolSystem.activeTool != m_ObjectToolSystem && m_ToolSystem.activeTool.toolID != "Line Tool")
            {
                return;
            }

            if ((m_ToolSystem.activeTool == m_TreeControllerTool || m_ToolSystem.activeTool.toolID == "Line Tool" || (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush)) && m_PrefabSystem.TryGetEntity(prefab, out Entity prefabEntity))
            {
                if (EntityManager.HasComponent<Vegetation>(prefabEntity))
                {
                    m_IsVegetation.Update(true);
                    List<PrefabBase> selectedPrefabs = m_TreeControllerTool.GetSelectedPrefabs();
                    bool isTree = false;
                    if (EntityManager.HasComponent<TreeData>(prefabEntity) && selectedPrefabs.Contains(prefab))
                    {
                        isTree = true;
                    }
                    else if (selectedPrefabs.Count > 1)
                    {
                        foreach (PrefabBase prefabBase in selectedPrefabs)
                        {
                            if (m_PrefabSystem.TryGetEntity(prefabBase, out Entity prefabEntity3) && EntityManager.HasComponent<TreeData>(prefabEntity3))
                            {
                                isTree = true;
                                break;
                            }
                        }
                    }

                    m_IsTree.Update(isTree);
                }
                else
                {
                    m_IsVegetation.Update(false);
                }
            }
            else if (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create && m_PrefabSystem.TryGetEntity(prefab, out Entity prefabEntity2))
            {
                m_IsVegetation.Update(EntityManager.HasComponent<Vegetation>(prefabEntity2));
                m_IsTree.Update(EntityManager.HasComponent<TreeData>(prefabEntity2));
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
