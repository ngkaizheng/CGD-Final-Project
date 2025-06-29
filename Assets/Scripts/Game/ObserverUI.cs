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

    public List<PlayerRef> allPlayerRefs = new List<PlayerRef>();
    public int currentIndex = 0;
    public CinemachineCamera currentActiveCamera; // Track currently active camera

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
                Debug.Log($"[ObserverUI] Switched to previous player at index {currentIndex}.");
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
                Debug.Log($"[ObserverUI] Switched to next player at index {currentIndex}.");
                ShowCurrentCamera();
                return;
            }
        }
    }

    private bool IsValidCameraTarget(PlayerRef playerRef)
    {
        var outsiderPair = ObserverController.Instance.GetOutsiderCameraPair(playerRef);
        if (outsiderPair.IsValid && outsiderPair.IsAlive && !outsiderPair.IsEscaped)
        {
            Debug.Log($"[ObserverUI] PlayerRef {playerRef} is a valid outsider camera target.");
            return true;
        }

        var pontianakPair = ObserverController.Instance.GetPontianakCameraPair(playerRef);
        bool validPontianak = pontianakPair.IsValid && pontianakPair.IsAlive;
        if (validPontianak)
            Debug.Log($"[ObserverUI] PlayerRef {playerRef} is a valid pontianak camera target.");
        return validPontianak;
    }

    private bool ShouldShowOutsiders()
    {
        foreach (var playerRef in InGamePlayerManager.Instance.outsiderDataDict)
        {
            var pair = ObserverController.Instance.GetOutsiderCameraPair(playerRef);
            if (pair.IsValid && pair.IsAlive && !pair.IsEscaped)
            {
                Debug.Log("[ObserverUI] At least one outsider is alive and not escaped.");
                return true;
            }
        }
        Debug.Log("[ObserverUI] No living outsiders found, will fallback to pontianak.");
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
            if (outsiderPair.IsValid && outsiderPair.IsAlive && !outsiderPair.IsEscaped)
            {
                Debug.Log($"[ObserverUI] Showing outsider camera for PlayerRef {allPlayerRefs[currentIndex]}.");
                SetActiveCamera(outsiderPair.Camera);
                return;
            }
            // If not valid, find next living outsider
            Debug.Log("[ObserverUI] Current outsider camera invalid, searching for next.");
            OnNextClicked();
        }
        else
        {
            // No outsiders alive, fallback to living pontianak
            var pontianakPair = ObserverController.Instance.GetPontianakCameraPair(allPlayerRefs[currentIndex]);
            if (pontianakPair.IsValid && pontianakPair.IsAlive)
            {
                Debug.Log($"[ObserverUI] Showing pontianak camera for PlayerRef {allPlayerRefs[currentIndex]}.");
                SetActiveCamera(pontianakPair.Camera);
                return;
            }
            // If not valid, find next living pontianak
            Debug.Log("[ObserverUI] Current pontianak camera invalid, searching for next.");
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
        if (show)
            OnNextClicked();
    }
    #endregion
}