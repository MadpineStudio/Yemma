# Sistema de Movimentação Modular Yemma

## Visão Geral

Este sistema implementa um## YemmaMovementProfile (ScriptableObject)

O sistema utiliza um ScriptableObject personalizado para configurar todos os parâmetros de movimento:

### Categorias de Configuração

#### 1. **Basic Movement**
- `maxVelocity`: Velocidade máxima de movimento
- `acceleration`: Taxa de aceleração
- `deceleration`: Taxa de desaceleração
- `velocityPower`: Curva de potência da velocidade

#### 2. **Rotation & Orientation**
- `rotationSpeed`: Velocidade de rotação do player
- `terrainAlignmentSpeed`: Velocidade de alinhamento ao terreno
- `tiltAmount`: Quantidade de inclinação baseada no movimento
- `tiltSpeed`: Velocidade da inclinação

#### 3. **Ground Detection**
- `groundLayers`: Camadas consideradas como chão
- `groundCheckDistance`: Distância de detecção do chão
- `groundCheckOffset`: Offset para raycast

#### 4. **Advanced Settings**
- `inputResponseCurve`: Curva de responsividade do input
- `slopeSpeedMultiplier`: Multiplicador em superfícies inclinadas
- `maxWalkableAngle`: Ângulo máximo caminhável

### Criação de Perfis

Use o menu: `Assets/Create/Yemma/Movement Profiles/` para criar:
- **Default Profile**: Configuração equilibrada
- **Slow Profile**: Movimento lento e preciso
- **Fast Profile**: Movimento rápido e responsivo
- **Smooth Profile**: Movimento fluido e suave
- **Rough Terrain Profile**: Para terrenos acidentados
- **Precise Profile**: Para plataformas precisas

### Como Usar

### 1. Setup Básico

**Opção A - Setup Automático:**
1. Selecione o GameObject do player
2. Use `Tools/Yemma/Movement System/Setup Yemma Controller`
3. Crie um perfil de movimento usando o menu
4. Atribua o perfil ao YemmaMovementController

**Opção B - Setup Manual:**
1. Adicione o componente `YemmaController` ao GameObject do player
2. Configure os componentes necessários:
   - `YemmaMovementController`
   - `InputManager`
   - `Rigidbody`
   - `YemmaMovementProfile`ura modular para controle de movimentação do player usando State Machine pattern. O sistema está organizado no namespace `Yemma.Movement` e foi projetado para ser extensível e fácil de manter.

## Arquitetura

### Estrutura de Pastas
```
Yemma/Movement/
├── Core/                          # Classes principais
│   ├── IYemmaMovementState.cs     # Interface para estados
│   └── YemmaMovementController.cs # Controlador principal
├── Data/                          # ScriptableObjects
│   └── YemmaMovementProfile.cs    # Perfil de movimento
├── Physics/                       # Cálculos de física
│   └── YemmaMovementPhysics.cs   # Sistema de física
├── StateMachine/                  # State Machine
│   ├── YemmaMovementStateMachine.cs
│   └── States/                    # Estados específicos
│       ├── YemmaMovementStateBase.cs
│       ├── YemmaIdleState.cs
│       └── YemmaWalkState.cs
├── Utils/                         # Utilitários
│   ├── YemmaMovementSetup.cs     # Configuração automática
│   └── YemmaMovementProfileFactory.cs # Factory de perfis
├── Editor/                        # Scripts de editor
│   ├── YemmaMovementProfileEditor.cs # Custom Inspector
│   └── YemmaMovementProfileMenu.cs   # Menu customizado
└── Profiles/                      # Perfis salvos
    └── (arquivos .asset)
```

### Componentes Principais

#### 1. YemmaController
- **Localização**: `Assets/Modules/Scripts/YemmaController/YemmaController.cs`
- **Função**: Controlador principal que orquestra todos os sistemas
- **Responsabilidades**:
  - Gerencia a state machine
  - Coordena updates de lógica e física
  - Fornece interface pública para outros sistemas

#### 2. YemmaMovementController
- **Localização**: `Movement/Core/YemmaMovementController.cs`
- **Função**: Centraliza funcionalidades de movimento e física
- **Responsabilidades**:
  - Controla Rigidbody e Transform
  - Detecta se está no chão
  - Aplica forças de movimento
  - Interface com sistema de física

#### 3. YemmaMovementPhysics
- **Localização**: `Movement/Physics/YemmaMovementPhysics.cs`
- **Função**: Todos os cálculos matemáticos de movimento
- **Responsabilidades**:
  - Cálculo de movimento relativo à câmera
  - Aplicação de aceleração/desaceleração
  - Projeção em superfícies inclinadas
  - Rotação e alinhamento ao terreno

#### 4. YemmaMovementStateMachine
- **Localização**: `Movement/StateMachine/YemmaMovementStateMachine.cs`
- **Função**: Gerencia transições entre estados
- **Responsabilidades**:
  - Controla estado atual
  - Executa transições
  - Sistema de debugging

## Estados Implementados

### YemmaIdleState
- **Quando ativa**: Player parado, sem input de movimento
- **Comportamento**:
  - Aplica desaceleração gradual
  - Alinha ao terreno
  - Transiciona para Walk quando há input

### YemmaWalkState
- **Quando ativa**: Player se movendo com input ativo
- **Comportamento**:
  - Aplica movimento baseado no input
  - Rotaciona player na direção do movimento
  - Alinha ao terreno
  - Transiciona para Idle quando para o input

## Como Usar

### 1. Setup Básico

1. Adicione o componente `YemmaController` ao GameObject do player
2. Configure os componentes necessários:
   - `YemmaMovementController`
   - `InputManager`
   - `Rigidbody`
   - `PhysicalProfile`

### 2. Configuração Automática

O sistema inclui uma classe `YemmaMovementSetup` para configuração automática:

```csharp
// A configuração automática é executada no Awake do YemmaController
// Para configuração manual, use:
var setup = new YemmaMovementSetup();
setup.AutoConfigureComponents(gameObject);
```

### 2. Criação de Perfis Customizados

```csharp
// Via código - usando Factory
var customProfile = YemmaMovementProfileFactory.CreateCustom(
    "My Custom Profile",
    velocity: 8f,
    accel: 12f,
    decel: 18f,
    rotSpeed: 15f
);

// Via Editor - através do menu
// Assets/Create/Yemma/Movement Profiles/Custom Profile
```

### 3. Exemplo de Uso

```csharp
// Acesso ao sistema de movimento
YemmaController yemma = GetComponent<YemmaController>();

// Verificar se está no chão
bool isGrounded = yemma.IsGrounded;

// Obter velocidade atual
Vector3 velocity = yemma.CurrentVelocity;

// Acessar perfil de movimento
var profile = yemma.MovementController.MovementProfile;
float maxSpeed = profile.maxVelocity;

// Modificar perfil em runtime (se necessário)
profile.maxVelocity = 10f;

// Acessar state machine diretamente (se necessário)
var currentState = yemma.MovementStateMachine.CurrentState;
```

### 4. Validação e Debug

```csharp
// Validar perfil
YemmaMovementProfile profile = GetComponent<YemmaMovementController>().MovementProfile;
if (!profile.ValidateProfile())
{
    Debug.LogError("Profile has validation errors!");
}

// Verificar se superfície é caminhável
bool walkable = profile.IsSurfaceWalkable(surfaceNormal);

// Calcular multiplicador de inclinação
float slopeMultiplier = profile.CalculateSlopeMultiplier(slopeAngle);
```

## Extensibilidade

### Adicionando Novos Estados

1. Crie uma nova classe herdando de `YemmaMovementStateBase`:

```csharp
using Yemma.Movement.StateMachine.States;

public class YemmaRunState : YemmaMovementStateBase
{
    public YemmaRunState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
        : base(controller, inputManager)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        // Lógica de entrada do estado
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        // Física específica do estado de corrida
        Vector2 movementInput = GetMovementInput();
        // Aplicar movimento mais rápido
        controller.ApplyMovement(movementInput * 1.5f);
    }
}
```

2. Adicione transições nos estados existentes:

```csharp
// Em YemmaWalkState, por exemplo
private void CheckRunTransition()
{
    if (inputManager.IsRunPressed() && HasMovementInput())
    {
        var runState = new YemmaRunState(controller, inputManager, stateMachine);
        stateMachine.ChangeState(runState);
    }
}
```

### Customizando Física

Modifique `YemmaMovementPhysics` para adicionar novos cálculos:

```csharp
public Vector3 CalculateRunMovement(Vector2 inputDirection, Vector3 currentVelocity)
{
    // Implementar lógica específica de corrida
    var baseMovement = CalculatePlayerMovement(inputDirection, currentVelocity);
    return baseMovement * 1.5f; // Exemplo de velocidade aumentada
}
```

## Vantagens da Arquitetura

1. **Modularidade**: Cada componente tem responsabilidade bem definida
2. **Extensibilidade**: Fácil adicionar novos estados e comportamentos
3. **Manutenibilidade**: Código organizado e fácil de debugar
4. **Reutilização**: Componentes podem ser reutilizados em outros projetos
5. **Performance**: Separação clara entre lógica e física
6. **Debugging**: Sistema de logging integrado para desenvolvimento

## Debugging

Ative o debugging no `YemmaController` para acompanhar transições de estado:

```csharp
[SerializeField] private bool enableStateDebugging = true;
```

Isto imprimirá logs no console mostrando todas as transições de estado.

## Integração com Sistema Existente

Este sistema foi projetado para complementar o sistema existente. Você pode:

1. Migrar gradualmente do sistema atual
2. Usar ambos sistemas em paralelo durante a transição
3. Aproveitar o `InputManager` e `PhysicalProfile` existentes

## Ferramentas de Editor

### Menu Tools/Yemma/Movement System/
- **Setup Yemma Controller**: Configura automaticamente componentes
- **Validate All Profiles**: Valida todos os perfis do projeto

### Custom Inspector
O YemmaMovementProfile possui um inspector customizado com:
- Seções organizadas e expansíveis
- Presets rápidos (Slow, Normal, Fast)
- Validação em tempo real
- Visualização de zonas de transição
- Botões de reset e validação

### Debug Features
- Raios de debug no Scene View
- Informações visuais de velocidade
- Logs de transição de estado
- Validação automática

## Perfis Pré-configurados

### Default Profile
Configuração equilibrada para uso geral:
- Velocidade: 6 m/s
- Aceleração: 10 m/s²
- Rotação: 12 rad/s

### Slow Profile
Para movimento preciso:
- Velocidade: 3 m/s
- Aceleração: 6 m/s²
- Ideal para puzzles

### Fast Profile
Para movimento dinâmico:
- Velocidade: 10 m/s
- Aceleração: 15 m/s²
- Ideal para ação

### Smooth Profile
Para movimento fluido:
- Curva de input suavizada
- Transições mais graduais
- Ideal para cinematics

### Rough Terrain Profile
Para terrenos difíceis:
- Alinhamento forte ao terreno
- Velocidade reduzida
- Ângulos de inclinação menores

### Precise Profile
Para plataformas:
- Controle muito responsivo
- Thresholds menores
- Aceleração alta

## Próximos Passos

Para expandir o sistema, considere adicionar:

1. **Estados adicionais**: Jump, Roll, Climb, etc.
2. **Sistema de animação**: Integração com Animator
3. **Efeitos visuais**: Partículas, trails, etc.
4. **Sistema de som**: Audio cues para estados
5. **Persistência**: Salvar/carregar configurações
6. **Multiplayer**: Sincronização de estados
7. **IA**: Estados para NPCs
