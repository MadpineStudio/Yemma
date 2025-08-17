float CalculateShadowToonMask(float headYEuler, float mainLightY, float Smoothness, float2 uv, UnityTexture2D _ShadowTex)
{
    float d = (headYEuler - mainLightY) / (3.14159 * 2.0);
    float f = frac(d);
    float isLess = 1.0 - step(.5, f);
    float isGreater = step(.5, f);
    float sum = isLess - isGreater;
    float f2 = abs(isGreater - f);
    float2 newUV = uv * float2(sum, 1.0);
    float col = clamp(tex2D(_ShadowTex, newUV).r, 0.0, 1.0);

    float t = abs(sin(_Time.y));

    col = 1.- step(f2, col);

    return col;
}
void CalculateColors_float(float3 LightColor, float3 ShadowColor, float gradient, out float3 Color)
{
    Color = lerp(ShadowColor,  LightColor, gradient);
}
void ToonFace_float(float headY, float mainLightY, float Threshold, UnityTexture2D ShadowTex, float2 uv, out float Out)
{
    Out = CalculateShadowToonMask(headY, mainLightY, Threshold, uv, ShadowTex);
}
