// <copyright file="SafelyRemoveSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Objects;
    using Game.Simulation;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// A system for resetting model states and removing custom components.
    /// </summary>
    public partial class SafelyRemoveSystem : GameSystemBase
    {
        private EndFrameBarrier m_EndFrameBarrier;
        private TimeSystem m_TimeSystem;
        private EntityQuery m_DeciduousTreeQuery;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafelyRemoveSystem"/> class.
        /// </summary>
        public SafelyRemoveSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_EndFrameBarrier = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_TimeSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<TimeSystem>();
            Enabled = false;
            m_Log.Info($"{nameof(SafelyRemoveSystem)}.{nameof(OnCreate)}");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            m_DeciduousTreeQuery = SystemAPI.QueryBuilder()
                .WithAllRW<DeciduousData, Tree>()
                .Build();
            RequireForUpdate(m_DeciduousTreeQuery);

            if (m_DeciduousTreeQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            TreeSeasonChangeJob treeSeasonChangeJob = new ()
            {
                m_DeciduousTreeDataType = SystemAPI.GetComponentTypeHandle<DeciduousData>(),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                m_TreeType = SystemAPI.GetComponentTypeHandle<Tree>(),
                buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
            };

            JobHandle jobHandle = JobChunkExtensions.ScheduleParallel(treeSeasonChangeJob, m_DeciduousTreeQuery, Dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;
        }

#if BURST
        [BurstCompile]
#endif
        private struct TreeSeasonChangeJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<Game.Objects.Tree> m_TreeType;
            public ComponentTypeHandle<DeciduousData> m_DeciduousTreeDataType;
            public EntityCommandBuffer.ParallelWriter buffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Objects.Tree> treeNativeArray = chunk.GetNativeArray(ref m_TreeType);
                NativeArray<DeciduousData> deciduousTreeNativeArray = chunk.GetNativeArray(ref m_DeciduousTreeDataType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    Game.Objects.Tree currentTreeData = treeNativeArray[i];
                    DeciduousData currentDeciduousTreeData = deciduousTreeNativeArray[i];
                    buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity, default);
                    if (currentTreeData.m_State == TreeState.Dead && currentDeciduousTreeData.m_TechnicallyDead == false)
                    {
                        currentTreeData.m_State = currentDeciduousTreeData.m_PreviousTreeState;
                        buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                    }

                    buffer.RemoveComponent<DeciduousData>(unfilteredChunkIndex, currentEntity);
                }
            }
        }
    }
}
