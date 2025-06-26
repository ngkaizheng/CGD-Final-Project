using UnityEngine;
using Fusion;

public class EscapeDoor : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private Collider blockCollider; // Add this for blocking

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        Player player = other.GetComponentInParent<Player>();
        if (player != null && player.isAlive() && player.playerRole == PlayerRole.OUTSIDER)
        {
            EscapeController.Instance.HandlePlayerEscape(player);
        }
    }

    public void SetDoorActive(bool active)
    {
        triggerCollider.enabled = active;
        if (blockCollider != null)
            blockCollider.enabled = !active; // Disable block when door is active
    }
}