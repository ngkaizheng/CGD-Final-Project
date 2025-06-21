using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(31505)]
public class SimpleAnimator : NetworkBehaviour
{
    private NetworkMecanimAnimator netAnim;
    private KCC kcc;
    // private PlayerAttack playerAttack; // Reference to PlayerAttack
    private Player player; // Reference to Player (for health/damage)
    private Animator animator;
    [SerializeField] private AudioSource movementAudioSource; // Dedicated AudioSource for Jump and Footstep SFX


    #region KCC State
    private Vector3 inputDirection;
    [Networked] private bool isGrounded { get; set; } // Networked grounded state
    [Networked] private bool isJump { get; set; } // Networked jump state
    [Networked] private bool isMoving { get; set; } // Networked moving state
    [Networked] private bool isSprint { get; set; } // Networked sprint state
    [Networked] private int AttackTick { get; set; } // Networked tick for attack trigger
    [Networked] private int UseItemTick { get; set; } // Networked tick for use item trigger
    [Networked] private int HurtTick { get; set; } // Networked tick for hurt trigger
    [Networked] private int DieTick { get; set; } // Networked tick for die trigger
    [Networked] private bool IsDead { get; set; } // Networked death state
    public int lastProcessedAttackTick; // Track the last processed attack tick
    public int lastProcessedUseItemTick; // Track the last processed use item tick
    public int lastProcessedHurtTick; // Track the last processed hurt tick
    public int lastProcessedDieTick; // Track the last processed die tick
    #endregion

    // STATIC MEMBER

    #region Animator Float Parameter
    // Defined based on Idle Walk Run Blend Speed Multiplier and Motion Threshold on StarterAssetsThirdPerson Animator
    private static readonly float IDLE = 0f;
    private static readonly float WALK = 2f;
    private static readonly float RUN = 6f;
    private static readonly float MULTIPLIER = 1f;
    #endregion

    #region Animator Hash
    // Defined based on StarterAssetsThirdPerson Animator Parameters
    private static readonly int SPEED_PARAM_HASH = Animator.StringToHash("Speed");
    private static readonly int JUMP_PARAM_HASH = Animator.StringToHash("Jump");
    private static readonly int IS_GROUNDED_PARAM_HASH = Animator.StringToHash("Grounded");
    private static readonly int FALL_PARAM_HASH = Animator.StringToHash("FreeFall");
    private static readonly int MOTION_SPEED_PARAM_HASH = Animator.StringToHash("MotionSpeed");
    private static readonly int ATTACK_PARAM_HASH = Animator.StringToHash("Attack");
    private static readonly int USE_ITEM_PARAM_HASH = Animator.StringToHash("UseItem");
    private static readonly int HURT_PARAM_HASH = Animator.StringToHash("Hurt");
    private static readonly int DIE_PARAM_HASH = Animator.StringToHash("Die");
    private static readonly int IS_DEAD_PARAM_HASH = Animator.StringToHash("IsDead");
    #endregion

    #region Movement State
    private float currentSpeedBlend = 0f;
    private float speedBlendDuration = 0.2f; // seconds to reach target
    #endregion
    private readonly SoundEffect[] footstepSounds = new SoundEffect[]
    {
        SoundEffect.Footstep_Player1,
        SoundEffect.Footstep_Player2,
        SoundEffect.Footstep_Player3,
        SoundEffect.Footstep_Player4,
        SoundEffect.Footstep_Player5
    };

    protected void Awake()
    {
        if (TryGetComponent(out NetworkMecanimAnimator anim))
        {
            netAnim = anim;
        }
        else
        {
            Debug.LogError("Could not find NetworkMecanimAnimator component on object " + gameObject.name);
        }

        if (TryGetComponent(out KCC kccComponent))
        {
            kcc = kccComponent;
        }
        else
        {
            Debug.LogError("Could not find KCC component on object " + gameObject.name);
        }
        // playerAttack = GetComponent<PlayerAttack>();
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();

        // Initialize movement AudioSource
        if (movementAudioSource == null)
        {
            movementAudioSource = gameObject.AddComponent<AudioSource>();
            movementAudioSource.playOnAwake = false;
            movementAudioSource.spatialBlend = 1f; // 3D audio
            movementAudioSource.maxDistance = 10f; // Shorter range for movement SFX
            movementAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (HasStateAuthority)
        {
            AttackTick = 0;
            UseItemTick = 0;
            HurtTick = 0;
            DieTick = 0;
            Debug.Log("Resetting animation ticks on scene load.");
        }
        Debug.Log("AttackTick: " + AttackTick + " UseItemTick: " + UseItemTick + " HurtTick: " + HurtTick + " DieTick: " + DieTick);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public override void FixedUpdateNetwork()
    {
        // if (!Runner.IsForward || IsProxy)
        //     return;
        if (!Runner.IsForward)
            return;
        CheckKCC();
        AnimatorBridge();
    }

    public override void Render()
    {
        // string logMessage = $"[{Object.Id}] Render, IsLocalPlayer={Object.InputAuthority == Runner.LocalPlayer}, ChildActive={gameObject.activeInHierarchy}";
        // Debug.Log(logMessage);
        // CheckKCC();
        AnimatorBridge();
    }

    private void CheckKCC()
    {
        KCCData fixedData = kcc.FixedData; // using KCC Fixed data


        inputDirection = fixedData.InputDirection; // Get input direction from KCC
        isMoving = !inputDirection.IsZero();

        isGrounded = fixedData.IsGrounded; // check grounded state from KCC
        isJump = fixedData.HasJumped; // check jump state from KCC
        // isSprint = fixedData.Sprint; // chechk sprint state KCC
        // isAttack = fixedData.isAttack; // check attack state from KCC
        Debug.Log("inputDirection: " + inputDirection + " isMoving: " + isMoving + " isGrounded: " + isGrounded + " isJump: " + isJump);
    }

    private void AnimatorBridge()
    {
        animator.SetFloat(MOTION_SPEED_PARAM_HASH, MULTIPLIER); // Add speed multiplier to Idle Walk Run Blend blend Tree
        animator.SetBool(IS_GROUNDED_PARAM_HASH, isGrounded); // set grounded bool state based on kcc.FixedData.IsGrounded
        animator.SetBool(IS_DEAD_PARAM_HASH, IsDead);

        if (isJump)
        {
            animator.SetBool(JUMP_PARAM_HASH, true); // set jump if kcc.FixedData.HasJumped
            if (HasStateAuthority)
            {
                RPC_PlayJumpSound();
            }
        }

        float targetSpeed = IDLE;
        if (isGrounded)
        {
            // set false to jump and fall parameter when grounded
            animator.SetBool(JUMP_PARAM_HASH, false);
            animator.SetBool(FALL_PARAM_HASH, false);

            // if (!isMoving)
            // {
            //     animator.SetFloat(SPEED_PARAM_HASH, IDLE); // Is not moving, set to Idle
            // }
            // else if (isSprint)
            // {
            //     animator.SetFloat(SPEED_PARAM_HASH, RUN); // Is moving and Sprint button pressed, set to Run
            // }
            // else
            // {
            //     animator.SetFloat(SPEED_PARAM_HASH, WALK); // Is moving, set to Walk
            // }
            if (!isMoving)
                targetSpeed = IDLE;
            else if (isSprint)
                targetSpeed = RUN;
            else
                targetSpeed = WALK;
        }
        else if (!isJump)
        {
            animator.SetBool(FALL_PARAM_HASH, true); // if not grounded and not jump the it is Fall
        }

        currentSpeedBlend = Mathf.MoveTowards(currentSpeedBlend, targetSpeed, (1f / speedBlendDuration) * Time.deltaTime);
        animator.SetFloat(SPEED_PARAM_HASH, currentSpeedBlend);

        // Set triggers only once per timer activation
        // Set triggers based on tick changes
        if (AttackTick > lastProcessedAttackTick)
        {
            Debug.Log("LastProcessedAttackTick: " + lastProcessedAttackTick + " AttackTick: " + AttackTick);
            animator.SetTrigger(ATTACK_PARAM_HASH);
            lastProcessedAttackTick = AttackTick;
            Debug.Log($"[{Object.Id}] Setting Attack trigger on tick {Runner.Tick}");
        }

        if (UseItemTick > lastProcessedUseItemTick)
        {
            animator.SetTrigger(USE_ITEM_PARAM_HASH);
            lastProcessedUseItemTick = UseItemTick;
            Debug.Log($"[{Object.Id}] Setting UseItem trigger on tick {Runner.Tick}");
        }

        if (HurtTick > lastProcessedHurtTick)
        {
            animator.SetTrigger(HURT_PARAM_HASH);
            lastProcessedHurtTick = HurtTick;
            Debug.Log($"[{Object.Id}] Setting Hurt trigger on tick {Runner.Tick}");
        }

        if (DieTick > lastProcessedDieTick)
        {
            animator.SetTrigger(DIE_PARAM_HASH);
            lastProcessedDieTick = DieTick;
            Debug.Log($"[{Object.Id}] Setting Die trigger on tick {Runner.Tick}");
        }
    }

    public void TriggerAttackAnimation()
    {
        if (!IsDead && HasStateAuthority)
        {
            AttackTick = Runner.Tick;
            Debug.Log($"[{Object.Id}] Triggered Attack animation (StateAuthority) on tick {Runner.Tick}");
        }
    }

    public void TriggerUseItemAnimation()
    {
        if (!IsDead && HasStateAuthority)
        {
            UseItemTick = Runner.Tick;
            Debug.Log($"[{Object.Id}] Triggered UseItem animation (StateAuthority) on tick {Runner.Tick}");
        }
    }

    public void TriggerHurtAnimation()
    {
        if (!IsDead && HasStateAuthority)
        {
            HurtTick = Runner.Tick;
            Debug.Log($"[{Object.Id}] Triggered Hurt animation (StateAuthority) on tick {Runner.Tick}");
        }
    }
    public void TriggerDieAnimation()
    {
        if (!IsDead && HasStateAuthority)
        {
            IsDead = true;
            DieTick = Runner.Tick;
            Debug.Log($"[{Object.Id}] Triggered Die animation (StateAuthority) on tick {Runner.Tick}");
        }
    }

    #region Audio SFX
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayJumpSound()
    {
        if (!Object.IsValid || movementAudioSource == null)
        {
            Debug.LogWarning($"[{Object.Id}] Cannot play Jump SFX: Object invalid or movementAudioSource is null!");
            return;
        }

        movementAudioSource.pitch = Random.Range(0.9f, 1.1f); // Random pitch for variation
        AudioController.Instance.PlaySoundEffect(SoundEffect.Jump, movementAudioSource);
        movementAudioSource.pitch = 1f; // Reset pitch
        Debug.Log($"[{Object.Id}] Played Jump SFX");
    }

    public void OnFootstep()
    {
        if (!HasStateAuthority || IsDead || !isMoving || !isGrounded) return;

        RPC_PlayFootstepSound();
        Debug.Log($"[{Object.Id}] Footstep triggered");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayFootstepSound()
    {
        if (!Object.IsValid || movementAudioSource == null)
        {
            Debug.LogWarning($"[{Object.Id}] Cannot play Footstep SFX: Object invalid or movementAudioSource is null!");
            return;
        }

        movementAudioSource.pitch = Random.Range(0.9f, 1.1f); // Random pitch for variation
        SoundEffect footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
        AudioController.Instance.PlaySoundEffect(footstepSound, movementAudioSource);
        movementAudioSource.pitch = 1f; // Reset pitch
        // Debug.Log($"[{Object.Id}] Played Footstep SFX");
    }

    public void OnJumpLand()
    {
        if (!HasStateAuthority || IsDead) return;

        RPC_PlayJumpLandSound();
        Debug.Log($"[{Object.Id}] Jump Land triggered");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayJumpLandSound()
    {
        if (!Object.IsValid || movementAudioSource == null)
        {
            Debug.LogWarning($"[{Object.Id}] Cannot play Jump Land SFX: Object invalid or movementAudioSource is null!");
            return;
        }

        movementAudioSource.pitch = Random.Range(0.9f, 1.1f); // Random pitch for variation
        AudioController.Instance.PlaySoundEffect(SoundEffect.JumpLand, movementAudioSource);
        movementAudioSource.pitch = 1f; // Reset pitch
        Debug.Log($"[{Object.Id}] Played Jump Land SFX");
    }

    #endregion
}

// SoundEffect[] hurtSounds = new SoundEffect[]
// {
//     SoundEffect.HurtPlayer1,
//     SoundEffect.HurtPlayer2,
//     SoundEffect.HurtPlayer3,
//     SoundEffect.HurtPlayer4,
//     SoundEffect.HurtPlayer5
// };
// SoundEffect randomHurtSound = hurtSounds[Random.Range(0, hurtSounds.Length)];
// GameController.Instance.RPC_PlaySoundEffect(randomHurtSound);