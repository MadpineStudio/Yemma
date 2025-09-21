using System;
using System.Collections.Generic;
using UnityEngine;
using Yemma.Movement.Core;

[Serializable]
public class AudioClips
{
    public string audioId;
    public AudioClip audioClip;
}
public class PickablePlaceLocal : InteractableBehaviour
{
    [SerializeField] private string keyId;
    [SerializeField] private Transform itemLocationPivot;
    [SerializeField] private List<GameObject> unlockableItens = new();
    [SerializeField] private List<AudioClips> audioClips;
    [SerializeField] private AudioSource audioSource;
    public PickableItemInteractableBehaviour pickableItem = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (keyId == null) keyId = "none";
    }

    // Update is called once per frame
    public override void Update()
    {
        if (itemLocationPivot.childCount == 0 && !gameObject.CompareTag("Interactable"))
        {
            pickableItem = null;
            EnableItens(false);
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
        if (pickableItem == null)
        {
            GameObject pickableObject = YemmaInteractorController.onGetPickedItem?.Invoke();
            pickableItem = pickableObject.GetComponent<PickableItemInteractableBehaviour>();
        }
        if (pickableItem != null)
        {
            Vector3 pivotScale = itemLocationPivot.parent.localScale;
            pickableItem.GetComponent<BoxCollider>().enabled = true;
            pickableItem.transform.parent = itemLocationPivot;
            pickableItem.transform.localScale = new Vector3(1f / pivotScale.x, 1f / pivotScale.y, 1f / pivotScale.z);
            pickableItem.transform.position = itemLocationPivot.position;
            pickableItem.transform.rotation = itemLocationPivot.rotation;
            if (pickableItem.keyId == keyId) EnableItens(true);
        }

    }
    private void EnableItens(bool active)
    {
        if(unlockableItens.Count > 0)unlockableItens.ForEach(item => item.SetActive(active));
    }
    private void PlayAudio(string audioId)
    {
        if(audioClips.Count > 0 && audioSource != null)
        audioSource.clip = audioClips.Find(clip => clip.audioId == audioId).audioClip;
        audioSource.Play();
    }
}
