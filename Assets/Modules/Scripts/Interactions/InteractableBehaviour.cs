using _Modules.Player.Script;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableBehaviour : MonoBehaviour
{
    public bool isPickable;

    
    protected bool canToggle;
    protected bool activated;
    protected bool hasChangend;
    void Start()
    {
        activated = false;
    }
    public virtual void Update()
    {
        if (activated)
        {
            Action();
        }
    }
    public virtual void ToggleActivation(){
        if(canToggle || hasChangend){
            activated = !activated;
        }
    }
    public virtual void Action() { }
}
