﻿// <copyright file="DeciduousSystem.cs" company="Yenyangs Mods. MIT License">
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
    /// System handles and additional seasonal changes to Deciduous Trees.
    /// </summary>
    public partial class DeciduousSystem : GameSystemBase
    {
        /// <summary>
        /// Relates to the update interval although the GetUpdateInterval isn't even using this.
        /// </summary>
        public const int UPDATES_PER_DAY = 32;
        private SimulationSystem m_SimulationSystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_DeciduousTreeQuery;
        private EntityQuery m_TreePrefabQuery;
        private ClimateSystem m_ClimateSystem;
        private SafelyRemoveSystem m_SafelyRemoveSystem;
        private PrefabSystem m_PrefabSystem;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeciduousSystem"/> class.
        /// </summary>
        public DeciduousSystem()
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
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ClimateSystem = World.GetOrCreateSystemManaged<ClimateSystem>();
            m_SafelyRemoveSystem = World.GetOrCreateSystemManaged<SafelyRemoveSystem>();
            m_Log.Info($"{nameof(DeciduousSystem)} created!");


            m_DeciduousTreeQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Game.Objects.Tree>(),
                        ComponentType.ReadWrite<DeciduousData>(),
                        ComponentType.ReadOnly<UpdateFrame>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Overridden>(),
                    },
                },
            });

            m_TreePrefabQuery = SystemAPI.QueryBuilder()
                .WithAll<TreeData>()
                .WithNone<PlaceholderObjectElement, Evergreen>()
                .Build();

            RequireForUpdate(m_TreePrefabQuery);
            RequireForUpdate(m_DeciduousTreeQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            Entity currentClimate = m_ClimateSystem.currentClimate;
            if (currentClimate == Entity.Null)
            {
                return;
            }

            if (!m_PrefabSystem.TryGetPrefab(m_ClimateSystem.currentClimate, out ClimatePrefab climatePrefab))
            {
                return;
            }

            uint updateFrame = SimulationUtils.GetUpdateFrame(m_SimulationSystem.frameIndex, 32, 16);
            m_DeciduousTreeQuery.ResetFilter();
            m_DeciduousTreeQuery.SetSharedComponentFilter(new UpdateFrame(updateFrame));

            // m_Log.Debug(FoliageUtils.GetSeasonFromSeasonID(climatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID));
            TreeSeasonChangeJob treeSeasonChangeJob = new()
            {
                m_TreeType = SystemAPI.GetComponentTypeHandle<Game.Objects.Tree>(),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                m_DeciduousTreeDataType = SystemAPI.GetComponentTypeHandle<DeciduousData>(),
                buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_Season = FoliageUtils.GetSeasonFromSeasonID(climatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID),
                m_LumberLookup = SystemAPI.GetComponentLookup<Lumber>(),
            };
            JobHandle jobHandle = JobChunkExtensions.ScheduleParallel(treeSeasonChangeJob, m_DeciduousTreeQuery, Dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;

            if (!TreeControllerMod.Instance.Settings.UseDeadModelDuringWinter)
            {
                m_SafelyRemoveSystem.Enabled = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            EntityQuery deciduousTreeQuery = SystemAPI.QueryBuilder()
                   .WithAllRW<Tree, DeciduousData>()
                   .Build();

            // This is for removing deciduous data from any trees that had it accidently added such as palm trees.
            RemoveDecidousDataJob removeDecidousDataJob = new ()
            {
                m_DeciduousTreeDataType = SystemAPI.GetComponentTypeHandle<DeciduousData>(),
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                m_EvergreenLookup = SystemAPI.GetComponentLookup<Evergreen>(isReadOnly: true),
                m_PrefabRefType = SystemAPI.GetComponentTypeHandle<PrefabRef>(isReadOnly: true),
                m_TreeType = SystemAPI.GetComponentTypeHandle<Tree>(),
                buffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
            };

            JobHandle jobHandle = removeDecidousDataJob.ScheduleParallel(deciduousTreeQuery, Dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            Dependency = jobHandle;
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
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
            public FoliageUtils.Season m_Season;
            [ReadOnly]
            public ComponentLookup<Lumber> m_LumberLookup;

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

                    if (m_LumberLookup.HasComponent(currentEntity))
                    {
                        if (currentDeciduousTreeData.m_PreviousTreeState != TreeState.Dead && currentTreeData.m_State == TreeState.Dead)
                        {
                            currentTreeData.m_State = currentDeciduousTreeData.m_PreviousTreeState;
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                            buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity);
                        }

                        buffer.RemoveComponent<DeciduousData>(unfilteredChunkIndex, currentEntity);
                        continue;
                    }

                    if (currentDeciduousTreeData.m_TechnicallyDead == true && currentTreeData.m_State != TreeState.Dead)
                    {
                        currentDeciduousTreeData.m_PreviousTreeState = currentTreeData.m_State;
                        currentDeciduousTreeData.m_TechnicallyDead = false;
                        buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentDeciduousTreeData);
                    }

                    if (m_Season == FoliageUtils.Season.Winter)
                    {
                        if (currentDeciduousTreeData.m_TechnicallyDead == false && currentTreeData.m_State != TreeState.Dead)
                        {
                            currentDeciduousTreeData.m_PreviousTreeState = currentTreeData.m_State;
                            currentTreeData.m_State = TreeState.Dead;
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                            buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity, default);
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentDeciduousTreeData);
                        }
                    }
                    else
                    {
                        if (currentTreeData.m_State == TreeState.Dead && currentDeciduousTreeData.m_TechnicallyDead == false && currentDeciduousTreeData.m_PreviousTreeState != TreeState.Dead)
                        {
                            currentTreeData.m_State = currentDeciduousTreeData.m_PreviousTreeState;
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                            buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity, default);
                        }
                        else if (currentDeciduousTreeData.m_PreviousTreeState != currentTreeData.m_State)
                        {
                            currentDeciduousTreeData.m_PreviousTreeState = currentTreeData.m_State;
                            if (currentTreeData.m_State == TreeState.Dead)
                            {
                                currentDeciduousTreeData.m_TechnicallyDead = true;
                            }

                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentDeciduousTreeData);
                        }
                    }
                }
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct RemoveDecidousDataJob : IJobChunk
        {
            [ReadOnly]
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<Game.Objects.Tree> m_TreeType;
            public ComponentTypeHandle<DeciduousData> m_DeciduousTreeDataType;
            public EntityCommandBuffer.ParallelWriter buffer;
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabRefType;
            [ReadOnly]
            public ComponentLookup<Evergreen> m_EvergreenLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Objects.Tree> treeNativeArray = chunk.GetNativeArray(ref m_TreeType);
                NativeArray<DeciduousData> deciduousTreeNativeArray = chunk.GetNativeArray(ref m_DeciduousTreeDataType);
                NativeArray<PrefabRef> prefabRefNativeArray = chunk.GetNativeArray(ref m_PrefabRefType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    Game.Objects.Tree currentTreeData = treeNativeArray[i];
                    DeciduousData currentDeciduousTreeData = deciduousTreeNativeArray[i];
                    PrefabRef prefabRef = prefabRefNativeArray[i];
                    if (m_EvergreenLookup.HasComponent(prefabRef.m_Prefab))
                    {
                        if (currentDeciduousTreeData.m_PreviousTreeState != TreeState.Dead && currentTreeData.m_State == TreeState.Dead)
                        {
                            currentTreeData.m_State = currentDeciduousTreeData.m_PreviousTreeState;
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                            buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity);
                        }

                        buffer.RemoveComponent<DeciduousData>(unfilteredChunkIndex, currentEntity);
                    }
                }
            }
        }
    }
}
