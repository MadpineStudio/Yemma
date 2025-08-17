// Função de randomização simples (pode ser substituída por uma mais complexa)
float Rand(float seed) {
    return frac(sin(seed) * 43758.5453);
}

// Cria uma matriz de rotação em torno do eixo Y
float3x3 RotationMatrixY(float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return float3x3(
        c, 0, s,
        0, 1, 0,
        -s, 0, c
    );
}
// Função para criar uma matriz de rotação 3x3 a partir de um eixo e um ângulo
float3x3 RotationMatrixAxisAngle(float3 axis, float angle)
{
    float c = cos(angle); // Cosseno do ângulo
    float s = sin(angle); // Seno do ângulo
    float t = 1.0 - c;    // 1 - cosseno

    // Normaliza o eixo
    axis = normalize(axis);

    // Componentes do eixo
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    // Matriz de rotação
    return float3x3(
        t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
        t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
        t * x * z - s * y,  t * y * z + s * x,  t * z * z + c
    );
}

float3 RotateVector(float3 axys, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return float3(axys.x * c + axys.z * s, axys.y, -axys.x * s + axys.z * c);
}

// Cria uma matriz de rotação para alinhar a grama com a normal da superfície
float3x3 CreateRotationMatrix(float3 normal, float randomAngle) {
    float3 up = float3(0, 1, 0);
    float3 axis = normalize(cross(up, normal));
    float angle = acos(dot(up, normal));
    return RotationMatrixAxisAngle(axis, angle + randomAngle);
}
float3x3 CalculaNovaRotacaoMatrix(float3 axis, float angle)
{
    float3 u = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float t = 1 - c;
    return float3x3(
        t * u.x * u.x + c,     t * u.x * u.y - s * u.z,   t * u.x * u.z + s * u.y,
        t * u.x * u.y + s * u.z, t * u.y * u.y + c,         t * u.y * u.z - s * u.x,
        t * u.x * u.z - s * u.y, t * u.y * u.z + s * u.x,   t * u.z * u.z + c
    );
}
