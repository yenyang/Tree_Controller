// <copyright file="FreeVegetationSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using static Colossal.Animations.Animation;

    /// <summary>
    /// Modifies the prices of vegetation prefabs, and handles Vegetation prefab component.
    /// </summary>
    public partial class ModifyVegetationPrefabsSystem : GameSystemBase
    {
        private EntityQuery m_FreeVegetationQuery;
        private EntityQuery m_TreeObjectGeometryQuery;
        private ILog m_Log;
        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_PlantDataWithOutVegetationQuery;
        private ToolOutputBarrier m_Barrier;

        /// <summary>
        /// Sets the construction cost of vegetation prefabs to 0.
        /// </summary>
        public void SetVegetationCostsToZero()
        {
            NativeArray<Entity> prefabEntities = m_FreeVegetationQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out PlaceableObjectData placeableObjectData))
                {
                    placeableObjectData.m_ConstructionCost = 0;
                    EntityManager.SetComponentData(entity, placeableObjectData);
                }
            }

            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.{nameof(SetVegetationCostsToZero)} Complete.");
        }

        /// <summary>
        /// Sets the construction cost of vegetation prefabs to 0.
        /// </summary>
        public void ResetVegetationCosts()
        {
            NativeArray<Entity> prefabEntities = m_FreeVegetationQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out PlaceableObjectData placeableObjectData)
                   && m_PrefabSystem.TryGetPrefab(entity, out PrefabBase prefab)
                   && prefab.TryGet(out PlaceableObject placeableObject))
                {
                    placeableObjectData.m_ConstructionCost = placeableObject.m_ConstructionCost;
                    EntityManager.SetComponentData(entity, placeableObjectData);
                }
            }

            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.{nameof(ResetVegetationCosts)} Complete.");
        }

        /// <summary>
        /// Sets the object geometry size of tree prefabs to trunk size.
        /// </summary>
        public void DecreaseObjectGeometrySize()
        {
            NativeArray<Entity> prefabEntities = m_TreeObjectGeometryQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out ObjectGeometryData objectGeometryData)
                    && EntityManager.TryGetComponent(entity, out Vegetation vegetationData))
                {
                    if (vegetationData.m_Size.x == 0 && vegetationData.m_Size.z == 0)
                    {
                        vegetationData.m_Size = objectGeometryData.m_Size;
                        EntityManager.SetComponentData(entity, vegetationData);
                    }

                    if (EntityManager.TryGetBuffer(entity, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                        && subMeshBuffer.Length > 5)
                    {
                        objectGeometryData.m_Size.x = objectGeometryData.m_LegSize.x;
                        objectGeometryData.m_Size.z = objectGeometryData.m_LegSize.z;
                        EntityManager.SetComponentData(entity, objectGeometryData);
                    }
                }
            }

            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.{nameof(DecreaseObjectGeometrySize)} Complete.");
        }


        /// <summary>
        /// Resets the object geometry size of tree prefabs back to dripline.
        /// </summary>
        public void ResetObjectGeometrySize()
        {
            NativeArray<Entity> prefabEntities = m_TreeObjectGeometryQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out ObjectGeometryData objectGeometryData)
                    && EntityManager.TryGetComponent(entity, out Vegetation vegetationData)
                    && EntityManager.TryGetBuffer(entity, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                    && subMeshBuffer.Length > 5)
                {
                    objectGeometryData.m_Size.x = vegetationData.m_Size.x;
                    objectGeometryData.m_Size.z = vegetationData.m_Size.z;
                    EntityManager.SetComponentData(entity, objectGeometryData);
                }
            }

            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.{nameof(ResetObjectGeometrySize)} Complete.");
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.OnCreate");

            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_Barrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();

            m_FreeVegetationQuery = SystemAPI.QueryBuilder()
               .WithAllRW<PlaceableObjectData>()
               .WithAll<Vegetation>()
               .WithNone<Deleted, Overridden>()
               .Build();

            m_TreeObjectGeometryQuery = SystemAPI.QueryBuilder()
               .WithAllRW<ObjectGeometryData>()
               .WithAll<TreeData, Vegetation>()
               .WithNone<Deleted, Overridden>()
               .Build();

            m_PlantDataWithOutVegetationQuery = SystemAPI.QueryBuilder()
                .WithAllRW<ObjectGeometryData>()
                .WithAll<PlantData>()
                .WithNone<Deleted, Vegetation>()
                .Build();

            RequireForUpdate(m_PlantDataWithOutVegetationQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            NativeArray<Entity> prefabEntities = m_PlantDataWithOutVegetationQuery.ToEntityArray(Allocator.Temp);
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer();

            foreach (Entity prefabEntity in prefabEntities)
            {
                buffer.AddComponent<Vegetation>(prefabEntity);
                if (EntityManager.TryGetComponent(prefabEntity, out ObjectGeometryData objectGeometryData))
                {
                    m_Log.Debug($"{nameof(FindTreesAndBushesSystem)}.{nameof(OnGameLoadingComplete)} objectGeometryData.m_size = {objectGeometryData.m_Size.x}:{objectGeometryData.m_Size.z}");
                    Vegetation vegetation = new Vegetation(new Unity.Mathematics.float3(objectGeometryData.m_Size.x, 0, objectGeometryData.m_Size.z));
                    buffer.SetComponent(prefabEntity, vegetation);

                    if (TreeControllerMod.Instance.Settings.LimitedTreeAnarchy
                        && EntityManager.HasComponent<TreeData>(prefabEntity)
                        && EntityManager.TryGetBuffer(prefabEntity, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                        && subMeshBuffer.Length > 5)
                    {
                        objectGeometryData.m_Size.x = objectGeometryData.m_LegSize.x;
                        objectGeometryData.m_Size.z = objectGeometryData.m_LegSize.z;
                        buffer.SetComponent(prefabEntity, objectGeometryData);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (TreeControllerMod.Instance.Settings.FreeVegetation)
            {
                SetVegetationCostsToZero();
            }
        }
    }
}
