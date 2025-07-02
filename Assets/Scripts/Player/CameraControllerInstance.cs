// using System.Collections;
// using Unity.Cinemachine;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;


// public class CameraController : MonoBehaviour
// {
//     [Header("Camera Settings")]
//     [SerializeField] public CinemachineCamera virtualCamera;

//     public static CameraController Instance { get; private set; }

//     [SerializeField] private Vignette vignette;

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;

//         // Cache the Vignette reference
//         CacheVignette();
//     }

//     private void CacheVignette()
//     {
//         if (virtualCamera == null) return;
//         CinemachineVolumeSettings volume = virtualCamera.GetComponent<CinemachineVolumeSettings>();
//         if (volume != null && volume.Profile != null && volume.Profile.TryGet(out Vignette vig))
//         {
//             vignette = vig;
//         }
//         else
//         {
//             vignette = null;
//             Debug.LogWarning("No Vignette found on the camera's Volume.");
//         }
//     }
//     public void SetVignetteIntensity(float intensity)
//     {
//         if (vignette != null)
//         {
//             vignette.intensity.value = intensity;
//         }
//     }

//     /// <summary>
//     /// Smoothly changes vignette intensity from start to end over duration seconds, using the provided AnimationCurve for interpolation.
//     /// </summary>
//     /// <param name="start">Starting intensity</param>
//     /// <param name="end">Ending intensity</param>
//     /// <param name="duration">Duration in seconds</param>
//     /// <param name="curve">AnimationCurve describing the flow (e.g., linear, ease-in, etc.)</param>
//     public IEnumerator ChangeVignetteIntensity(float start, float end, float duration, float fadeOutDuration = 0f, AnimationCurve curve = null, float delay = 0f)
//     {
//         if (vignette == null)
//             yield break;

//         if (delay > 0f)
//             yield return new WaitForSeconds(delay);

//         float timer = 0f;
//         while (timer < duration)
//         {
//             float t = timer / duration;
//             float evaluated = curve != null ? curve.Evaluate(t) : t; // Default to linear if curve is null
//             vignette.intensity.value = Mathf.Lerp(start, end, evaluated);
//             timer += Time.deltaTime;
//             yield return null;
//         }
//         vignette.intensity.value = end;

//         // Animate from end back to 0
//         timer = 0f;
//         while (timer < fadeOutDuration)
//         {
//             float t = timer / fadeOutDuration;
//             float evaluated = curve != null ? curve.Evaluate(t) : t;
//             vignette.intensity.value = Mathf.Lerp(end, 0f, evaluated);
//             timer += Time.deltaTime;
//             yield return null;
//         }
//         vignette.intensity.value = 0f;
//     }

//     public Vignette GetVignette() => vignette;

// }

// // public void SetFollowTarget(Transform lookAt, Transform followTarget)
// // {
// //     if (virtualCamera != null)
// //     {
// //         virtualCamera.Follow = followTarget;
// //         virtualCamera.LookAt = lookAt;
// //     }
// //     else
// //     {
// //         Debug.LogWarning("Virtual Camera is not assigned.");
// //     }
// // }