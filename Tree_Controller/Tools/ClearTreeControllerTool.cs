// <copyright file="ClearTreeControllerTool.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Tools
{
    using Colossal.Logging;
    using Game;
    using Game.Tools;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// Removed Recently Changed Components.
    /// </summary>
    public partial class ClearTreeControllerTool : GameSystemBase
    {
        private EntityQuery m_RecentlyChangedQuery;
        private ToolOutputBarrier m_ToolOutputBarrier;
        private ILog m_Log;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_ToolOutputBarrier = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_Log = TreeControllerMod.Instance.Logger;
            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            m_RecentlyChangedQuery = SystemAPI.QueryBuilder()
                .WithAll<RecentlyChanged>()
                .Build();

            RequireForUpdate(m_RecentlyChangedQuery);
            RemoveRecentlyChangedComponent removeRecentlyChangedComponentJob = new()
            {
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                buffer = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
            };
            JobHandle jobHandle = removeRecentlyChangedComponentJob.ScheduleParallel(m_RecentlyChangedQuery, Dependency);
            m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;
            m_Log.Debug($"{nameof(ClearTreeControllerTool)}.{nameof(OnUpdate)} Removed Recently Changed Components");
        }

#if BURST
        [BurstCompile]
#endif
        private struct RemoveRecentlyChangedComponent : IJobChunk
        {
            public EntityTypeHandle m_EntityType;
            public EntityCommandBuffer.ParallelWriter buffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    buffer.RemoveComponent<RecentlyChanged>(unfilteredChunkIndex, currentEntity);
                }
            }
        }
    }
}
