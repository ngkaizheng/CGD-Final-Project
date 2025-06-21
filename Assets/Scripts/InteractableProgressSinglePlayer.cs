// using UnityEngine;
// using Fusion;

// public class Interactable : NetworkBehaviour, IProgressInteractable
// {
//     [Networked, OnChangedRender(nameof(OnColorChanged))] public Color NetworkedColor { get; set; }
//     [Networked] public float progress { get; set; } = 0.0f;
//     [Networked] public float progressSpeed { get; set; } = 0.1f;
//     [Networked] public bool progressCompleted { get; set; } = false;
//     [Networked] public bool isBeingInteractedWith { get; set; } = false;
//     private Renderer cachedRenderer;

//     private void Awake()
//     {
//         cachedRenderer = GetComponent<Renderer>();
//     }

//     public override void Spawned()
//     {
//         // Apply initial color and prompt state on spawn
//         ApplyColor();
//     }

//     public void OnInteract(Player player)
//     {
//         if (Object.HasStateAuthority)
//         {
//             player.GetComponent<SimpleAnimator>().SetInteracting(true);
//             isBeingInteractedWith = true;
//         }
//     }

//     public override void FixedUpdateNetwork()
//     {
//         if (Object.HasStateAuthority)
//         {
//             if (isBeingInteractedWith)
//             {
//                 if (!progressCompleted)
//                 {
//                     progress += Runner.DeltaTime * progressSpeed;
//                     if (progress >= 1.0f)
//                     {
//                         progress = 1.0f;
//                         progressCompleted = true;
//                         NetworkedColor = Random.ColorHSV();
//                     }
//                 }
//             }
//             else
//             {
//                 // Reset progress if not completed
//                 if (!progressCompleted)
//                 {
//                     progress = 0f;
//                 }
//                 // Optionally, reset progressCompleted if you want it to be reusable:
//                 // progressCompleted = false;
//             }

//             // Reset flag for next tick
//             isBeingInteractedWith = false;
//         }
//     }

//     public float GetProgress()
//     {
//         return progress;
//     }

//     public void SetPromptVisible(bool visible)
//     {
//         // Example: Show or hide interaction prompt
//         Debug.Log($"SetPromptVisible called with {visible}");
//     }

//     public string GetPromptMessage()
//     {
//         return "Press E to interact";
//     }

//     private void OnColorChanged()
//     {
//         ApplyColor();
//     }
//     private void ApplyColor()
//     {
//         if (cachedRenderer != null)
//         {
//             cachedRenderer.material.color = NetworkedColor;
//         }
//     }
// }