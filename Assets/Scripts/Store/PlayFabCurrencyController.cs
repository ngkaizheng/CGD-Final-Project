using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabCurrencyController : MonoBehaviour
{
    public static PlayFabCurrencyController Instance { get; private set; }


    public GameEventInt OnCurrencyBalanceUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GetCurrencyBalance();
    }

    #region Grant Currency
    public void GrantCurrency(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            Amount = amount,
            VirtualCurrency = GameConfig.CURRENCY_CODE
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request, OnGrantCurrencySuccess, OnGrantCurrencyError);
    }

    private void OnGrantCurrencySuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log($"Granted {result.BalanceChange} {result.VirtualCurrency} to player. New balance: {result.Balance}");
        OnCurrencyBalanceUpdated?.Raise(result.Balance);
    }

    private void OnGrantCurrencyError(PlayFabError error)
    {
        Debug.LogWarning($"Failed to grant currency: {error.GenerateErrorReport()}");
        // You can add error UI here
    }
    #endregion

    #region  Get Currency Balance
    public void GetCurrencyBalance()
    {
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest(),
            result => OnGetCurrencyBalanceSuccess(result),
            error => OnGetCurrencyBalanceError(error)
        );
    }

    private void OnGetCurrencyBalanceSuccess(GetUserInventoryResult result)
    {
        if (result.VirtualCurrency != null && result.VirtualCurrency.TryGetValue(GameConfig.CURRENCY_CODE, out int balance))
        {
            Debug.Log($"Current currency balance: {balance}");
            OnCurrencyBalanceUpdated?.Raise(balance); // Raise event with current balance
        }
        else
        {
            Debug.LogWarning("Currency balance not found in inventory.");
            OnCurrencyBalanceUpdated?.Raise(0); // Return 0 if currency not found
        }
    }

    private void OnGetCurrencyBalanceError(PlayFabError error)
    {
        Debug.LogWarning($"Failed to get currency balance: {error.GenerateErrorReport()}");
        OnCurrencyBalanceUpdated?.Raise(0); // Return 0 on error
    }
    #endregion
}