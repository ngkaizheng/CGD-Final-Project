using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class HauntAbility : PlayerAbility
{
    [Header("Haunt Settings")]
    [SerializeField] private float radius = 20f;
    [SerializeField] private float damage = 0f;
    [SerializeField] private float revealDuration = 5f;
    [SerializeField] private LayerMask affectedLayerMasks; // Use LayerMask array

    // [SerializeField] private ParticleSystem hauntVFX;
    [Networked, Capacity(8)] NetworkLinkedList<PlayerRef> HauntedPlayers => default;
    [Networked, Capacity(8)] NetworkLinkedList<Outsider> HauntedOutsiders => default;

    protected override void ExecuteAbility()
    {
        if (!Object.HasStateAuthority) return;

        Debug.Log("Pontianak uses Haunt ability!");

        // Play VFX
        // if (hauntVFX != null)
        //     Runner.Spawn(hauntVFX, transform.position);

        // Find all targets in radius on specified layers
        var colliders = Physics.OverlapSphere(transform.position, radius, affectedLayerMasks);

        // Clear previous results
        HauntedPlayers.Clear();
        HauntedOutsiders.Clear();

        foreach (var col in colliders)
        {
            Transform parent = col.transform.parent;
            // Debug.Log($"Found collider: {col.name} at position {col.transform.position} with parent {parent?.name}");
            if (parent != null && parent.TryGetComponent<Outsider>(out var outsider))
            {
                // Debug.Log($"Haunting outsider: {outsider.Object.InputAuthority}");

                // Add to haunted players list
                HauntedPlayers.Add(outsider.Object.InputAuthority);
                HauntedOutsiders.Add(outsider);

                // Send feedback to the haunted player
                RPC_ApplyHauntEffects(outsider.Object.InputAuthority);
            }
        }
        // Notify Pontianak
        if (HauntedPlayers.Count > 0)
            RPC_PontianakFeedback();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ApplyHauntEffects([RpcTarget] PlayerRef target)
    {
        Debug.Log($"You have been haunted! (to player {target})");
        // Add VFX, SFX, UI, etc. for the haunted player here
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_PontianakFeedback()
    {
        // Only runs on Pontianak's machine
        // Debug.Log($"Successfully haunted {HauntedCount} players!");

        Debug.Log($"Haunted players: {string.Join(", ", HauntedPlayers)}");
        foreach (var outsider in HauntedOutsiders)
        {
            if (outsider != null)
                StartCoroutine(ApplyXRayEffect(outsider.playerModel));
        }
        // Add VFX, SFX, UI, etc. for the Pontianak here
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private IEnumerator ApplyXRayEffect(GameObject playerObj)
    {
        var renderers = playerObj.GetComponentsInChildren<Renderer>();
        var originalLayers = new int[renderers.Length];
        // var originalMaterials = new Material[renderers.Length];

        // Store original state and apply X-ray
        for (int i = 0; i < renderers.Length; i++)
        {
            originalLayers[i] = renderers[i].gameObject.layer;
            // originalMaterials[i] = renderers[i].material;

            renderers[i].gameObject.layer = LayerMask.NameToLayer("X-Ray");
            // renderers[i].material = xRayMaterial;
        }

        // Wait for duration
        yield return new WaitForSeconds(revealDuration);

        // Restore original state
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) // Check if object still exists
            {
                renderers[i].gameObject.layer = originalLayers[i];
                // renderers[i].material = originalMaterials[i];
            }
        }
    }
}