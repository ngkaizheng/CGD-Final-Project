using UnityEngine;

public interface IInteractable
{
    void OnInteract(Player player);
    // float GetInteractionDistance(); // Maximum distance for interaction
    // Transform GetTransform(); // Transform for distance calculations
    bool CanInteract(Player player);
    string GetPromptMessage();
    void SetPromptVisible(bool visible);
    bool IsInteractable { get; set; }
}