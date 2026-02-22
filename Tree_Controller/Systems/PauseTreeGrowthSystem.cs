// <copyright file="MigrateNoTreeGrowthSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// System migrates NoTreeGrowth component to vanilla Game.Objects.Decoration. Also Handles DisableTreeGrowth Setting.
    /// </summary>
    public partial class PauseTreeGrowthSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_NoTreeGrowthQuery;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_PauseTreeGrowthQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="PauseTreeGrowthSystem"/> class.
        /// </summary>
        public PauseTreeGrowthSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_Log.Info($"[{nameof(PauseTreeGrowthSystem)}] {nameof(OnCreate)}");

            m_NoTreeGrowthQuery = SystemAPI.QueryBuilder()
                .WithAllRW<NoTreeGrowth>()
                .Build();

            m_PauseTreeGrowthQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Objects.Tree>()
                .WithDisabledRW<Game.Objects.Decoration>()
                .WithNone<Game.Common.Deleted, Game.Tools.Temp, Game.Common.Overridden, Lumber>()
                .Build();

            RequireAnyForUpdate(m_PauseTreeGrowthQuery);
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (m_NoTreeGrowthQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            RemoveNoTreeGrowthJob removeNoTreeGrowthJob = new RemoveNoTreeGrowthJob()
            {
                m_DecorationLookup = SystemAPI.GetComponentLookup<Game.Objects.Decoration>(isReadOnly: true),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_TreeGrowthDisabled = TreeControllerMod.Instance.Settings.DisableTreeGrowth,
                m_DeciduousLookup = SystemAPI.GetComponentLookup<DeciduousData>(isReadOnly: true),
            };
            Dependency = removeNoTreeGrowthJob.ScheduleParallel(m_NoTreeGrowthQuery, Dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(Dependency);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (!TreeControllerMod.Instance.Settings.DisableTreeGrowth)
            {
                Enabled = false;
                return;
            }

            PauseTreeGrowthJob pauseTreeGrowthJob = new PauseTreeGrowthJob()
            {
                m_DecorationLookup = SystemAPI.GetComponentLookup<Game.Objects.Decoration>(isReadOnly: true),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_DeciduousLookup = SystemAPI.GetComponentLookup<DeciduousData>(isReadOnly: true),
            };
            Dependency = pauseTreeGrowthJob.ScheduleParallel(m_PauseTreeGrowthQuery, Dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(Dependency);
        }

#if BURST
        [BurstCompile]
#endif
        private struct RemoveNoTreeGrowthJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public EntityCommandBuffer.ParallelWriter buffer;
            [ReadOnly]
            public ComponentLookup<Game.Objects.Decoration> m_DecorationLookup;
            [ReadOnly]
            public bool m_TreeGrowthDisabled;
            [ReadOnly]
            public ComponentLookup<DeciduousData> m_DeciduousLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];

                    if (m_TreeGrowthDisabled &&
                        m_DecorationLookup.HasComponent(currentEntity))
                    {
                        buffer.SetComponentEnabled<Game.Objects.Decoration>(unfilteredChunkIndex, currentEntity, true);

                        if (m_DeciduousLookup.TryGetComponent(currentEntity, out DeciduousData decidous))
                        {
                            decidous.m_PermanentDecoration = true;
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, decidous);
                        }
                    }

                    buffer.RemoveComponent<NoTreeGrowth>(unfilteredChunkIndex, currentEntity);
                }
            }
        }


#if BURST
        [BurstCompile]
#endif
        private struct PauseTreeGrowthJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public EntityCommandBuffer.ParallelWriter buffer;
            [ReadOnly]
            public ComponentLookup<Game.Objects.Decoration> m_DecorationLookup;
            [ReadOnly]
            public ComponentLookup<DeciduousData> m_DeciduousLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];

                    if (m_DecorationLookup.HasComponent(currentEntity))
                    {
                        buffer.SetComponentEnabled<Game.Objects.Decoration>(unfilteredChunkIndex, currentEntity, true);

                        if (m_DeciduousLookup.TryGetComponent(currentEntity, out DeciduousData decidous))
                        {
                            decidous.m_PermanentDecoration = true;
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, decidous);
                        }
                    }
                }
            }
        }
    }
}