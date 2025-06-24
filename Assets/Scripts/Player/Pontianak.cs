using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class Pontianak : Player
{
    [Header("Pontianak Abilities")]
    [SerializeField] private List<PlayerAbility> abilities = new List<PlayerAbility>();
    private Dictionary<InputButton, PlayerAbility> abilityMap = new Dictionary<InputButton, PlayerAbility>();

    public PlayerAttack playerAttack;
    [Networked] public bool isAttacking { get; set; }

    protected override void Awake()
    {
        base.Awake();
        playerAttack = GetComponent<PlayerAttack>();
    }

    public override void Spawned()
    {
        base.Spawned();
        BuildAbilityMap();
    }

    public override void FixedUpdateNetwork()
    {
        if (!isAlive()) return;
        base.FixedUpdateNetwork();
    }

    public void CheckAbilityInput(NetworkButtons currentButtons, NetworkButtons prevButtons)
    {
        foreach (var kvp in abilityMap)
        {
            if (kvp.Value.CanActivate(currentButtons, prevButtons))
            {
                Debug.Log($"Activating ability: {kvp.Value.GetType().Name} for player {Object.InputAuthority}");
                kvp.Value.Activate();
                break; // Only activate one ability per input
            }
        }
    }

    private void BuildAbilityMap()
    {
        abilityMap.Clear();
        foreach (var ability in abilities)
        {
            if (ability != null)
                abilityMap[ability.RequiredInput] = ability;
        }
    }

    public void AddAbility(PlayerAbility newAbility)
    {
        if (newAbility != null && !abilities.Contains(newAbility))
        {
            abilities.Add(newAbility);
            BuildAbilityMap();
            Debug.Log($"Added new ability: {newAbility.GetType().Name}");
        }
    }

    public void RemoveAbility(PlayerAbility abilityToRemove)
    {
        if (abilities.Contains(abilityToRemove))
        {
            abilities.Remove(abilityToRemove);
            BuildAbilityMap();
            Debug.Log($"Removed ability: {abilityToRemove.GetType().Name}");
        }
    }

    protected override bool IsMoveable()
    {
        if (isAttacking) // This is your Pontianak-specific flag
            return false;
        return base.IsMoveable();
    }
}