using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Interactable : NetworkBehaviour, IProgressInteractable
{
    [Networked, OnChangedRender(nameof(OnColorChanged))] public Color NetworkedColor { get; set; }
    [Networked] public float progress { get; set; } = 0.0f;
    [Networked] public float progressSpeed { get; set; } = 0.1f;
    [Networked] public bool progressCompleted { get; set; } = false;
    [Networked] private float lastSavePoint { get; set; } = 0f;

    private static readonly float[] savePoints = { 0.25f, 0.5f, 0.75f };
    private HashSet<PlayerRef> interactingPlayers = new HashSet<PlayerRef>();
    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
    }

    public override void Spawned()
    {
        ApplyColor();
    }

    public void OnInteract(Player player)
    {
        if (Object.HasStateAuthority)
        {
            player.GetComponent<SimpleAnimator>().SetInteracting(true);
            // Add player to interacting set
            interactingPlayers.Add(player.Object.InputAuthority);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            int interactingCount = interactingPlayers.Count;

            if (interactingCount > 0)
            {
                if (!progressCompleted)
                {
                    // Progress speed increases with more players
                    float totalSpeed = progressSpeed * interactingCount;
                    progress += Runner.DeltaTime * totalSpeed;

                    foreach (var sp in savePoints)
                    {
                        if (progress >= sp && lastSavePoint < sp)
                        {
                            lastSavePoint = sp;
                        }
                    }

                    if (progress >= 1.0f)
                    {
                        progress = 1.0f;
                        progressCompleted = true;
                        lastSavePoint = 1.0f;
                        NetworkedColor = Random.ColorHSV();
                    }
                }
            }
            else
            {
                if (!progressCompleted)
                {
                    progress = lastSavePoint;
                }
            }

            // Reset for next tick
            interactingPlayers.Clear();
        }
    }

    public float GetProgress()
    {
        return progress;
    }

    public void SetPromptVisible(bool visible)
    {
        Debug.Log($"SetPromptVisible called with {visible}");
    }

    public string GetPromptMessage()
    {
        return "Press E to interact";
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