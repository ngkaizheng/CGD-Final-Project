using UnityEngine;
using Fusion;

public class Outsider : Player
{
    [Header("Outsider Settings")]
    [Networked] public PlayerHealth Health { get; private set; }
    [Networked] public bool IsEscaped { get; set; } = false;

    [Header("Interaction Item")]
    [SerializeField] private GameObject axeObj; // Reference to the axe prefab in inspector

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Spawned()
    {
        base.Spawned();
        Health = GetComponent<PlayerHealth>();

        axeObj.SetActive(false);
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

    #region Axe Interaction
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetAxeActive(bool isActive)
    {
        if (axeObj != null)
        {
            axeObj.gameObject.SetActive(isActive);
        }
    }
    #endregion
}