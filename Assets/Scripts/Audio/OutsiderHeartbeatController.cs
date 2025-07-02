using UnityEngine;

public class OutsiderHeartbeatController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private SoundEffect heartbeatSound = SoundEffect.HeartBeat;
    [SerializeField] private float maxHeartbeatDistance = 20f;
    [SerializeField] private float minHeartbeatInterval = 0.5f;
    [SerializeField] private float maxHeartbeatInterval = 3f;
    [SerializeField] private AnimationCurve distanceToIntervalCurve;
    [SerializeField] private AudioSource audioSource;

    [Header("Events")]
    [SerializeField] private GameEvent gameInitToEveryoneEvent;

    private Transform pontianakTransform;
    private bool isInitialized = false;
    private float currentDistance;
    private float heartbeatTimer = 0f;
    public bool isSelf = false;

    private void OnEnable()
    {
        if (gameInitToEveryoneEvent != null)
            gameInitToEveryoneEvent.OnRaised.AddListener(OnGameInit);

        if (!isSelf)
            OnGameInit();
    }

    private void OnDisable()
    {
        if (gameInitToEveryoneEvent != null)
            gameInitToEveryoneEvent.OnRaised.RemoveListener(OnGameInit);
    }

    public void OnGameInit()
    {
        var pontianak = FindFirstObjectByType<Pontianak>();
        if (pontianak != null)
        {
            pontianakTransform = pontianak.transform;
            isInitialized = true;
            CalculateNewHeartbeatInterval();
        }
        else
        {
            Debug.LogWarning("Pontianak not found in scene!");
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.minDistance = 100f;
            audioSource.maxDistance = 15f;
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        currentDistance = Vector3.Distance(transform.position, pontianakTransform.position);

        // Always update the interval based on current distance
        float normalizedDistance = Mathf.Clamp01(currentDistance / maxHeartbeatDistance);
        float curveValue = distanceToIntervalCurve.Evaluate(1f - normalizedDistance);
        float interval = Mathf.Lerp(minHeartbeatInterval, maxHeartbeatInterval, curveValue);

        Debug.Log("Normalized Distance: " + normalizedDistance +
                  ", Curve Value: " + curveValue +
                  ", Interval: " + interval);

        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer <= 0f)
        {
            audioSource.transform.position = pontianakTransform.position;
            AudioController.Instance.PlaySoundEffect(heartbeatSound, audioSource);
            // AudioController.Instance.PlaySoundEffectAtPosition(heartbeatSound, pontianakTransform.position);
            heartbeatTimer = interval; // Use the latest interval
        }
    }

    private void CalculateNewHeartbeatInterval()
    {
        float normalizedDistance = Mathf.Clamp01(currentDistance / maxHeartbeatDistance);
        float curveValue = distanceToIntervalCurve.Evaluate(1f - normalizedDistance);
        float interval = Mathf.Lerp(minHeartbeatInterval, maxHeartbeatInterval, curveValue);
        heartbeatTimer = interval;
    }

    private void OnDrawGizmosSelected()
    {
        if (!isInitialized) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, pontianakTransform.position);
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, currentDistance);
    }
}