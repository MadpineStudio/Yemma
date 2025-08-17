// Obtem a uv em tamanho fixo

float2 GetTileUV_128x128(float2 uv, int tileIndex)
{
    tileIndex = clamp(tileIndex, 0, 4); // 5 tiles (0-4)
    float2 tileStart = float2(tileIndex * 128.0, 0.0);
        return (uv * 128.0 + tileStart) / float2(640.0, 128.0);
}

float randSeed(float2 seed)
{
    return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
}
// Random baseado em multiplicação de primos
float rand(float2 co) {
    float a = 12.9898;
    float b = 78.233;
    float c = 43758.5453;
    float dt = dot(co, float2(a, b));
    float sn = fmod(dt, 3.14159);
    return frac(sin(sn) * c);
}

// Retorna 1 ou -1 baseado em um inteiro (hash determinístico)
int randomSign(int seed) {
    // Usa um grande primo para dispersão
    uint hash = seed * 0x51633E2D;
    // Pega o bit menos significativo (0 ou 1) e mapeia para -1 ou 1
    return ((hash & 1) << 1) - 1;
}

float ParticleRandom(uint particleID) {
    // Um hash simples usando primos grandes
    uint hash = particleID * 0x51633E2Du;
    hash = (hash ^ 61u) ^ (hash >> 16u);
    hash *= 9u;
    hash = hash ^ (hash >> 4u);
    hash *= 0x27d4eb2du;
    hash = hash ^ (hash >> 15u);

    // Converte para float entre -1 e 1
    return float(hash % 2000u) / 1000.0 - 1.0;
}

void UvByID_float(float2 Uv, int index, out float2 IndexedUv)
{
    IndexedUv = GetTileUV_128x128(Uv, index);
}

void RandByFloat2_float(int Index, out float Rand)
{
    Rand = ParticleRandom(Index);
}

// look at
void LookAtVector_float(float3 position, float3 target, out float3 forward)
{
    forward = normalize(target - position);
}

void LookAtRotation_float(float3 position, float3 target, float3 up, out float3 forward, out float3 right, out float3 newUp)
{
    forward = normalize(target - position);
    right = normalize(cross(up, forward));
    newUp = cross(forward, right);
}
void LookAtVertex_float(
    float3 VertexPosition,  // Posição do vértice no espaço local
    out float3 BillboardedPosition  // Posição final com billboard
)
{
    // Calcula a direção para a câmera
    float3 forward = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz);
    float3 up = float3(0, 1, 0);
    float3 right = normalize(cross(up, forward));
    
    // Aplica a rotação de billboard ao vértice
    BillboardedPosition = mul(float3x3(right, up, forward), VertexPosition);
}