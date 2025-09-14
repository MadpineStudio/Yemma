using UnityEngine;

namespace Yemma.Movement.Core
{
    /// <summary>
    /// Controlador de animações do Yemma.
    /// </summary>
    
    public class YemmaAnimationController
    {

        public enum YemmaAnimations
        {
            Idle,
            CollectWalk,
            Run,
            Jump
        }

        private YemmaMovementController controller;

        public YemmaAnimationController(YemmaMovementController controller)
        {
            this.controller = controller;
        }
        public void ChangeState(YemmaAnimations newState, float blendTime = 0.2f)
        {
            // Implementar lógica de mudança de animação aqui
            // Exemplo: animator.Play(newState.ToString());
            if (this.controller.Animator != null)
            {
                this.controller.Animator.CrossFade("Bake-" + newState.ToString(), blendTime);
            }
        }
    }
}
