using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;


public class PlayFabInventoryController : MonoBehaviour
{
    public static PlayFabInventoryController Instance { get; private set; }


    // Stores which skins the player owns (using PlayFab Item IDs)
    private HashSet<string> ownedSkinIds = new HashSet<string>();

    public GameEvent OnInventoryLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadPlayerInventory();
    }

    public void LoadPlayerInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetInventorySuccess, OnGetInventoryFailure);
    }

    private void OnGetInventorySuccess(GetUserInventoryResult result)
    {
        ownedSkinIds.Clear();
        foreach (var item in result.Inventory)
        {
            if (item.ItemClass == "Skin")
            {
                ownedSkinIds.Add(item.ItemId);
                Debug.Log($"Owned skin: {item.ItemId}");
            }
        }

        Debug.Log("Player inventory loaded successfully.");
        OnInventoryLoaded.Raise();
    }

    private void OnGetInventoryFailure(PlayFabError error)
    {
        Debug.LogWarning($"Failed to load player inventory: {error.GenerateErrorReport()}");
    }

    public void PurchaseItem(string itemId, int price)
    {
        if (IsSkinOwned(itemId))
        {
            Debug.LogWarning($"Player already owns skin: {itemId}");
            return;
        }

        // Add the skin to the player's inventory
        var request = new PurchaseItemRequest
        {
            ItemId = itemId,
            VirtualCurrency = GameConfig.CURRENCY_CODE,
            Price = price,
            CatalogVersion = "Skin"
        };

        PlayFabClientAPI.PurchaseItem(request, OnPurchaseItemSuccess, OnPurchaseItemFailure);
    }
    private void OnPurchaseItemSuccess(PurchaseItemResult result)
    {
        foreach (var item in result.Items)
        {
            Debug.Log($"Successfully purchased item: {item.ItemId}");
            ownedSkinIds.Add(item.ItemId);
        }
        OnInventoryLoaded.Raise();
        PlayFabCurrencyController.Instance.GetCurrencyBalance(); // Refresh currency balance
    }
    private void OnPurchaseItemFailure(PlayFabError error)
    {
        Debug.LogWarning($"Failed to purchase item: {error.GenerateErrorReport()}");
    }


    public bool IsSkinOwned(string skinId)
    {
        return ownedSkinIds.Contains(skinId);
    }
}