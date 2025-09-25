using UnityEngine;

public class BridgePedestal : PickablePlaceLocal
{
    private bool _rotated;
    public override void Update()
    {
        if (itemLocationPivot.childCount == 0 && !gameObject.CompareTag("Interactable"))
        {
            pickableItem = null;
            gameObject.tag = "Interactable";
        }
        else if (pickableItem != null && !gameObject.CompareTag("PickupPlaceFull"))
        {
            gameObject.tag = "PickupPlaceFull";
        }
        if (pickableItem == null && _rotated)
        {
            _rotated = false;
            ActivateBridge(false);
        }
    }
    public override void ToggleActivation()
    {
        base.ToggleActivation();
        Debug.Log("Teste");
        if (keyId == pickableItem.keyId)
        {
            _rotated = true;
            ActivateBridge(true);
        }

    }
    private void ActivateBridge(bool activate)
    {
        Animator animator;

        if (unlockableItens.Count > 0 && unlockableItens[0].TryGetComponent<Animator>(out animator))
        {
            animator.Play(activate ? "Rotate" : "UnRotate");
        }
    }
}
