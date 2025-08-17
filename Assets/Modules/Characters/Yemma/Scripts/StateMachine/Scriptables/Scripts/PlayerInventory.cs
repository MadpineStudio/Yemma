using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerInventory", menuName = "Player/inventory", order = 1)]

public class PlayerInventory : ScriptableObject
{
    [Header("Unlockables")]
    public bool para_Glider;
}
