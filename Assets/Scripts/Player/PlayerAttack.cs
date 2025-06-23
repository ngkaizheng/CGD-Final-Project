using UnityEngine;
using Fusion;
using System.Collections;

public class PlayerAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Collider attackCollider; // Assign in Inspector
    [SerializeField] private float attackEnableDuration = 0.2f; // How long the collider stays enabled after attack hit

    [Networked] public float AttackDamage { get; set; } = 100f;
    [Networked] private float AttackCooldown { get; set; } = 0f;
    [SerializeField] private float attackCooldownTime = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource attackAudioSource; // Dedicated AudioSource for Attack SFX
    private SimpleAnimator simpleAnimator; // Reference to SimpleAnimator


    private void Awake()
    {
        simpleAnimator = GetComponent<SimpleAnimator>();
        if (attackCollider != null)
            attackCollider.enabled = false;

        // Initialize Attack AudioSource
        if (attackAudioSource == null)
        {
            attackAudioSource = gameObject.AddComponent<AudioSource>();
            attackAudioSource.playOnAwake = false;
            attackAudioSource.spatialBlend = 1f; // 3D audio
            attackAudioSource.maxDistance = 15f; // Suitable for melee attack range
            attackAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && AttackCooldown > 0f)
        {
            AttackCooldown -= Runner.DeltaTime;
        }
    }

    public void PerformAttack()
    {
        if (HasStateAuthority && AttackCooldown <= 0f)
        {
            AttackCooldown = attackCooldownTime;
            simpleAnimator.TriggerAttackAnimation(); // Trigger attack animation
            RPC_PlayAttackSound();
        }
    }

    // Called by animation event at the correct frame
    public void OnAttackHit()
    {
        if (!HasStateAuthority) return;
        if (attackCollider != null)
        {
            StartCoroutine(EnableAttackColliderTemporarily());
        }
    }

    private IEnumerator EnableAttackColliderTemporarily()
    {
        attackCollider.enabled = true;
        yield return new WaitForSeconds(attackEnableDuration);
        attackCollider.enabled = false;
    }

    // Example: handle collision with damageable targets
    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority || !attackCollider.enabled) return;

        Debug.Log($"[{Object.Id}] PlayerAttack: OnTriggerEnter with {other.name}");

        // Prevent self-hit
        if (other.transform.root == transform.root) return;

        IDamageable target = DamageableUtility.FindDamageable(other.transform.root.gameObject);
        if (target != null)
        {
            target.TakeDamage(AttackDamage, Object.InputAuthority);
            // Optionally, disable collider immediately after hit
            attackCollider.enabled = false;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackSound()
    {
        if (!Object.IsValid || attackAudioSource == null)
        {
            Debug.LogWarning($"[{Object.Id}] Cannot play Attack SFX: Object invalid or attackAudioSource is null!");
            return;
        }

        attackAudioSource.pitch = Random.Range(0.9f, 1.1f); // Random pitch for variation
        AudioController.Instance.PlaySoundEffect(SoundEffect.Attack, attackAudioSource);
        attackAudioSource.pitch = 1f; // Reset pitch
    }

}