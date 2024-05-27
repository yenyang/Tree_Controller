// <copyright file="ReloadFoliageColorDataSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Tree_Controller.Systems
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.PSI.Environment;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.Prefabs.Climate;
    using Game.Rendering;
    using Game.Simulation;
    using Tree_Controller.Settings;
    using Tree_Controller.Utils;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// Replaces colors of vanilla trees.
    /// </summary>
    public partial class ReloadFoliageColorDataSystem : GameSystemBase
    {
        private readonly Dictionary<TreeSeasonIdentifier, Game.Rendering.ColorSet> m_YenyangsColorSets = new ()
        {
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Spring }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.945f, 0.941f, 0.957f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Summer }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.625f, 0.141f, 0.098f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.934f, 0.250f, 0.109f, 1.000f), m_Channel1 = new (0.785f, 0.313f, 0.137f, 1.000f), m_Channel2 = new (0.625f, 0.141f, 0.098f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "AppleTree01"), m_Season = FoliageUtils.Season.Winter }, new () { m_Channel0 = new (0.670f, 0.523f, 0.409f, 1.000f), m_Channel1 = new (0.642f, 0.574f, 0.428f, 1.000f), m_Channel2 = new (0.689f, 0.608f, 0.466f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "BirchTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.981f, 0.773f, 0.270f, 1.000f), m_Channel1 = new (0.867f, 0.586f, 0.137f, 1.000f), m_Channel2 = new (0.957f, 0.664f, 0.199f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_AlderTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.373f, 0.500f, 0.324f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_AlderTree01"), m_Season = FoliageUtils.Season.Winter }, new () { m_Channel0 = new (0.409f, 0.509f, 0.344f, 1.000f), m_Channel1 = new (0.335f, 0.462f, 0.265f, 1.000f), m_Channel2 = new (0.373f, 0.500f, 0.324f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_ChestnutTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.981f, 0.707f, 0.099f, 1.000f), m_Channel1 = new (0.961f, 0.531f, 0.031f, 1.000f), m_Channel2 = new (0.984f, 0.664f, 0.094f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_ChestnutTree01"), m_Season = FoliageUtils.Season.Winter }, new () { m_Channel0 = new (0.606f, 0.219f, 0f, 1.000f), m_Channel1 = new (0.633f, 0.227f, 0.051f, 1.000f), m_Channel2 = new (0.379f, 0.082f, 0f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "EU_PoplarTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.961f, 0.781f, 0.344f, 1.000f), m_Channel1 = new (0.793f, 0.613f, 0.141f, 1.000f), m_Channel2 = new (0.984f, 0.789f, 0.281f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "FlowerBushWild01"), m_Season = FoliageUtils.Season.Spring }, new () { m_Channel0 = new (0.310f, 0.463f, 0.310f, 1.000f), m_Channel1 = new (0.329f, 0.443f, 0.294f, 1.000f), m_Channel2 = new (0.32f, 0.45f, 0.3f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "FlowerBushWild02"), m_Season = FoliageUtils.Season.Summer }, new () { m_Channel0 = new (0.310f, 0.463f, 0.310f, 1.000f), m_Channel1 = new (0.329f, 0.443f, 0.294f, 1.000f), m_Channel2 = new (0.32f, 0.45f, 0.3f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_HickoryTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.965f, 0.805f, 0.066f, 1.000f), m_Channel1 = new (0.914f, 0.582f, 0.125f, 1.000f), m_Channel2 = new (0.863f, 0.504f, 0.242f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_LindenTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.930f, 0.600f, 0.008f, 1.000f), m_Channel1 = new (0.633f, 0.320f, 0.016f, 1.000f), m_Channel2 = new (0.852f, 0.297f, 0.004f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_LondonPlaneTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.922f, 0.535f, 0.106f, 1.000f), m_Channel1 = new (0.871f, 0.676f, 0.168f, 1.000f), m_Channel2 = new (0.543f, 0.188f, 0.070f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "NA_LondonPlaneTree01"), m_Season = FoliageUtils.Season.Winter }, new () { m_Channel0 = new (0.680f, 0.379f, 0.051f, 1.000f), m_Channel1 = new (0.605f, 0.340f, 0.063f, 1.000f), m_Channel2 = new (0.508f, 0.199f, 0.032f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "OakTree01"), m_Season = FoliageUtils.Season.Autumn }, new () { m_Channel0 = new (0.957f, 0.356f, 0.113f, 1.000f), m_Channel1 = new (0.957f, 0.266f, 0.125f, 1.000f), m_Channel2 = new (0.961f, 0.469f, 0.281f, 1.000f) } },
            { new () { m_PrefabID = new ("StaticObjectPrefab", "OakTree01"), m_Season = FoliageUtils.Season.Winter }, new () { m_Channel0 = new (0.934f, 0.25f, 0.109f, 1.000f), m_Channel1 = new (0.785f, 0.313f, 0.137f, 1.000f), m_Channel2 = new (0.902f, 0.148f, 0.059f, 1.000f) } },
        };

        private PrefabSystem m_PrefabSystem;
        private EntityQuery m_PlantPrefabQuery;
        private TreeControllerSettings.ColorVariationSetYYTC m_ColorVariationSet;
        private bool m_Run = true;
        private Dictionary<TreeSeasonIdentifier, Game.Rendering.ColorSet> m_VanillaColorSets;
        private ClimateSystem m_ClimateSystem;
        private FoliageUtils.Season m_Season = FoliageUtils.Season.Spring;
        private ILog m_Log;
        private string m_ContentFolder;
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_PlantQuery;

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

        private Dictionary<TreeSeasonIdentifier, ColorSet> YenyangsColorSets => m_YenyangsColorSets;

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
            m_ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "Mods_Yenyang_Tree_Controller", "FoliageColorData", "Custom");
            System.IO.Directory.CreateDirectory(m_ContentFolder);
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
            if (mode.IsGame())
            {
                Run = true;
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

                for (int i = 0; i <= 3; i++)
                {
                    if (!EntityManager.TryGetBuffer(subMeshBuffer[i].m_SubMesh, isReadOnly: false, out DynamicBuffer<ColorVariation> colorVariationBuffer))
                    {
                        continue;
                    }

                    PrefabBase prefabBase = m_PrefabSystem.GetPrefab<PrefabBase>(e);
                    PrefabID prefabID = prefabBase.GetPrefabID();

                    if (colorVariationBuffer.Length < 4 || prefabBase.name.ToLower().Contains("palm"))
                    {
                        buffer.AddComponent<Evergreen>(e);
                        continue;
                    }

                    for (int j = 0; j < 4; j++)
                    {
#if VERBOSE
                        m_Log.Verbose($"{prefabID.GetName()} {(TreeState)(int)Math.Pow(2, i - 1)} {(FoliageUtils.Season)j} {colorVariationBuffer[j].m_ColorSet.m_Channel0} {colorVariationBuffer[j].m_ColorSet.m_Channel1} {colorVariationBuffer[j].m_ColorSet.m_Channel2}");
#endif
                        TreeSeasonIdentifier treeSeasonIdentifier = new ()
                        {
                            m_PrefabID = prefabID,
                            m_Season = (FoliageUtils.Season)j,
                        };

                        ColorVariation currentColorVariation = colorVariationBuffer[j];
                        if (!m_VanillaColorSets.ContainsKey(treeSeasonIdentifier))
                        {
                            m_VanillaColorSets.Add(treeSeasonIdentifier, currentColorVariation.m_ColorSet);
                            TrySaveDefaultCustomColorSet(currentColorVariation.m_ColorSet, treeSeasonIdentifier.m_PrefabID, treeSeasonIdentifier.m_Season);
                        }

                        bool setToDifferentSeason = false;
                        if ((TreeControllerMod.Instance.Settings.UseDeadModelDuringWinter && m_Season == FoliageUtils.Season.Spring && treeSeasonIdentifier.m_Season == FoliageUtils.Season.Winter) || TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Spring)
                        {
                            treeSeasonIdentifier.m_Season = FoliageUtils.Season.Spring;
                            setToDifferentSeason = true;
                        }
                        else if (TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Autumn)
                        {
                            treeSeasonIdentifier.m_Season = FoliageUtils.Season.Autumn;
                            setToDifferentSeason = true;
                            currentColorVariation.m_ColorSet = colorVariationBuffer[2].m_ColorSet;
                            if (!m_VanillaColorSets.ContainsKey(treeSeasonIdentifier))
                            {
                                m_VanillaColorSets.Add(treeSeasonIdentifier, currentColorVariation.m_ColorSet);
                            }
                        }

                        if (TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Custom)
                        {
                            if (TryLoadCustomColorSet(treeSeasonIdentifier.m_PrefabID, treeSeasonIdentifier.m_Season, out CustomColorSet customColorSet))
                            {
                                currentColorVariation.m_ColorSet = customColorSet.ColorSet;
                                colorVariationBuffer[j] = currentColorVariation;
                                m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} Imported Colorset for {prefabID} in {treeSeasonIdentifier.m_Season}");
                            }
                        }
                        else if (TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Yenyangs && YenyangsColorSets.ContainsKey(treeSeasonIdentifier))
                        {
                            currentColorVariation.m_ColorSet = YenyangsColorSets[treeSeasonIdentifier];
                            colorVariationBuffer[j] = currentColorVariation;
                            m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} Changed Colorset for {prefabID} in {treeSeasonIdentifier.m_Season}");
                        }
                        else if (TreeControllerMod.Instance.Settings.ColorVariationSet == TreeControllerSettings.ColorVariationSetYYTC.Vanilla && m_VanillaColorSets.ContainsKey(treeSeasonIdentifier))
                        {
                            currentColorVariation.m_ColorSet = m_VanillaColorSets[treeSeasonIdentifier];
                            colorVariationBuffer[j] = currentColorVariation;
                            m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} Reset Colorset for {prefabID} in {treeSeasonIdentifier.m_Season}");
                        }
                        else if (setToDifferentSeason)
                        {
                            currentColorVariation.m_ColorSet = m_VanillaColorSets[treeSeasonIdentifier];
                            colorVariationBuffer[j] = currentColorVariation;
                            m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(OnUpdate)} Reset Colorset {TreeControllerMod.Instance.Settings.ColorVariationSet} for {prefabID} in {(FoliageUtils.Season)j}");
                        }
                    }
                }
            }

            plantPrefabEntities.Dispose();
            m_Run = false;
            m_ColorVariationSet = TreeControllerMod.Instance.Settings.ColorVariationSet;

            buffer.AddComponent<BatchesUpdated>(m_PlantQuery, EntityQueryCaptureMode.AtPlayback);
        }

        private bool TrySaveDefaultCustomColorSet(ColorSet colorSet, PrefabID prefabID, FoliageUtils.Season season)
        {
            string foliageColorDataFilePath = Path.Combine(m_ContentFolder, $"{prefabID.GetName()}-{(int)season}{season}.xml");

            if (!File.Exists(foliageColorDataFilePath))
            {
                return TrySaveCustomColorSet(colorSet, prefabID, season);
            }

            return false;
        }

        private bool TrySaveCustomColorSet(ColorSet colorSet, PrefabID prefabID, FoliageUtils.Season season)
        {
            string foliageColorDataFilePath = Path.Combine(m_ContentFolder, $"{prefabID.GetName()}-{(int)season}{season}.xml");
            CustomColorSet customColorSet = new CustomColorSet(colorSet, prefabID, season);

            try
            {
                XmlSerializer serTool = new XmlSerializer(typeof(CustomColorSet)); // Create serializer
                using (System.IO.FileStream file = System.IO.File.Create(foliageColorDataFilePath)) // Create file
                {
                    serTool.Serialize(file, customColorSet); // Serialize whole properties
                }

                return true;
            }
            catch (Exception ex)
            {
                m_Log.Warn($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(TrySaveCustomColorSet)} Could not save values for {prefabID}. Encountered exception {ex}");
                return false;
            }
        }

        private bool TryLoadCustomColorSet(PrefabID prefabID, FoliageUtils.Season season, out CustomColorSet result)
        {
            string foliageColorDataFilePath = Path.Combine(m_ContentFolder, $"{prefabID.GetName()}-{(int)season}{season}.xml");
            result = default;
            if (File.Exists(foliageColorDataFilePath))
            {
                try
                {
                    XmlSerializer serTool = new XmlSerializer(typeof(CustomColorSet)); // Create serializer
                    using System.IO.FileStream readStream = new System.IO.FileStream(foliageColorDataFilePath, System.IO.FileMode.Open); // Open file
                    result = (CustomColorSet)serTool.Deserialize(readStream); // Des-serialize to new Properties


                    m_Log.Debug($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(TryLoadCustomColorSet)} loaded repository for {prefabID}.");
                    return true;
                }
                catch (Exception ex)
                {
                    m_Log.Warn($"{nameof(ReloadFoliageColorDataSystem)}.{nameof(TryLoadCustomColorSet)} Could not get default values for Set {prefabID}. Encountered exception {ex}");
                    return false;
                }
            }

            return false;
        }

        private struct TreeSeasonIdentifier
        {
            public PrefabID m_PrefabID;
            public FoliageUtils.Season m_Season;
        }
    }
}
