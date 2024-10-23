using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using JetBrains.Annotations;

namespace TreeWindsController
{
    [FileLocation(nameof(TreeWindsController))]
    [SettingsUITabOrder(WindTab)]
    [SettingsUIGroupOrder(DisableWinds, GlobalWindGroup, GrassBaseGroup, GrassGustGroup, GrassFlutterGroup, TreeBaseGroup, TreeGustGroup, TreeFlutterGroup)]
    [SettingsUIShowGroupName(DisableWinds, GlobalWindGroup, GrassBaseGroup, GrassGustGroup, GrassFlutterGroup, TreeBaseGroup, TreeGustGroup, TreeFlutterGroup)]
    public class Setting : ModSetting
    {
        //Tabs
        public const string WindTab = "Wind";

        //Groups
        public const string GlobalWindGroup = "Global Wind Settings";
        public const string GrassBaseGroup = "Grass Base Settings";
        public const string GrassGustGroup = "Grass Gust Settings";
        public const string GrassFlutterGroup = "Grass Flutter Settings";
        public const string TreeBaseGroup = "Tree Base Settings";
        public const string TreeGustGroup = "Tree Gust Settings";
        public const string TreeFlutterGroup = "Tree Flutter Settings";

        public const string DisableWinds = "DisableWinds";

        public bool _toggleWind;
        
        public Setting(IMod mod) : base(mod)
        {
            WindControlSystem.Instance.Initialize();
        }

        [SettingsUISection(WindTab, DisableWinds)]
        public bool WindEnabled
        {
            get => _toggleWind;
            set
            {
                _toggleWind = value;
                

                WindControlSystem.Instance.windEnabled = value;
                
            }
        }



        // Global Wind Settings
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 3f, step = 0.1f, unit = Unit.kPercentage)]
        public float WindGlobalStrength
        {
            get => WindControlSystem.Instance.globalSettings.globalStrengthScale.value;
            set
            {
                if (!WindControlSystem.Instance.isInitialized)
                {
                    WindControlSystem.Instance.Initialize(); // Ensure initialization before applying settings
                }

                WindControlSystem.Instance.globalSettings.globalStrengthScale.value = value;
               
            }
        }

        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 3f, step = 0.1f, unit = Unit.kPercentage)]
        public float WindGlobalStrength2
        {
            get => WindControlSystem.Instance.globalSettings.globalStrengthScale2.value;
            set
            {
                if (!WindControlSystem.Instance.isInitialized)
                {
                    WindControlSystem.Instance.Initialize(); // Ensure initialization before applying settings
                }

                WindControlSystem.Instance.globalSettings.globalStrengthScale2.value = value;
                
            }
        }

        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 360f, step = 1f, unit = Unit.kAngle)]
        public float WindDirection
        {
            get => WindControlSystem.Instance.globalSettings.windDirection.value;
            set
            {
                if (!WindControlSystem.Instance.isInitialized)
                {
                    WindControlSystem.Instance.Initialize(); // Ensure initialization before applying settings
                }

                WindControlSystem.Instance.globalSettings.windDirection.value = value;
                
            }
        }

        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 90f, step = 1f, unit = Unit.kAngle)]
        public float WindDirectionVariance
        {
            get => WindControlSystem.Instance.globalSettings.windDirectionVariance.value;
            set
            {
                if (!WindControlSystem.Instance.isInitialized)
                {
                    WindControlSystem.Instance.Initialize(); // Ensure initialization before applying settings
                }

                WindControlSystem.Instance.globalSettings.windDirectionVariance.value = value;
               
            }
        }

        // Repeat this pattern for all other sliders (e.g., GrassWindBaseStrength, GrassWindGustStrength, TreeWindBaseStrength, etc.)


        // Wind Direction Variance Period
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0f, max = 120f, step = 0.1f, unit = Unit.kPercentage)]
        public float WindDirectionVariancePeriod
        {
            get => WindControlSystem.Instance.globalSettings.windDirectionVariancePeriod.value;
            set
            {
                if (!WindControlSystem.Instance.isInitialized)
                {
                    WindControlSystem.Instance.Initialize();
                }

                WindControlSystem.Instance.globalSettings.windDirectionVariancePeriod.value = value;
                
            }
        }

        // Wind Interpolation Duration
        [SettingsUISection(WindTab, GlobalWindGroup)]
        [SettingsUISlider(min = 0.0001f, max = 5f, step = 0.01f, unit = Unit.kPercentage)]
        public float WindInterpolationDuration
        {
            get => WindControlSystem.Instance.globalSettings.interpolationDuration.value;
            set
            {
                if (!WindControlSystem.Instance.isInitialized)
                {
                    WindControlSystem.Instance.Initialize();
                }

                WindControlSystem.Instance.globalSettings.interpolationDuration.value = value;
               
            }
        }

       


        public override void SetDefaults()
        {
            WindControlSystem.Instance.windEnabled = false;
            WindControlSystem.Instance.grassSettings.baseStrength.value = 0.5f;
            WindControlSystem.Instance.grassSettings.gustStrength.value = 0.5f;
            WindControlSystem.Instance.treeSettings.baseStrength.value = 0.5f;
            WindControlSystem.Instance.treeSettings.gustStrength.value = 0.5f;
            _toggleWind = false;
        }
    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Tree Winds Controller" },
                { m_Setting.GetOptionTabLocaleID(Setting.WindTab), "Wind" },
                { m_Setting.GetOptionGroupLocaleID(Setting.DisableWinds), "Toggle Wind" },
                { m_Setting.GetOptionGroupLocaleID(Setting.GlobalWindGroup), "Global Wind Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.GrassBaseGroup), "Grass Base Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.GrassGustGroup), "Grass Gust Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.TreeBaseGroup), "Tree Base Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.TreeGustGroup), "Tree Gust Settings" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WindEnabled)), "Toggle Wind" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WindEnabled)), "Toggles the wind (uncheck to toggle wind off and check to toggle wind on)" },


                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WindGlobalStrength)), "Global Wind Strength" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WindGlobalStrength)), "Controls the global wind strength" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WindGlobalStrength2)), "Global Wind Strength 2" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WindGlobalStrength2)), "Controls the global wind strength" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WindDirection)), "Global Wind Direction" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WindDirection)), "Controls the global wind direction" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WindDirectionVariance)), "Global Wind Direction variance" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WindDirectionVariance)), "Controls the global wind direction variance" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WindDirectionVariancePeriod)), "Global Wind Direction variance period" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WindDirectionVariancePeriod)), "Controls the global wind direction variance period" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WindInterpolationDuration)), "Global Wind Interpolation Duration" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WindInterpolationDuration)), "Controls the global wind interpolation duration" },


               
            };
        }

        public void Unload()
        {
        }
    }
}
