// <copyright file="LumberSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

#define BURST
namespace Tree_Controller.Systems
{
    using Colossal.Collections;
    using Colossal.Logging;
    using Colossal.Mathematics;
    using Game;
    using Game.Areas;
    using Game.City;
    using Game.Common;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Simulation;
    using Game.Tools;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// Adds/Removes lumber to/from WoodResource trees. And manages Decoration component.
    /// </summary>
    public partial class LumberSystem : GameSystemBase
    {
        private EntityQuery m_LumberQuery;
        private ILog m_Log;
        private ModificationEndBarrier m_Barrier;
        private Game.Objects.UpdateCollectSystem m_ObjectUpdateCollectSystem;
        private Game.Areas.SearchSystem m_AreaSearchSystem;
        private Game.Objects.SearchSystem m_ObjectSearchSystem;
        private NaturalResourceSystem m_NaturalResourceSystem;
        private EntityQuery m_WoodResourceAreaQuery;

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
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_ObjectUpdateCollectSystem = World.GetOrCreateSystemManaged<Game.Objects.UpdateCollectSystem>();
            m_AreaSearchSystem = World.GetOrCreateSystemManaged<Game.Areas.SearchSystem>();
            m_ObjectSearchSystem = World.GetOrCreateSystemManaged<Game.Objects.SearchSystem>();
            m_NaturalResourceSystem = World.GetOrCreateSystemManaged<NaturalResourceSystem>();

            m_WoodResourceAreaQuery = SystemAPI.QueryBuilder()
                .WithAll<Extractor, Game.Areas.WoodResource>()
                .WithNone<Deleted, Temp>()
                .Build();

            m_LumberQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Lumber>()
                .WithNone<Deleted, Temp>()
                .Build();

            RequireForUpdate(m_WoodResourceAreaQuery);

            m_Log.Info($"{nameof(LumberSystem)} created!");

            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            
            if (!m_LumberQuery.IsEmptyIgnoreFilter)
            {
                RemoveLumberJob removeLumberJob = new()
                {
                    buffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                };
                JobHandle lumberJobHandle = JobChunkExtensions.ScheduleParallel(removeLumberJob, m_LumberQuery, Dependency);
                m_Barrier.AddJobHandleForProducer(lumberJobHandle);
                Dependency = lumberJobHandle;
            }

            if (m_WoodResourceAreaQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            // Multiple parts copied from vanilla with edits.
            NativeQueue<Entity> updateBuffer = new NativeQueue<Entity>(Allocator.TempJob);
            NativeList<Entity> nativeList = new NativeList<Entity>(Allocator.TempJob);
            NativeQueue<Entity>.ParallelWriter updateBuffer2 = updateBuffer.AsParallelWriter();
            if (m_ObjectUpdateCollectSystem.isUpdated)
            {
                JobHandle dependencies2;
                NativeList<Bounds2> updatedBounds = m_ObjectUpdateCollectSystem.GetUpdatedBounds(out dependencies2);
                FindUpdatedAreasWithBoundsJob jobData2 = new FindUpdatedAreasWithBoundsJob() {
                    m_Bounds = updatedBounds.AsDeferredJobArray(),
                    m_AreaTree = m_AreaSearchSystem.GetSearchTree(readOnly: true, out var dependencies3),
                    m_WoodResourceData = SystemAPI.GetBufferLookup<WoodResource>(isReadOnly: true),
                    m_Nodes = SystemAPI.GetBufferLookup<Game.Areas.Node>(isReadOnly: true),
                    m_Triangles = SystemAPI.GetBufferLookup<Game.Areas.Triangle>(isReadOnly: true),
                    m_UpdateBuffer = updateBuffer2,
                };
                JobHandle jobHandle2 = jobData2.Schedule(updatedBounds, 1, JobHandle.CombineDependencies(Dependency, dependencies2, dependencies3));
                m_ObjectUpdateCollectSystem.AddBoundsReader(jobHandle2);
                m_AreaSearchSystem.AddSearchTreeReader(jobHandle2);
                Dependency = jobHandle2;
            }

            // Multiple parts copied from vanilla with edits.
            JobHandle outJobHandle2;
            NativeList<ArchetypeChunk> updatedAreaChunks = m_WoodResourceAreaQuery.ToArchetypeChunkListAsync(Allocator.TempJob, out outJobHandle2);
            CollectUpdatedAreasJob collectUpdatedAreasJob = new CollectUpdatedAreasJob()
            {
                m_UpdatedAreaChunks = updatedAreaChunks,
                m_EntityType = SystemAPI.GetEntityTypeHandle(),
                m_UpdateBuffer = updateBuffer,
                m_UpdateList = nativeList,
            };
            UpdateAreaResourcesJob updateAreaResourcesJob = new UpdateAreaResourcesJob()
            {
                m_UpdateList = nativeList.AsDeferredJobArray(),
                m_ObjectTree = m_ObjectSearchSystem.GetStaticSearchTree(readOnly: true, out var dependencies4),

                m_TreeData = SystemAPI.GetComponentLookup<Game.Objects.Tree>(isReadOnly: true),
                m_DecorationData = SystemAPI.GetComponentLookup<Game.Objects.Decoration>(isReadOnly: true),
                m_TransformData = SystemAPI.GetComponentLookup<Game.Objects.Transform>(isReadOnly: true),

                m_PrefabRefData = SystemAPI.GetComponentLookup<Game.Prefabs.PrefabRef>(isReadOnly: true),
                m_ExtractorAreaData = SystemAPI.GetComponentLookup<Game.Prefabs.ExtractorAreaData>(isReadOnly: true),
                m_PrefabTreeData = SystemAPI.GetComponentLookup<Game.Prefabs.TreeData>(isReadOnly: true),
                m_Nodes = SystemAPI.GetBufferLookup<Game.Areas.Node>(isReadOnly: true),
                m_Triangles = SystemAPI.GetBufferLookup<Game.Areas.Triangle>(isReadOnly: true),
                m_ExtractorData = SystemAPI.GetComponentLookup<Game.Areas.Extractor>(isReadOnly: true),
                m_WoodResources = SystemAPI.GetBufferLookup<Game.Areas.WoodResource>(isReadOnly: true),
                buffer = m_Barrier.CreateCommandBuffer(),
            };
            JobHandle jobHandle3 = IJobExtensions.Schedule(collectUpdatedAreasJob, JobHandle.CombineDependencies(Dependency, outJobHandle2));
            JobHandle jobHandle4 = updateAreaResourcesJob.Schedule(nativeList, 1, JobHandle.CombineDependencies(jobHandle3, dependencies4));
            m_Barrier.AddJobHandleForProducer(jobHandle4);
            updateBuffer.Dispose(jobHandle3);
            nativeList.Dispose(jobHandle4);
            updatedAreaChunks.Dispose(jobHandle3);
            m_ObjectSearchSystem.AddStaticSearchTreeReader(jobHandle4);
            m_NaturalResourceSystem.AddReader(jobHandle4);
            Enabled = false;
        }

        // Copied From Vanilla with some edits.
        [BurstCompile]
        private struct FindUpdatedAreasWithBoundsJob : IJobParallelForDefer
        {
            private struct AreaIterator : INativeQuadTreeIterator<AreaSearchItem, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<AreaSearchItem, QuadTreeBoundsXZ>
            {
                public Bounds2 m_Bounds;

                public BufferLookup<WoodResource> m_WoodResourceData;

                public BufferLookup<Node> m_Nodes;

                public BufferLookup<Triangle> m_Triangles;

                public NativeQueue<Entity>.ParallelWriter m_UpdateBuffer;

                public bool Intersect(QuadTreeBoundsXZ bounds)
                {
                    return MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds);
                }

                public void Iterate(QuadTreeBoundsXZ bounds, AreaSearchItem item)
                {
                    if (MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds) && m_WoodResourceData.HasBuffer(item.m_Area))
                    {
                        Triangle2 triangle = AreaUtils.GetTriangle2(m_Nodes[item.m_Area], m_Triangles[item.m_Area][item.m_Triangle]);
                        if (MathUtils.Intersect(m_Bounds, triangle))
                        {
                            m_UpdateBuffer.Enqueue(item.m_Area);
                        }
                    }
                }
            }


            [ReadOnly]
            public NativeArray<Bounds2> m_Bounds;

            [ReadOnly]
            public NativeQuadTree<AreaSearchItem, QuadTreeBoundsXZ> m_AreaTree;

            [ReadOnly]
            public BufferLookup<WoodResource> m_WoodResourceData;

            [ReadOnly]
            public BufferLookup<Node> m_Nodes;

            [ReadOnly]
            public BufferLookup<Triangle> m_Triangles;

            public NativeQueue<Entity>.ParallelWriter m_UpdateBuffer;

            public void Execute(int index)
            {
                AreaIterator areaIterator = new()
                {
                    m_Bounds = m_Bounds[index],
                    m_WoodResourceData = m_WoodResourceData,
                    m_Nodes = m_Nodes,
                    m_Triangles = m_Triangles,
                    m_UpdateBuffer = m_UpdateBuffer,
                };
                m_AreaTree.Iterate(ref areaIterator);
                m_WoodResourceData = areaIterator.m_WoodResourceData;
                m_Nodes = areaIterator.m_Nodes;
                m_Triangles = areaIterator.m_Triangles;
            }
        }

        [BurstCompile]
        private struct CollectUpdatedAreasJob : IJob
        {
            [StructLayout(LayoutKind.Sequential, Size = 1)]
            private struct EntityComparer : IComparer<Entity>
            {
                public int Compare(Entity x, Entity y)
                {
                    return x.Index - y.Index;
                }
            }

            [ReadOnly]
            public NativeList<ArchetypeChunk> m_UpdatedAreaChunks;

            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            public NativeQueue<Entity> m_UpdateBuffer;

            public NativeList<Entity> m_UpdateList;

            public void Execute()
            {
                int count = m_UpdateBuffer.Count;
                int num = 0;
                for (int i = 0; i < m_UpdatedAreaChunks.Length; i++)
                {
                    num += m_UpdatedAreaChunks[i].Count;
                }

                m_UpdateList.ResizeUninitialized(count + num);
                for (int k = 0; k < count; k++)
                {
                    m_UpdateList[k] = m_UpdateBuffer.Dequeue();
                }

                for (int l = 0; l < m_UpdatedAreaChunks.Length; l++)
                {
                    NativeArray<Entity> nativeArray = m_UpdatedAreaChunks[l].GetNativeArray(m_EntityType);
                    for (int m = 0; m < nativeArray.Length; m++)
                    {
                        m_UpdateList[count++] = nativeArray[m];
                    }
                }

                m_UpdateList.Sort(default(EntityComparer));
                Entity entity = Entity.Null;
                int num3 = 0;
                int num4 = 0;
                while (num3 < m_UpdateList.Length)
                {
                    Entity entity2 = m_UpdateList[num3++];
                    if (entity2 != entity)
                    {
                        m_UpdateList[num4++] = entity2;
                        entity = entity2;
                    }
                }

                if (num4 < m_UpdateList.Length)
                {
                    m_UpdateList.RemoveRange(num4, m_UpdateList.Length - num4);
                }
            }
        }

        // Copied from Vanilla with edits.
        [BurstCompile]
        public struct UpdateAreaResourcesJob : IJobParallelForDefer
        {
            [ReadOnly]
            public NativeArray<Entity> m_UpdateList;

            [ReadOnly]
            public NativeQuadTree<Entity, QuadTreeBoundsXZ> m_ObjectTree;

            [ReadOnly]
            public ComponentLookup<Tree> m_TreeData;

            [ReadOnly]
            public ComponentLookup<Decoration> m_DecorationData;

            [ReadOnly]
            public ComponentLookup<Transform> m_TransformData;

            [ReadOnly]
            public ComponentLookup<PrefabRef> m_PrefabRefData;

            [ReadOnly]
            public ComponentLookup<ExtractorAreaData> m_ExtractorAreaData;

            [ReadOnly]
            public ComponentLookup<TreeData> m_PrefabTreeData;

            [ReadOnly]
            public BufferLookup<Node> m_Nodes;

            [ReadOnly]
            public BufferLookup<Triangle> m_Triangles;

            [ReadOnly]
            public ComponentLookup<Extractor> m_ExtractorData;

            [ReadOnly]
            public BufferLookup<WoodResource> m_WoodResources;

            public EntityCommandBuffer buffer;

            public void Execute(int index)
            {
                Entity entity = m_UpdateList[index];
                DynamicBuffer<Node> nodes = m_Nodes[entity];
                DynamicBuffer<Triangle> triangles = m_Triangles[entity];
                if (m_ExtractorData.HasComponent(entity))
                {
                    PrefabRef prefabRef = m_PrefabRefData[entity];
                    ExtractorAreaData extractorAreaData = m_ExtractorAreaData[prefabRef.m_Prefab];
                    if (extractorAreaData.m_MapFeature == MapFeature.Forest &&
                        m_WoodResources.HasBuffer(entity))
                    {
                        ReviewTrees(nodes, triangles);
                    }
                }
            }

            private void ReviewTrees(DynamicBuffer<Node> nodes, DynamicBuffer<Triangle> triangles)
            {
                TreeIterator treeIterator = new TreeIterator()
                {
                    m_TransformData = m_TransformData,
                    m_PrefabRefData = m_PrefabRefData,
                    m_PrefabTreeData = m_PrefabTreeData,
                    m_DecorationData = m_DecorationData,
                    buffer = buffer,
                };
                for (int i = 0; i < triangles.Length; i++)
                {
                    treeIterator.m_Triangle = AreaUtils.GetTriangle2(nodes, triangles[i]);
                    treeIterator.m_Bounds = MathUtils.Bounds(treeIterator.m_Triangle);
                    m_ObjectTree.Iterate(ref treeIterator);
                }
            }
        }

        // Copied from vanilla
        private struct TreeIterator : INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>
        {
            public Bounds2 m_Bounds;

            public Triangle2 m_Triangle;

            public ComponentLookup<Transform> m_TransformData;

            public ComponentLookup<PrefabRef> m_PrefabRefData;

            public ComponentLookup<TreeData> m_PrefabTreeData;

            public ComponentLookup<Decoration> m_DecorationData;

            public EntityCommandBuffer buffer;

            public bool Intersect(QuadTreeBoundsXZ bounds)
            {
                if ((bounds.m_Mask & (BoundsMask.IsTree | BoundsMask.NotOverridden)) != (BoundsMask.IsTree | BoundsMask.NotOverridden))
                {
                    return false;
                }
                if (!MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds))
                {
                    return false;
                }
                return MathUtils.Intersect(bounds.m_Bounds.xz, m_Triangle);
            }

            public void Iterate(QuadTreeBoundsXZ bounds, Entity entity)
            {
                if ((bounds.m_Mask & (BoundsMask.IsTree | BoundsMask.NotOverridden)) != (BoundsMask.IsTree | BoundsMask.NotOverridden) || !MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds) || !MathUtils.Intersect(bounds.m_Bounds.xz, m_Triangle))
                {
                    return;
                }

                Transform transform = m_TransformData[entity];
                if (MathUtils.Intersect(m_Triangle, transform.m_Position.xz) &&
                    entity != Entity.Null &&
                    m_DecorationData.HasComponent(entity))
                {
                    // Disable Decoration Component
                    buffer.SetComponentEnabled<Decoration>(entity, false);
                    buffer.AddComponent<Lumber>(entity);
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
                    if (entityNativeArray[i] == Entity.Null)
                    {
                        continue;
                    }

                    buffer.RemoveComponent<Lumber>(unfilteredChunkIndex, entityNativeArray[i]);
                }
            }
        }

    }
}
