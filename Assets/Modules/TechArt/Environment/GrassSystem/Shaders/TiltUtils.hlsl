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

// Função para criar uma matriz de rotação em torno do eixo Y
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

// Função para rotacionar um vértice em torno de um pivot, levando em consideração a inclinação
float3 RotateAroundPivot(float3 vertex, float3 pivot, float3x3 rotationMatrix)
{
    // Move o vértice para o espaço relativo ao pivot
    float3 vertexRelative = vertex - pivot;

    // Aplica a rotação ao vértice
    float3 vertexRotated = mul(rotationMatrix, vertexRelative);

    // Move o vértice de volta para o espaço mundial
    vertexRotated += pivot;

    // Aplica a rotação à normal (para manter a consistência da iluminação)
    // normal = mul(rotationMatrix, normal);

    return vertexRotated;
}
