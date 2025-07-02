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
    [SerializeField] private float freezeDuration = 2.8f;
    private Dictionary<GameObject, Coroutine> _activeXRayEffects = new Dictionary<GameObject, Coroutine>();

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("VFX Settings")]
    [SerializeField] private float shakeAmplitude = 0.5f;
    [SerializeField] private float shakeFrequency = 50f;
    [SerializeField] private float shakeDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Add this line


    // [SerializeField] private ParticleSystem hauntVFX;
    [Networked, Capacity(8)] NetworkLinkedList<PlayerRef> HauntedPlayers => default;
    [Networked, Capacity(8)] NetworkLinkedList<Outsider> HauntedOutsiders => default;

    private SimpleAnimator simpleAnimator;
    private Pontianak pontianak; // Reference to Pontianak component

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D audio
        }
        simpleAnimator = GetComponentInParent<SimpleAnimator>();
        pontianak = GetComponentInParent<Pontianak>();

        HauntCooldownUI.Instance.Initialize(this);
    }

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
        StartCoroutine(SetUsingAbilityState(pontianak, freezeDuration));

        RPC_HauntEffectsForAll();

        // Notify Pontianak
        // if (HauntedPlayers.Count > 0)
        RPC_PontianakFeedback();
    }

    private IEnumerator SetUsingAbilityState(Pontianak pontianak, float duration)
    {
        pontianak.isUsingAbility = true;
        yield return new WaitForSeconds(duration);
        pontianak.isUsingAbility = false;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HauntEffectsForAll()
    {
        // Prevent server/client mode issues
        // if (Object.HasStateAuthority && !Object.HasInputAuthority) return;
        simpleAnimator.TriggerScreamAnimation();

        Debug.Log("Applying haunt effects for all players!");
        CameraShaker.Instance.ShakeOnce(shakeAmplitude, shakeFrequency, shakeDuration);
        AudioController.Instance.PlaySoundEffect(SoundEffect.PontianakHaunt, audioSource, true);
        // if (Object.InputAuthority == Runner.LocalPlayer) return; // Pontianak's machine
        // StartCoroutine(CameraController.Instance.ChangeVignetteIntensity(0f, 0.6f, revealDuration, fadeOutDuration, shakeCurve));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ApplyHauntEffects([RpcTarget] PlayerRef target)
    {
        Debug.Log($"You have been haunted! (to player {target})");
        // Add VFX, SFX, UI, etc. for the haunted player here
        //Find its own player object
        var playerObject = Runner.GetPlayerObject(target);
        if (playerObject == null)
        {
            Debug.LogWarning($"Player object for {target} not found!");
            return;
        }
        var camController = playerObject.GetComponentInChildren<CameraController>();
        if (camController == null)
        {
            Debug.LogWarning($"CameraController for player {target} not found!");
            return;
        }
        StartCoroutine(camController.ChangeVignetteIntensity(0f, 0.6f, revealDuration, fadeOutDuration, shakeCurve));

        // StartCoroutine(CameraController.Instance.ChangeVignetteIntensity(0f, 0.6f, revealDuration, fadeOutDuration, shakeCurve, shakeDuration));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_PontianakFeedback()
    {
        // Only runs on Pontianak's machine

        Debug.Log($"Haunted players: {string.Join(", ", HauntedPlayers)}");
        StartCoroutine(DelayedXRayEffect());
        // foreach (var outsider in HauntedOutsiders)
        // {
        //     if (outsider != null)
        //         StartCoroutine(ApplyXRayEffect(outsider.playerModel));
        // }
        // Add VFX, SFX, UI, etc. for the Pontianak here
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private IEnumerator DelayedXRayEffect()
    {
        yield return new WaitForSeconds(shakeDuration);
        foreach (var outsider in HauntedOutsiders)
        {
            if (outsider != null)
                StartCoroutine(ApplyXRayEffect(outsider.playerModel));
        }
    }

    // private IEnumerator ApplyXRayEffect(GameObject playerObj)
    // {
    //     var renderers = playerObj.GetComponentsInChildren<Renderer>();
    //     var originalLayers = new int[renderers.Length];
    //     // var originalMaterials = new Material[renderers.Length];

    //     // Store original state and apply X-ray
    //     for (int i = 0; i < renderers.Length; i++)
    //     {
    //         originalLayers[i] = renderers[i].gameObject.layer;
    //         // originalMaterials[i] = renderers[i].material;

    //         renderers[i].gameObject.layer = LayerMask.NameToLayer("X-Ray");
    //         // renderers[i].material = xRayMaterial;
    //     }

    //     // Wait for duration
    //     yield return new WaitForSeconds(revealDuration);

    //     // Restore original state
    //     for (int i = 0; i < renderers.Length; i++)
    //     {
    //         if (renderers[i] != null) // Check if object still exists
    //         {
    //             renderers[i].gameObject.layer = originalLayers[i];
    //             // renderers[i].material = originalMaterials[i];
    //         }
    //     }
    // }
    private IEnumerator ApplyXRayEffect(GameObject playerObj)
    {
        // Cancel previous effect if it exists
        if (_activeXRayEffects.TryGetValue(playerObj, out var existingCoroutine))
        {
            StopCoroutine(existingCoroutine);
            _activeXRayEffects.Remove(playerObj);
        }

        var renderers = playerObj.GetComponentsInChildren<Renderer>();
        var originalLayers = new int[renderers.Length];

        // Store original state
        for (int i = 0; i < renderers.Length; i++)
        {
            originalLayers[i] = renderers[i].gameObject.layer;
            renderers[i].gameObject.layer = LayerMask.NameToLayer("X-Ray");
        }

        // Start new effect and track it
        var coroutine = StartCoroutine(RestoreLayersAfterDelay(playerObj, renderers, originalLayers, revealDuration));
        _activeXRayEffects.Add(playerObj, coroutine);

        yield return coroutine;
    }

    private IEnumerator RestoreLayersAfterDelay(GameObject playerObj, Renderer[] renderers, int[] originalLayers, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Restore original layers
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].gameObject.layer = originalLayers[i];
            }
        }

        // Clean up
        _activeXRayEffects.Remove(playerObj);
    }
    private void OnDestroy()
    {
        // Stop all active X-Ray effects when the ability is destroyed
        foreach (var coroutine in _activeXRayEffects.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        _activeXRayEffects.Clear();
    }
}