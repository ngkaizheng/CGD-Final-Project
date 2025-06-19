using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;

public class PlayFabUserController : MonoBehaviour
{
    public static PlayFabUserController Instance;

    [Header("User Info")]
    public string Email;
    public string DisplayName;
    public string PlayFabId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUserInfo(string email, string displayName, string playFabId)
    {
        Email = email;
        DisplayName = displayName;
        PlayFabId = playFabId;
    }

    public void ClearUserInfo()
    {
        Email = null;
        DisplayName = null;
        PlayFabId = null;
    }

    //Update user displayname

    public void UpdateDisplayname(string newDisplayName)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newDisplayName
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => OnDisplayNameUpdated(result),
            error => OnDisplayNameUpdateError(error)
        );
    }

    private void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log($"Display name updated to: {result.DisplayName}");
        DisplayName = result.DisplayName;
    }

    private void OnDisplayNameUpdateError(PlayFabError error)
    {
        Debug.LogError($"Failed to update display name: {error.GenerateErrorReport()}");
        // You can add error UI here
    }
}