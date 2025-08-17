using UnityEngine;

public class FPSCap : MonoBehaviour 
{
    void Start()
    {
        // Limita a 60 FPS
        Application.targetFrameRate = 60;
        
        // Opcional: Desativa VSync para melhor controle
        QualitySettings.vSyncCount = 0;
    }
}