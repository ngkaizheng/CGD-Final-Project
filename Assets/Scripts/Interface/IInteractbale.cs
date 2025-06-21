using UnityEngine;

public interface IInteractable
{
    void OnInteract(Player player);
    // float GetInteractionDistance(); // Maximum distance for interaction
    // Transform GetTransform(); // Transform for distance calculations
    // string GetPromptMessage();
    void SetPromptVisible(bool visible);
}