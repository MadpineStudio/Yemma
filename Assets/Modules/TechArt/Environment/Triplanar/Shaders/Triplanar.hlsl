void triplanar_float(float3 posWS, float3 normalWS, float hardness, UnityTexture2D texA, UnityTexture2D texB,
UnityTexture2D noiseTex, float GrassScale, float RockScale, float NoiseScale, float3 baseColor, float3 color, out float3 result)
{
    float3 absNormal = abs(normalWS);
    float3 weights = pow(absNormal, hardness);
    weights /= (weights.x + weights.y + weights.z);

    float2 uvX = posWS.yz * RockScale;
    float2 uvY = posWS.xz * GrassScale;
    float2 uvZ = posWS.xy * RockScale;

    float2 noiseX = tex2D(noiseTex, uvX * NoiseScale).rg - 0.5;
    float2 noiseY = tex2D(noiseTex, uvY * NoiseScale).rg - 0.5;
    float2 noiseZ = tex2D(noiseTex, uvZ * NoiseScale).rg - 0.5;

    uvX += noiseX;
    uvY += noiseY;
    uvZ += noiseZ;

    float rockSampleX = tex2D(texA, uvX).xyz;
    float rockSampleZ = tex2D(texA, uvZ).xyz;
    float4 grassSample = tex2D(texB, uvY);

    // float3 sideColorX = lerp(baseColor, color, rockSampleX);
    // float3 sideColorZ = lerp(baseColor, color, rockSampleZ);

    result = weights.x * rockSampleX + weights.y * grassSample.rgb + weights.z * rockSampleZ;
    // result = weights.x * sideColorX + weights.y * grassSample.rgb + weights.z * sideColorZ;

}
