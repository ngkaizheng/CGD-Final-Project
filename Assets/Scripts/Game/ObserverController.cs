using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using Fusion;

public class ObserverController : MonoBehaviour
{
    [Header("Events")]
    public GameEvent gameInitEvent; // Assign in inspector

    public Dictionary<PlayerRef, PlayerCameraPair> outsiderCameras = new Dictionary<PlayerRef, PlayerCameraPair>();
    public Dictionary<PlayerRef, PlayerCameraPair> pontianakCameras = new Dictionary<PlayerRef, PlayerCameraPair>();

    public static ObserverController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (gameInitEvent != null)
            gameInitEvent.OnRaised.AddListener(OnGameInit);
    }

    private void OnDisable()
    {
        if (gameInitEvent != null)
            gameInitEvent.OnRaised.RemoveListener(OnGameInit);
    }

    private void OnGameInit()
    {
        StoreRoleCameras();
        ObserverUI.Instance.Initialize();
    }

    private void StoreRoleCameras()
    {
        outsiderCameras.Clear();
        pontianakCameras.Clear();

        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var pair = new PlayerCameraPair
            {
                Camera = player.simpleCameraFollow.vcam,
                Player = player
            };

            if (pair.Camera != null) // Only store valid pairs
            {
                if (player.playerRole == PlayerRole.OUTSIDER)
                    outsiderCameras[player.Object.InputAuthority] = pair;
                else if (player.playerRole == PlayerRole.PONTIANAK)
                    pontianakCameras[player.Object.InputAuthority] = pair;
            }
        }
    }

    // Get a player's camera by PlayerRef
    public PlayerCameraPair GetOutsiderCameraPair(PlayerRef playerRef)
        => outsiderCameras.TryGetValue(playerRef, out var pair) ? pair : default;

    public PlayerCameraPair GetPontianakCameraPair(PlayerRef playerRef)
        => pontianakCameras.TryGetValue(playerRef, out var pair) ? pair : default;
}

[System.Serializable]
public struct PlayerCameraPair
{
    public CinemachineCamera Camera;
    public Player Player;

    public bool IsValid => Camera != null && Player != null;
    public bool IsAlive => IsValid && Player.isAlive();
}