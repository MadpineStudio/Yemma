
float CalculateLight(float3 lightDir, float3 N, float toonFactor)
{
    float lightGradient = dot(lightDir, N);
    return step(toonFactor, lightGradient);
}
void toon_float(float3 N, float3 posOS, float3 lightDir, float toonFactor, out float Out)
{
    float3 NWS = mul(unity_ObjectToWorld, float4(N, 0.));
    float toon = CalculateLight(-lightDir, NWS, toonFactor);
    
    Out = toon;
}