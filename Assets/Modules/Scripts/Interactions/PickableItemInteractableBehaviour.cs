using UnityEngine;
public class PickableItemInteractableBehaviour : InteractableBehaviour
{
    public string keyId;
    void Start()
    {
        if(keyId == null ) keyId = gameObject.name;
    }
    public override void ToggleActivation()
    {

    }
}
