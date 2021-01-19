#if !defined(SHADOW_PROJECT_VERTEX)
#define SHADOW_PROJECT_VERTEX

struct Attributes\
{\
    float3 vertex : POSITION;\
    float4 tangent: TANGENT;\
    float4 extrusion : COLOR;\
};\

struct Varyings\
{\
    float4 vertex : SV_POSITION;\
};\

uniform float3 _LightPos;\
uniform float  _ShadowRadius;


Varyings ProjectShadow(Attributes v)
{
    Varyings o;
    float3 vertexWS = TransformObjectToWorld(v.vertex);  // This should be in world space
    float3 lightDir = _LightPos - vertexWS;
    lightDir.z = 0;

    // Start of code to see if this point should be extruded
    float3 lightDirection = normalize(lightDir);

    float3 endpoint = vertexWS + (_ShadowRadius * -lightDirection);

    float3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
    float sharedShadowTest = saturate(ceil(dot(lightDirection, worldTangent)));

    // Start of code to calculate offset
    float3 vertexWS0 = TransformObjectToWorld(float3(v.extrusion.xy, 0));
    float3 shadowDir0 = vertexWS0 - _LightPos;
    shadowDir0.z = 0;
    shadowDir0 = normalize(shadowDir0);

    float3 shadowDir = normalize(shadowDir0);

    float3 sharedShadowOffset = sharedShadowTest * _ShadowRadius * shadowDir;

    float3 position;
    position = vertexWS + sharedShadowOffset;

    o.vertex = TransformWorldToHClip(position);

    return o;
}

#endif
