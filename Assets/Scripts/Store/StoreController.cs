using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreController : MonoBehaviour
{
    public static StoreController Instance { get; private set; }

    [Header("Section References")]
    public Button section1Button; //Outsider
    public Button section2Button; //Pontianak
    public GameObject section1Content;
    public GameObject section2Content;

    [Header("Skin Section")]
    [SerializeField] private Transform section1SkinContentParent;
    [SerializeField] private Transform section2SkinContentParent;
    [SerializeField] private GameObject skinItemPrefab;

    [Header("Skin Data")]
    [SerializeField] private List<SkinData> outsiderSkins;    // Assign ScriptableObjects in Inspector
    [SerializeField] private List<SkinData> pontianakSkins;   // Assign ScriptableObjects in Inspector

    // Separate lists for each section's spawned items
    private List<GameObject> spawnedOutsiderSkinItems = new List<GameObject>();
    private List<GameObject> spawnedPontianakSkinItems = new List<GameObject>();

    [Header("Currency UI")]
    [SerializeField] private TMP_Text playerBalanceText;
    private int playerBalance = 0;

    [Header("Events")]
    public GameEvent OnInventoryLoaded;
    public GameEventInt OnCurrencyBalanceUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        section1Button.onClick.AddListener(() => ShowSection(StoreSection.Outsider));
        section2Button.onClick.AddListener(() => ShowSection(StoreSection.Pontianak));

        OnInventoryLoaded.OnRaised.AddListener(PopulateAllSkinSections);
        ShowSection(StoreSection.Outsider);

        // Player balance UI
        OnCurrencyBalanceUpdated.OnRaised.AddListener(OnGetBalanceSuccess);
    }

    private void ShowSection(StoreSection section)
    {
        section1Content.SetActive(section == StoreSection.Outsider);
        section2Content.SetActive(section == StoreSection.Pontianak);
    }

    #region Skin Management
    private void PopulateAllSkinSections()
    {
        ClearSkinSection(spawnedOutsiderSkinItems);
        ClearSkinSection(spawnedPontianakSkinItems);

        PopulateOutsiderSkinSection();
        PopulatePontianakSkinSection();
    }

    private void PopulateOutsiderSkinSection()
    {
        PopulateSkinSection(outsiderSkins, section1SkinContentParent, spawnedOutsiderSkinItems);
    }

    private void PopulatePontianakSkinSection()
    {
        PopulateSkinSection(pontianakSkins, section2SkinContentParent, spawnedPontianakSkinItems);
    }
    private void PopulateSkinSection(List<SkinData> skinList, Transform parent, List<GameObject> spawnedList)
    {
        foreach (var skinData in skinList)
        {
            var skinItemGO = Instantiate(skinItemPrefab, parent);
            var skinItemUI = skinItemGO.GetComponent<SkinItemUI>();
            skinItemUI.Setup(skinData, OnBuySkinClicked);
            spawnedList.Add(skinItemGO);
        }
    }

    private void ClearSkinSection(List<GameObject> spawnedList)
    {
        foreach (var list in spawnedList)
        {
            Destroy(list);
        }
        spawnedList.Clear();
    }

    private void OnBuySkinClicked(SkinData skinData)
    {
        if (PlayFabInventoryController.Instance.IsSkinOwned(skinData.itemId))
        {
            Debug.LogWarning($"Player already owns skin: {skinData.itemId}");
            return;
        }
        PlayFabInventoryController.Instance.PurchaseItem(skinData.itemId, skinData.price);
    }
    #endregion

    #region Player Balance UI
    private void OnGetBalanceSuccess(int balance)
    {
        playerBalance = balance;
        if (playerBalanceText != null)
            playerBalanceText.text = $"{playerBalance}";
    }
    #endregion
}

public enum StoreSection
{
    Outsider,
    Pontianak
}


// private void PopulateOutsiderSkinSection()
// {
//     foreach (var skinData in outsiderSkins)
//     {
//         var skinItemGO = Instantiate(skinItemPrefab, section1SkinContentParent);
//         var skinItemUI = skinItemGO.GetComponent<SkinItemUI>();
//         skinItemUI.Setup(skinData, OnBuySkinClicked);
//         spawnedOutsiderSkinItems.Add(skinItemGO);
//     }
// }

// private void PopulatePontianakSkinSection()
// {
//     foreach (var skinData in pontianakSkins)
//     {
//         var skinItemGO = Instantiate(skinItemPrefab, section2SkinContentParent);
//         var skinItemUI = skinItemGO.GetComponent<SkinItemUI>();
//         skinItemUI.Setup(skinData, OnBuySkinClicked);
//         spawnedPontianakSkinItems.Add(skinItemGO);
//     }
// }