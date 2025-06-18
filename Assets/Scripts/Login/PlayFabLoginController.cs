using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.SceneManagement;
using Unity.Collections;

public class PlayFabLoginController : MonoBehaviour
{
    [Header("PlayFab Login Info")]
    [Tooltip("Custom ID for logging into PlayFab")]
    public string customId;

    [Tooltip("Title ID for your PlayFab game (set in PlayFab dashboard)")]
    [SerializeField] private string titleId = "10DA3D";

    [Header("PlayFab Account Info (Read-Only)")]
    [ReadOnly] public string playFabId;
    [ReadOnly] public string entityId;
    [ReadOnly] public string entityType;

    /// <summary>
    /// Callback for login related events.
    /// This can be used to update UI or trigger other actions when login status changes.
    /// </summary>
    public Action<string> OnLoginStatusUpdated;
    /// <summary>
    /// Callback for sign-up related events.
    /// This can be used to update UI or trigger other actions when sign-up status changes.
    /// </summary>
    public Action<string> OnSignUpStatusUpdated;
    public static PlayFabLoginController Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    #region Login Methods
    public void LoginWithEmail(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            TitleId = titleId,
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, result =>
        {
            Debug.Log("Login successful!");
            playFabId = result.PlayFabId;
            entityId = result.EntityToken.Entity.Id;
            entityType = result.EntityToken.Entity.Type;

            StartCoroutine(LoadLoginSceneAsync());
        },
        error =>
        {
            Debug.LogWarning($"Login failed: {error.ErrorMessage}");
            if (error.Error == PlayFabErrorCode.AccountNotFound)
            {
                OnLoginStatusUpdated?.Invoke("Account not found. Please sign up first.");
            }
            else
            {
                Debug.LogWarning($"Login error: {error.GenerateErrorReport()}");
                OnLoginStatusUpdated?.Invoke(error.ErrorMessage);
            }
        });
    }
    #endregion

    #region Sign Up Methods
    public void SignUpWithEmail(string email, string password, string displayName)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            DisplayName = displayName,
            TitleId = titleId,
            RequireBothUsernameAndEmail = false,
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, result =>
        {
            Debug.Log("Sign up successful!");
            playFabId = result.PlayFabId;
            entityId = result.EntityToken.Entity.Id;
            entityType = result.EntityToken.Entity.Type;

            OnSignUpStatusUpdated?.Invoke("Sign up successful! You can now log in.");
        },
        error =>
        {
            Debug.LogWarning($"Sign up failed: {error.GenerateErrorReport()}");
            OnSignUpStatusUpdated?.Invoke(error.ErrorMessage);
        });
    }
    #endregion

    private System.Collections.IEnumerator LoadLoginSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GameConfig.MAIN_MENU_SCENE);
        asyncLoad.allowSceneActivation = false;

        int countdown = 3;
        float timer = 0f;
        float interval = 1f;

        // Show countdown: 3, 2, 1
        while (countdown > 0)
        {
            Debug.Log(countdown); // Replace with your UI update if needed
                                  // TODO: Update your countdown UI here

            timer = 0f;
            while (timer < interval)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            countdown--;
        }

        // After countdown, wait until loading is done
        Debug.Log("Waiting for scene to finish loading...");
        // TODO: Update your UI to show "1" or "Loading..." here

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Now allow the scene to activate
        asyncLoad.allowSceneActivation = true;
    }

}