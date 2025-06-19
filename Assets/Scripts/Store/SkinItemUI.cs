using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SkinItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text skinNameText;
    [SerializeField] private Image skinIconImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private SkinData skinData;

    public void Setup(SkinData data, Action<SkinData> onBuy)
    {
        skinData = data;

        skinNameText.text = data.skinName;
        skinIconImage.sprite = data.skinIcon;
        priceText.text = data.price.ToString();

        bool isOwned = PlayFabInventoryController.Instance.IsSkinOwned(data.itemId);
        buyButton.interactable = !isOwned;
        buyButton.GetComponentInChildren<TMP_Text>().text = isOwned ? "Owned" : "Buy";
        buyButton.onClick.RemoveAllListeners();
        if (!isOwned)
            buyButton.onClick.AddListener(() => onBuy?.Invoke(skinData));
    }
}