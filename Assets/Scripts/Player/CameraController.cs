using Unity.Cinemachine;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera virtualCamera;

    public static CameraController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineCamera>();
        }
    }

    public void SetFollowTarget(Transform lookAt, Transform followTarget)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = followTarget;
            virtualCamera.LookAt = lookAt;
        }
        else
        {
            Debug.LogWarning("Virtual Camera is not assigned.");
        }
    }
}