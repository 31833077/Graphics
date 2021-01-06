using System;
using UnityEngine.Serialization;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEngine.Rendering.HighDefinition
{
    public enum HDLensFlareType
    {
        DataDriven,
        FullScreen
    }

    [Serializable]
    public sealed class HDLensFlareTypeParameter : VolumeParameter<HDLensFlareType> { };

    /// <summary>
    /// A volume component that holds settings for the Lens Flare effect.
    /// </summary>
    [Serializable, VolumeComponentMenu("Post-processing/HD Lens Flare")]
    [HelpURL(Documentation.baseURL + Documentation.version + Documentation.subURL + "Post-Processing-HDLensFlare" + Documentation.endURL)]
    public sealed class HDLensFlare : VolumeComponent, IPostProcessComponent
    {
        /// <summary>
        /// Set the level of brightness to filter out pixels under this level. This value is expressed in gamma-space. A value above 0 will disregard energy conservation rules.
        /// </summary>
        public BoolParameter enable = new BoolParameter(false);

        public HDLensFlareTypeParameter type = new HDLensFlareTypeParameter();

        /// <summary>
        /// Set the level of brightness to filter out pixels under this level. This value is expressed in gamma-space. A value above 0 will disregard energy conservation rules.
        /// </summary>
        [Tooltip("Set the level of brightness to filter out pixels under this level. This value is expressed in gamma-space. A value above 0 will disregard energy conservation rules.")]
        public MinFloatParameter threshold = new MinFloatParameter(0f, 0f);

        /// <summary>
        /// Controls the strength of the LensFlare filter.
        /// </summary>
        [Tooltip("Controls the strength of the LensFlare filter.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        /// <summary>
        /// Tells if the effect needs to be rendered or not.
        /// </summary>
        /// <returns><c>true</c> if the effect should be rendered, <c>false</c> otherwise.</returns>
        public bool IsActive()
        {
            return intensity.value > 0f;
        }
    }
}
