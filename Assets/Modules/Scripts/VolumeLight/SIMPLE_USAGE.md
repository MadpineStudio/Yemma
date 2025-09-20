# Shader Simples de Raios de Luz

## Como Usar

### 1. Criar o Material
- Criar um novo Material
- Escolher o shader **Custom/S_LightPath**
- Ajustar as propriedades:

### 2. Propriedades do Shader

| Propriedade | Descrição | Valores Recomendados |
|-------------|-----------|---------------------|
| **Light Color** | Cor da luz | Amarelo quente (1, 0.9, 0.7, 0.5) |
| **Light Texture** | Textura do raio (opcional) | Gradient radial ou "white" |
| **Intensity** | Intensidade da luz | 1.0 - 2.0 |
| **Falloff Power** | Suavidade das bordas | 1.5 - 2.0 |

### 3. Setup da Geometria
- Criar um **Plane** ou **Quad**
- Posicionar onde você quer o raio de luz
- Rotacionar para a direção desejada
- Aplicar o material

### 4. Dicas para Melhores Resultados

#### Para raios de sol:
```
Color: (1, 0.95, 0.8, 0.3)
Intensity: 1.5
Falloff Power: 2.0
```

#### Para luz de vitral:
```
Color: (0.8, 0.9, 1, 0.4)
Intensity: 1.0
Falloff Power: 1.5
```

#### Para luz mística:
```
Color: (0.7, 0.8, 1, 0.6)
Intensity: 2.0
Falloff Power: 1.0
```

### 5. Múltiplos Raios
- Duplicar o plane várias vezes
- Variar levemente a cor e intensidade
- Posicionar em diferentes ângulos
- Usar diferentes texturas se necessário

### 6. Texturas Recomendadas
- **Gradiente radial**: Para efeito suave
- **Noise texture**: Para textura mais orgânica  
- **Custom patterns**: Para efeitos específicos de vitral

## Exemplo de Uso Rápido

1. Criar Plane (3D Object > Plane)
2. Criar Material com shader Custom/S_LightPath
3. Arrastar material para o Plane
4. Ajustar Transform:
   - Rotation: (90, 0, 0) para luz vertical
   - Scale: (2, 1, 5) para raio alongado
5. Ajustar propriedades do material conforme necessário

Simples assim! 🌟