using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class OutsiderObjectiveController : NetworkBehaviour
{
    public static OutsiderObjectiveController Instance { get; private set; }

    [Header("Config")]
    // [SerializeField] private Collider escapeGateCollider;
    [SerializeField] private int spawnTreeEachOutsider = 3; // Number of trees each outsider needs to chop

    [Header("Events")]
    [SerializeField] private GameEvent gameInitEvent;
    [SerializeField] private GameEvent treeChoppedEvent;
    [SerializeField] private GameEvent objectiveCompleteEvent;

    [Networked] public int TreesRequired { get; set; }
    [Networked] public int TreesChopped { get; set; }

    private HashSet<Interactable> choppedTrees = new HashSet<Interactable>();

    private void Awake()
    {
        gameInitEvent.OnRaised.AddListener(OnGameInit);
    }

    private void OnDestroy()
    {
        gameInitEvent.OnRaised.RemoveListener(OnGameInit);
        treeChoppedEvent.OnRaised.RemoveListener(OnTreeChopped);
    }

    private void OnGameInit() //Only Server is executing this function
    {
        if (Runner.IsServer)
        {
            InitializeObjectives();
        }
    }

    public override void Spawned()
    {
        if (Instance != null && Instance != this)
        {
            Runner.Despawn(Object);
            return;
        }
        Instance = this;
        treeChoppedEvent.OnRaised.AddListener(OnTreeChopped);
    }

    private void InitializeObjectives()
    {
        // Calculate required trees (3 per outsider)
        int outsiderCount = InGamePlayerManager.Instance.outsiderDataDict.Count;
        TreesRequired = outsiderCount * spawnTreeEachOutsider;
        TreesChopped = 0;

        Debug.Log($"Initialized objectives: {TreesRequired} trees required ({outsiderCount} outsiders)");
    }

    private void OnTreeChopped()
    {
        if (!Runner.IsServer) return;

        TreesChopped++;
        Debug.Log($"Tree chopped! Progress: {TreesChopped}/{TreesRequired}");

        if (TreesChopped >= TreesRequired)
        {
            RPC_HandleObjectiveComplete();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HandleObjectiveComplete()
    {
        Debug.Log("OUTSIDER OBJECTIVE COMPLETE!");

        // // Disable escape gate collider
        // if (escapeGateCollider != null)
        // {
        //     escapeGateCollider.enabled = false;
        // }

        // Play sound/effects
        objectiveCompleteEvent.Raise();
    }

    // For UI to display progress
    public string GetObjectiveProgress()
    {
        return $"Chop Trees: {TreesChopped}/{TreesRequired}";
    }
}