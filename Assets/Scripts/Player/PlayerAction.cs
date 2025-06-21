using UnityEngine;
using Fusion;

public class PlayerAction : NetworkBehaviour
{
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactMask = default;

    private IInteractable lastPromptedInteractable;
    private bool isCurrentlyInteracting = false;
    private Player player;

    public override void Spawned()
    {
        player = GetComponent<Player>();
    }


    public override void Render()
    {
        if (!Object.HasInputAuthority) return;
        UpdateInteractionUI();
    }

    private IInteractable FindClosestInteractable(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, interactDistance, interactMask);
        IInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (var col in colliders)
        {
            if (col.transform.root.gameObject == player.transform.root.gameObject) continue;
            var interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float dist = Vector3.Distance(position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }
        }
        return closest;
    }

    public void UpdateInteractionUI()
    {
        var closestInteractable = FindClosestInteractable(player.transform.position);

        // Prompt UI
        if (lastPromptedInteractable != null && lastPromptedInteractable != closestInteractable)
            GameUI.SetInteractPromptActive(false, null);

        if (closestInteractable != null && closestInteractable != lastPromptedInteractable)
            GameUI.SetInteractPromptActive(true, closestInteractable);

        // if (isCurrentlyInteracting && closestInteractable == null)
        // {
        //     player.GetComponent<SimpleAnimator>().SetInteracting(false);
        //     isCurrentlyInteracting = false;
        // }

        // Progress Bar UI
        if (closestInteractable is IProgressInteractable progressInteractable)
        {
            GameUI.SetProgressBarActive(true);
            GameUI.UpdateProgressBar(progressInteractable.GetProgress());
        }
        else
        {
            GameUI.SetProgressBarActive(false);
        }

        lastPromptedInteractable = closestInteractable;
    }

    public void Interact(bool isInteracting)
    {
        var closestInteractable = FindClosestInteractable(player.transform.position);

        if (isInteracting && closestInteractable != null)
        {
            isCurrentlyInteracting = true;
            Debug.Log($"Interacting with {closestInteractable}");
            closestInteractable.OnInteract(player);
        }
        else if (isCurrentlyInteracting)
        {
            Debug.Log("Interact Action Released");
            player.GetComponent<SimpleAnimator>().SetInteracting(false);
            isCurrentlyInteracting = false;
        }
    }
}