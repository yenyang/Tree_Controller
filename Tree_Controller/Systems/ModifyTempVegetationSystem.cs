// <copyright file="ModifyTempTreesSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Systems
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Modifies Temp entities that are also trees.
    /// </summary>
    public partial class ModifyTempVegetationSystem : GameSystemBase
    {
        private ILog m_Log;
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private NetToolSystem m_NetToolSystem;
        private EntityQuery m_TempVegetationQuery;
        private EntityQuery m_TempTreeQuery;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_Log.Info($"{nameof(ModifyTempVegetationSystem)}.OnCreate");
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ToolSystem.EventToolChanged += (ToolBaseSystem tool) => Enabled = tool == m_ObjectToolSystem || (tool.toolID != null && tool.toolID == "Line Tool") || tool == m_NetToolSystem;

            m_TempVegetationQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Temp>()
                .WithAny<Game.Objects.Tree, Game.Objects.Plant>()
                .WithNone<Deleted, Overridden>()
                .Build();

            m_TempTreeQuery = SystemAPI.QueryBuilder()
                .WithAll<Updated, Temp, Game.Objects.Tree>()
                .WithNone<Deleted, Overridden>()
                .Build();

            RequireForUpdate(m_TempVegetationQuery);
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {

            NativeArray<Entity> tempTreeEntities = m_TempTreeQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity tempTreeEntity in tempTreeEntities)
            {
                if (EntityManager.TryGetComponent(tempTreeEntity, out Game.Objects.Tree tree))
                {
                    tree.m_State = Game.Objects.TreeState.Stump;
                    EntityManager.SetComponentData(tempTreeEntity, tree);
                }
            }
        }
    }
}
