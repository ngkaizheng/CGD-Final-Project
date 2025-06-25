using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections.Generic;
using Unity.Cinemachine;

public class ObserverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject observerPanel;

    private List<PlayerRef> allPlayerRefs = new List<PlayerRef>();
    private int currentIndex = 0;
    private CinemachineCamera currentActiveCamera; // Track currently active camera

    public static ObserverUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ShowObserverUI(false); // Start with UI hidden
    }

    // private void OnEnable()
    // {
    //     previousButton.onClick.AddListener(OnPreviousClicked);
    //     nextButton.onClick.AddListener(OnNextClicked);
    // }

    public void Initialize()
    {
        previousButton.onClick.AddListener(OnPreviousClicked);
        nextButton.onClick.AddListener(OnNextClicked);
        RefreshPlayerList();
    }

    private void OnDestroy()
    {
        previousButton.onClick.RemoveListener(OnPreviousClicked);
        nextButton.onClick.RemoveListener(OnNextClicked);

        // Clean up current camera reference
        if (currentActiveCamera != null)
        {
            currentActiveCamera.gameObject.SetActive(false);
            currentActiveCamera = null;
        }
    }

    private void RefreshPlayerList()
    {
        allPlayerRefs.Clear();
        // Combine both outsider and pontianak players
        allPlayerRefs.AddRange(InGamePlayerManager.Instance.outsiderDataDict);
        allPlayerRefs.AddRange(InGamePlayerManager.Instance.pontianakDataDict);

        currentIndex = 0;
    }

    private void OnPreviousClicked()
    {
        if (allPlayerRefs.Count == 0) return;

        for (int i = 1; i <= allPlayerRefs.Count; i++)
        {
            int testIndex = (currentIndex - i + allPlayerRefs.Count) % allPlayerRefs.Count;
            if (IsValidCameraTarget(allPlayerRefs[testIndex]))
            {
                currentIndex = testIndex;
                ShowCurrentCamera();
                return;
            }
        }
    }

    private void OnNextClicked()
    {
        if (allPlayerRefs.Count == 0) return;

        for (int i = 1; i <= allPlayerRefs.Count; i++)
        {
            int testIndex = (currentIndex + i) % allPlayerRefs.Count;
            if (IsValidCameraTarget(allPlayerRefs[testIndex]))
            {
                currentIndex = testIndex;
                ShowCurrentCamera();
                return;
            }
        }
    }

    private bool IsValidCameraTarget(PlayerRef playerRef)
    {
        // First try to get outsider camera
        var outsiderPair = ObserverController.Instance.GetOutsiderCameraPair(playerRef);
        if (outsiderPair.IsValid && outsiderPair.IsAlive) return true;

        // Fallback to pontianak camera if no valid outsider
        var pontianakPair = ObserverController.Instance.GetPontianakCameraPair(playerRef);
        return pontianakPair.IsValid && pontianakPair.IsAlive;
    }

    private bool ShouldShowOutsiders()
    {
        // Check if any outsiders are still alive
        foreach (var playerRef in InGamePlayerManager.Instance.outsiderDataDict)
        {
            var pair = ObserverController.Instance.GetOutsiderCameraPair(playerRef);
            if (pair.IsValid && pair.IsAlive) return true;
        }
        return false;
    }

    private void ShowCurrentCamera()
    {
        if (allPlayerRefs.Count == 0) return;

        bool outsidersAlive = ShouldShowOutsiders();

        if (outsidersAlive)
        {
            // Only show living outsider cameras
            var outsiderPair = ObserverController.Instance.GetOutsiderCameraPair(allPlayerRefs[currentIndex]);
            if (outsiderPair.IsValid && outsiderPair.IsAlive)
            {
                SetActiveCamera(outsiderPair.Camera);
                return;
            }
            // If not valid, find next living outsider
            OnNextClicked();
        }
        else
        {
            // No outsiders alive, fallback to living pontianak
            var pontianakPair = ObserverController.Instance.GetPontianakCameraPair(allPlayerRefs[currentIndex]);
            if (pontianakPair.IsValid && pontianakPair.IsAlive)
            {
                SetActiveCamera(pontianakPair.Camera);
                return;
            }
            // If not valid, find next living pontianak
            OnNextClicked();
        }
    }

    private void SetActiveCamera(CinemachineCamera newCamera)
    {
        // Disable previous camera if exists
        if (currentActiveCamera != null)
        {
            currentActiveCamera.gameObject.SetActive(false);
        }

        // Enable new camera and store reference
        newCamera.gameObject.SetActive(true);
        currentActiveCamera = newCamera;
    }

    #region Public Methods
    public void ShowObserverUI(bool show)
    {
        observerPanel.SetActive(show);
    }
    #endregion
}