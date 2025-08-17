float4 Billboard(float4 vertex, float3 _WorldSpaceCameraPos, float4x4 UNITY_MATRIX_VP, float4x4 unity_ObjectToWorld)
{
    // Remove a rotação do objeto para que ele sempre fique de frente para a câmera
    float3 vpos = mul((float3x3)unity_ObjectToWorld, vertex.xyz);
    float4 worldPos = float4(unity_ObjectToWorld._m03_m13_m23, 1.0);

    // Vetor para a câmera
    float3 forward = -normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
    float3 right = normalize(cross(float3(0, 1, 0), forward)); // Eixo Y como "up"
    float3 up = normalize(cross(forward, right));

    vpos = worldPos.xyz + right * vpos.x + up * vpos.y;
    return mul(UNITY_MATRIX_VP, float4(vpos, 1.0));
}
