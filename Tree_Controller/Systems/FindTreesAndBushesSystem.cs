// <copyright file="FindTreesAndBushesSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using System;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// Finds trees and bushes and adds Deciduous or Evergreen components.
    /// </summary>
    public partial class FindTreesAndBushesSystem : GameSystemBase
    {
        /// <summary>
        /// Relates to the update interval although the GetUpdateInterval isn't even using this.
        /// </summary>
        public const int UPDATES_PER_DAY = 32;
        private SimulationSystem m_SimulationSystem;
        private TimeSystem m_TimeSystem;
        private EntityQuery m_TreeQuery;
        private SafelyRemoveSystem m_SafelyRemoveSystem;
        private ILog m_Log;
        private EndFrameBarrier m_EndFrameBarrier;
        private PrefabSystem m_PrefabSystem;
        private ModifyVegetationPrefabsSystem m_ModifyVegetationPrefabsSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindTreesAndBushesSystem"/> class.
        /// </summary>
        public FindTreesAndBushesSystem()
        {
        }

        /// <inheritdoc/>
        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 512;
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_TimeSystem = World.GetOrCreateSystemManaged<TimeSystem>();
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_SafelyRemoveSystem = World.GetOrCreateSystemManaged<SafelyRemoveSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ModifyVegetationPrefabsSystem = World.GetOrCreateSystemManaged<ModifyVegetationPrefabsSystem>();
            m_TreeQuery = SystemAPI.QueryBuilder()
                .WithAll<UpdateFrame, Game.Prefabs.PrefabRef, Game.Objects.Tree>()
                .WithNone<Deleted, Temp, Evergreen, DeciduousData, Overridden, Lumber>()
                .Build();

            RequireForUpdate(m_TreeQuery);
            m_Log.Info($"{nameof(FindTreesAndBushesSystem)} created!");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (!m_TreeQuery.IsEmptyIgnoreFilter && TreeControllerMod.Instance.Settings.UseDeadModelDuringWinter)
            {
                uint updateFrame = SimulationUtils.GetUpdateFrame(m_SimulationSystem.frameIndex, 32, 16);
                m_TreeQuery.ResetFilter();
                m_TreeQuery.SetSharedComponentFilter(new UpdateFrame(updateFrame));
                FindTreePrefabRefsJob findTreePrefabRefJob = new()
                {
                    m_PrefabRefType = SystemAPI.GetComponentTypeHandle<Game.Prefabs.PrefabRef>(),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_TreeType = SystemAPI.GetComponentTypeHandle<Game.Objects.Tree>(),
                    buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EvergreenData = SystemAPI.GetComponentLookup<Evergreen>(isReadOnly: true),
                };
                JobHandle jobHandle = JobChunkExtensions.ScheduleParallel(findTreePrefabRefJob, m_TreeQuery, Dependency);
                m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
                Dependency = jobHandle;
                if (m_SafelyRemoveSystem.Enabled)
                {
                    m_SafelyRemoveSystem.Enabled = false;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            PrefabID vegetationPrefabID = new PrefabID("UIAssetCategoryPrefab", "Vegetation");
            if (!m_PrefabSystem.TryGetPrefab(vegetationPrefabID, out PrefabBase vegetationPrefab))
            {
                m_Log.Error(new Exception("Tree controller cound not find the vegetation tab prefab"));
                return;
            }

            if (!m_PrefabSystem.TryGetEntity(vegetationPrefab, out Entity vegetationEntity))
            {
                m_Log.Error(new Exception("Tree controller cound not find the vegetation tab entity"));
                return;
            }

            if (!EntityManager.TryGetBuffer(vegetationEntity, true, out DynamicBuffer<UIGroupElement> buffer))
            {
                m_Log.Error(new Exception("Tree controller cound not find the vegetation tab group element buffer."));
                return;
            }

            foreach (UIGroupElement element in buffer)
            {
                if (EntityManager.HasComponent<Vegetation>(element.m_Prefab))
                {
                    continue;
                }

                EntityManager.AddComponent<Vegetation>(element.m_Prefab);
                if (EntityManager.TryGetComponent(element.m_Prefab, out ObjectGeometryData objectGeometryData))
                {
                    m_Log.Debug($"{nameof(FindTreesAndBushesSystem)}.{nameof(OnGameLoadingComplete)} objectGeometryData.m_size = {objectGeometryData.m_Size.x}:{objectGeometryData.m_Size.z}");
                    Vegetation vegetation = new Vegetation(new Unity.Mathematics.float3(objectGeometryData.m_Size.x, 0, objectGeometryData.m_Size.z));
                    EntityManager.SetComponentData(element.m_Prefab, vegetation);

                    if (TreeControllerMod.Instance.Settings.LimitedTreeAnarchy)
                    {
                        objectGeometryData.m_Size.x = objectGeometryData.m_LegSize.x;
                        objectGeometryData.m_Size.z = objectGeometryData.m_LegSize.z;
                        EntityManager.SetComponentData(element.m_Prefab, objectGeometryData);
                    }
                }
                else
                {
                    if (m_PrefabSystem.TryGetPrefab(element.m_Prefab, out PrefabBase prefabBase))
                    {
                        m_Log.Info($"{nameof(FindTreesAndBushesSystem)}.{nameof(OnGameLoadingComplete)} A vegetation prefab: {prefabBase.name} didn't have Object Geometry Data.'");
                    }
                    else
                    {
                        m_Log.Info($"{nameof(FindTreesAndBushesSystem)}.{nameof(OnGameLoadingComplete)} An unknown vegetation prefab didn't have Object Geometry Data.'");
                    }
                }
            }

            if (TreeControllerMod.Instance.Settings.FreeVegetation)
            {
                m_ModifyVegetationPrefabsSystem.SetVegetationCostsToZero();
            }

            base.OnGameLoadingComplete(purpose, mode);
        }

#if BURST
        [BurstCompile]
#endif
        private struct FindTreePrefabRefsJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            [ReadOnly]
            public ComponentTypeHandle<Game.Prefabs.PrefabRef> m_PrefabRefType;
            public EntityCommandBuffer.ParallelWriter buffer;
            [ReadOnly]
            public ComponentTypeHandle<Game.Objects.Tree> m_TreeType;
            [ReadOnly]
            public ComponentLookup<Evergreen> m_EvergreenData;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Prefabs.PrefabRef> prefabRefNativeArray = chunk.GetNativeArray(ref m_PrefabRefType);
                NativeArray<Game.Objects.Tree> treeNativeArray = chunk.GetNativeArray(ref m_TreeType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    Game.Prefabs.PrefabRef currentPrefabRef = prefabRefNativeArray[i];
                    Entity prefabEntity = currentPrefabRef.m_Prefab;
                    Game.Objects.Tree currentTreeData = treeNativeArray[i];
                    if (!m_EvergreenData.HasComponent(prefabEntity)) // Is Deciduous?
                    {
                        DeciduousData currentDeciduousTreeData = default;
                        currentDeciduousTreeData.m_PreviousTreeState = currentTreeData.m_State;
                        if (currentTreeData.m_State == TreeState.Dead)
                        {
                            currentDeciduousTreeData.m_TechnicallyDead = true;
                        }
                        else
                        {
                            currentDeciduousTreeData.m_TechnicallyDead = false;
                        }

                        buffer.AddComponent(unfilteredChunkIndex, currentEntity, currentDeciduousTreeData);
                    }
                    else
                    {
                        buffer.AddComponent(unfilteredChunkIndex, currentEntity, default(Evergreen));
                    }
                }
            }
        }
    }
}
