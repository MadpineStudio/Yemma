 // Função para criar uma matriz de rotação em torno do eixo X
float3x3 RotationMatrixX(float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return float3x3(
        1, 0, 0,
        0, c, -s,
        0, s, c
    );
}

// Função para criar uma matriz de rotação em torno do eixo Z
float3x3 RotationMatrixZ(float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return float3x3(
        c, -s, 0,
        s, c, 0,
        0, 0, 1
    );
}
