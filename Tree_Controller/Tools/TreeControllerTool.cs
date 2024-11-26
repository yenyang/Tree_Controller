// <copyright file="TreeControllerTool.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
#define BURST
namespace Tree_Controller.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Colossal.Annotations;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Input;
    using Game.Net;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using Tree_Controller;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.InputSystem;


    /// <summary>
    /// Tool for controlling tree state or prefab.
    /// </summary>
    public partial class TreeControllerTool : ToolBaseSystem
    {
        private ProxyAction m_ApplyAction;
        private ProxyAction m_SecondaryApplyAction;
        private OverlayRenderSystem m_OverlayRenderSystem;
        private ToolOutputBarrier m_ToolOutputBarrier;
        private EntityQuery m_VegetationQuery;
        private ObjectToolSystem m_ObjectToolSystem;
        private NativeList<Entity> m_SelectedTreePrefabEntities;
        [CanBeNull]
        private PrefabBase m_OriginallySelectedPrefab;
        private ILog m_Log;
        private TreeControllerUISystem m_TreeControllerUISystem;

        /// <inheritdoc/>
        public override string toolID => "Tree Controller Tool";

        /// <summary>
        /// Gets or sets the OriginallySelectedPrefab.
        /// </summary>
        public PrefabBase OriginallySelectedPrefab { get => m_OriginallySelectedPrefab; set => m_OriginallySelectedPrefab = value; }

        /// <summary>
        /// Adds the selected Prefab to the list by finding prefab entity.
        /// </summary>
        /// <param name="prefab">PrefabBase from object tool.</param>
        public void SelectTreePrefab(PrefabBase prefab)
        {
            Entity prefabEntity = m_PrefabSystem.GetEntity(prefab);

            if (EntityManager.HasComponent<Vegetation>(prefabEntity) && !m_SelectedTreePrefabEntities.Contains(prefabEntity))
            {
                m_SelectedTreePrefabEntities.Add(prefabEntity);
                if (m_OriginallySelectedPrefab == null)
                {
                    m_OriginallySelectedPrefab = prefab;
                }

                m_TreeControllerUISystem.UpdateSelectionSet = true;
                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(SelectTreePrefab)} selected {prefab.name} prefabEntity = {prefabEntity.Index}.{prefabEntity.Version}");
                if (m_ToolSystem.activeTool == this || m_ToolSystem.activeTool.toolID == "Line Tool")
                {
                    m_ToolSystem.EventPrefabChanged?.Invoke(prefab);
                }

                if (!m_TreeControllerUISystem.RecentlySelectedPrefabSet)
                {
                    m_TreeControllerUISystem.ResetPrefabSets();
                }
            }
        }

        /// <summary>
        /// Removes the selected Prefab from the list by prefab entity.
        /// </summary>
        /// <param name="prefab">PrefabBase from object tool.</param>
        public void UnselectTreePrefab(PrefabBase prefab)
        {
            Entity prefabEntity = m_PrefabSystem.GetEntity(prefab);
            if (m_SelectedTreePrefabEntities.Contains(prefabEntity))
            {
                m_SelectedTreePrefabEntities.RemoveAt(m_SelectedTreePrefabEntities.IndexOf(prefabEntity));
                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(UnselectTreePrefab)} removed {prefab.name} prefabEntity = {prefabEntity.Index}.{prefabEntity.Version}");
                if (m_SelectedTreePrefabEntities.Length == 0)
                {
                    m_OriginallySelectedPrefab = null;
                }

                m_TreeControllerUISystem.UpdateSelectionSet = true;
                if (m_ToolSystem.activeTool == this || m_ToolSystem.activeTool.toolID == "Line Tool")
                {
                    m_ToolSystem.EventPrefabChanged?.Invoke(prefab);
                }

                if (!m_TreeControllerUISystem.RecentlySelectedPrefabSet)
                {
                    m_TreeControllerUISystem.ResetPrefabSets();
                }
            }
        }

        /// <inheritdoc/>
        public override void GetAvailableSnapMask(out Snap onMask, out Snap offMask)
        {
            base.GetAvailableSnapMask(out onMask, out offMask);
            onMask |= Snap.ContourLines;
            offMask |= Snap.ContourLines;
        }


        /// <summary>
        /// Resets the selected Tree Prefabs.
        /// </summary>
        public void ClearSelectedTreePrefabs()
        {
            m_SelectedTreePrefabEntities.Clear();
            m_OriginallySelectedPrefab = null;
        }

        /// <summary>
        /// Gets a list of the PrefabBases of the prefabs that have been selected.
        /// </summary>
        /// <returns>List of PrefabBases.</returns>
        public List<PrefabBase> GetSelectedPrefabs()
        {
            List<PrefabBase> names = new List<PrefabBase>();
            foreach (Entity entity in m_SelectedTreePrefabEntities)
            {
                if (m_PrefabSystem.TryGetPrefab(entity, out PrefabBase prefab))
                {
                    names.Add(prefab);
                }
            }

            return names;
        }

        /// <inheritdoc/>
        public override PrefabBase GetPrefab()
        {
            if (m_SelectedTreePrefabEntities.Length > 0 && m_OriginallySelectedPrefab != null)
            {
                return m_OriginallySelectedPrefab;
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool TrySetPrefab(PrefabBase prefab)
        {
            Entity prefabEntity = m_PrefabSystem.GetEntity(prefab);
            if (m_ToolSystem.activePrefab != null)
            {
                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(TrySetPrefab)} Toolsystem:{m_ToolSystem.activePrefab.name}");
            }

            if (prefab != null)
            {
                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(TrySetPrefab)} PrefabChange:{prefab.name} ");
            }

            if (m_ObjectToolSystem.GetPrefab() != null)
            {
                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(TrySetPrefab)} ObjectToolSystem:{m_ObjectToolSystem.GetPrefab().name}");
            }

            if (EntityManager.HasComponent<Vegetation>(prefabEntity) && !EntityManager.HasComponent<PlaceholderObjectElement>(prefabEntity))
            {
                bool ctrlKeyPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
                if ((!ctrlKeyPressed && !m_TreeControllerUISystem.RecentlySelectedPrefabSet) || (m_ToolSystem.activeTool == m_ObjectToolSystem && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush))
                {
                    ClearSelectedTreePrefabs();
                    SelectTreePrefab(prefab);
                }
                else if (m_SelectedTreePrefabEntities.Contains(prefabEntity)
                    && m_SelectedTreePrefabEntities.Length > 1
                    && !m_TreeControllerUISystem.UpdateSelectionSet
                    && !m_TreeControllerUISystem.RecentlySelectedPrefabSet)
                {
                    UnselectTreePrefab(prefab);
                    List<PrefabBase> list = GetSelectedPrefabs();
                    if (!list.Contains(prefab))
                    {
                        foreach (Entity e in m_SelectedTreePrefabEntities)
                        {
                            if (m_PrefabSystem.TryGetPrefab(e, out PrefabBase nextPrefab))
                            {
                                m_OriginallySelectedPrefab = nextPrefab;
                                m_TreeControllerUISystem.TrySetPrefabNextFrame = nextPrefab;
                                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(TrySetPrefab)} trying to set prefab to {nextPrefab.name}");
                                break;
                            }
                        }
                    }
                }
                else if (!m_TreeControllerUISystem.UpdateSelectionSet && !m_TreeControllerUISystem.RecentlySelectedPrefabSet)
                {
                    SelectTreePrefab(prefab);
                }
                else if (!m_TreeControllerUISystem.RecentlySelectedPrefabSet && prefab != m_OriginallySelectedPrefab)
                {
                    m_OriginallySelectedPrefab = prefab;
                    m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(TrySetPrefab)} setting originallySelectedPrefab to {prefab.name}.");
                }

                if (m_ToolSystem.activeTool == this)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            if (m_TreeControllerUISystem.SelectionMode == Selection.Single)
            {
                m_ToolRaycastSystem.typeMask = TypeMask.StaticObjects | TypeMask.Net;
                m_ToolRaycastSystem.netLayerMask = Layer.Road | Layer.PublicTransportRoad;
            }
            else if (m_TreeControllerUISystem.SelectionMode == Selection.BuildingOrNet)
            {
                m_ToolRaycastSystem.typeMask = TypeMask.StaticObjects | TypeMask.Net | TypeMask.Terrain;
                m_ToolRaycastSystem.netLayerMask = Layer.Road | Layer.PublicTransportRoad;
            }
            else if (m_TreeControllerUISystem.SelectionMode == Selection.Radius)
            {
                m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
            }
            else if (m_TreeControllerUISystem.SelectionMode == Selection.Map)
            {
                m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
            }
        }

        /// <summary>
        /// For stopping the tool. Probably with esc key.
        /// </summary>
        public void RequestDisable()
        {
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }

        /// <summary>
        /// Gets a prefab entity from the selected tree prefabs given a random parameter.
        /// </summary>
        /// <param name="random">A source of randomness.</param>
        /// <returns>A random prefab entity from selected or Enity.null.</returns>
        public Entity GetNextPrefabEntity(ref Unity.Mathematics.Random random)
        {
            if (m_SelectedTreePrefabEntities.Length > 0)
            {
                int iterations = random.NextInt(10);
                for (int i = 0; i < iterations; i++)
                {
                    random.NextInt();
                }

                Entity result = m_SelectedTreePrefabEntities[random.NextInt(m_SelectedTreePrefabEntities.Length)];

                return result;
            }

            return Entity.Null;
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(OnGameLoadingComplete)} Old Tool Order:");
            foreach (ToolBaseSystem toolBaseSystem in m_ToolSystem.tools)
            {
                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(OnGameLoadingComplete)} {toolBaseSystem.toolID}");
            }

            m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(OnGameLoadingComplete)} New Order:");
            m_ToolSystem.tools.Remove(this);
            m_ToolSystem.tools.Insert(0, this);

            foreach (ToolBaseSystem toolBaseSystem in m_ToolSystem.tools)
            {
                m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(OnGameLoadingComplete)} {toolBaseSystem.toolID}");
            }
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            Enabled = false;
            m_Log = TreeControllerMod.Instance.Logger;
            m_Log.Info($"[{nameof(TreeControllerTool)}] {nameof(OnCreate)}");
            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_TreeControllerUISystem = World.GetOrCreateSystemManaged<TreeControllerUISystem>();
            m_SelectedTreePrefabEntities = new NativeList<Entity>(0, Allocator.Persistent);
            base.OnCreate();

            m_VegetationQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Game.Objects.Transform>(),
                    },
                    Any = new ComponentType[]
                    {
                        ComponentType.ReadWrite<Tree>(),
                        ComponentType.ReadOnly<Plant>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Overridden>(),
                        ComponentType.ReadOnly<RecentlyChanged>(),
                    },
                },
            });

            RequireForUpdate(m_VegetationQuery);

            m_ApplyAction = TreeControllerMod.Instance.Settings.GetAction(TreeControllerMod.ApplyMimicAction);

            m_SecondaryApplyAction = TreeControllerMod.Instance.Settings.GetAction(TreeControllerMod.SecondaryApplyMimicAction);
        }

        /// <inheritdoc/>
        protected override void OnStartRunning()
        {
            m_ApplyAction.shouldBeEnabled = true;
            m_SecondaryApplyAction.shouldBeEnabled = true;
            m_Log.Debug($"{nameof(TreeControllerTool)}.{nameof(OnStartRunning)}");
        }

        /// <inheritdoc/>
        protected override void OnStopRunning()
        {
            m_ApplyAction.shouldBeEnabled = false;
            m_SecondaryApplyAction.shouldBeEnabled = false;
        }

        /// <inheritdoc/>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = Dependency;
            NativeList<TreeState> selectedTreeStates = m_TreeControllerUISystem.GetSelectedAges();
            bool raycastFlag = GetRaycastResult(out Entity e, out RaycastHit hit);
            bool isVegetationPrefabFlag = false;
            if (EntityManager.TryGetComponent(e, out PrefabRef prefabEntity))
            {
                if (EntityManager.HasComponent<Vegetation>(prefabEntity))
                {
                    isVegetationPrefabFlag = true;
                }
            }

            bool hasTreeComponentFlag = EntityManager.HasComponent<Game.Objects.Tree>(e);
            bool hasTransformComponentFlag = EntityManager.HasComponent<Game.Objects.Transform>(e);
            bool hasBufferFlag = EntityManager.HasBuffer<Game.Objects.SubObject>(e);

            if (m_TreeControllerUISystem.Radius > 0 && raycastFlag && m_TreeControllerUISystem.SelectionMode == Selection.Radius) // Radius Circle
            {
                ToolRadiusJob toolRadiusJob = new ()
                {
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_Position = new Vector3(hit.m_HitPosition.x, hit.m_Position.y, hit.m_HitPosition.z),
                    m_Radius = m_TreeControllerUISystem.Radius,
                };
                inputDeps = IJobExtensions.Schedule(toolRadiusJob, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }

            if (m_ApplyAction.WasPressedThisFrame())
            {
                if (m_TreeControllerUISystem.SelectionMode == Selection.Single || m_TreeControllerUISystem.SelectionMode == Selection.BuildingOrNet)
                {
                    if (raycastFlag && isVegetationPrefabFlag)
                    {
                        if (m_TreeControllerUISystem.AtLeastOneAgeSelected && hasTreeComponentFlag)
                        {

                            ChangeTreeStateJob changeTreeStateJob = new ()
                            {
                                m_Entity = e,
                                m_Random = new ((uint)UnityEngine.Random.Range(1, 100000)),
                                m_Ages = selectedTreeStates,
                                m_Tree = EntityManager.GetComponentData<Tree>(e),
                                buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                            };
                            inputDeps = changeTreeStateJob.Schedule(inputDeps);
                            m_ToolOutputBarrier.AddJobHandleForProducer(inputDeps);
                        }

                        bool doNotApplyTreePrefab = false;
                        if (EntityManager.TryGetComponent(e, out PrefabRef prefabRef))
                        {
                            if (m_PrefabSystem.TryGetPrefab(prefabRef, out PrefabBase prefabBase))
                            {
                                if (prefabBase.GetType() == typeof(RoadPrefab))
                                {
                                    doNotApplyTreePrefab = true;
                                }
                            }
                        }

                        if (!m_SelectedTreePrefabEntities.IsEmpty && !doNotApplyTreePrefab && isVegetationPrefabFlag)
                        {
                            ChangePrefabRefJob changePrefabRefJob = new ()
                            {
                                m_Entity = e,
                                m_SelectedPrefabEntities = m_SelectedTreePrefabEntities,
                                m_Random = new ((uint)UnityEngine.Random.Range(1, 100000)),
                                buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                                m_Ages = selectedTreeStates,
                                m_TreeDataLookup = SystemAPI.GetComponentLookup<TreeData>(isReadOnly: true),
                                m_TreeLookup = SystemAPI.GetComponentLookup<Tree>(isReadOnly: true),
                            };
                            inputDeps = changePrefabRefJob.Schedule(inputDeps);
                            m_ToolOutputBarrier.AddJobHandleForProducer(inputDeps);
                        }
                    }
                    else if (raycastFlag)
                    {
                        ProcessBufferForTrees(e, hit, ref inputDeps); // Loops through buffer in Static Object with Subobjects and changes tree age/prefab for all subobject trees within radius.
                    }
                }
            }
            else if (m_ApplyAction.IsPressed() && m_TreeControllerUISystem.SelectionMode == Selection.Radius && raycastFlag)
            {
                bool overridePrefab = !m_SelectedTreePrefabEntities.IsEmpty;
                if (m_TreeControllerUISystem.AtLeastOneAgeSelected || overridePrefab)
                {
                    TreeChangerWithinRadius changeTreeAgeWithinRadiusJob = new()
                    {
                        m_EntityType = SystemAPI.GetEntityTypeHandle(),
                        m_Position = hit.m_HitPosition,
                        m_Radius = m_TreeControllerUISystem.Radius,
                        m_Ages = selectedTreeStates,
                        m_TransformType = SystemAPI.GetComponentTypeHandle<Game.Objects.Transform>(isReadOnly: true),
                        m_TreeType = SystemAPI.GetComponentTypeHandle<Game.Objects.Tree>(),
                        buffer = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                        m_PrefabRefType = SystemAPI.GetComponentTypeHandle<PrefabRef>(),
                        m_OverrideState = m_TreeControllerUISystem.AtLeastOneAgeSelected,
                        m_OverridePrefab = overridePrefab,
                        m_Random = new((uint)UnityEngine.Random.Range(1, 100000)),
                        m_PrefabEntities = m_SelectedTreePrefabEntities,
                        m_TreeDataLookup = SystemAPI.GetComponentLookup<TreeData>(isReadOnly: true),
                        m_TreeLookup = SystemAPI.GetComponentLookup<Tree>(isReadOnly: true),
                        m_VegetationLookup = SystemAPI.GetComponentLookup<Vegetation>(isReadOnly: true),
                    };
                    inputDeps = JobChunkExtensions.ScheduleParallel(changeTreeAgeWithinRadiusJob, m_VegetationQuery, inputDeps);
                    m_ToolOutputBarrier.AddJobHandleForProducer(inputDeps);
                }
            }
            else if (m_SecondaryApplyAction.WasPressedThisFrame() && m_TreeControllerUISystem.SelectionMode == Selection.Map && raycastFlag)
            {
                bool overridePrefab = !m_SelectedTreePrefabEntities.IsEmpty;
                if (m_TreeControllerUISystem.AtLeastOneAgeSelected || overridePrefab)
                {
                    TreeChangerWholeMap changeTreeAgeWholeMap = new ()
                    {
                        m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle,
                        m_Ages = selectedTreeStates,
                        m_TreeType = SystemAPI.GetComponentTypeHandle<Game.Objects.Tree>(),
                        buffer = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                        m_PrefabRefType = GetComponentTypeHandle<PrefabRef>(),
                        m_OverrideState = m_TreeControllerUISystem.AtLeastOneAgeSelected,
                        m_OverridePrefab = overridePrefab,
                        m_Random = new ((uint)UnityEngine.Random.Range(1, 100000)),
                        m_PrefabEntities = m_SelectedTreePrefabEntities,
                        m_TreeDataLookup = SystemAPI.GetComponentLookup<TreeData>(isReadOnly: true),
                        m_TreeLookup = SystemAPI.GetComponentLookup<Tree>(isReadOnly: true),
                        m_VegetationLookup = SystemAPI.GetComponentLookup<Vegetation>(isReadOnly: true),
                    };
                    inputDeps = JobChunkExtensions.ScheduleParallel(changeTreeAgeWholeMap, m_VegetationQuery, inputDeps);
                    m_ToolOutputBarrier.AddJobHandleForProducer(inputDeps);
                }
            }
            else if (raycastFlag && isVegetationPrefabFlag && hasTransformComponentFlag) // Single Tree Circle
            {
                TreeCircleRenderJob treeCircleRenderJob = new ()
                {
                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                    m_Transform = EntityManager.GetComponentData<Game.Objects.Transform>(e),
                };
                inputDeps = IJobExtensions.Schedule(treeCircleRenderJob, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
            }
            else if (raycastFlag && hasBufferFlag) // Subobject Circles
            {
                if (EntityManager.TryGetBuffer(e, isReadOnly: true, out DynamicBuffer<Game.Objects.SubObject> buffer))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        Entity subObject = buffer[i].m_SubObject;
                        bool isSubobjectVegetationPrefabFlag = false;
                        if (EntityManager.TryGetComponent(subObject, out PrefabRef subObjectPrefabEntity))
                        {
                            if (EntityManager.HasComponent<Vegetation>(subObjectPrefabEntity))
                            {
                                isSubobjectVegetationPrefabFlag = true;
                            }
                        }

                        if (isSubobjectVegetationPrefabFlag && EntityManager.HasComponent<Game.Objects.Transform>(subObject))
                        {
                            Game.Objects.Transform currentTransform = EntityManager.GetComponentData<Game.Objects.Transform>(subObject);
                            float radius = 2f;
                            if (m_TreeControllerUISystem.SelectionMode == Selection.Radius)
                            {
                                radius = m_TreeControllerUISystem.Radius;
                            }

                            if (CheckForHoveringOverTree(new Vector3(hit.m_HitPosition.x, hit.m_Position.y, hit.m_HitPosition.z), currentTransform.m_Position, radius) || m_TreeControllerUISystem.SelectionMode == Selection.BuildingOrNet)
                            {
                                TreeCircleRenderJob treeCircleRenderJob = new ()
                                {
                                    m_OverlayBuffer = m_OverlayRenderSystem.GetBuffer(out JobHandle outJobHandle),
                                    m_Transform = currentTransform,
                                };
                                inputDeps = IJobExtensions.Schedule(treeCircleRenderJob, JobHandle.CombineDependencies(inputDeps, outJobHandle));
                                m_OverlayRenderSystem.AddBufferWriter(inputDeps);
                            }
                        }
                    }
                }
            }

            if (m_ApplyAction.WasReleasedThisFrame() && m_TreeControllerUISystem.SelectionMode == Selection.Radius)
            {
                applyMode = ApplyMode.Clear;
            }
            else if (applyMode == ApplyMode.Clear)
            {
                applyMode = ApplyMode.None;
            }

            selectedTreeStates.Dispose(inputDeps);
            return inputDeps;
        }

        /// <inheritdoc/>
        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);
        }

        /// <inheritdoc/>
        protected override void OnGamePreload(Purpose purpose, GameMode mode)
        {
            base.OnGamePreload(purpose, mode);
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// Checks selected ages for at least one selection.
        /// </summary>
        /// <param name="ages">An array of bools for the selected ages.</param>
        /// <returns>True if at least one age is true. False if they are all false.</returns>
        private bool CheckForAtLeastOneSelectedAge(bool[] ages, ref NativeList<TreeState> states)
        {
            bool flag = false;
            for (int i = 0; i < ages.Length; i++)
            {
                if (ages[i])
                {
                    flag = true;
                    TreeState state = (TreeState)(int)Math.Pow(2, i - 1);
                    states.Add(state);
                }
            }

            if (flag)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Will loop through subobjects of the entity and change tree state. In future will change prefab.
        /// </summary>
        /// <param name="e">Entity that was hit by raycast.</param>
        /// <param name="hit">Raycast information.</param>
        /// <param name="jobHandle">So input deps can be passed along.</param>
        private void ProcessBufferForTrees(Entity e, RaycastHit hit, ref JobHandle jobHandle)
        {
            if (EntityManager.TryGetBuffer(e, isReadOnly: true, out DynamicBuffer<Game.Objects.SubObject> buffer))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    Entity subObject = buffer[i].m_SubObject;
                    bool isVegetationPrefabFlag = false;
                    if (EntityManager.TryGetComponent(subObject, out PrefabRef prefabEntity))
                    {
                        if (EntityManager.HasComponent<Vegetation>(prefabEntity))
                        {
                            isVegetationPrefabFlag = true;
                        }
                    }

                    if (isVegetationPrefabFlag && EntityManager.HasComponent<Game.Objects.Transform>(subObject))
                    {
                        Game.Objects.Transform currentTransform = EntityManager.GetComponentData<Game.Objects.Transform>(subObject);
                        if (CheckForHoveringOverTree(new Vector3(hit.m_HitPosition.x, hit.m_Position.y, hit.m_HitPosition.z), currentTransform.m_Position, 2f) || m_TreeControllerUISystem.SelectionMode == Selection.BuildingOrNet)
                        {
                            if (m_TreeControllerUISystem.AtLeastOneAgeSelected && EntityManager.HasComponent<Tree>(subObject))
                            {
                                NativeList<TreeState> selectedTreeStates = m_TreeControllerUISystem.GetSelectedAges();
                                ChangeTreeStateJob changeTreeStateJob = new ()
                                {
                                    m_Entity = subObject,
                                    m_Random = new ((uint)UnityEngine.Random.Range(1, 100000)),
                                    m_Ages = selectedTreeStates,
                                    m_Tree = EntityManager.GetComponentData<Tree>(subObject),
                                    buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                                };
                                jobHandle = changeTreeStateJob.Schedule(jobHandle);
                                m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
                                selectedTreeStates.Dispose(jobHandle);
                            }

                            bool doNotApplyTreePrefab = false;
                            if (EntityManager.TryGetComponent(e, out PrefabRef prefabRef))
                            {
                                if (m_PrefabSystem.TryGetPrefab(prefabRef, out PrefabBase prefabBase))
                                {
                                    if (prefabBase.GetType() == typeof(RoadPrefab))
                                    {
                                        doNotApplyTreePrefab = true;
                                    }
                                }
                            }

                            if (!m_SelectedTreePrefabEntities.IsEmpty && !doNotApplyTreePrefab)
                            {
                                NativeList<TreeState> selectedTreeStates = m_TreeControllerUISystem.GetSelectedAges();
                                ChangePrefabRefJob changePrefabRefJob = new ()
                                {
                                    m_Entity = subObject,
                                    m_SelectedPrefabEntities = m_SelectedTreePrefabEntities,
                                    m_Random = new ((uint)UnityEngine.Random.Range(1, 100000)),
                                    buffer = m_ToolOutputBarrier.CreateCommandBuffer(),
                                    m_Ages = selectedTreeStates,
                                    m_TreeDataLookup = SystemAPI.GetComponentLookup<TreeData>(isReadOnly: true),
                                    m_TreeLookup = SystemAPI.GetComponentLookup<Tree>(isReadOnly: true),
                                };
                                jobHandle = changePrefabRefJob.Schedule(jobHandle);
                                m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle);
                                selectedTreeStates.Dispose(jobHandle);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compares position of cursor and tree with regrads to a given radius.
        /// </summary>
        /// <param name="cursorPosition">From the raycast hit.</param>
        /// <param name="treePosition">Transform from the tree.</param>
        /// <param name="radius">The radius for comparison.</param>
        /// <returns>True if tree position is whithin radius from cursor position.</returns>
        private bool CheckForHoveringOverTree(float3 cursorPosition, float3 treePosition, float radius)
        {
            float minRadius = 1f;
            radius = Mathf.Max(radius, minRadius);
            float2 cursorPositionXZ = new (cursorPosition.x, cursorPosition.z);
            float2 treePositionXZ = new (treePosition.x, treePosition.z);
            if (Unity.Mathematics.math.distance(cursorPositionXZ, treePositionXZ) < radius)
            {
                return true;
            }

            return false;
        }

#if BURST
        [BurstCompile]
#endif
        private struct TreeChangerWithinRadius : IJobChunk
        {
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<Game.Objects.Tree> m_TreeType;
            public ComponentTypeHandle<Game.Objects.Transform> m_TransformType;
            public bool m_OverrideState;
            public bool m_OverridePrefab;
            public NativeList<TreeState> m_Ages;
            public EntityCommandBuffer.ParallelWriter buffer;
            public ComponentTypeHandle<Game.Prefabs.PrefabRef> m_PrefabRefType;
            public NativeList<Entity> m_PrefabEntities;
            public Unity.Mathematics.Random m_Random;
            public float m_Radius;
            public float3 m_Position;
            public ComponentLookup<Tree> m_TreeLookup;
            public ComponentLookup<TreeData> m_TreeDataLookup;
            public ComponentLookup<Vegetation> m_VegetationLookup;

            /// <summary>
            /// Executes job which will change state or prefab for trees within a radius.
            /// </summary>
            /// <param name="chunk">ArchteypeChunk of IJobChunk.</param>
            /// <param name="unfilteredChunkIndex">Use for EntityCommandBuffer.ParralelWriter.</param>
            /// <param name="useEnabledMask">Part of IJobChunk. Unsure what it does.</param>
            /// <param name="chunkEnabledMask">Part of IJobChunk. Not sure what it does.</param>
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Objects.Transform> transformNativeArray = chunk.GetNativeArray(ref m_TransformType);
                NativeArray<Game.Objects.Tree> treeNativeArray = chunk.GetNativeArray(ref m_TreeType);
                NativeArray<Game.Prefabs.PrefabRef> prefabRefNativeArray = chunk.GetNativeArray(ref m_PrefabRefType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (CheckForHoveringOverTree(m_Position, transformNativeArray[i].m_Position, m_Radius))
                    {
                        Entity currentEntity = entityNativeArray[i];
                        if (m_OverrideState && m_TreeLookup.HasComponent(currentEntity) && m_OverridePrefab == false)
                        {
                            Game.Objects.Tree currentTreeData = treeNativeArray[i];
                            currentTreeData.m_State = GetTreeState(m_Ages, currentTreeData);
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                            buffer.AddComponent<RecentlyChanged>(unfilteredChunkIndex, currentEntity);
                            buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity);
                            continue;
                        }

                        if (m_OverridePrefab == true)
                        {
                            PrefabRef currentPrefabRef = prefabRefNativeArray[i];

                            // This checks for plants that are not in the vegetation tab.
                            if (!m_VegetationLookup.HasComponent(currentPrefabRef.m_Prefab))
                            {
                                continue;
                            }

                            if (m_PrefabEntities.Length > 0)
                            {
                                currentPrefabRef.m_Prefab = m_PrefabEntities[m_Random.NextInt(m_PrefabEntities.Length)];
                            }
                            else
                            {
                                continue;
                            }

                            // Convert plant to tree.
                            if (m_TreeDataLookup.HasComponent(currentPrefabRef.m_Prefab) && !m_TreeLookup.HasComponent(currentEntity))
                            {
                                buffer.AddComponent<Tree>(unfilteredChunkIndex, currentEntity);
                                Tree tree = new () { m_Growth = 0, m_State = GetTreeState(m_Ages, new Tree() { m_Growth = 0, m_State = TreeState.Adult }) };
                                buffer.SetComponent(unfilteredChunkIndex, currentEntity, tree);
                            }

                            // Convert tree to Plant.
                            else if (!m_TreeDataLookup.HasComponent(currentPrefabRef.m_Prefab) && m_TreeLookup.HasComponent(currentEntity))
                            {
                                buffer.RemoveComponent<Tree>(unfilteredChunkIndex, currentEntity);
                            }

                            // Override state of existing tree and prefab.
                            else if (m_OverrideState && m_TreeDataLookup.HasComponent(currentPrefabRef.m_Prefab) && m_TreeLookup.HasComponent(currentEntity))
                            {
                                Game.Objects.Tree currentTreeData = treeNativeArray[i];
                                currentTreeData.m_State = GetTreeState(m_Ages, currentTreeData);
                                buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                            }

                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentPrefabRef);
                            buffer.RemoveComponent<Evergreen>(unfilteredChunkIndex, currentEntity);
                            buffer.RemoveComponent<DeciduousData>(unfilteredChunkIndex, currentEntity);
                            buffer.AddComponent<RecentlyChanged>(unfilteredChunkIndex, currentEntity);
                            buffer.AddComponent<Updated>(unfilteredChunkIndex, currentEntity);
                            buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity);
                        }
                    }
                }
            }

            /// <summary>
            /// Checks the radius and position and returns true if tree is there.
            /// </summary>
            /// <param name="cursorPosition">Float3 from Raycast.</param>
            /// <param name="treePosition">Float3 position from Transform.</param>
            /// <param name="radius">Radius usually passed from settings.</param>
            /// <returns>True if tree position is within radius of position. False if not.</returns>
            private bool CheckForHoveringOverTree(float3 cursorPosition, float3 treePosition, float radius)
            {
                float minRadius = 5f;
                radius = Mathf.Max(radius, minRadius);
                if (Unity.Mathematics.math.distance(cursorPosition, treePosition) < radius)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Gets a specified tree state or random tree state from Ages array.
            /// </summary>
            /// <param name="ages">Selected ages.</param>
            /// <param name="tree">Tree so original TreeState can be used if no ages are selected.</param>
            /// <returns>TreeState.</returns>
            private TreeState GetTreeState(NativeList<TreeState> ages, Game.Objects.Tree tree)
            {
                if (ages.Length > 0)
                {
                    TreeState returnState = ages[m_Random.NextInt(ages.Length)];
                    return returnState;
                }

                return tree.m_State;
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct TreeChangerWholeMap : IJobChunk
        {
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<Game.Objects.Tree> m_TreeType;
            public bool m_OverrideState;
            public bool m_OverridePrefab;
            public NativeList<TreeState> m_Ages;
            public EntityCommandBuffer.ParallelWriter buffer;
            public ComponentTypeHandle<Game.Prefabs.PrefabRef> m_PrefabRefType;
            public NativeList<Entity> m_PrefabEntities;
            public Unity.Mathematics.Random m_Random;
            public ComponentLookup<Tree> m_TreeLookup;
            public ComponentLookup<TreeData> m_TreeDataLookup;
            public ComponentLookup<Vegetation> m_VegetationLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityNativeArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<Game.Objects.Tree> treeNativeArray = chunk.GetNativeArray(ref m_TreeType);
                NativeArray<Game.Prefabs.PrefabRef> prefabRefNativeArray = chunk.GetNativeArray(ref m_PrefabRefType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity currentEntity = entityNativeArray[i];
                    if (m_OverrideState && m_TreeLookup.HasComponent(currentEntity) && m_OverridePrefab == false)
                    {
                        Game.Objects.Tree currentTreeData = treeNativeArray[i];
                        currentTreeData.m_State = GetTreeState(m_Ages, currentTreeData);
                        buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                        buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity);
                        continue;
                    }

                    if (m_OverridePrefab == true)
                    {
                        PrefabRef currentPrefabRef = prefabRefNativeArray[i];

                        // This checks for plants that are not in the vegetation tab.
                        if (!m_VegetationLookup.HasComponent(currentPrefabRef.m_Prefab))
                        {
                            continue;
                        }

                        if (m_PrefabEntities.Length > 0)
                        {
                            currentPrefabRef.m_Prefab = m_PrefabEntities[m_Random.NextInt(m_PrefabEntities.Length)];
                        }
                        else
                        {
                            continue;
                        }

                        // Convert plant to tree.
                        if (m_TreeDataLookup.HasComponent(currentPrefabRef.m_Prefab) && !m_TreeLookup.HasComponent(currentEntity))
                        {
                            buffer.AddComponent<Tree>(unfilteredChunkIndex, currentEntity);
                            Tree tree = new () { m_Growth = 0, m_State = GetTreeState(m_Ages, new Tree() { m_Growth = 0, m_State = TreeState.Adult }) };
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, tree);
                        }

                        // Convert tree to Plant.
                        else if (!m_TreeDataLookup.HasComponent(currentPrefabRef.m_Prefab) && m_TreeLookup.HasComponent(currentEntity))
                        {
                            buffer.RemoveComponent<Tree>(unfilteredChunkIndex, currentEntity);
                        }

                        // Override state of existing tree and prefab.
                        else if (m_OverrideState && m_TreeDataLookup.HasComponent(currentPrefabRef.m_Prefab) && m_TreeLookup.HasComponent(currentEntity))
                        {
                            Game.Objects.Tree currentTreeData = treeNativeArray[i];
                            currentTreeData.m_State = GetTreeState(m_Ages, currentTreeData);
                            buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentTreeData);
                        }

                        buffer.SetComponent(unfilteredChunkIndex, currentEntity, currentPrefabRef);
                        buffer.RemoveComponent<Evergreen>(unfilteredChunkIndex, currentEntity);
                        buffer.RemoveComponent<DeciduousData>(unfilteredChunkIndex, currentEntity);
                        buffer.AddComponent<Updated>(unfilteredChunkIndex, currentEntity);
                        buffer.AddComponent<BatchesUpdated>(unfilteredChunkIndex, currentEntity);
                    }
                }
            }

            /// <summary>
            /// Gets a specified tree state or random tree state from Ages array.
            /// </summary>
            /// <param name="ages">Selected ages.</param>
            /// <param name="tree">Tree so original TreeState can be used if no ages are selected.</param>
            /// <returns>TreeState.</returns>
            private TreeState GetTreeState(NativeList<TreeState> ages, Game.Objects.Tree tree)
            {
                if (ages.Length > 0)
                {
                    TreeState returnState = ages[m_Random.NextInt(ages.Length)];
                    return returnState;
                }

                return tree.m_State;
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct TreeCircleRenderJob : IJob
        {
            public OverlayRenderSystem.Buffer m_OverlayBuffer;
            public Game.Objects.Transform m_Transform;

            /// <summary>
            /// Draws circles around trees.
            /// </summary>
            public void Execute()
            {
                m_OverlayBuffer.DrawCircle(new UnityEngine.Color(.88f, .26f, 0.90f), default, 0.25f, 0, new float2(0, 1), m_Transform.m_Position, 3f);
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct ToolRadiusJob : IJob
        {
            public OverlayRenderSystem.Buffer m_OverlayBuffer;
            public float3 m_Position;
            public float m_Radius;

            /// <summary>
            /// Draws tool radius.
            /// </summary>
            public void Execute()
            {
                m_OverlayBuffer.DrawCircle(new UnityEngine.Color(.52f, .80f, .86f, 1f), default, m_Radius / 20f, 0, new float2(0, 1), m_Position, m_Radius * 2f);
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct ChangePrefabRefJob : IJob
        {
            public Entity m_Entity;
            public NativeList<Entity> m_SelectedPrefabEntities;
            public EntityCommandBuffer buffer;
            public Unity.Mathematics.Random m_Random;
            public NativeList<TreeState> m_Ages;
            public ComponentLookup<TreeData> m_TreeDataLookup;
            public ComponentLookup<Tree> m_TreeLookup;

            /// <summary>
            /// Changes prefab ref for specified entity.
            /// </summary>
            public void Execute()
            {
                PrefabRef prefabRef;
                if (!m_SelectedPrefabEntities.IsEmpty)
                {
                    prefabRef = new PrefabRef(m_SelectedPrefabEntities[m_Random.NextInt(m_SelectedPrefabEntities.Length)]);

                    // convert plant to tree.
                    if (m_TreeDataLookup.HasComponent(prefabRef.m_Prefab) && !m_TreeLookup.HasComponent(m_Entity))
                    {
                        buffer.AddComponent<Tree>(m_Entity);
                        Tree tree = new () { m_Growth = 0, m_State = GetTreeState(m_Ages) };
                        buffer.SetComponent(m_Entity, tree);
                    }

                    // convert tree to plant.
                    else if (!m_TreeDataLookup.HasComponent(prefabRef.m_Prefab) && m_TreeLookup.HasComponent(m_Entity))
                    {
                        buffer.RemoveComponent<Tree>(m_Entity);
                    }

                    buffer.RemoveComponent<DeciduousData>(m_Entity);
                    buffer.RemoveComponent<Evergreen>(m_Entity);
                    buffer.SetComponent(m_Entity, prefabRef);
                    buffer.AddComponent<Updated>(m_Entity);
                }
            }

            /// <summary>
            /// Gets a specified tree state or random tree state from Ages array.
            /// </summary>
            /// <param name="ages">Selected ages.</param>
            /// <returns>TreeState.</returns>
            private TreeState GetTreeState(NativeList<TreeState> ages)
            {
                if (ages.Length > 0)
                {
                    TreeState returnState = ages[m_Random.NextInt(ages.Length)];
                    return returnState;
                }

                return TreeState.Adult;
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct ChangeTreeStateJob : IJob
        {
            public Entity m_Entity;
            public Tree m_Tree;
            public NativeList<TreeState> m_Ages;
            public EntityCommandBuffer buffer;
            public Unity.Mathematics.Random m_Random;

            /// <summary>
            /// Changes TreeState for specfied tree entity.
            /// </summary>
            public void Execute()
            {
                m_Tree.m_State = GetTreeState(m_Ages, m_Tree);
                buffer.SetComponent(m_Entity, m_Tree);
                buffer.AddComponent<BatchesUpdated>(m_Entity);
            }

            /// <summary>
            /// Gets a specified tree state or random tree state from Ages array.
            /// </summary>
            /// <param name="ages">Selected ages.</param>
            /// <param name="tree">Tree so original TreeState can be used if no ages are selected.</param>
            /// <returns>TreeState.</returns>
            private TreeState GetTreeState(NativeList<TreeState> ages, Game.Objects.Tree tree)
            {
                if (ages.Length > 0)
                {
                    TreeState returnState = ages[m_Random.NextInt(ages.Length)];
                    return returnState;
                }

                return tree.m_State;
            }
        }
    }
}
