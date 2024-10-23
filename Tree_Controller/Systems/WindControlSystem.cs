// <copyright file="WindControlSystem.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace TreeController.TreeWindsController
{
    using Colossal.Logging;
    using Game;
    using Game.Rendering;
    using Tree_Controller;
    using Tree_Controller.Domain;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// A system to control the swaying of trees and vegetation from the wind.
    /// </summary>
    public partial class WindControlSystem : GameSystemBase
    {
        private bool m_WindEnabled;
        private GlobalWindSettings m_GlobalSettings = new GlobalWindSettings();
        private AnimationCurveParameter m_StrengthVarianceAnimation;
        private ClampedFloatParameter m_Strength;
        private ClampedFloatParameter m_StrengthVariance;
        private ClampedFloatParameter m_StrengthVariancePeriod;
        private ILog m_Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindControlSystem"/> class.
        /// </summary>
        public WindControlSystem()
        {
        }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            base.OnCreate();
            m_Log = TreeControllerMod.Instance.Logger;
            m_Log.Info($"{nameof(WindControlSystem)}.{nameof(OnCreate)}");
            m_Strength = new ClampedFloatParameter(0.25f, 0f, 1f);
            m_StrengthVariance = new ClampedFloatParameter(0f, 0f, 1f);
            m_StrengthVariancePeriod = new ClampedFloatParameter(25f, 0.01f, 120f);
            m_StrengthVarianceAnimation = new AnimationCurveParameter(new AnimationCurve());

            m_Log.Info("WindControlSystem Initialized.");
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_WindEnabled)
            {
                // Apply the current wind settings from WindControlSystem to the game's volume component
                ApplyWindSettings();
            }
            else
            {
                // If wind is disabled, call DisableAllWind
                DisableAllWind();
            }

            // Optionally, evaluate wind gusts using the time
            var windVolumeComponent = VolumeManager.instance.stack.GetComponent<WindVolumeComponent>();
            if (windVolumeComponent != null)
            {
                float time = (float)SystemAPI.Time.ElapsedTime;
                windVolumeComponent.windGustStrengthControl.value.Evaluate(time);
                windVolumeComponent.windTreeGustStrengthControl.value.Evaluate(time);
            }
        }

        private float ClampedValueRatio(ClampedFloatParameter cfp, float ratio)
        {
            return cfp.min + (ratio * (cfp.max - cfp.min));
        }

        /// <summary>
        /// Applies Wind Settings if enabled in settings.
        /// </summary>
        private void ApplyWindSettings()
        {
            if (!m_WindEnabled)
            {
                DisableAllWind();
                return;
            }

            var windVolumeComponent = VolumeManager.instance.stack.GetComponent<WindVolumeComponent>();
            if (windVolumeComponent != null)
            {
                // Calculate wind strength with variance using animation curve
                float minStrength = m_Strength.value - ((m_StrengthVariance.value / 2f) * m_Strength.value);
                float maxStrength = m_Strength.value + ((m_StrengthVariance.value / 2f) * m_Strength.value);

                m_StrengthVarianceAnimation.value = new AnimationCurve(
                    new Keyframe(0f, minStrength),
                    new Keyframe(m_StrengthVariancePeriod.value, maxStrength),
                    new Keyframe(2 * m_StrengthVariancePeriod.value, minStrength)
                );

                float time = (float)SystemAPI.Time.DeltaTime;
                float strengthAnim = m_StrengthVarianceAnimation.value.Evaluate(time % (2 * m_StrengthVariancePeriod.value));

                m_GlobalSettings.globalStrengthScale.value = ClampedValueRatio(m_GlobalSettings.globalStrengthScale, strengthAnim);

                // Apply wind settings to volume component
                windVolumeComponent.windGlobalStrengthScale.Override(m_GlobalSettings.globalStrengthScale.value);
                windVolumeComponent.windGlobalStrengthScale2.Override(m_GlobalSettings.globalStrengthScale2.value);
                windVolumeComponent.windDirection.Override(m_GlobalSettings.windDirection.value);
                windVolumeComponent.windDirectionVariance.Override(m_GlobalSettings.windDirectionVariance.value);
                windVolumeComponent.windDirectionVariancePeriod.Override(m_GlobalSettings.windDirectionVariancePeriod.value);
                windVolumeComponent.windParameterInterpolationDuration.Override(m_GlobalSettings.interpolationDuration.value);
            }
            else
            {
                m_Log.Warn("WindVolumeComponent not found.");
            }
        }

        /// <summary>
        /// Disables all wind.
        /// </summary>
        private void DisableAllWind()
        {
            var windVolumeComponent = VolumeManager.instance.stack.GetComponent<WindVolumeComponent>();
            if (windVolumeComponent != null)
            {
                // Disable all wind settings
                windVolumeComponent.windGlobalStrengthScale.Override(0f);
                windVolumeComponent.windGlobalStrengthScale2.Override(0f);
                windVolumeComponent.windBaseStrength.Override(0f);
                windVolumeComponent.windGustStrength.Override(0f);
                windVolumeComponent.windTreeBaseStrength.Override(0f);
                windVolumeComponent.windTreeGustStrength.Override(0f);
                windVolumeComponent.windTreeFlutterStrength.Override(0f);
                m_Log.Debug("All wind settings disabled.");
            }
        }
    }
}
