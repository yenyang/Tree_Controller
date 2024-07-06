// <copyright file="ReloadFoliageColorDataSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Tree_Controller.Systems
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.PSI.Common;
    using Colossal.PSI.Environment;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Prefabs.Climate;
    using Game.Rendering;
    using Game.Simulation;
    using Game.Tools;
    using Tree_Controller.Settings;
    using Tree_Controller.Utils;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.UniversalDelegates;
    using Unity.Jobs;
    using static Game.Rendering.OverlayRenderSystem;

    /// <summary>
    /// Replaces colors of vanilla trees.
    /// </summary>
    public partial class ReloadFoliageColorDataSystem : GameSystemBase
    {
        private const string ColorPainterID = "ColorPainterTool";

        private readonly Dictionary<SeasonIdentifier, Game.Rendering.ColorSet> m_YenyangsColorSets = new ()
        {
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Spring, m_Index = 0 }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.945f, 0.941f, 0.957f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Summer, m_Index = 1 }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.625f, 0.141f, 0.098f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.934f, 0.250f, 0.109f, 1.000f), m_Channel1 = new (0.785f, 0.313f, 0.137f, 1.000f), m_Channel2 = new (0.625f, 0.141f, 0.098f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Winter, m_Index = 3 }, new () { m_Channel0 = new (0.670f, 0.523f, 0.409f, 1.000f), m_Channel1 = new (0.642f, 0.574f, 0.428f, 1.000f), m_Channel2 = new (0.689f, 0.608f, 0.466f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "BirchTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.981f, 0.773f, 0.270f, 1.000f), m_Channel1 = new (0.867f, 0.586f, 0.137f, 1.000f), m_Channel2 = new (0.957f, 0.664f, 0.199f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_AlderTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.373f, 0.500f, 0.324f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_AlderTree01"), m_Season = FoliageUtils.Season.Winter, m_Index = 3 }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.373f, 0.500f, 0.324f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_ChestnutTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.981f, 0.707f, 0.099f, 1.000f), m_Channel1 = new (0.961f, 0.531f, 0.031f, 1.000f), m_Channel2 = new (0.984f, 0.664f, 0.094f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_ChestnutTree01"), m_Season = FoliageUtils.Season.Winter, m_Index = 3 }, new () { m_Channel0 = new (0.606f, 0.219f, 0f, 1.000f), m_Channel1 = new (0.633f, 0.227f, 0.051f, 1.000f), m_Channel2 = new (0.379f, 0.082f, 0f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_PoplarTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.961f, 0.781f, 0.344f, 1.000f), m_Channel1 = new (0.793f, 0.613f, 0.141f, 1.000f), m_Channel2 = new (0.984f, 0.789f, 0.281f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "FlowerBushWild01"), m_Season = FoliageUtils.Season.Spring, m_Index = 0 }, new () { m_Channel0 = new (0.310f, 0.463f, 0.310f, 1.000f), m_Channel1 = new (0.329f, 0.443f, 0.294f, 1.000f), m_Channel2 = new (0.32f, 0.45f, 0.3f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "FlowerBushWild02"), m_Season = FoliageUtils.Season.Summer, m_Index = 1 }, new () { m_Channel0 = new (0.310f, 0.463f, 0.310f, 1.000f), m_Channel1 = new (0.329f, 0.443f, 0.294f, 1.000f), m_Channel2 = new (0.32f, 0.45f, 0.3f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_HickoryTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.965f, 0.805f, 0.066f, 1.000f), m_Channel1 = new (0.914f, 0.582f, 0.125f, 1.000f), m_Channel2 = new (0.863f, 0.504f, 0.242f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_LindenTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.930f, 0.600f, 0.008f, 1.000f), m_Channel1 = new (0.633f, 0.320f, 0.016f, 1.000f), m_Channel2 = new (0.852f, 0.297f, 0.004f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_LondonPlaneTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.922f, 0.535f, 0.106f, 1.000f), m_Channel1 = new (0.871f, 0.676f, 0.168f, 1.000f), m_Channel2 = new (0.543f, 0.188f, 0.070f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_LondonPlaneTree01"), m_Season = FoliageUtils.Season.Winter, m_Index = 3 }, new () { m_Channel0 = new (0.680f, 0.379f, 0.051f, 1.000f), m_Channel1 = new (0.605f, 0.340f, 0.063f, 1.000f), m_Channel2 = new (0.508f, 0.199f, 0.032f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "OakTree01"), m_Season = FoliageUtils.Season.Autumn, m_Index = 2 }, new () { m_Channel0 = new (0.957f, 0.356f, 0.113f, 1.000f), m_Channel1 = new (0.957f, 0.266f, 0.125f, 1.000f), m_Channel2 = new (0.961f, 0.469f, 0.281f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "OakTree01"), m_Season = FoliageUtils.Season.Winter, m_Index = 3 }, new () { m_Channel0 = new (0.934f, 0.25f, 0.109f, 1.000f), m_Channel1 = new (0.785f, 0.313f, 0.137f, 1.000f), m_Channel2 = new (0.902f, 0.148f, 0.059f, 1.000f) } },
        };

        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_PlantPrefabQuery;
        private TreeControllerSettings.ColorVariationSetYYTC m_ColorVariationSet;
        private bool m_Run = true;
        private Dictionary<SeasonIdentifier, Game.Rendering.ColorSet> m_VanillaColorSets;
        private Dictionary<SeasonIdentifier, Game.Rendering.ColorSet> m_SpringColorSets;
        private Dictionary<SeasonIdentifier, Game.Rendering.ColorSet> m_AutumnColorSets;
        private ClimateSystem m_ClimateSystem;
        private FoliageUtils.Season m_Season = FoliageUtils.Season.Spring;
        private ILog m_Log;
        private string m_ContentFolder;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_PlantQuery;
        private ToolBaseSystem m_ColorPainterTool;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReloadFoliageColorDataSystem"/> class.
        /// </summary>
        public ReloadFoliageColorDataSystem()
        {
        }

        /// <summary>
        /// Sets a value indicating whether to reload foliage color data.
        /// </summary>
        public bool Run { set => m_Run = value; }

        private Dictionary<SeasonIdentifier, ColorSet> YenyangsColorSets => m_YenyangsColorSets;

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_ClimateSystem = World.GetOrCreateSystemManaged<ClimateSystem>();
            m_Log.Info($"{typeof(ReloadFoliageColorDataSystem)}.{nameof(OnCreate)}");
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_ColorVariationSet = TreeControllerMod.Instance.Settings.ColorVariationSet;
            m_VanillaColorSets = new ();
            m_SpringColorSets = new ();
            m_AutumnColorSets = new ();
            m_ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "Recolor", "SavedColorSet", "Custom");
            m_PlantPrefabQuery = SystemAPI.QueryBuilder()
            .WithAll<PlantData, SubMesh>()
            .WithNone<PlaceholderObjectElement, Evergreen>()
            .Build();

            RequireForUpdate(m_PlantPrefabQuery);

            m_PlantQuery = SystemAPI.QueryBuilder()
                .WithAll<Game.Objects.Plant>()
                .WithNone<Deleted, Game.Common.Overridden, Game.Tools.Temp, Evergreen>()
                .Build();
        }

        /// <inheritdoc/>
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (World.GetOrCreateSystemManaged<ToolSystem>().tools.Find(x => x.toolID.Equals(ColorPainterID)) is ToolBaseSystem colorPainterTool)
            {
                // Found it
                m_Log.Info($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnGameLoadingComplete)} found Color Painter Tool.");
                MethodInfo colorPainterHasCustomColorVariation = colorPainterTool.GetType().GetMethod("HasCustomColorVariation");
                if (colorPainterHasCustomColorVariation is not null)
                {
                    m_ColorPainterTool = colorPainterTool;
                    m_Log.Info($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnGameLoadingComplete)} saved Color Painter Tool");
                }
            }
            else
            {
                m_Log.Info($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnGameLoadingComplete)} Color Painter Tool tool not found");
            }

            if (mode.IsGame())
            {
                Run = true;
            }
            else
            {
                return;
            }

            if (m_VanillaColorSets.Count > 0)
            {
                return;
            }

            EntityCommandBuffer buffer = m_EndFrameBarrier.CreateCommandBuffer();
            JobHandle plantPrefabJobHandle;
            NativeList<Entity> plantPrefabEntities = m_PlantPrefabQuery.ToEntityListAsync(Allocator.Temp, out plantPrefabJobHandle);
            plantPrefabJobHandle.Complete();
            foreach (Entity e in plantPrefabEntities)
            {
                if (!EntityManager.TryGetBuffer(e, isReadOnly: true, out DynamicBuffer<SubMesh> subMeshBuffer))
                {
                    continue;
                }

                for (int i = 0; i < Math.Min(4, subMeshBuffer.Length); i++)
                {
                    if (!EntityManager.TryGetBuffer(subMeshBuffer[i].m_SubMesh, isReadOnly: false, out DynamicBuffer<ColorVariation> colorVariationBuffer))
                    {
                        continue;
                    }

                    PrefabBase prefabBase = m_PrefabSystem.GetPrefab<PrefabBase>(subMeshBuffer[i].m_SubMesh);
                    PrefabID prefabID = prefabBase.GetPrefabID();

                    if (colorVariationBuffer.Length < 4 || prefabBase.name.ToLower().Contains("palm"))
                    {
                        buffer.AddComponent<Evergreen>(e);
                        continue;
                    }

                    for (int j = 0; j < colorVariationBuffer.Length; j++)
                    {
#if VERBOSE
                        m_Log.Verbose($"{prefabID.GetName()} {(TreeState)(int)Math.Pow(2, i - 1)} {(FoliageUtils.Season)j} {colorVariationBuffer[j].m_ColorSet.m_Channel0} {colorVariationBuffer[j].m_ColorSet.m_Channel1} {colorVariationBuffer[j].m_ColorSet.m_Channel2}");
#endif
                        ColorVariation currentColorVariation = colorVariationBuffer[j];
                        m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnGameLoadingComplete)} Saving vanilla color variation {prefabID} for index : {j}.");

                        if (!FoliageUtils.TryGetSeasonFromColorGroupID(currentColorVariation.m_GroupID, out FoliageUtils.Season season))
                        {
                            continue;
                        }

                        m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnGameLoadingComplete)} season is {season}.");

                        SeasonIdentifier treeSeasonIdentifier = new ()
                        {
                            m_PrefabID = prefabID,
                            m_Season = season,
                            m_Index = j,
                        };

                        if (!m_VanillaColorSets.ContainsKey(treeSeasonIdentifier))
                        {
                            m_VanillaColorSets.Add(treeSeasonIdentifier, currentColorVariation.m_ColorSet);
                            m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnGameLoadingComplete)} saved.");
                        }

                        if (!m_SpringColorSets.ContainsKey(treeSeasonIdentifier))
                        {
                            if (colorVariationBuffer.Length == 4)
                            {
                                m_SpringColorSets.Add(treeSeasonIdentifier, colorVariationBuffer[0].m_ColorSet);
                            }
                            else if (colorVariationBuffer.Length == 8 && (season == FoliageUtils.Season.Spring || season == FoliageUtils.Season.Summer))
                            {
                                m_SpringColorSets.Add(treeSeasonIdentifier, currentColorVariation.m_ColorSet);
                            }
                            else if (colorVariationBuffer.Length == 8 && (season == FoliageUtils.Season.Autumn || season == FoliageUtils.Season.Winter))
                            {
                                ColorSet newColorSet = currentColorVariation.m_ColorSet;
                                newColorSet.m_Channel0 = new UnityEngine.Color(0.1981132f, 0.3962264f, 0.1981132f, 1);
                                newColorSet.m_Channel1 = new UnityEngine.Color(0.1981132f, 0.3962264f, 0.1981132f, 1);
                                m_SpringColorSets.Add(treeSeasonIdentifier, newColorSet);
                            }
                        }

                        if (!m_AutumnColorSets.ContainsKey(treeSeasonIdentifier))
                        {
                            if (colorVariationBuffer.Length == 4)
                            {
                                m_AutumnColorSets.Add(treeSeasonIdentifier, colorVariationBuffer[2].m_ColorSet);
                            }
                            else if (colorVariationBuffer.Length == 8)
                            {
                                m_AutumnColorSets.Add(treeSeasonIdentifier, colorVariationBuffer[6].m_ColorSet);
                            }
                        }
                    }
                }
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
            m_Season = FoliageUtils.GetSeasonFromSeasonID(climatePrefab.FindSeasonByTime(m_ClimateSystem.currentDate).Item1.m_NameID);
            if (lastSeason != m_Season)
            {
                m_Run = true;
            }

            if (!m_Run && TreeControllerMod.Instance.Settings.ColorVariationSet == m_ColorVariationSet)
            {
                return;
            }

            EntityCommandBuffer buffer = m_EndFrameBarrier.CreateCommandBuffer();
            JobHandle plantPrefabJobHandle;
            NativeList<Entity> plantPrefabEntities = m_PlantPrefabQuery.ToEntityListAsync(Allocator.Temp, out plantPrefabJobHandle);
            plantPrefabJobHandle.Complete();


            foreach (Entity e in plantPrefabEntities)
            {
                if (!EntityManager.TryGetBuffer(e, isReadOnly: false, out DynamicBuffer<SubMesh> subMeshBuffer))
                {
                    continue;
                }

                for (int i = 0; i < Math.Min(4, subMeshBuffer.Length); i++)
                {
                    if (!EntityManager.TryGetBuffer(subMeshBuffer[i].m_SubMesh, isReadOnly: false, out DynamicBuffer<ColorVariation> colorVariationBuffer))
                    {
                        continue;
                    }

                    PrefabBase meshPrefabBase = m_PrefabSystem.GetPrefab<PrefabBase>(subMeshBuffer[i].m_SubMesh);
                    PrefabID meshPrefabID = meshPrefabBase.GetPrefabID();
                    PrefabBase treePrefabBase = m_PrefabSystem.GetPrefab<PrefabBase>(e);
                    PrefabID treePrefabID = treePrefabBase.GetPrefabID();

                    for (int j = 0; j < colorVariationBuffer.Length; j++)
                    {
                        ColorVariation currentColorVariation = colorVariationBuffer[j];

                        if (!FoliageUtils.TryGetSeasonFromColorGroupID(currentColorVariation.m_GroupID, out FoliageUtils.Season season))
                        {
                            continue;
                        }

                        SeasonIdentifier meshSeasonIdentifier = new ()
                        {
                            m_PrefabID = meshPrefabID,
                            m_Season = season,
                            m_Index = j,
                        };

                        SeasonIdentifier treeSeasonIdentifier = new ()
                        {
                            m_PrefabID = treePrefabID,
                            m_Season = season,
                            m_Index = j,
                        };

                        if (File.Exists(GetAssetSeasonIdentifierFilePath(meshSeasonIdentifier)))
                        {
                            continue;
                        }

                        if (m_ColorPainterTool is not null)
                        {
                            MethodInfo hasCustomColorVariation = m_ColorPainterTool.GetType().GetMethod("HasCustomColorVariation");
                            object results = hasCustomColorVariation.Invoke(m_ColorPainterTool, new object[] { subMeshBuffer[i].m_SubMesh, j });
                            if ((bool)results)
                            {
                                if (m_PrefabSystem.TryGetPrefab(subMeshBuffer[i].m_SubMesh, out PrefabBase prefab))
                                {
                                    m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}{nameof(OnUpdate)} {prefab.name} Has Custom color variation.");
                                } else
                                {
                                    m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}{nameof(OnUpdate)} Has Custom color variation. couldn't get prefab.");
                                }

                                continue;
                            }
                        }

                        if ((TreeControllerMod.Instance.Settings.UseDeadModelDuringWinter && m_Season == FoliageUtils.Season.Spring && meshSeasonIdentifier.m_Season == FoliageUtils.Season.Winter && TreeControllerMod.Instance.Settings.ColorVariationSet != TreeControllerSettings.ColorVariationSetYYTC.Autumn)
                            || (TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Spring && (season == FoliageUtils.Season.Autumn || season == FoliageUtils.Season.Winter))
                            || ((TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Yenyangs && !EntityManager.HasComponent<TreeData>(e) && !m_YenyangsColorSets.ContainsKey(meshSeasonIdentifier)) && (season == FoliageUtils.Season.Autumn || season == FoliageUtils.Season.Winter)))
                        {
                            if (m_SpringColorSets.ContainsKey(meshSeasonIdentifier))
                            {
                                currentColorVariation.m_ColorSet = m_SpringColorSets[meshSeasonIdentifier];
                                colorVariationBuffer[j] = currentColorVariation;
                                m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} set to spring Colorset {TreeControllerMod.Instance.Settings.ColorVariationSet} for {meshPrefabID} in index {j} : {meshSeasonIdentifier.m_Season}");
                                continue;
                            }
                        }
                        else if (TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Autumn)
                        {
                            if (m_AutumnColorSets.ContainsKey(meshSeasonIdentifier))
                            {
                                currentColorVariation.m_ColorSet = m_AutumnColorSets[meshSeasonIdentifier];
                                colorVariationBuffer[j] = currentColorVariation;
                                m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} set to autumn Colorset {TreeControllerMod.Instance.Settings.ColorVariationSet} for {meshPrefabID} in index {j} : {meshSeasonIdentifier.m_Season}");
                                continue;
                            }
                        }

                        if (TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Yenyangs && YenyangsColorSets.ContainsKey(treeSeasonIdentifier))
                        {
                            currentColorVariation.m_ColorSet = YenyangsColorSets[treeSeasonIdentifier];
                            colorVariationBuffer[j] = currentColorVariation;
                            m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} Changed Colorset for {meshPrefabID} in {treeSeasonIdentifier.m_Season}");
                        }
                        else if (m_VanillaColorSets.ContainsKey(meshSeasonIdentifier))
                        {
                            currentColorVariation.m_ColorSet = m_VanillaColorSets[meshSeasonIdentifier];
                            colorVariationBuffer[j] = currentColorVariation;
                            m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} Reset Colorset for {meshPrefabID} in {meshSeasonIdentifier.m_Season}");
                        }
                        else
                        {
                            m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} Did nothing for: {TreeControllerMod.Instance.Settings.ColorVariationSet} for {meshPrefabID} in index {j} : {meshSeasonIdentifier.m_Season}");
                        }
                    }
                }
            }

            plantPrefabEntities.Dispose();
            m_Run = false;
            m_ColorVariationSet = TreeControllerMod.Instance.Settings.ColorVariationSet;

            buffer.AddComponent<BatchesUpdated>(m_PlantQuery, EntityQueryCaptureMode.AtPlayback);
        }

        // Taken from Recolor mod.
        private string GetAssetSeasonIdentifierFilePath(SeasonIdentifier assetSeasonIdentifier)
        {
            string prefabType = assetSeasonIdentifier.m_PrefabID.ToString().Remove(assetSeasonIdentifier.m_PrefabID.ToString().IndexOf(':'));
            return Path.Combine(m_ContentFolder, $"{prefabType}-{assetSeasonIdentifier.m_PrefabID.GetName()}-{assetSeasonIdentifier.m_Index}.xml");
        }

        private struct SeasonIdentifier
        {
            public PrefabID m_PrefabID;
            public FoliageUtils.Season m_Season;
            public int m_Index;
        }

    }
}
