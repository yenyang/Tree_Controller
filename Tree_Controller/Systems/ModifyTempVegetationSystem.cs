// <copyright file="ModifyTempVegetationSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Input;
    using Game.Net;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Tools;
    using System;
    using Tree_Controller.Tools;
    using Tree_Controller.Utils;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.UniversalDelegates;

    /// <summary>
    /// Modifies Temp entities that are also trees.
    /// </summary>
    public partial class ModifyTempVegetationSystem : GameSystemBase
    {
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetToolSystem m_NetToolSystem;
        private EntityQuery m_TempOwnedTreeQuery;
        private EntityQuery m_TempOwnedVegetationQuery;
        private EntityQuery m_TempTreeQuery;
        private EntityQuery m_AppliedQuery;
        private TreeControllerUISystem m_UISystem;
        private Unity.Mathematics.Random m_Random;
        private ushort m_RandomSeed;
        private ProxyAction m_ApplyAction;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_Log.Info($"{nameof(ModifyTempVegetationSystem)}.{nameof(OnCreate)}");
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_NetToolSystem = World.GetOrCreateSystemManaged<NetToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_UISystem = World.GetOrCreateSystemManaged<TreeControllerUISystem>();
            m_ApplyAction = TreeControllerMod.Instance.Settings.GetAction(TreeControllerMod.ApplyMimicAction);
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) =>
            {
                Enabled = tool == m_ObjectToolSystem || (tool.toolID != null && tool.toolID == "Line Tool") || tool == m_NetToolSystem;
                m_ApplyAction.shouldBeEnabled = Enabled;
            };

            m_Random = new Unity.Mathematics.Random((ushort)DateTime.Now.Millisecond);
            m_RandomSeed = (ushort)m_Random.NextInt(0, ushort.MaxValue);

            m_TempOwnedVegetationQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Temp, Owner, PseudoRandomSeed>()
                .WithAny<Game.Objects.Tree, Game.Objects.Plant>()
                .WithNone<Deleted, Overridden>()
                .Build();

            m_TempOwnedTreeQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Temp, Owner, Game.Objects.Tree>()
                .WithNone<Deleted, Overridden>()
                .Build();

            m_TempTreeQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Temp, Game.Objects.Tree, PseudoRandomSeed>()
                .WithNone<Deleted, Overridden, Owner>()
                .Build();

            m_AppliedQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Applied, Game.Objects.Tree, PseudoRandomSeed, Owner>()
                .WithNone<Deleted, Overridden, Temp>()
                .Build();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (!m_TempOwnedVegetationQuery.IsEmptyIgnoreFilter)
            {
                NativeArray<Entity> entities = m_TempOwnedVegetationQuery.ToEntityArray(Allocator.Temp);
                ushort i = 1;
                foreach (Entity entity in entities)
                {
                    if (EntityManager.TryGetComponent(entity, out PseudoRandomSeed pseudoRandomSeed))
                    {
                        if (m_RandomSeed + i < ushort.MaxValue)
                        {
                            pseudoRandomSeed.m_Seed = (ushort)(m_RandomSeed + i);
                        }
                        else
                        {
                            pseudoRandomSeed.m_Seed = (ushort)(m_RandomSeed - i);
                        }

                        i++;
                        EntityManager.SetComponentData(entity, pseudoRandomSeed);
                    }
                }
            }

            if (TreeControllerMod.Instance.Settings.IncludeStumps && !m_TempTreeQuery.IsEmptyIgnoreFilter)
            {
                NativeArray<Entity> entities = m_TempTreeQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entities)
                {
                    if (!EntityManager.TryGetComponent(entity, out Game.Objects.Tree tree)
                        || !EntityManager.TryGetComponent(entity, out PrefabRef prefabRef)
                        || !EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                        || subMeshBuffer.Length <= 5)
                    {
                        continue;
                    }

                    if (tree.m_State == Game.Objects.TreeState.Dead && tree.m_Growth == 255)
                    {
                        tree.m_State = Game.Objects.TreeState.Stump;
                        tree.m_Growth = 0;
                        EntityManager.SetComponentData(entity, tree);
                    }
                }
            }

            if (!m_TempOwnedTreeQuery.IsEmptyIgnoreFilter)
            {
                NativeArray<Entity> entities = m_TempOwnedTreeQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entities)
                {
                    if (!EntityManager.TryGetComponent(entity, out PrefabRef prefabRef)
                        || !EntityManager.TryGetComponent(entity, out Game.Objects.Tree tree)
                        || !EntityManager.TryGetComponent(entity, out PseudoRandomSeed pseudoRandomSeed))
                    {
                        continue;
                    }

                    bool includeStump = false;
                    if (EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                       && subMeshBuffer.Length > 5)
                    {
                        includeStump = true;
                    }

                    Unity.Mathematics.Random random = new (pseudoRandomSeed.m_Seed);
                    TreeState nextTreeState = m_UISystem.GetNextTreeState(ref random, includeStump);
                    tree.m_State = nextTreeState;
                    EntityManager.SetComponentData(entity, tree);
                }
            }

            if (!m_AppliedQuery.IsEmptyIgnoreFilter)
            {
                int i = 0;
                NativeArray<Entity> entities = m_AppliedQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entities)
                {
                    if (!EntityManager.TryGetComponent(entity, out PrefabRef prefabRef)
                        || !EntityManager.TryGetComponent(entity, out Game.Objects.Tree tree)
                        || !EntityManager.TryGetComponent(entity, out PseudoRandomSeed pseudoRandomSeed)
                        || !EntityManager.TryGetComponent(entity, out Owner owner)
                        || !EntityManager.HasComponent<Edge>(owner.m_Owner))
                    {
                        continue;
                    }

                    if (m_RandomSeed + i < ushort.MaxValue)
                    {
                        pseudoRandomSeed.m_Seed = (ushort)(m_RandomSeed + i);
                    }
                    else
                    {
                        pseudoRandomSeed.m_Seed = (ushort)(m_RandomSeed - i);
                    }

                    i++;
                    EntityManager.SetComponentData(entity, pseudoRandomSeed);

                    bool includeStump = false;
                    if (EntityManager.TryGetBuffer(prefabRef.m_Prefab, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                       && subMeshBuffer.Length > 5)
                    {
                        includeStump = true;
                    }

                    Unity.Mathematics.Random random = new (pseudoRandomSeed.m_Seed);
                    TreeState nextTreeState = m_UISystem.GetNextTreeState(ref random, includeStump);
                    tree.m_State = nextTreeState;
                    EntityManager.SetComponentData(entity, tree);
                }
            }

            var applyAction = m_ToolSystem.activeTool.GetMemberValue("applyAction");
            if (applyAction as UIInputAction.State != null)
            {
                UIInputAction.State state = applyAction as UIInputAction.State;
                if (state.action.IsPressed() || state.action.WasPerformedThisFrame())
                {
                    m_RandomSeed = (ushort)m_Random.NextInt(0, ushort.MaxValue);
                }
            }
        }
    }
}
