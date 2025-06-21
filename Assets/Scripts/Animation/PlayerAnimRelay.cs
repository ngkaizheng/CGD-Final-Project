using UnityEngine;

public class PlayerAnimRelay : MonoBehaviour
{
    // private PlayerAttack playerAttack;
    private SimpleAnimator simpleAnimator;

    private void Awake()
    {
        // playerAttack = GetComponentInParent<PlayerAttack>();
        // if (playerAttack == null)
        // {
        //     Debug.LogError($"[{gameObject.name}] AnimationEventRelay: PlayerAttack component not found in parents!");
        // }

        simpleAnimator = GetComponentInParent<SimpleAnimator>();
        if (simpleAnimator == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerAnimRelay: SimpleAnimator component not found in parents!");
        }
    }

    // public void OnAttackHit()
    // {
    //     if (playerAttack != null)
    //     {
    //         Debug.Log($"[{gameObject.name}] AnimationEventRelay: Forwarding OnAttackHit to PlayerAttack.");
    //         playerAttack.OnAttackHit();
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"[{gameObject.name}] AnimationEventRelay: PlayerAttack reference is null, cannot call OnAttackHit!");
    //     }
    // }

    public void OnFootstep()
    {
        if (simpleAnimator != null)
        {
            Debug.Log($"[{gameObject.name}] PlayerAnimRelay: Forwarding OnFootstep to SimpleAnimator.");
            simpleAnimator.OnFootstep();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerAnimRelay: SimpleAnimator reference is null, cannot call OnFootstep!");
        }
    }

    public void OnJumpLand()
    {
        if (simpleAnimator != null)
        {
            Debug.Log($"[{gameObject.name}] PlayerAnimRelay: Forwarding OnJumpLand to SimpleAnimator.");
            simpleAnimator.OnJumpLand();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] PlayerAnimRelay: SimpleAnimator reference is null, cannot call OnJumpLand!");
        }
    }
}