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
    public partial class FreeVegetationSystem : GameSystemBase
    {
        private EntityQuery m_VegetationPrefabQuery;
        private ILog m_Log;
        private PrefabSystem m_PrefabSystem;

        /// <summary>
        /// Sets the construction cost of vegetation prefabs to 0.
        /// </summary>
        public void SetVegetationCostsToZero()
        {
            NativeArray<Entity> prefabEntities = m_VegetationPrefabQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out PlaceableObjectData placeableObjectData))
                {
                    placeableObjectData.m_ConstructionCost = 0;
                    EntityManager.SetComponentData(entity, placeableObjectData);
                }
            }

            m_Log.Info($"{nameof(FreeVegetationSystem)}.{nameof(SetVegetationCostsToZero)} Complete.");
        }

        /// <summary>
        /// Sets the construction cost of vegetation prefabs to 0.
        /// </summary>
        public void ResetVegetationCosts()
        {
            NativeArray<Entity> prefabEntities = m_VegetationPrefabQuery.ToEntityArray(Allocator.Temp);
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

            m_Log.Info($"{nameof(FreeVegetationSystem)}.{nameof(ResetVegetationCosts)} Complete.");
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_Log.Info($"{nameof(FreeVegetationSystem)}.OnCreate");


            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            m_VegetationPrefabQuery = SystemAPI.QueryBuilder()
               .WithAllRW<PlaceableObjectData>()
               .WithAll<Vegetation>()
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
