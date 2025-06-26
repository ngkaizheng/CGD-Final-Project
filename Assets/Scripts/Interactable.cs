using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Interactable : NetworkBehaviour, IProgressInteractable
{
    [Networked] public float progress { get; set; } = 0.0f;
    [Networked] public float progressSpeed { get; set; } = 0.1f;
    [Networked] public bool progressCompleted { get; set; } = false;
    [Networked] private float lastSavePoint { get; set; } = 0f;

    public PlayerRole playerRoleCanInteract = PlayerRole.ALL;
    public bool IsInteractable { get; set; } = true;

    private static readonly float[] savePoints = { 0.25f, 0.5f, 0.75f };
    private HashSet<PlayerRef> interactingPlayers = new HashSet<PlayerRef>();
    [Networked, OnChangedRender(nameof(onTreeTopRigidbodyChanged))]
    private bool isTreeTopKinematic { get; set; } = true;

    [Header("Tree Top Settings")]
    [SerializeField] private Rigidbody treeTopRigidbody;

    [Header("Events")]
    [SerializeField] private GameEvent treeChoppedEvent;


    private void Awake()
    {
        if (treeTopRigidbody == null)
        {
            treeTopRigidbody = GetComponentInChildren<Rigidbody>();
        }
    }

    public override void Spawned()
    {
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

    public bool CanInteract(Player player)
    {
        return IsInteractable && (playerRoleCanInteract & player.playerRole) != 0;
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
                        isTreeTopKinematic = false;
                        IsInteractable = false;
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

    private void onTreeTopRigidbodyChanged()
    {
        if (treeTopRigidbody != null)
        {
            treeTopRigidbody.isKinematic = isTreeTopKinematic;

            // When tree falls, notify objective system
            if (!isTreeTopKinematic && Runner.IsServer)
            {
                treeChoppedEvent.Raise();
            }
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
}