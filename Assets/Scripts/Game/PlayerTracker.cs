using UnityEngine;
using Fusion;
using System.Collections;

public class PlayerTracker : NetworkBehaviour
{
    public static PlayerTracker Instance { get; private set; }

    [Networked, Capacity(8)]
    public NetworkLinkedList<PlayerRef> LivingPlayers { get; } = default;

    [Networked, Capacity(8)]
    public NetworkLinkedList<PlayerRef> EscapedPlayers { get; } = default;

    [Networked, Capacity(8)]
    public NetworkDictionary<PlayerRef, bool> PlayerRoles { get; } // False for Outsider, True for Pontianak
    [Header("Events")]
    [SerializeField] private GameEvent gameInitEvent;


    private void Awake()
    {
        gameInitEvent.OnRaised.AddListener(OnGameInit);
    }

    private void OnDestroy()
    {
        gameInitEvent.OnRaised.RemoveListener(OnGameInit);
    }

    private void OnGameInit() //Only Server is executing this function
    {
        if (Runner.IsServer)
        {
            InitializeLivingPlayers();
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
    }

    public void InitializeLivingPlayers()
    {
        LivingPlayers.Clear();
        EscapedPlayers.Clear();
        PlayerRoles.Clear();

        foreach (var playerRef in Runner.ActivePlayers)
        {
            // var player = Runner.GetPlayerObject(playerRef)?.GetComponentInChildren<Outsider>();
            // if (player != null && player.isAlive())
            // {
            //     LivingPlayers.Add(playerRef);
            // }
            // Check for Outsider
            var playerObj = Runner.GetPlayerObject(playerRef);
            if (playerObj != null)
            {
                // Check for Outsider
                var outsider = playerObj.GetComponentInChildren<Outsider>();
                if (outsider != null && outsider.isAlive())
                {
                    LivingPlayers.Add(playerRef);
                    PlayerRoles.Set(playerRef, false); // true for Outsider
                    continue;
                }
                // Check for Pontianak
                var pontianak = playerObj.GetComponentInChildren<Pontianak>();
                if (pontianak != null && pontianak.isAlive())
                {
                    // LivingPlayers.Add(playerRef); // No need for Pontianak
                    PlayerRoles.Set(playerRef, true); // false for Pontianak
                }
            }
        }
    }

    public void OnPlayerDied(PlayerRef playerRef)
    {
        LivingPlayers.Remove(playerRef);
    }

    public void OnPlayerLeft(PlayerRef playerRef)
    {
        LivingPlayers.Remove(playerRef);
        PlayerRoles.Remove(playerRef);
    }

    public void OnPlayerEscaped(PlayerRef playerRef)
    {
        LivingPlayers.Remove(playerRef);
        EscapedPlayers.Add(playerRef);
    }

    public bool IsAnyPlayerAlive()
    {
        return LivingPlayers.Count > 0;
    }

    public bool IsPlayerPontianak(PlayerRef playerRef)
    {
        return PlayerRoles.ContainsKey(playerRef) && PlayerRoles[playerRef];
    }

    public int GetTotalOutsiders(NetworkRunner runner)
    {
        int count = 0;
        foreach (var playerRef in runner.ActivePlayers)
        {
            if (runner.GetPlayerObject(playerRef)?.GetComponentInChildren<Outsider>() != null)
            {
                count++;
            }
        }
        return count;
    }
}