// <copyright file="LumberSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Logging;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// Adds/Removes lumber to/from WoodResource trees.
    /// </summary>
    public partial class LumberSystem : GameSystemBase
    {
        private SimulationSystem m_SimulationSystem;
        private TimeSystem m_TimeSystem;
        private EntityQuery m_WoodResourceQuery;
        private EntityQuery m_LumberQuery;
        private SafelyRemoveSystem m_SafelyRemoveSystem;
        private ILog m_Log;
        private EndFrameBarrier m_EndFrameBarrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="LumberSystem"/> class.
        /// </summary>
        public LumberSystem()
        {
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
            m_WoodResourceQuery = SystemAPI.QueryBuilder()
               .WithAllRW<WoodResource>()
               .WithNone<Deleted, Temp>()
               .Build();
            RequireForUpdate(m_WoodResourceQuery);
            m_LumberQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Lumber>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_Log.Info($"{nameof(LumberSystem)} created!");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            JobHandle lumberJobHandle = new ();
            if (!m_LumberQuery.IsEmptyIgnoreFilter)
            {
                RemoveLumberJob removeLumberJob = new()
                {
                    buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                };
                lumberJobHandle = JobChunkExtensions.ScheduleParallel(removeLumberJob, m_LumberQuery, Dependency);
                m_EndFrameBarrier.AddJobHandleForProducer(lumberJobHandle);
            }

            JobHandle woodResourceJobHandle = new ();
            if (!m_WoodResourceQuery.IsEmptyIgnoreFilter)
            {
                FindLumberJob findLumberJob = new()
                {
                    buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_WoodResouceType = SystemAPI.GetBufferTypeHandle<WoodResource>(),
                };
                woodResourceJobHandle = JobChunkExtensions.ScheduleParallel(findLumberJob, m_WoodResourceQuery, lumberJobHandle);
                m_EndFrameBarrier.AddJobHandleForProducer(woodResourceJobHandle);
            }

            Dependency = JobHandle.CombineDependencies(Dependency, woodResourceJobHandle, lumberJobHandle);
            Enabled = false;
        }

#if BURST
        [BurstCompile]
#endif
        private struct FindLumberJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter buffer;
            [ReadOnly]
            public BufferTypeHandle<WoodResource> m_WoodResouceType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                BufferAccessor<WoodResource> woodResourceBufferAcessory = chunk.GetBufferAccessor(ref m_WoodResouceType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<WoodResource> woodResourceDynamicBuffer = woodResourceBufferAcessory[i];
                    foreach (WoodResource woodResource in woodResourceDynamicBuffer)
                    {
                        buffer.AddComponent<Lumber>(unfilteredChunkIndex, woodResource.m_Tree, default);
                    }
                }
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct RemoveLumberJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter buffer;
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    buffer.RemoveComponent<Lumber>(unfilteredChunkIndex, currentEntity);
                }
            }
        }
    }
}
