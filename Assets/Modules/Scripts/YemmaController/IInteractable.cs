using UnityEngine;

namespace Yemma
{
    /// <summary>
    /// Interface para objetos que podem ser interagidos pelo player
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Chamado quando o player interage com o objeto (pressiona E)
        /// </summary>
        /// <param name="player">Referência ao YemmaController do player</param>
        void OnInteract(YemmaController player);
        
        /// <summary>
        /// Verifica se o objeto pode ser interagido no momento
        /// </summary>
        bool CanInteract { get; }
        
        /// <summary>
        /// Distância máxima para interação
        /// </summary>
        float InteractionDistance { get; }
        
        /// <summary>
        /// Texto que aparece quando o player pode interagir (opcional)
        /// </summary>
        string InteractionPrompt { get; }
    }
}