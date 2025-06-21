using UnityEngine;
using Fusion;

public class Interactable : NetworkBehaviour, IInteractable
{
    [Networked, OnChangedRender(nameof(OnColorChanged))] public Color NetworkedColor { get; set; }
    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
    }

    public override void Spawned()
    {
        // Apply initial color and prompt state on spawn
        ApplyColor();
    }

    public void OnInteract(Player player)
    {
        if (Object.HasStateAuthority)
        {
            NetworkedColor = Random.ColorHSV();
            player.GetComponent<SimpleAnimator>().SetInteracting(true);
        }
    }

    public void SetPromptVisible(bool visible)
    {
        // Example: Show or hide interaction prompt
        Debug.Log($"SetPromptVisible called with {visible}");
    }
    private void OnColorChanged()
    {
        ApplyColor();
    }
    private void ApplyColor()
    {
        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = NetworkedColor;
        }
    }
}