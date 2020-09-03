using System;
using UnityEngine.Rendering.HighDefinition.Attributes;

namespace UnityEngine.Rendering.HighDefinition
{
    /// <summary>Engine lighting property.</summary>
    public enum LightingProperty
    {
        /// <summary>No debug output.</summary>
        None = 0,
        /// <summary>Render only diffuse.</summary>
        DiffuseOnly,
        /// <summary>Render only specular.</summary>
        SpecularOnly,
        /// <summary>Render only direct diffuse.</summary>
        DirectDiffuseOnly,
        /// <summary>Render only direct specular.</summary>
        DirectSpecularOnly,
        /// <summary>Render only indirect diffuse.</summary>
        IndirectDiffuseOnly,
        /// <summary>Render only reflection.</summary>
        ReflectionOnly,
        /// <summary>Render only refraction.</summary>
        RefractionOnly,
        /// <summary>Render only emissive.</summary>
        EmissiveOnly
    }

    /// <summary>Output a specific debug mode.</summary>
    public enum DebugFullScreen
    {
        /// <summary>No debug output.</summary>
        None,
        /// <summary>Depth buffer.</summary>
        Depth,
        WorldSpacePosition,
        WorldSpaceVertexNormal,
        /// <summary>Screen space ambient occlusion buffer.</summary>
        ScreenSpaceAmbientOcclusion,
        /// <summary>Motion vectors buffer.</summary>
        MotionVectors
    }

    /// <summary>Use this request to define how to render an AOV.</summary>
    public unsafe struct AOVRequest
    {
        /// <summary>Default settings.</summary>
        [Obsolete("Since 2019.3, use AOVRequest.NewDefault() instead.")]
        public static readonly AOVRequest @default = default;
        /// <summary>Default settings.</summary>
        /// <returns></returns>
        public static AOVRequest NewDefault() => new AOVRequest
        {
            m_MaterialProperty = MaterialSharedProperty.None,
            m_LightingProperty = LightingProperty.None,
            m_DebugFullScreen = DebugFullScreen.None,
            m_LightFilterProperty = DebugLightFilterMode.None,
            m_RequestMode = Camera.RenderRequestMode.None
        };



        Camera.RenderRequestMode m_RequestMode;
        MaterialSharedProperty m_MaterialProperty;
        LightingProperty m_LightingProperty;
        DebugLightFilterMode m_LightFilterProperty;
        DebugFullScreen m_DebugFullScreen;

        AOVRequest* thisPtr
        {
            get
            {
                fixed (AOVRequest* pThis = &this)
                    return pThis;
            }
        }

        /// <summary>Create a new instance by copying values from <paramref name="other"/>.</summary>
        /// <param name="other"></param>
        public AOVRequest(AOVRequest other)
        {
            m_MaterialProperty = other.m_MaterialProperty;
            m_LightingProperty = other.m_LightingProperty;
            m_DebugFullScreen = other.m_DebugFullScreen;
            m_LightFilterProperty = other.m_LightFilterProperty;
            m_RequestMode = other.m_RequestMode;
        }

        /// <summary>State the property to render. In case of several SetFullscreenOutput chained call, only last will be used.</summary>
        /// <param name="materialProperty">The property to render.</param>
        /// <returns>A ref return to chain calls.</returns>
        public ref AOVRequest SetFullscreenOutput(MaterialSharedProperty materialProperty)
        {
            m_MaterialProperty = materialProperty;
            return ref *thisPtr;
        }

        /// <summary>State the property to render. In case of several SetFullscreenOutput chained call, only last will be used.</summary>
        /// <param name="lightingProperty">The property to render.</param>
        /// <returns>A ref return to chain calls.</returns>
        public ref AOVRequest SetFullscreenOutput(LightingProperty lightingProperty)
        {
            m_LightingProperty = lightingProperty;
            return ref *thisPtr;
        }

        /// <summary>State the property to render. In case of several SetFullscreenOutput chained call, only last will be used.</summary>
        /// <param name="debugFullScreen">The property to render.</param>
        /// <returns>A ref return to chain calls.</returns>
        public ref AOVRequest SetFullscreenOutput(DebugFullScreen debugFullScreen)
        {
            m_DebugFullScreen = debugFullScreen;
            return ref *thisPtr;
        }

        /// <summary>Set the light filter to use.</summary>
        /// <param name="filter">The light filter to use</param>
        /// <returns>A ref return to chain calls.</returns>
        public ref AOVRequest SetLightFilter(DebugLightFilterMode filter)
        {
            m_LightFilterProperty = filter;
            return ref *thisPtr;
        }

        public ref AOVRequest SetRenderRequestMode(Camera.RenderRequestMode mode)
        {
            m_RequestMode = mode;
            return ref *thisPtr;
        }

        /// <summary>
        /// Populate the debug display settings with the AOV data.
        /// </summary>
        /// <param name="debug">The debug display settings to fill.</param>
        public void FillDebugData(DebugDisplaySettings debug)
        {
            debug.SetDebugViewCommonMaterialProperty(m_MaterialProperty);

            switch (m_LightingProperty)
            {
                case LightingProperty.DiffuseOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.DiffuseLighting);
                    break;
                case LightingProperty.SpecularOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.SpecularLighting);
                    break;
                case LightingProperty.DirectDiffuseOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.DirectDiffuseLighting);
                    break;
                case LightingProperty.DirectSpecularOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.DirectSpecularLighting);
                    break;
                case LightingProperty.IndirectDiffuseOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.IndirectDiffuseLighting);
                    break;
                case LightingProperty.ReflectionOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.ReflectionLighting);
                    break;
                case LightingProperty.RefractionOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.RefractionLighting);
                    break;
                case LightingProperty.EmissiveOnly:
                    debug.SetDebugLightingMode(DebugLightingMode.EmissiveLighting);
                    break;
                default:
                {
                    debug.SetDebugLightingMode(DebugLightingMode.None);
                    break;
                }
            }

            debug.SetDebugLightFilterMode(m_LightFilterProperty);

            switch (m_DebugFullScreen)
            {
                case DebugFullScreen.None:
                    debug.SetFullScreenDebugMode(FullScreenDebugMode.None);
                    break;
                case DebugFullScreen.Depth:
                    debug.SetFullScreenDebugMode(FullScreenDebugMode.DepthPyramid);
                    break;
                case DebugFullScreen.WorldSpacePosition:
                    debug.SetFullScreenDebugMode(FullScreenDebugMode.WorldSpacePosition);
                    break;
                case DebugFullScreen.WorldSpaceVertexNormal:
                    debug.SetDebugViewVarying(DebugViewVarying.VertexNormalWS);
                    break;
                case DebugFullScreen.ScreenSpaceAmbientOcclusion:
                    debug.SetFullScreenDebugMode(FullScreenDebugMode.ScreenSpaceAmbientOcclusion);
                    break;
                case DebugFullScreen.MotionVectors:
                    debug.SetFullScreenDebugMode(FullScreenDebugMode.MotionVectors);
                    break;
                default:
                    throw new ArgumentException("Unknown DebugFullScreen");
            }


            switch (m_RequestMode)
            {
                case Camera.RenderRequestMode.None:
                    break;
                case Camera.RenderRequestMode.ObjectIdR:
                    //@TODO:
                    break;
                case Camera.RenderRequestMode.DepthR:
                    debug.SetFullScreenDebugMode(FullScreenDebugMode.DepthPyramid);
                    break;
                case Camera.RenderRequestMode.WorldNormalVertexRGB:
                    debug.SetDebugViewVarying(DebugViewVarying.VertexNormalWS);
                    break;
                case Camera.RenderRequestMode.WorldPositionRGB:
                    debug.SetFullScreenDebugMode(FullScreenDebugMode.WorldSpacePosition);
                    break;
                case Camera.RenderRequestMode.EntityIdRG:
                    //@TODO:
                    break;
                case Camera.RenderRequestMode.BaseColorRGB:
                    //@TODO: Validate that this works correctly in specular & metallic mode
                    debug.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.Albedo);
                    break;
                case Camera.RenderRequestMode.SpecularColorRGB:
                    debug.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.Specular);
                    break;
                case Camera.RenderRequestMode.MetallicR:
                    debug.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.Metal);
                    break;
                case Camera.RenderRequestMode.EmissionRGB:
                    debug.SetDebugLightingMode(DebugLightingMode.EmissiveLighting);
                    break;
                case Camera.RenderRequestMode.WorldNormalPerPixelRGB:
                    debug.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.Normal);
                    break;
                case Camera.RenderRequestMode.SmoothnessR:
                    debug.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.Smoothness);
                    break;
                case Camera.RenderRequestMode.OcclusionR:
                    debug.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.AmbientOcclusion);
                    break;
                case Camera.RenderRequestMode.DiffuseColorRGBA:
                    //@TODO: not correct... Need to perform specular / albedo conversion
                    debug.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.Albedo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(m_RequestMode), m_RequestMode, null);
            }
        }
    }
}

