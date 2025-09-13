using UnityEngine;

namespace Yemma.Movement.Utils
{
    /// <summary>
    /// Configurador automático para o sistema de movimento do Yemma
    /// Ajuda a configurar automaticamente os componentes necessários
    /// </summary>
    [System.Serializable]
    public class YemmaMovementSetup
    {
        [Header("Auto Setup")]
        [Tooltip("Configura automaticamente os componentes necessários")]
        public bool autoSetup = true;

        [Header("Components")]
        public Transform yemmaBody;
        public Rigidbody playerRigidbody;
        public PhysicalProfile physicalProfile;
        public InputManager inputManager;

        [Header("Ground Detection")]
        public LayerMask groundLayer = 1;
        public float groundCheckDistance = 0.4f;
        public Vector3 groundCheckOffset = Vector3.up * 0.2f;

        /// <summary>
        /// Configura automaticamente os componentes do Yemma
        /// </summary>
        public void AutoConfigureComponents(GameObject yemmaObject)
        {
            if (!autoSetup) return;

            // Configura Rigidbody
            if (playerRigidbody == null)
            {
                playerRigidbody = yemmaObject.GetComponent<Rigidbody>();
                if (playerRigidbody == null)
                {
                    playerRigidbody = yemmaObject.AddComponent<Rigidbody>();
                    ConfigureRigidbody(playerRigidbody);
                }
            }

            // Configura InputManager
            if (inputManager == null)
            {
                inputManager = yemmaObject.GetComponent<InputManager>();
                if (inputManager == null)
                {
                    Debug.LogWarning("InputManager não encontrado! Certifique-se de adicionar o componente InputManager.");
                }
            }

            // Configura Transform do corpo
            if (yemmaBody == null)
            {
                yemmaBody = yemmaObject.transform;
            }

            Debug.Log("YemmaMovementSetup: Configuração automática concluída!");
        }

        /// <summary>
        /// Configura o Rigidbody com valores padrão otimizados
        /// </summary>
        private void ConfigureRigidbody(Rigidbody rb)
        {
            rb.mass = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 5f;
            rb.freezeRotation = false; // Permitir rotação para alinhamento ao terreno
            
            // Constraints para evitar rotação indesejada
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        /// <summary>
        /// Cria um PhysicalProfile padrão se não existir
        /// </summary>
        public PhysicalProfile CreateDefaultPhysicalProfile()
        {
            var profile = ScriptableObject.CreateInstance<PhysicalProfile>();
            
            // Valores padrão otimizados para movimento responsivo
            profile.velocity = 6f;
            profile.acceleration = 10f;
            profile.decceleration = 15f;
            profile.velPower = 0.96f;
            profile.rotationVelocity = 12f;
            profile.tiltVelocity = 8f;
            profile.tiltAmount = 25f;
            
            return profile;
        }

        /// <summary>
        /// Valida se todos os componentes necessários estão configurados
        /// </summary>
        public bool ValidateSetup()
        {
            bool isValid = true;

            if (yemmaBody == null)
            {
                Debug.LogError("YemmaMovementSetup: yemmaBody não está configurado!");
                isValid = false;
            }

            if (playerRigidbody == null)
            {
                Debug.LogError("YemmaMovementSetup: playerRigidbody não está configurado!");
                isValid = false;
            }

            if (inputManager == null)
            {
                Debug.LogError("YemmaMovementSetup: inputManager não está configurado!");
                isValid = false;
            }

            if (physicalProfile == null)
            {
                Debug.LogWarning("YemmaMovementSetup: physicalProfile não está configurado! Usando valores padrão.");
            }

            return isValid;
        }
    }
}
