// <copyright file="TreeObjectDefinitionSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Systems
{
    using System.Collections.Generic;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Tools;
    using Tree_Controller.Tools;
    using Tree_Controller.Utils;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Overrides tree state on placement with object tool based on setting.
    /// </summary>
    public partial class TreeObjectDefinitionSystem : GameSystemBase
    {
        private readonly Dictionary<TreeState, float> BrushTreeStateAges = new ()
        {
            { 0, 0 },
            { TreeState.Teen, ObjectUtils.TREE_AGE_PHASE_CHILD },
            { TreeState.Adult, ObjectUtils.TREE_AGE_PHASE_CHILD + ObjectUtils.TREE_AGE_PHASE_TEEN },
            { TreeState.Elderly, ObjectUtils.TREE_AGE_PHASE_CHILD + ObjectUtils.TREE_AGE_PHASE_TEEN + ObjectUtils.TREE_AGE_PHASE_ADULT },
            { TreeState.Dead, ObjectUtils.TREE_AGE_PHASE_CHILD + ObjectUtils.TREE_AGE_PHASE_TEEN + ObjectUtils.TREE_AGE_PHASE_ADULT + ObjectUtils.TREE_AGE_PHASE_ELDERLY + .00001f },
            { TreeState.Stump, ObjectUtils.TREE_AGE_PHASE_CHILD + ObjectUtils.TREE_AGE_PHASE_TEEN + ObjectUtils.TREE_AGE_PHASE_ADULT + ObjectUtils.TREE_AGE_PHASE_ELDERLY + ObjectUtils.TREE_AGE_PHASE_DEAD },
        };

        private ToolSystem m_ToolSystem;
        private TreeControllerUISystem m_TreeControllerUISystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_ObjectDefinitionQuery;
        private TreeControllerTool m_TreeControllerTool;
        private ILog m_Log;
        private ToolRaycastSystem m_ToolRaycastSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeObjectDefinitionSystem"/> class.
        /// </summary>
        public TreeObjectDefinitionSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_TreeControllerUISystem = World.GetOrCreateSystemManaged<TreeControllerUISystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_TreeControllerTool = World.GetOrCreateSystemManaged<TreeControllerTool>();
            m_ToolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
            m_Log.Info($"[{nameof(TreeObjectDefinitionSystem)}] {nameof(OnCreate)}");
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_ObjectToolSystem || (tool.toolID != null && tool.toolID == "Line Tool");
            m_ObjectDefinitionQuery = SystemAPI.QueryBuilder()
                .WithAllRW<CreationDefinition, Game.Tools.ObjectDefinition>()
                .WithAll<Updated>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_ObjectDefinitionQuery);
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            NativeArray<Entity> entities = m_ObjectDefinitionQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition currentCreationDefinition))
                {
                    entities.Dispose();
                    return;
                }

                if (!EntityManager.TryGetComponent(entity, out ObjectDefinition currentObjectDefinition) || !EntityManager.HasComponent<Vegetation>(currentCreationDefinition.m_Prefab))
                {
                    entities.Dispose();
                    return;
                }

                Entity prefabEntity = currentCreationDefinition.m_Prefab;
                Unity.Mathematics.Random random = new ((uint)currentCreationDefinition.m_RandomSeed);
                if ((m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush) || m_ToolSystem.activeTool.toolID == "Line Tool")
                {
                    prefabEntity = m_TreeControllerTool.GetNextPrefabEntity(ref random);
                    if (prefabEntity != Entity.Null)
                    {
                        currentCreationDefinition.m_Prefab = prefabEntity;
                        EntityManager.SetComponentData(entity, currentCreationDefinition);
                    }
                }

                if (m_ToolSystem.activeTool.toolID != "Line Tool" && EntityManager.HasComponent<TreeData>(prefabEntity))
                {
                    // This is a hack to prevent the vanilla age row from appearing.

                    // m_ObjectToolSystem.SetMemberValue("allowAge", false);
                    bool includeStump = false;
                    if (EntityManager.HasComponent<Game.Prefabs.TreeData>(prefabEntity)
                       && EntityManager.TryGetBuffer(prefabEntity, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                       && subMeshBuffer.Length > 5)
                    {
                        includeStump = true;
                    }

                    TreeState nextTreeState = m_TreeControllerUISystem.GetNextTreeState(ref random, includeStump);
                    if (BrushTreeStateAges.ContainsKey(nextTreeState))
                    {
                        currentObjectDefinition.m_Age = BrushTreeStateAges[nextTreeState];
                    }
                    else
                    {
                        currentObjectDefinition.m_Age = ObjectUtils.TREE_AGE_PHASE_CHILD + ObjectUtils.TREE_AGE_PHASE_TEEN;
                    }

                    EntityManager.SetComponentData(entity, currentObjectDefinition);
                }
            }

            if (TreeControllerMod.Instance.Settings.ConstrainBrush
                && m_ToolSystem.activeTool == m_ObjectToolSystem
                && m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush
                && m_ToolRaycastSystem.GetRaycastResult(out RaycastResult result)
                && !EntityManager.HasComponent<Deleted>(result.m_Owner))
            {
                foreach (Entity entity in entities)
                {
                    if (!EntityManager.TryGetComponent(entity, out ObjectDefinition currentObjectDefinition))
                    {
                        continue;
                    }

                    float2 objectXZ = new (currentObjectDefinition.m_Position.x, currentObjectDefinition.m_Position.z);
                    float2 raycastXZ = new (result.m_Hit.m_Position.x, result.m_Hit.m_Position.z);
                    if (Vector2.Distance(objectXZ, raycastXZ) > (0.333f * m_ObjectToolSystem.brushSize))
                    {
                        EntityManager.DestroyEntity(entity);
                    }
                }
            }

            entities.Dispose();
        }

    }
}
