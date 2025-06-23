using UnityEngine;
using Fusion;

public class Outsider : Player
{
    [Header("Outsider Settings")]
    [Networked] public PlayerHealth Health { get; private set; }

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
}