// <copyright file="FindTreesAndBushesSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using Tree_Controller.Components;
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
        private EntityQuery m_LumberResourceNeededQuery;
        private SafelyRemoveSystem m_SafelyRemoveSystem;
        private ILog m_Log;
        private EndFrameBarrier m_EndFrameBarrier;

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
            m_TreeQuery = SystemAPI.QueryBuilder()
                .WithAll<UpdateFrame, Game.Prefabs.PrefabRef, Game.Objects.Tree>()
                .WithNone<Deleted, Temp, Evergreen, DeciduousData, Overridden, LumberResource>()
                .Build();

            m_LumberResourceNeededQuery = SystemAPI.QueryBuilder()
                .WithAll<UpdateFrame, Game.Prefabs.PrefabRef, Game.Objects.Tree>()
                .WithAny<Evergreen, DeciduousData>()
                .WithNone<Deleted, Temp, Overridden, LumberResource>()
                .Build();

            RequireForUpdate(m_TreeQuery);
            m_Log.Info($"{nameof(FindTreesAndBushesSystem)} created!");
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (!m_LumberResourceNeededQuery.IsEmptyIgnoreFilter)
            {
                EntityCommandBuffer buffer = m_EndFrameBarrier.CreateCommandBuffer();
                NativeArray<Entity> entities = m_LumberResourceNeededQuery.ToEntityArray(Allocator.Temp);
                buffer.AddComponent<LumberResource>(entities);
                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i] != Entity.Null)
                    {
                        buffer.SetComponentEnabled<LumberResource>(entities[i], false);
                    }
                }
            }
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
                    m_DecorationLookup = SystemAPI.GetComponentLookup<Game.Objects.Decoration>(isReadOnly: true),
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
            [ReadOnly]
            public ComponentLookup<Game.Objects.Decoration> m_DecorationLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Prefabs.PrefabRef> prefabRefNativeArray = chunk.GetNativeArray(ref m_PrefabRefType);
                NativeArray<Game.Objects.Tree> treeNativeArray = chunk.GetNativeArray(ref m_TreeType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    if (entityNativeArray[i] == Entity.Null)
                    {
                        continue;
                    }

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

                        if (m_DecorationLookup.HasComponent(currentEntity) &&
                            m_DecorationLookup.IsComponentEnabled(currentEntity))
                        {
                            currentDeciduousTreeData.m_PermanentDecoration = true;
                        }

                        buffer.AddComponent(unfilteredChunkIndex, currentEntity, currentDeciduousTreeData);
                    }
                    else
                    {
                        buffer.AddComponent(unfilteredChunkIndex, currentEntity, default(Evergreen));
                    }

                    buffer.AddComponent(unfilteredChunkIndex, currentEntity, default(LumberResource));
                    buffer.SetComponentEnabled<LumberResource>(unfilteredChunkIndex, currentEntity, false);
                }
            }
        }
    }
}
