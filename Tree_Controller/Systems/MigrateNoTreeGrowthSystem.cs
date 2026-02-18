// <copyright file="MigrateNoTreeGrowthSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Logging;
    using Game;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// System migrates NoTreeGrowth component to vanilla Game.Objects.Decoration.
    /// </summary>
    public partial class MigrateNoTreeGrowthSystem : GameSystemBase
    {
        private ILog m_Log;
        private EntityQuery m_NoTreeGrowthQuery;
        private EndFrameBarrier m_EndFrameBarrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateNoTreeGrowthSystem"/> class.
        /// </summary>
        public MigrateNoTreeGrowthSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_Log.Info($"[{nameof(MigrateNoTreeGrowthSystem)}] {nameof(OnCreate)}");

            m_NoTreeGrowthQuery = SystemAPI.QueryBuilder()
                .WithAllRW<NoTreeGrowth>()
                .Build();

            RequireAnyForUpdate(m_NoTreeGrowthQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
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
    }
}