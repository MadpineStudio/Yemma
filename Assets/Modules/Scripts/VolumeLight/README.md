# Sistema de Luz Volumétrica para Vitrais

Este sistema implementa feixes de luz volumétrica realistas que simulam luz passando através de vitrais coloridos.

## Componentes do Sistema

### 1. S_LightPath.shader
Shader principal que renderiza os feixes de luz volumétrica com:
- **Máscara UV**: Texture para controlar o padrão da luz (vitrais)
- **Gradientes radiais**: Falloff natural da luz do centro para as bordas
- **Ruído volumétrico**: Efeito de partículas de poeira na luz
- **Animação**: Movimento sutil do ruído para simular atmosfera
- **Transparência**: Blending correto para múltiplas camadas de luz

### 2. VolumetricLightController.cs
Controlador individual para cada feixe de luz com:
- **Configurações visuais**: Cor, intensidade, falloff
- **Animações**: Flickering automático, fade in/out
- **Performance**: Cache de Shader Property IDs
- **Validação**: Atualização em tempo real no editor

### 3. StainedGlassLightSystem.cs
Sistema global para gerenciar múltiplos feixes:
- **Ciclo dia/noite**: Mudança automática de cor e intensidade
- **Efeitos climáticos**: Chuva e nuvens afetam a luz
- **Sincronização**: Controle centralizado de todos os feixes
- **Automação**: Detecção automática de luzes na cena

## Como Usar

### Setup Básico

1. **Criar o Material**:
   ```
   - Criar novo Material
   - Usar shader "Custom/S_LightPath"
   - Atribuir texture de vitral em "Light Mask"
   - Atribuir texture de ruído em "Volume Noise"
   ```

2. **Criar Geometry**:
   ```
   - Criar Plane ou Quad
   - Posicionar onde a luz deve aparecer
   - Rotacionar para direção desejada
   - Aplicar o material
   ```

3. **Adicionar Controller**:
   ```
   - Adicionar VolumetricLightController ao objeto
   - Configurar as propriedades na inspector
   - Ajustar intensidade e cor
   ```

### Setup Avançado com Sistema

1. **Criar Sistema Global**:
   ```
   - Criar Empty GameObject
   - Adicionar StainedGlassLightSystem
   - Configurar settings globais
   ```

2. **Configurar Múltiplas Luzes**:
   ```
   - O sistema detecta automaticamente VolumetricLightControllers
   - Ou manualmente adicionar à lista lightBeams
   - Configurar offsets de delay para variação
   ```

## Configurações do Shader

### Light Properties
- **Light Color**: Cor base da luz
- **Intensity**: Intensidade geral (0-5)

### Volumetric Properties
- **Light Mask**: Texture do padrão do vitral
- **Volume Noise**: Texture para efeito volumétrico
- **Volumetric Intensity**: Força do efeito de ruído (0-2)

### Gradient Controls
- **Falloff Power**: Controla a suavidade do gradiente (0.1-5)
- **Center Fade**: Tamanho da área central (0-1)
- **Edge Softness**: Suavidade das bordas (0-1)

### Animation
- **Scroll Speed**: Velocidade de animação do ruído
- **Noise Scale**: Escala da texture de ruído

## Texturas Recomendadas

### Light Mask (Vitral)
- **Resolução**: 512x512 ou 1024x1024
- **Formato**: RGB com canal Alpha
- **Conteúdo**: Padrão do vitral em RGB, máscara em Alpha
- **Filtro**: Bilinear

### Volume Noise
- **Resolução**: 256x256 ou 512x512
- **Formato**: Grayscale ou RGB
- **Conteúdo**: Ruído Perlin ou texture de nuvens
- **Wrap Mode**: Repeat
- **Filtro**: Bilinear

## Exemplos de Uso

### Igreja/Catedral
```csharp
// Setup para ambiente de igreja
lightSystem.SetGlobalColor(new Color(1f, 0.9f, 0.7f)); // Luz quente
lightSystem.SetGlobalIntensity(0.8f);
lightSystem.enableTimeOfDay = true;
lightSystem.dayDuration = 300f; // 5 minutos por dia
```

### Ambiente Mágico
```csharp
// Setup para ambiente fantasioso
lightSystem.SetGlobalColor(new Color(0.7f, 0.9f, 1f)); // Luz azulada
lightSystem.enableGlobalFlicker = true;
lightSystem.globalFlickerIntensity = 0.3f;
```

### Efeito Climático
```csharp
// Simular tempestade
lightSystem.SetWeather(0.8f, 0.6f); // 80% nuvens, 60% chuva
lightSystem.globalFlickerSpeed = 3f; // Flicker mais rápido
```

## Performance

### Otimizações Implementadas
- **Shader Property IDs**: Cached para evitar string lookups
- **LOD System**: Pode ser implementado baseado na distância
- **Frustum Culling**: Unity cuida automaticamente
- **Batching**: Objetos com mesmo material são batchados

### Recomendações
- **Máximo 10-15 feixes** por cena para performance
- **Usar LOD** para feixes distantes
- **Combinar geometria** quando possível
- **Otimizar texturas** (compressão adequada)

## Troubleshooting

### Luz não aparece
- Verificar se o material usa o shader correto
- Confirmar que Blend Mode está em Transparent
- Verificar se a cor alpha não está 0

### Performance baixa
- Reduzir número de feixes
- Diminuir resolução das texturas
- Usar LOD system
- Verificar overdraw

### Efeito muito sutil
- Aumentar Intensity
- Ajustar Falloff Power
- Verificar Light Mask alpha channel
- Aumentar Volumetric Intensity

## Extensões Futuras

### Possíveis Melhorias
1. **Ray Marching**: Para efeito volumétrico mais realista
2. **Light Scattering**: Simulação de espalhamento atmosférico
3. **Dynamic Occlusion**: Oclusão baseada em objetos da cena
4. **Particle Integration**: Integração com sistema de partículas
5. **Realtime Shadows**: Sombras dos feixes de luz

### Integration com Post-Processing
- Bloom para maior brilho
- Color Grading para ajuste de atmosfera
- Fog para maior profundidade