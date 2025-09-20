using System.Collections.Generic;
using UnityEngine;

public class IlusoryPlatformManager : MonoBehaviour
{
    public delegate void OnPlatformInteractDelegate(IlusoryPlatformBehaviour platformBehaviour);
    public static OnPlatformInteractDelegate OnPlatformInteract;
    [SerializeField] private List<IlusoryPlatformBehaviour> platforms = new();
    void OnEnable()
    {
        OnPlatformInteract += OnPlatformInteractionHandler;
    }
    void OnDisable()
    {
        OnPlatformInteract = OnPlatformInteractionHandler;        
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnPlatformInteractionHandler(IlusoryPlatformBehaviour platformBehaviour)
    {
        Debug.Log("BBBBBBBBBBBBB");
    }
    

}
