using Fusion;
using UnityEngine;

public abstract class PlayerAbility : NetworkBehaviour
{
    [Header("Base Ability Settings")]
    [SerializeField] protected float cooldown = 3f;
    [SerializeField] protected InputButton inputButton = InputButton.Q;
    [Networked] protected TickTimer CooldownTimer { get; set; }
    protected Player player;

    public bool IsOnCooldown => !CooldownTimer.ExpiredOrNotRunning(Runner);
    public float CooldownProgress => IsOnCooldown ? CooldownTimer.RemainingTime(Runner).Value : 0f;
    public InputButton RequiredInput => inputButton;

    protected virtual void Awake()
    {
        player = CommonUtils.GetComponentAnywhere<Player>(gameObject);
    }

    public bool CanActivate(NetworkButtons currentButtons, NetworkButtons prevButtons)
    {
        return currentButtons.WasPressed(prevButtons, inputButton) && !IsOnCooldown;
    }

    // public virtual void Activate(bool inputPressed)
    // {
    //     if (inputPressed && !IsOnCooldown)
    //     {
    //         ExecuteAbility();
    //         CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);
    //     }
    // }
    public virtual void Activate()
    {
        if (!Object.HasStateAuthority) return;
        ExecuteAbility();
        CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldown);
    }

    protected abstract void ExecuteAbility();

    // For UI to display cooldown
    public virtual string GetAbilityName() => GetType().Name;

    //Get the cooldown duration in seconds
    public virtual float GetCooldownDuration()
    {
        return cooldown;
    }
}