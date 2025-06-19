using UnityEngine;

[CreateAssetMenu(fileName = "SkinData", menuName = "Store/Skin Data")]
public class SkinData : ScriptableObject
{
    public string itemId; // Unique identifier for the skin (PlayFab Item ID)
    public string skinName;
    public string skinDescription;
    public Sprite skinIcon;
    public PlayerRole role; // OUTSIDER or PONTIANAK
    public int price;

    // Add any additional properties or methods needed for the skin data
}