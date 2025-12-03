// <copyright file="ModifyTreeGrowthSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Prefabs.Climate;
    using Game.Simulation;
    using Game.Tools;
    using Tree_Controller.Utils;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// System overrides query for TreeGrowthSystem. Not compatible with other mods that alter this query.
    /// </summary>
    public partial class ModifyTreeGrowthSystem : GameSystemBase
    {
        private TreeGrowthSystem m_TreeGrowthSystem;
        private EntityQuery m_ModifiedTreeGrowthQuery;
        private ClimateSystem m_ClimateSystem;
        private PrefabSystem m_PrefabSystem;
        private FoliageUtils.Season m_Season = FoliageUtils.Season.Spring;
        private ILog m_Log;
        private EntityQuery m_DisabledTreeGrowthQuery;
        private EntityQuery m_DeciduousWinterTreeGrowthQuery;
        private EntityQuery m_RegularTreeGrowthQuery;
        private EntityQuery m_LumberQuery;
        private EndFrameBarrier m_EndFrameBarrier;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyTreeGrowthSystem"/> class.
        /// </summary>
        public ModifyTreeGrowthSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_TreeGrowthSystem = World.GetOrCreateSystemManaged<TreeGrowthSystem>();
            m_ClimateSystem = World.GetOrCreateSystemManaged<ClimateSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_Log.Info($"[{nameof(ModifyTreeGrowthSystem)}] {nameof(OnCreate)}");


            m_DisabledTreeGrowthQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Objects.Tree>()
                .WithNone<Deleted, Temp, Overridden, Lumber, NoTreeGrowth>()
                .Build();

            m_DeciduousWinterTreeGrowthQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Objects.Tree, DeciduousData>()
                .WithNone<Deleted, Temp, Overridden, Lumber, NoTreeGrowth>()
                .Build();

            m_RegularTreeGrowthQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Objects.Tree, NoTreeGrowth>()
                .WithNone<Deleted, Temp, Overridden>()
                .Build();

            m_LumberQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Objects.Tree, Lumber, NoTreeGrowth>()
                .WithNone<Deleted, Temp, Overridden>()
                .Build();

            RequireAnyForUpdate(m_DisabledTreeGrowthQuery, m_DeciduousWinterTreeGrowthQuery, m_RegularTreeGrowthQuery, m_LumberQuery);
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (mode == GameMode.Game)
            {
                m_ModifiedTreeGrowthQuery = SystemAPI.QueryBuilder()
                    .WithAll<UpdateFrame>()
                    .WithAllRW<Game.Objects.Tree>()
                    .WithNone<Deleted, Temp, Overridden, NoTreeGrowth>()
                    .Build();

                m_TreeGrowthSystem.SetMemberValue("m_TreeQuery", m_ModifiedTreeGrowthQuery);
                m_Log.Info($"[{nameof(ModifyTreeGrowthSystem)}] {nameof(OnUpdate)} ModifiedTreeGrowthQuery Activated.");
                m_TreeGrowthSystem.RequireForUpdate(m_ModifiedTreeGrowthQuery);
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            Entity currentClimate = m_ClimateSystem.currentClimate;
            if (currentClimate == Entity.Null)
            {
                return;
            }

            ClimatePrefab climatePrefab = m_PrefabSystem.GetPrefab<ClimatePrefab>(m_ClimateSystem.currentClimate);

            FoliageUtils.Season lastSeason = m_Season;
            m_Season = FoliageUtils.GetSeasonFromSeasonID(climatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.name);

            if (!m_LumberQuery.IsEmptyIgnoreFilter)
            {
                RemoveNoTreeGrowthJob removeNoTreeGrowthJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                };

                JobHandle lumberJob = JobChunkExtensions.ScheduleParallel(removeNoTreeGrowthJob, m_LumberQuery, Dependency);
                m_EndFrameBarrier.AddJobHandleForProducer(lumberJob);
                Dependency = lumberJob;
            }

            if (!m_DisabledTreeGrowthQuery.IsEmptyIgnoreFilter && TreeControllerMod.Instance.Settings.DisableTreeGrowth)
            {
                AddNoTreeGrowthJob addNoTreeGrowthJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                };

                JobHandle jobHandle = JobChunkExtensions.ScheduleParallel(addNoTreeGrowthJob, m_DisabledTreeGrowthQuery, Dependency);
                m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
                Dependency = jobHandle;
                return;
            }
            else if (TreeControllerMod.Instance.Settings.DisableTreeGrowth)
            {
                return;
            }

            if (!m_RegularTreeGrowthQuery.IsEmptyIgnoreFilter && (!TreeControllerMod.Instance.Settings.UseDeadModelDuringWinter || m_Season != FoliageUtils.Season.Winter))
            {
                RemoveNoTreeGrowthJob removeNoTreeGrowthJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                };

                JobHandle jobHandle = JobChunkExtensions.ScheduleParallel(removeNoTreeGrowthJob, m_RegularTreeGrowthQuery, Dependency);
                m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
                Dependency = jobHandle;
                return;
            }

            if (!m_DeciduousWinterTreeGrowthQuery.IsEmptyIgnoreFilter && TreeControllerMod.Instance.Settings.UseDeadModelDuringWinter && m_Season == FoliageUtils.Season.Winter)
            {
                AddNoTreeGrowthJob addNoTreeGrowthJob = new ()
                {
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                };

                JobHandle jobHandle = JobChunkExtensions.ScheduleParallel(addNoTreeGrowthJob, m_DeciduousWinterTreeGrowthQuery, Dependency);
                m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
                Dependency = jobHandle;
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct RemoveNoTreeGrowthJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public EntityCommandBuffer.ParallelWriter buffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    buffer.RemoveComponent<NoTreeGrowth>(unfilteredChunkIndex, currentEntity);
                }
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct AddNoTreeGrowthJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public EntityCommandBuffer.ParallelWriter buffer;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    buffer.AddComponent<NoTreeGrowth>(unfilteredChunkIndex, currentEntity);
                }
            }
        }
    }
}