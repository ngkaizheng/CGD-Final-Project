using Fusion;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Game Timer")]
    [SerializeField] public float gameDurationSeconds = 60f; // 1 minute

    public GameEvent gameInitEvent; // Assign in inspector

    [Networked] public TickTimer GameTimer { get; set; }
    [Networked] public TickTimer LastChanceTimer { get; set; }

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

    public override void Spawned()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);

        // Timer will be started by OnGameInit
    }

    private void OnGameInit() //Only Server is executing this function
    {
        if (Object.HasStateAuthority)
        {
            GameTimer = TickTimer.CreateFromSeconds(Runner, gameDurationSeconds);
            StartCoroutine(GameTimerCoroutine());
        }
    }

    private IEnumerator GameTimerCoroutine()
    {
        RPC_SetGameTimerActive(true);
        // Wait until the timer expires
        while (!GameTimer.Expired(Runner))
        {
            yield return null;
        }
        GameTimer = TickTimer.None;
        RPC_SetGameTimerExpired();


        if (Object.HasStateAuthority)
            LastChanceTimer = TickTimer.CreateFromSeconds(Runner, 5f);

        // Wait until last chance timer expires
        while (!LastChanceTimer.Expired(Runner))
        {
            yield return null;
        }
        LastChanceTimer = TickTimer.None;

        // Damage all Outsiders using InGamePlayerManager
        foreach (var playerRef in InGamePlayerManager.Instance.outsiderDataDict)
        {
            // Get the Player object for this PlayerRef
            var playerObj = Runner.GetPlayerObject(playerRef);
            if (playerObj == null) continue;

            var outsider = playerObj.GetComponentInChildren<Outsider>();
            if (outsider != null && outsider.isAlive() && !outsider.IsEscaped)
            {
                outsider.Health.TakeDamage(100, InGamePlayerManager.Instance.pontianakDataDict[0]);
            }
        }
        // RPC_ShowEndGame();
    }

    private void InitializeUIForLocalPlayer()
    {
        if (Runner.TryGetPlayerObject(Runner.LocalPlayer, out var playerObj))
        {
            var playerData = playerObj.GetComponentInChildren<Player>();
            ObjectiveUI.SetActiveForRole(playerData.playerRole);
            HauntCooldownUI.SetActiveForRole(playerData.playerRole);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetGameTimerActive(bool isActive)
    {
        GameUI.SetGameTimerActive(isActive);
        InitializeUIForLocalPlayer();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetGameTimerExpired()
    {
        GameUI.SetGameTimerExpired();
    }


    // public override void Render()
    // {
    //     if (!Object.IsValid) return;
    //     if (Object.HasStateAuthority && GameTimer.Expired(Runner))
    //     {
    //         GameTimer = TickTimer.None;
    //         RPC_ShowEndGame();
    //     }
    // }
    public override void Render()
    {
        if (GameTimer.IsRunning)
        {
            float secondsLeft = (float)GameTimer.RemainingTime(Runner);
            GameUI.UpdateGameTimer(secondsLeft);
        }
        else if (GameTimer.Expired(Runner))
        {
            GameUI.UpdateGameTimer(0);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowEndGame()
    {
        // EndGameUI.Instance.Show(isHost: Runner.IsServer || Runner.IsSharedModeMasterClient);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_EndGame()
    {
        // StartCoroutine(EndGameRoutine());
        StartCoroutine(EndGame());
    }

    // private IEnumerator EndGameRoutine()
    // {
    //     if (!(Runner.IsServer || Runner.IsSharedModeMasterClient))
    //     {
    //         Runner.Shutdown();
    //         SceneManager.LoadScene(GameConfig.MAIN_MENU_SCENE);
    //         yield break;
    //     }
    //     else
    //     {
    //         yield return new WaitForSeconds(0.5f);
    //         Runner.Shutdown();
    //         SceneManager.LoadScene(GameConfig.MAIN_MENU_SCENE);
    //     }
    // }

    public void EndGameCheck()
    {
        if (Runner.IsServer)
        {
            RPC_EndGame();
        }
        else
        {
            StartCoroutine(EndGame());
        }
    }

    public IEnumerator EndGame()
    {
        if (Runner.IsServer)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Runner.Shutdown();
        SceneManager.LoadScene(GameConfig.MAIN_MENU_SCENE);
    }

    #region Time Used
    public int GetCurrentTimeUsedMilliseconds()
    {
        if (GameTimer.IsRunning)
        {
            float remainingTime = (float)GameTimer.RemainingTime(Runner);
            float timeUsedSeconds = gameDurationSeconds - remainingTime;
            return Mathf.RoundToInt(timeUsedSeconds * 1000); // Convert to milliseconds
        }
        return 0;
    }
    #endregion
}