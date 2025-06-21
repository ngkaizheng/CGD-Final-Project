// using UnityEngine;
// using Fusion;

// public class PlayerAction : NetworkBehaviour
// {
//     [SerializeField] private float interactDistance = 2f;
//     [SerializeField] private LayerMask interactMask = default;

//     private IInteractable lastPromptedInteractable;
//     private bool isCurrentlyInteracting = false;

//     private Player player;

//     public override void Spawned()
//     {
//         player = GetComponent<Player>();
//     }

//     /// <summary>
//     /// Call this every frame or when player moves to update the interact prompt.
//     /// </summary>
//     public void UpdateInteractPrompt(Transform playerTransform)
//     {
//         Collider[] colliders = Physics.OverlapSphere(playerTransform.position, interactDistance, interactMask);
//         IInteractable closestInteractable = null;
//         float closestDist = float.MaxValue;

//         foreach (var col in colliders)
//         {
//             if (col.transform.root.gameObject == playerTransform.root.gameObject) continue;

//             var interactable = col.GetComponent<IInteractable>();
//             if (interactable != null)
//             {
//                 float dist = Vector3.Distance(playerTransform.position, col.transform.position);
//                 if (dist < closestDist)
//                 {
//                     closestDist = dist;
//                     closestInteractable = interactable;
//                 }
//             }
//         }

//         // Hide prompt on the previous interactable if it's not the closest anymore
//         if (lastPromptedInteractable != null && lastPromptedInteractable != closestInteractable)
//         {
//             GameUI.SetInteractPromptActive(false, null);
//         }

//         // Show prompt on the new closest interactable, only if it's different
//         if (closestInteractable != null && closestInteractable != lastPromptedInteractable)
//         {
//             GameUI.SetInteractPromptActive(true, closestInteractable);
//         }
//         // Stop interacting if player left the interactable while holding the button ---
//         if (isCurrentlyInteracting && closestInteractable == null)
//         {
//             player.GetComponent<SimpleAnimator>().SetInteracting(false);
//             isCurrentlyInteracting = false;
//         }

//         lastPromptedInteractable = closestInteractable;
//     }

//     /// <summary>
//     /// Call this when the player presses the interact button.
//     /// </summary>
//     public void Interact(Transform playerTransform, bool isInteracting)
//     {
//         if (isInteracting)
//         {
//             Collider[] colliders = Physics.OverlapSphere(playerTransform.position, interactDistance, interactMask);
//             IInteractable closestInteractable = null;
//             float closestDist = float.MaxValue;

//             foreach (var col in colliders)
//             {
//                 if (col.transform.root.gameObject == playerTransform.root.gameObject) continue;

//                 var interactable = col.GetComponent<IInteractable>();
//                 if (interactable != null)
//                 {
//                     float dist = Vector3.Distance(playerTransform.position, col.transform.position);
//                     if (dist < closestDist)
//                     {
//                         closestDist = dist;
//                         closestInteractable = interactable;
//                     }
//                 }
//             }

//             if (closestInteractable != null)
//             {
//                 isCurrentlyInteracting = true;
//                 Debug.Log($"Interacting with {closestInteractable} at distance {closestDist}");
//                 closestInteractable.OnInteract(GetComponent<Player>());
//             }
//             else
//             {
//                 Debug.Log("No interactable found in range.");
//             }
//         }
//         else
//         {
//             Debug.Log("Interact Action Released");
//             player.GetComponent<SimpleAnimator>().SetInteracting(false);
//             isCurrentlyInteracting = false;
//         }
//     }
// }