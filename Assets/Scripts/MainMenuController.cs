using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuController : MonoBehaviour
{
    #region Variables
    [Header("Main Menu Section")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Multiplayer Section")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button quickPlayButton;
    [SerializeField] private Button multiplayerBackButton;
    [SerializeField] private TMP_InputField roomIdInputField;
    [SerializeField] private TMP_Text roomInputFieldStatusText;
    [SerializeField] private TMP_Text multiplayerMainStatusText;

    [Header("Lobby Section")]
    [SerializeField] private Button lobbyBackButton;
    [SerializeField] private Button startGameInLobbyButton;

    [Header("Store Section")]
    [SerializeField] private Button storeButton;
    [SerializeField] private Button storeBackButton;

    [Header("Settings Section")]
    [SerializeField] private Button settingsBackButton;

    [Header("Profile Section")]
    [SerializeField] private Button profileButton;

    [Header("Layout Settings")]
    [SerializeField] private GameObject mainMenuLayout;
    [SerializeField] private GameObject settingsLayout;
    [SerializeField] private GameObject multiplayerLayout;
    [SerializeField] private GameObject lobbyLayout;
    [SerializeField] private GameObject storeLayout;

    private LobbyLogic lobbyLogic;
    public static MainMenuController Instance { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lobbyLogic = FindAnyObjectByType<LobbyLogic>();

        // Main menu buttons
        startGameButton.onClick.AddListener(() => ShowSection(MenuState.Multiplayer));
        settingsButton.onClick.AddListener(() => GameSettings.Instance.ToggleSettingsPanel());
        exitButton.onClick.AddListener(ExitGame);

        // Multiplayer buttons
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        quickPlayButton.onClick.AddListener(QuickPlay);
        multiplayerBackButton.onClick.AddListener(() => ShowSection(MenuState.MainMenu));

        // Lobby buttons
        lobbyBackButton.onClick.AddListener(ExitNetworkAndShowMainMenu);
        startGameInLobbyButton.onClick.AddListener(() => StartGameOrReadyInLobby());

        // Settings buttons
        // settingsBackButton.onClick.AddListener(() => GameSettings.Instance.ToggleSettingsPanel());

        // Store button
        storeButton.onClick.AddListener(() => ShowSection(MenuState.Store));
        storeBackButton.onClick.AddListener(() => ShowSection(MenuState.MainMenu));

        // Profile button
        profileButton.onClick.AddListener(() => ProfileController.Instance.ShowProfilePanel());
    }

    private void Start()
    {
        ShowSection(MenuState.MainMenu);
    }
    private void OnDestroy()
    {
        startGameButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        createRoomButton.onClick.RemoveAllListeners();
        joinRoomButton.onClick.RemoveAllListeners();
        quickPlayButton.onClick.RemoveAllListeners();
        multiplayerBackButton.onClick.RemoveAllListeners();
        lobbyBackButton.onClick.RemoveAllListeners();
        startGameInLobbyButton.onClick.RemoveAllListeners();
        settingsBackButton.onClick.RemoveAllListeners();
        storeButton.onClick.RemoveAllListeners();
    }
    #endregion

    #region Navigation Methods

    public void ShowSection(MenuState state)
    {
        mainMenuLayout.SetActive(state == MenuState.MainMenu);
        multiplayerLayout.SetActive(state == MenuState.Multiplayer);
        lobbyLayout.SetActive(state == MenuState.Lobby);
        settingsLayout.SetActive(state == MenuState.Settings);
        storeLayout.SetActive(state == MenuState.Store);
        ClearUI();

        if (state == MenuState.Store)
            PlayFabCurrencyController.Instance.GetCurrencyBalance();
    }

    private async void ExitNetworkAndShowMainMenu()
    {
        await lobbyLogic.LeaveRoom();
        ShowSection(MenuState.MainMenu);
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Multiplayer Methods
    private async void CreateRoom()
    {
        //Random a room ID
        string roomId = $"Room{Random.Range(1000, 9999)}";
        var result = await lobbyLogic.CreateRoom(roomId);
        if (result.Ok)
        {
            ShowSection(MenuState.Lobby);
        }
        else
        {
            multiplayerMainStatusText.text = $"Failed to create room: {result.ErrorMessage}";
        }
    }

    private async void JoinRoom()
    {
        string roomId = roomIdInputField.text.Trim();
        if (string.IsNullOrEmpty(roomId))
        {
            roomInputFieldStatusText.text = "Please enter a valid room ID.";
            return;
        }

        var result = await lobbyLogic.JoinRoom(roomId);
        if (result.Ok)
        {
            ShowSection(MenuState.Lobby);
        }
        else
        {
            multiplayerMainStatusText.text = $"Failed to join room: {result.ErrorMessage}";
        }
    }

    private async void QuickPlay()
    {
        var result = await lobbyLogic.QuickPlay();
        if (result.Ok)
        {
            ShowSection(MenuState.Lobby);
        }
        else
        {
            multiplayerMainStatusText.text = $"Quick play failed: {result.ErrorMessage}";
        }
    }

    public async void HandleLeftRoom(bool isKicked = false)
    {
        await lobbyLogic.LeaveRoom();
        ShowSection(MenuState.MainMenu);
        // Optional: Show kick notification
    }

    private void StartGameOrReadyInLobby()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnStartGameOrReadyClicked();
        }
    }
    #endregion


    #region UI Update Methods
    public void UpdateRoomInputFieldStatus(string message)
    {
        roomInputFieldStatusText.text = message;
    }
    public void UpdateMultiplayerMainStatus(string message)
    {
        multiplayerMainStatusText.text = message;
    }
    public void ClearUI()
    {
        roomIdInputField.text = "";
        roomInputFieldStatusText.text = "";
        multiplayerMainStatusText.text = "";
    }
    #endregion
}

public enum MenuState
{
    MainMenu,
    Multiplayer,
    Lobby,
    Settings,
    Store
}