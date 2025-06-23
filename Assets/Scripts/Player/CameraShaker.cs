using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Fusion;

public class CameraShaker : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    private CinemachineBasicMultiChannelPerlin perlin;

    private float shakeTimer;
    private float totalShakeTime;
    private float magnitude;
    private float roughness;
    private float fadeInTime;
    private float fadeOutTime;

    public static CameraShaker Instance { get; private set; }

    private void Awake()
    {
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        //     return;
        // }
        // Instance = this;

        vcam = GetComponent<CinemachineCamera>();
        if (vcam != null)
            perlin = vcam.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Instance = this;
        }
    }


    public void Shake(float magnitude, float roughness, float fadeIn, float fadeOut, float duration)
    {
        this.magnitude = magnitude;
        this.roughness = roughness;
        this.fadeInTime = fadeIn;
        this.fadeOutTime = fadeOut;
        totalShakeTime = duration;
        shakeTimer = 0f;
        if (perlin != null)
        {
            perlin.AmplitudeGain = 0f;
            perlin.FrequencyGain = 0f;
        }
    }

    public void ShakeOnce(float amplitudeGain, float frequencyGain, float duration)
    {
        if (perlin == null) return;
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(amplitudeGain, frequencyGain, duration));
    }

    private IEnumerator ShakeCoroutine(float amplitudeGain, float frequencyGain, float duration)
    {
        Debug.Log($"Shaking camera with Amplitude: {amplitudeGain}, Frequency: {frequencyGain}, Duration: {duration}");
        perlin.AmplitudeGain = amplitudeGain;
        perlin.FrequencyGain = frequencyGain;
        yield return new WaitForSeconds(duration);
        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
    }
    // private void Update()
    // {
    //     // 0.3, 50 is a good shake for haunted effect
    //     // // For testing, every 5 seconds, shake the camera
    //     // if (Time.time % 5 < 0.1f)
    //     // {
    //     //     Shake(1f, 2f, 0.1f, 0.1f, 1f);
    //     // }

    //     if (perlin == null || totalShakeTime <= 0f) return;

    //     shakeTimer += Time.deltaTime;
    //     float fade = 1f;

    //     if (shakeTimer < fadeInTime)
    //         fade = Mathf.Clamp01(shakeTimer / fadeInTime);
    //     else if (shakeTimer > totalShakeTime - fadeOutTime)
    //         fade = Mathf.Clamp01((totalShakeTime - shakeTimer) / fadeOutTime);

    //     if (shakeTimer < totalShakeTime)
    //     {
    //         perlin.AmplitudeGain = magnitude * fade;
    //         perlin.FrequencyGain = roughness * fade;
    //     }
    //     else
    //     {
    //         perlin.AmplitudeGain = 0f;
    //         perlin.FrequencyGain = 0f;
    //         totalShakeTime = 0f;
    //     }
    // }
}