#ifndef OUTLINE_SHADER_INCLUDED
#define OUTLINE_SHADER_INCLUDED

static float2 sampleOffsets[8] = {
    float2(1, 1), float2(1, -1), float2(-1, 1), float2(-1, -1),
    float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1)
};

float SampleDepth(float2 uv, float thickness)
{
    float depth = 0;
    for (int i = 0; i < 8; i++)
    {
        depth += SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv + sampleOffsets[i] * thickness).r;
    }
    return depth / 8;
}

float SampleNormal(float2 uv, float thickness)
{
    float3 normal = 0;
    for (int i = 0; i < 8; i++)
    {
        normal += SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv + sampleOffsets[i] * thickness).rgb;
    }
    return length(normal / 8);
}

float Outline_float(float2 uv, float thickness, float depthSensitivity, float normalSensitivity)
{
    float depth = SampleDepth(uv, thickness);
    float normal = SampleNormal(uv, thickness);

    float depthDifference = abs(depth - SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv).r);
    float normalDifference = abs(length(SHADERGRAPH_SAMPLE_SCENE_NORMAL(uv).rgb) - normal);

    float outline = saturate(depthDifference * depthSensitivity + normalDifference * normalSensitivity);
    return outline;
}

void Outline_float(float2 uv, float thickness, float depthSensitivity, float normalSensitivity, out float Out)
{
    Out = Outline_float(uv, thickness, depthSensitivity, normalSensitivity);
}

#endif