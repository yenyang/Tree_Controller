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
        private ToolOutputBarrier m_Barrier; // This system is set to run on SystemUpdatePhase.ToolUpdate so this is the appropriate Barrier to use for the OnUpdate. The public methods can be called anytime so they cannot reliable use this barrier without causing an error.

        /// <summary>
        /// Sets the construction cost of vegetation prefabs to 0.
        /// </summary>
        public void SetVegetationCostsToZero()
        {
            NativeArray<Entity> prefabEntities = m_FreeVegetationQuery.ToEntityArray(Allocator.Temp); // Important to use Allocator.Temp. You do not need to dispose of a Temp allocator. Forgetting to dispose a TempJob allocator will produce a memory leak.
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out PlaceableObjectData placeableObjectData))
                {
                    placeableObjectData.m_ConstructionCost = 0; // PlaceableObjectData is a copy of the component from the Prefab Entity. This sets the construction cost to 0.
                    EntityManager.SetComponentData(entity, placeableObjectData); // This sets the component on the Entity. EntityManager.SetComponentData causes an immediate sync point which is not ideal, so this may become an Entity Command Buffer eventually as that will produce less sync points and have better performance.
                }
            }

            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.{nameof(SetVegetationCostsToZero)} Complete.");
        }

        /// <summary>
        /// Sets the construction cost of vegetation prefabs to 0.
        /// </summary>
        public void ResetVegetationCosts()
        {
            NativeArray<Entity> prefabEntities = m_FreeVegetationQuery.ToEntityArray(Allocator.Temp); // Important to use Allocator.Temp. You do not need to dispose of a Temp allocator. Forgetting to dispose a TempJob allocator will produce a memory leak.
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out PlaceableObjectData placeableObjectData)
                   && m_PrefabSystem.TryGetPrefab(entity, out PrefabBase prefab)
                   && prefab.TryGet(out PlaceableObject placeableObject)) // The PlaceableObject prefab component on the PrefabBase is the "Source of Truth" and should contain the original values for that prefab.
                {
                    placeableObjectData.m_ConstructionCost = placeableObject.m_ConstructionCost; // Set the copy of the prefab entity component's construction cost to the value found on the prefab base component.
                    EntityManager.SetComponentData(entity, placeableObjectData); // This sets the component on the Entity. EntityManager.SetComponentData causes an immediate sync point which is not ideal, so this may become an Entity Command Buffer eventually as that will produce less sync points and have better performance.
                }
            }

            m_Log.Info($"{nameof(ModifyVegetationPrefabsSystem)}.{nameof(ResetVegetationCosts)} Complete.");
        }

        /// <summary>
        /// Sets the object geometry size of tree prefabs to trunk size.
        /// </summary>
        public void DecreaseObjectGeometrySize()
        {
            NativeArray<Entity> prefabEntities = m_TreeObjectGeometryQuery.ToEntityArray(Allocator.Temp); // Important to use Allocator.Temp. You do not need to dispose of a Temp allocator. Forgetting to dispose a TempJob allocator will produce a memory leak.
            foreach (Entity entity in prefabEntities)
            {
                if (EntityManager.TryGetComponent(entity, out ObjectGeometryData objectGeometryData)
                    && EntityManager.TryGetComponent(entity, out Vegetation vegetationData)) // Vegetation is a custom component that Tree Controller assigns and uses. It probably should have been VegetationData to conform to vanilla naming standards for Prefab Entity components.
                {
                    if (vegetationData.m_Size.x == 0 && vegetationData.m_Size.z == 0)
                    {
                        vegetationData.m_Size = objectGeometryData.m_Size; // If vegetation size has not been assigned then this sets the copy of the prefab entity component's size to that of objectGeometryData size. This is done before the objectGeometryData size is changed by the mod to record the value before the mod changes it.
                        EntityManager.SetComponentData(entity, vegetationData); // This sets the component on the Entity. EntityManager.SetComponentData causes an immediate sync point which is not ideal, so this may become an Entity Command Buffer eventually as that will produce less sync points and have better performance.
                    }

                    if (EntityManager.TryGetBuffer(entity, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                        && subMeshBuffer.Length > 5)c
                    {
                        objectGeometryData.m_Size.x = objectGeometryData.m_LegSize.x; // Copy the leg size to the general object geometry size so that the conflict check zone is just the tree leg size instead of the full diameter of elderly tree. Remember this is just a copy of the object geometry component from the prefab entity. 
                        objectGeometryData.m_Size.z = objectGeometryData.m_LegSize.z; // Copy the leg size to the general object geometry size so that the conflict check zone is just the tree leg size instead of the full diameter of elderly tree. Remember this is just a copy of the object geometry component from the prefab entity. 
                        EntityManager.SetComponentData(entity, objectGeometryData); // This sets the component on the Entity. EntityManager.SetComponentData causes an immediate sync point which is not ideal, so this may become an Entity Command Buffer eventually as that will produce less sync points and have better performance.
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

            // Get system references.
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
            NativeArray<Entity> prefabEntities = m_PlantDataWithOutVegetationQuery.ToEntityArray(Allocator.Temp); // Important to use Allocator.Temp. You do not need to dispose of a Temp allocator. Forgetting to dispose a TempJob allocator will produce a memory leak.
            EntityCommandBuffer buffer = m_Barrier.CreateCommandBuffer(); // Create the command buffer that we will schedule structural changes too.

            foreach (Entity prefabEntity in prefabEntities)
            {
                buffer.AddComponent<Vegetation>(prefabEntity);  // Queue up the structural change of adding a component to the prefab entity. To be played back automatically with ToolOutputBarrier. When using a barrier you should not manually playback the ECB, nor  should you dispose of the ECB. All handled by the barrier.
                if (EntityManager.TryGetComponent(prefabEntity, out ObjectGeometryData objectGeometryData))
                {
                    m_Log.Debug($"{nameof(FindTreesAndBushesSystem)}.{nameof(OnGameLoadingComplete)} objectGeometryData.m_size = {objectGeometryData.m_Size.x}:{objectGeometryData.m_Size.z}");
                    Vegetation vegetation = new Vegetation(new Unity.Mathematics.float3(objectGeometryData.m_Size.x, 0, objectGeometryData.m_Size.z));
                    buffer.SetComponent(prefabEntity, vegetation); // Queue up the structural change of setting a component on the prefab entity. To be played back automatically with ToolOutputBarrier. When using a barrier you should not manually playback the ECB, nor  should you dispose of the ECB. All handled by the barrier.

                    if (TreeControllerMod.Instance.Settings.LimitedTreeAnarchy
                        && EntityManager.HasComponent<TreeData>(prefabEntity)
                        && EntityManager.TryGetBuffer(prefabEntity, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer)
                        && subMeshBuffer.Length > 5)  // Checking for a submesh buffer length of > 5 verifies that we are looking at a Tree and not a Wild bush or plant. Trees have submeshes for: child, teen, adult, elderly, dead, and stump.
                    {
                        objectGeometryData.m_Size.x = objectGeometryData.m_LegSize.x; // Copy the leg size to the general object geometry size so that the conflict check zone is just the tree leg size instead of the full diameter of elderly tree. Remember this is just a copy of the object geometry component from the prefab entity. 
                        objectGeometryData.m_Size.z = objectGeometryData.m_LegSize.z; // Copy the leg size to the general object geometry size so that the conflict check zone is just the tree leg size instead of the full diameter of elderly tree. Remember this is just a copy of the object geometry component from the prefab entity. 
                        buffer.SetComponent(prefabEntity, objectGeometryData); // Queue up the structural change of setting a component on the prefab entity. To be played back automatically with ToolOutputBarrier. When using a barrier you should not manually playback the ECB, nor  should you dispose of the ECB. All handled by the barrier.
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (TreeControllerMod.Instance.Settings.FreeVegetation) // When the game is loaded, if the user has the settings run the method to set costs to 0. No significant harm if the costs are already 0.
            {
                SetVegetationCostsToZero();
            }
        }
    }
}
