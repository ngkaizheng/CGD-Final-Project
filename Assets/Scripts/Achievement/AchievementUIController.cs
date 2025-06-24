using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUIController : MonoBehaviour
{
    public static AchievementUIController Instance { get; private set; }

    [Header("Section References")]
    public Button section1Button; //Outsider
    public Button section2Button; //Pontianak
    public GameObject section1Content;
    public GameObject section2Content;

    [Header("Achievement Section")]
    [SerializeField] private Transform section1AchievementContentParent;
    [SerializeField] private Transform section2AchievementContentParent;
    [SerializeField] private GameObject achievementItemPrefab;

    [Header("Achievement Data")]
    [SerializeField] private List<AchievementData> outsiderAchievements;
    [SerializeField] private List<AchievementData> pontianakAchievements;

    // Separate lists for each section's spawned items
    private List<GameObject> spawnedOutsiderAchievementItems = new List<GameObject>();
    private List<GameObject> spawnedPontianakAchievementItems = new List<GameObject>();

    [Header("Events")]
    public GameEvent OnAchievementsLoaded;

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
        section1Button.onClick.AddListener(() => ShowSection(AchievementSection.Outsider));
        section2Button.onClick.AddListener(() => ShowSection(AchievementSection.Pontianak));
        OnAchievementsLoaded.OnRaised.AddListener(PopulateAllAchievementSections);
        ShowSection(AchievementSection.Outsider);
    }

    private void OnDestroy()
    {
        section1Button.onClick.RemoveListener(() => ShowSection(AchievementSection.Outsider));
        section2Button.onClick.RemoveListener(() => ShowSection(AchievementSection.Pontianak));
        OnAchievementsLoaded.OnRaised.RemoveListener(PopulateAllAchievementSections);
    }

    private void ShowSection(AchievementSection section)
    {
        section1Content.SetActive(section == AchievementSection.Outsider);
        section2Content.SetActive(section == AchievementSection.Pontianak);
    }

    #region Achievement Management
    private void PopulateAllAchievementSections()
    {
        ClearAchievementItems();

        PopulateOutsiderAchievements();
        PopulatePontianakAchievements();
    }

    private void PopulateOutsiderAchievements()
    {
        PopulateAchivementSection(outsiderAchievements, section1AchievementContentParent, spawnedOutsiderAchievementItems);
    }

    private void PopulatePontianakAchievements()
    {
        PopulateAchivementSection(pontianakAchievements, section2AchievementContentParent, spawnedPontianakAchievementItems);
    }

    private void PopulateAchivementSection(List<AchievementData> achievements, Transform contentParent, List<GameObject> spawnedItems)
    {
        foreach (var achievement in achievements)
        {
            GameObject item = Instantiate(achievementItemPrefab, contentParent);
            item.GetComponent<AchievementItemUI>().Setup(achievement);
            spawnedItems.Add(item);
        }
    }

    private void ClearAchievementItems()
    {
        foreach (var item in spawnedOutsiderAchievementItems)
        {
            Destroy(item);
        }
        spawnedOutsiderAchievementItems.Clear();

        foreach (var item in spawnedPontianakAchievementItems)
        {
            Destroy(item);
        }
        spawnedPontianakAchievementItems.Clear();
    }
    #endregion
}

public enum AchievementSection
{
    Outsider,
    Pontianak
}