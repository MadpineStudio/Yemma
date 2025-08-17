using UnityEngine;

public class ParaGliderInteractable : InteractableBehaviour
{
    [SerializeField] private PlayerInventory mainPlayerInventory;
    public override void ToggleActivation()
    {
        // base.ToggleActivation();
        mainPlayerInventory.para_Glider = true;
    }
}
