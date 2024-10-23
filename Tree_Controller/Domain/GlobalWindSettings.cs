// <copyright file="GlobalWindSettings.cs" company="Yenyangs Mods. MIT License">
// Copyright (c) Yenyangs Mods. MIT License. All rights reserved.
// </copyright>

namespace Tree_Controller.Domain
{
    using UnityEngine.Rendering;

    public class GlobalWindSettings
    {
        public ClampedFloatParameter globalStrengthScale = new ClampedFloatParameter(1f, 0f, 3f);
        public ClampedFloatParameter globalStrengthScale2 = new ClampedFloatParameter(1f, 0f, 3f);
        public ClampedFloatParameter windDirection = new ClampedFloatParameter(65f, 0f, 360f);
        public ClampedFloatParameter windDirectionVariance = new ClampedFloatParameter(25f, 0f, 90f);
        public ClampedFloatParameter windDirectionVariancePeriod = new ClampedFloatParameter(15f, 0.01f, 120f);
        public ClampedFloatParameter interpolationDuration = new ClampedFloatParameter(0.5f, 0.0001f, 5f);
    }
}
