# Sistema de Interação com Eventos - Yemma

## Visão Geral

O sistema de interação foi refinado para funcionar com eventos, permitindo que objetos como puzzles e monólitos controlem o modo de interação do player quando ele pressiona a tecla "E".

## Componentes do Sistema

### 1. IInteractable (Interface)
Interface que objetos interagíveis devem implementar:
- `OnInteract(YemmaController player)` - Chamado quando player interage
- `CanInteract` - Se pode ser interagido
- `InteractionDistance` - Distância máxima para interação
- `InteractionPrompt` - Texto de prompt (ex: "Pressione E para...")

### 2. YemmaInteractionSystem
Sistema de detecção automática de objetos interagíveis:
- Detecta objetos com IInteractable por raycast e/ou esfera
- Processa input da tecla "E"
- Dispara eventos quando encontra/perde objetos interagíveis

### 3. YemmaController (Atualizado)
Controller principal com eventos integrados:
- `OnEnterInteractionMode` - Evento quando entra no modo interação
- `OnExitInteractionMode` - Evento quando sai do modo interação
- `OnInteractWithObject` - Evento quando interage com objeto

## Como Usar

### Setup Básico

1. **No YemmaController GameObject:**
   - Adicione o componente `YemmaInteractionSystem`
   - Configure a distância de interação e layers
   - Opcionalmente configure os eventos no Inspector

2. **Em objetos interagíveis (ex: Monólito):**
   - Adicione o componente `MonolitoInteractable` (ou crie seu próprio IInteractable)
   - Configure distância e texto de interação
   - Certifique-se que o objeto está na layer correta

### Exemplo: Monólito Interagível

```csharp
// O MonolitoInteractable já está pronto para uso
// Basta adicionar o componente ao GameObject do Monólito
```

### Criando Objetos Interagíveis Customizados

```csharp
public class PuzzleInteractable : MonoBehaviour, IInteractable
{
    public bool CanInteract => !puzzleSolved;
    public float InteractionDistance => 2f;
    public string InteractionPrompt => "Pressione E para resolver puzzle";

    public void OnInteract(YemmaController player)
    {
        // Ativa modo de interação
        player.EnterInteractionMode();
        
        // Sua lógica do puzzle aqui
        StartPuzzle();
    }
    
    private void StartPuzzle()
    {
        // Implementar lógica do puzzle
        // Quando terminar, chame player.ExitInteractionMode()
    }
}
```

### Configurando Eventos no Inspector

No YemmaController você pode configurar:
- **OnEnterInteractionMode**: Disparado quando entra no modo (ex: desabilitar UI, tocar som)
- **OnExitInteractionMode**: Disparado quando sai do modo (ex: reabilitar UI)
- **OnInteractWithObject**: Disparado quando interage com qualquer objeto

## Fluxo de Funcionamento

1. Player se aproxima de objeto interagível
2. YemmaInteractionSystem detecta o objeto
3. Dispara evento `OnInteractableFound`
4. Player pressiona "E"
5. Chama `IInteractable.OnInteract()`
6. Objeto pode chamar `player.EnterInteractionMode()`
7. Dispara evento `OnEnterInteractionMode`
8. Player fica em modo de interação (inputs bloqueados conforme configuração)
9. Quando terminar, objeto chama `player.ExitInteractionMode()`
10. Dispara evento `OnExitInteractionMode`

## Benefícios

- **Desacoplado**: Objetos controlam seu próprio comportamento de interação
- **Flexível**: Cada objeto pode decidir como reagir à interação
- **Baseado em Eventos**: Fácil de conectar com outros sistemas
- **Configurável**: Diferentes distâncias e comportamentos por objeto
- **Debugável**: Logs automáticos e gizmos visuais

## Configurações Importantes

### YemmaInteractionSystem
- `interactionDistance`: Distância padrão de detecção
- `interactionLayers`: Layers que serão verificadas
- `useRaycast`: Se usa detecção por raycast (direção da câmera)
- `useSphereDetection`: Se usa detecção esférica (ao redor do player)

### YemmaController
- `blockInputsInInteractionMode`: Se bloqueia inputs WASD durante interação
- `blockMovementInInteractionMode`: Se bloqueia movimento durante interação