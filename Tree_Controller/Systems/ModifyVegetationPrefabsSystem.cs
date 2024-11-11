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
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Modifies the prices of vegetation prefabs.
    /// </summary>
    public partial class ModifyVegetationPrefabsSystem : GameSystemBase
    {
        private EntityQuery m_FreeVegetationQuery;
        private EntityQuery m_TreeObjectGeometryQuery;
        private ILog m_Log;
        private PrefabSystem m_PrefabSystem;

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
        /// Sets the object geometry bounds of tree prefabs to something smaller.
        /// </summary>
        public void DecreaseObjectGeometryBounds()
        {
            NativeArray<Entity> prefabEntities = m_TreeObjectGeometryQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out ObjectGeometryData objectGeometryData))
                {
                    objectGeometryData.m_Size.x = objectGeometryData.m_LegSize.x;
                    objectGeometryData.m_Size.z = objectGeometryData.m_LegSize.z;
                    EntityManager.SetComponentData(entity, objectGeometryData);
                }
            }

            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.{nameof(DecreaseObjectGeometryBounds)} Complete.");
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.OnCreate");

            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            m_FreeVegetationQuery = SystemAPI.QueryBuilder()
               .WithAllRW<PlaceableObjectData>()
               .WithAll<Vegetation>()
               .WithNone<Deleted, Overridden>()
               .Build();

            m_TreeObjectGeometryQuery = SystemAPI.QueryBuilder()
               .WithAllRW<ObjectGeometryData>()
               .WithAll<TreeData>()
               .WithNone<Deleted, Overridden>()
               .Build();

            Enabled = false;
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            return;
        }
    }
}
