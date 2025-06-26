using UnityEngine;
using Fusion;

public class Outsider : Player
{
    [Header("Outsider Settings")]
    [Networked] public PlayerHealth Health { get; private set; }
    [Networked] public bool IsEscaped { get; set; } = false;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Spawned()
    {
        base.Spawned();
        Health = GetComponent<PlayerHealth>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!isAlive()) return;
        base.FixedUpdateNetwork();
    }

    public override bool isAlive()
    {
        return Health != null && Health.IsAlive;
    }

    //Set isEscaped function
    public void SetIsEscaped(bool isEscaped)
    {
        IsEscaped = isEscaped;
    }
}