using Fusion;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Game Timer")]
    [SerializeField] private float gameDurationSeconds = 60f; // 1 minute

    public GameEvent gameInitEvent; // Assign in inspector

    [Networked] public TickTimer GameTimer { get; set; }

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
        DontDestroyOnLoad(gameObject);

        // Timer will be started by OnGameInit
    }

    private void OnGameInit()
    {
        if (Object.HasStateAuthority)
        {
            GameUI.SetGameTimerActive(true);
            GameTimer = TickTimer.CreateFromSeconds(Runner, gameDurationSeconds);
            StartCoroutine(GameTimerCoroutine());
        }
    }

    private IEnumerator GameTimerCoroutine()
    {
        // Wait until the timer expires
        while (!GameTimer.Expired(Runner))
        {
            yield return null;
        }
        GameTimer = TickTimer.None;
        GameUI.SetGameTimerExpired();
        RPC_ShowEndGame();
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
        StartCoroutine(EndGameRoutine());
    }

    private IEnumerator EndGameRoutine()
    {
        if (!(Runner.IsServer || Runner.IsSharedModeMasterClient))
        {
            Runner.Shutdown();
            SceneManager.LoadScene(GameConfig.MAIN_MENU_SCENE);
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            Runner.Shutdown();
            SceneManager.LoadScene(GameConfig.MAIN_MENU_SCENE);
        }
    }
}