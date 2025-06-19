using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private TMP_Text playerRoleText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Button playerRolePrevButton;
    [SerializeField] private Button playerRoleNextButton;

    [Header("Skin")]
    [SerializeField] private Image skinImage;
    [SerializeField] private Button prevSkinButton;
    [SerializeField] private Button nextSkinButton;

    [Header("Ready")]
    [Tooltip("Icon to indicate if the player is ready.")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Sprite readyIcon;

    [Header("Kick")]
    [Tooltip("Button to kick the player from the lobby. Only visible for host.")]
    [SerializeField] private Button kickButton;
    [SerializeField] private Sprite kickIcon;
    [SerializeField] private GameObject kickButtonContainer;

    private LobbyPlayerData _playerData;
    private PlayerRole _playerRole = PlayerRole.OUTSIDER; // Default role
    private int currentSkinIndex = 0;
    private List<SkinData> availableSkins; // for local player skin selection

    // public void Initialize(string playerId, LobbyPlayerData playerData, bool isLocalPlayer, bool isHost)
    // {
    //     _playerData = playerData;

    //     string displayId = playerId;
    //     var match = _playerIdRegex.Match(playerId);
    //     if (match.Success)
    //         displayId = $"P{match.Groups[1].Value}";

    //     playerIdText.text = displayId;
    //     playerNameText.text = isLocalPlayer ? $"{playerData.Nickname} (You)" : playerData.Nickname.ToString();

    //     if (readyButton != null)
    //     {
    //         if (isLocalPlayer)
    //         {
    //             readyButton.onClick.RemoveAllListeners();
    //             readyButton.onClick.AddListener(() => OnReadyButtonClicked());
    //         }
    //         readyButton.image.sprite = readyIcon;
    //         readyButton.image.color = playerData.IsReady ? UISettings.EnabledColor : UISettings.DisabledColor;

    //     }

    //     if (kickButton != null)
    //     {
    //         if (isHost && isLocalPlayer)
    //         {
    //             kickButton.gameObject.SetActive(false);
    //             return;
    //         }
    //         kickButton.gameObject.SetActive(isHost);
    //         kickButton.image.sprite = kickIcon;
    //         kickButton.onClick.RemoveAllListeners();
    //         kickButton.onClick.AddListener(() => OnKickButtonClicked());
    //     }
    // }
    public void Initialize(LobbyPlayerData playerData, bool isLocalPlayer, bool isHost, PlayerRole role = PlayerRole.OUTSIDER)
    {
        _playerData = playerData;
        _playerRole = role;
        // availableSkins = skins ?? PlayFabInventoryController.Instance.GetOwnedSkins();

        playerRoleText.text = _playerRole == PlayerRole.OUTSIDER ? "Outsider" : "Pontianak";
        playerNameText.text = (string)(isLocalPlayer ? $"{playerData.Nickname} (You)" : playerData.Nickname);

        // Skin setup
        // if (skinImage != null && availableSkins != null && availableSkins.Count > 0)
        // {
        //     currentSkinIndex = 0; // Reset to first skin
        //     UpdateSkinImage();
        // }
        // For local player, setup skin cycling
        if (isLocalPlayer)
        {
            availableSkins = PlayFabInventoryController.Instance.GetOwnedSkinsByRole(_playerRole);
            // Find the index of the currently selected skin
            currentSkinIndex = Mathf.Max(0, availableSkins.FindIndex(s => s.itemId == playerData.SelectedSkinId.ToString()));
        }
        else
        {
            availableSkins = null;
            currentSkinIndex = 0;
        }

        UpdateSkinImage(playerData.SelectedSkinId.ToString());

        if (prevSkinButton != null)
        {
            prevSkinButton.onClick.RemoveAllListeners();
            prevSkinButton.onClick.AddListener(OnPrevSkinClicked);
            prevSkinButton.interactable = isLocalPlayer && availableSkins.Count > 1;
        }

        if (nextSkinButton != null)
        {
            nextSkinButton.onClick.RemoveAllListeners();
            nextSkinButton.onClick.AddListener(OnNextSkinClicked);
            nextSkinButton.interactable = isLocalPlayer && availableSkins.Count > 1;
        }

        // Ready button
        if (readyButton != null)
        {
            if (isLocalPlayer)
            {
                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(OnReadyButtonClicked);
            }
            readyButton.image.sprite = readyIcon;
            UpdateReadyButton(playerData.IsReady);
        }

        // Kick button
        if (kickButton != null)
        {
            if (isHost && isLocalPlayer)
            {
                // kickButton.gameObject.SetActive(false);
                kickButtonContainer.SetActive(false);
            }
            else
            {
                kickButtonContainer.SetActive(isHost);
                // kickButton.gameObject.SetActive(isHost);
                kickButton.image.sprite = kickIcon;
                kickButton.onClick.RemoveAllListeners();
                kickButton.onClick.AddListener(OnKickButtonClicked);
            }
        }

        // Player role buttons
        if (playerRolePrevButton != null)
        {
            if (!isLocalPlayer)
            {
                playerRolePrevButton.gameObject.SetActive(false);
            }
            else
            {
                playerRolePrevButton.onClick.RemoveAllListeners();
                playerRolePrevButton.onClick.AddListener(OnRoleChangeButtonClicked);
            }
        }
        if (playerRoleNextButton != null)
        {
            if (!isLocalPlayer)
            {
                playerRoleNextButton.gameObject.SetActive(false);
            }
            else
            {
                playerRoleNextButton.onClick.RemoveAllListeners();
                playerRoleNextButton.onClick.AddListener(OnRoleChangeButtonClicked);
            }
        }
    }

    private void UpdateSkinImage(string skinId)
    {
        var skinData = PlayFabInventoryController.Instance.GetSkinData(skinId);
        if (skinImage != null && skinData != null)
        {
            skinImage.sprite = skinData.skinIcon;
        }
    }
    private void OnPrevSkinClicked()
    {
        if (availableSkins == null || availableSkins.Count == 0) return;
        currentSkinIndex = (currentSkinIndex - 1 + availableSkins.Count) % availableSkins.Count;
        var newSkin = availableSkins[currentSkinIndex];
        _playerData.RPC_SetSelectedSkin(newSkin.itemId);
        UpdateSkinImage(newSkin.itemId);
    }

    private void OnNextSkinClicked()
    {
        if (availableSkins == null || availableSkins.Count == 0) return;
        currentSkinIndex = (currentSkinIndex + 1) % availableSkins.Count;
        var newSkin = availableSkins[currentSkinIndex];
        _playerData.RPC_SetSelectedSkin(newSkin.itemId);
        UpdateSkinImage(newSkin.itemId);
    }

    private void OnRoleChangeButtonClicked()
    {
        _playerRole = (PlayerRole)(((int)_playerRole + 1) % Enum.GetValues(typeof(PlayerRole)).Length);
        _playerData.RPC_SetPlayerRole(_playerRole);
        playerRoleText.text = _playerRole == PlayerRole.OUTSIDER ? "Outsider" : "Pontianak";

        // Update available skins for new role
        availableSkins = PlayFabInventoryController.Instance.GetOwnedSkinsByRole(_playerRole);
        currentSkinIndex = 0;
        if (availableSkins.Count > 0)
        {
            var newSkin = availableSkins[0];
            _playerData.RPC_SetSelectedSkin(newSkin.itemId);
            UpdateSkinImage(newSkin.itemId);
        }

        // Update prev/next skin button interactability
        if (prevSkinButton != null)
            prevSkinButton.interactable = availableSkins.Count > 1;
        if (nextSkinButton != null)
            nextSkinButton.interactable = availableSkins.Count > 1;
    }

    private void OnDestroy()
    {
        if (kickButton != null)
        {
            kickButton.onClick.RemoveAllListeners();
        }
    }

    public void UpdatePlayerItem(string newName, bool isReady, bool isLocalPlayer, string selectedSkinId = null, PlayerRole? role = null)
    {
        // Update name
        playerNameText.text = isLocalPlayer ? $"{newName} (You)" : newName;

        // Update ready state
        if (readyButton != null)
        {
            UpdateReadyButton(isReady);
        }

        // Update role if provided
        if (role.HasValue)
        {
            _playerRole = role.Value;
            playerRoleText.text = _playerRole == PlayerRole.OUTSIDER ? "Outsider" : "Pontianak";
        }

        // Update skin if provided
        if (!string.IsNullOrEmpty(selectedSkinId))
        {
            UpdateSkinImage(selectedSkinId);
        }
    }

    public void UpdateReadyButton(bool isReady)
    {
        if (readyButton != null)
        {
            readyButton.image.color = isReady ? UISettings.EnabledColor : UISettings.DisabledColor;
            readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Ready" : "Not Ready";
        }
    }

    private void OnReadyButtonClicked()
    {
        _playerData.RPC_SetReady(!_playerData.IsReady);
    }

    private void OnKickButtonClicked()
    {
        _playerData.RPC_KickPlayer();
    }

    private static readonly System.Text.RegularExpressions.Regex _playerIdRegex =
    new System.Text.RegularExpressions.Regex(@"\[(?:Player|player):(\d+)\]");
}

