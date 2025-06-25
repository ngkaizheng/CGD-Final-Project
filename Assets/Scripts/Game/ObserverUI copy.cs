// using UnityEngine;
// using UnityEngine.UI;
// using Fusion;
// using System.Collections.Generic;
// using Unity.Cinemachine;

// public class ObserverUI : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] private Button previousButton;
//     [SerializeField] private Button nextButton;

//     [Header("Role")]
//     [SerializeField] private bool isOutsider = true; // true = outsider, false = pontianak

//     private List<PlayerRef> playerRefs = new List<PlayerRef>();
//     private int currentIndex = 0;

//     private void OnEnable()
//     {
//         previousButton.onClick.AddListener(OnPreviousClicked);
//         nextButton.onClick.AddListener(OnNextClicked);
//         RefreshPlayerList();
//         ShowCurrentCamera();
//     }

//     private void OnDisable()
//     {
//         previousButton.onClick.RemoveListener(OnPreviousClicked);
//         nextButton.onClick.RemoveListener(OnNextClicked);
//     }

//     private void RefreshPlayerList()
//     {
//         playerRefs.Clear();
//         if (isOutsider)
//         {
//             foreach (var playerRef in InGamePlayerManager.Instance.outsiderDataDict)
//                 playerRefs.Add(playerRef);
//         }
//         else
//         {
//             foreach (var playerRef in InGamePlayerManager.Instance.pontianakDataDict)
//                 playerRefs.Add(playerRef);
//         }
//         currentIndex = 0;
//     }

//     private void OnPreviousClicked()
//     {
//         if (playerRefs.Count == 0) return;
//         int startIndex = currentIndex;
//         do
//         {
//             currentIndex = (currentIndex - 1 + playerRefs.Count) % playerRefs.Count;
//         }
//         while (!IsPlayerAlive(playerRefs[currentIndex]) && currentIndex != startIndex);
//         ShowCurrentCamera();
//     }

//     private void OnNextClicked()
//     {
//         if (playerRefs.Count == 0) return;
//         int startIndex = currentIndex;
//         do
//         {
//             currentIndex = (currentIndex + 1) % playerRefs.Count;
//         }
//         while (!IsPlayerAlive(playerRefs[currentIndex]) && currentIndex != startIndex);
//         ShowCurrentCamera();
//     }

//     private bool IsPlayerAlive(PlayerRef playerRef)
//     {
//         var playerObj = NetworkRunner.GetRunnerForGameObject(gameObject).GetPlayerObject(playerRef);
//         if (playerObj == null) return false;
//         var player = playerObj.GetComponent<Player>();
//         return player != null && player.isAlive();
//     }

//     private void ShowCurrentCamera()
//     {
//         if (playerRefs.Count == 0) return;

//         var pair = isOutsider
//             ? ObserverController.Instance.GetOutsiderCameraPair(playerRefs[currentIndex])
//             : ObserverController.Instance.GetPontianakCameraPair(playerRefs[currentIndex]);

//         if (pair.IsValid && pair.IsAlive)
//         {
//             // Disable all cameras first
//             foreach (var c in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
//                 c.gameObject.SetActive(false);

//             pair.Camera.gameObject.SetActive(true);
//         }
//         else
//         {
//             // Handle dead/invalid players (e.g., skip to next)
//             OnNextClicked();
//         }
//     }
// }