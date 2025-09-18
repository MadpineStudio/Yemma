using UnityEngine;
using Yemma.Movement.Core;

public class PickablePlaceLocal : InteractableBehaviour
{
    [SerializeField] private Transform itemLocationPivot;
    public GameObject pickableItem = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
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
    }
    public override void ToggleActivation()
    {
        if (pickableItem != null) return;
        if (pickableItem == null) pickableItem = YemmaInteractorController.onGetPickedItem?.Invoke();
        if (pickableItem != null)
        {
            Vector3 pivotScale = itemLocationPivot.parent.localScale;
            pickableItem.GetComponent<BoxCollider>().enabled = true;
            pickableItem.transform.parent = itemLocationPivot;
            pickableItem.transform.localScale = new Vector3(1f / pivotScale.x, 1f / pivotScale.y, 1f / pivotScale.z);
            pickableItem.transform.position = itemLocationPivot.position;
            pickableItem.transform.rotation = itemLocationPivot.rotation;
        }

    }
}
