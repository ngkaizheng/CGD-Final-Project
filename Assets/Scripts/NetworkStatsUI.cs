using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using TMPro;

public class NetworkStatsUI : NetworkBehaviour
{
    public GameObject fps; // Reference to the FPS text GameObject (child)
    public GameObject ping; // Reference to the Ping text GameObject (child)

    [Networked] private bool ShowStats { get; set; } // Networked toggle for stats visibility

    public static NetworkStatsUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Set target FPS to 120 (foreground & background)
        Application.targetFrameRate = 120;

        // Prevent Unity from slowing down in background
        Application.runInBackground = true; // Keep running when minimized
        QualitySettings.vSyncCount = 0;     // Disable VSync (avokes frame pacing issues)
    }

    public override void Spawned()
    {
        Runner.MakeDontDestroyOnLoad(gameObject);

        // Ensure fps and ping are assigned and have TMP_Text
        if (fps == null || ping == null)
        {
            Debug.LogWarning("FPS or Ping GameObject not assigned in Inspector!");
        }
        else
        {
            if (fps.GetComponent<TMP_Text>() == null)
                Debug.LogWarning("FPS GameObject has no TMP_Text component!");
            if (ping.GetComponent<TMP_Text>() == null)
                Debug.LogWarning("Ping GameObject has no TMP_Text component!");
        }

        // Initialize ShowStats on server
        if (Object.HasStateAuthority)
        {
            ShowStats = true;
        }
    }

    public override void Render()
    {
        // Handle input only for local player
        if (HasInputAuthority && Keyboard.current?.f1Key.wasPressedThisFrame == true)
        {
            // Toggle stats visibility via RPC to sync across network
            RPC_ToggleStats();
            Debug.Log($"Stats toggled. ShowStats: {ShowStats}");
        }

        // Update UI if stats are visible
        if (ShowStats)
        {
            UpdateFPS();
            UpdatePing();
        }
        else
        {
            // Hide text if stats are disabled
            if (fps != null) fps.SetActive(false);
            if (ping != null) ping.SetActive(false);
        }
    }

    private void UpdateFPS()
    {
        if (fps != null)
        {
            TMP_Text fpsText = fps.GetComponent<TMP_Text>();
            if (fpsText != null)
            {
                fps.SetActive(true); // Ensure GameObject is active
                float fpsValue = 1.0f / Time.deltaTime;
                fpsText.text = $"FPS: {fpsValue:F1}";
            }
            else
            {
                Debug.LogWarning("FPS GameObject has no TMP_Text component!");
            }
        }
    }

    private void UpdatePing()
    {
        if (ping != null)
        {
            TMP_Text pingText = ping.GetComponent<TMP_Text>();
            if (pingText != null)
            {
                ping.SetActive(true); // Ensure GameObject is active
                if (Runner.IsRunning)
                {
                    double pingValue = Runner.GetPlayerRtt(PlayerRef.None) * 1000;
                    if (pingValue >= 0) // Valid ping value
                    {
                        pingText.text = $"Ping: {pingValue:F0}ms";
                    }
                    else
                    {
                        pingText.text = "Ping: N/A";
                        Debug.LogWarning($"Invalid ping value received: {pingValue}");
                    }
                }
                else
                {
                    pingText.text = "Ping: Not Connected";
                    Debug.LogWarning("NetworkRunner is not running. Ping not available.");
                }
            }
            else
            {
                Debug.LogWarning("Ping GameObject has no TMP_Text component!");
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ToggleStats()
    {
        if (Object.HasStateAuthority)
        {
            ShowStats = !ShowStats; // Toggle networked property on server
        }
    }
}