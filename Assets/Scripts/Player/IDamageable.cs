using Fusion;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, PlayerRef attacker);
    void Heal(int amount);
    NetworkBool IsAlive { get; }
}

public static class DamageableUtility
{
    public static IDamageable FindDamageable(GameObject target)
    {
        if (target == null)
            return null;

        // 1. Check the GameObject itself
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log($"Found IDamageable on {target.name}");
            return damageable;
        }

        // 2. Check parents
        damageable = target.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log($"Found IDamageable in parent of {target.name}");
            return damageable;
        }

        // 3. Check children
        damageable = target.GetComponentInChildren<IDamageable>();
        if (damageable != null)
        {
            Debug.Log($"Found IDamageable in child of {target.name}");
            return damageable;
        }

        Debug.Log($"No IDamageable found for {target.name}");
        return null;
    }
}