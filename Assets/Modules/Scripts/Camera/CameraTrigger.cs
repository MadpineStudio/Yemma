using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private int cameraId;
    [SerializeField] CameraManager cameraManager;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trocando Cameras");
            cameraManager.cameras.Find(cameraObject => cameraObject.id == cameraId).camera.Prioritize();
        }
    }
}
