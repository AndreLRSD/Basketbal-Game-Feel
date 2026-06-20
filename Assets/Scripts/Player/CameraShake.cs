using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Intensity")]
    [SerializeField] private float minIntensity = 0.04f;
    [SerializeField] private float maxIntensity = 0.18f;

    [Header("Duration")]
    [SerializeField] private float minDuration = 0.08f;
    [SerializeField] private float maxDuration = 0.2f;

    [Header("Motion")]
    [SerializeField] private float frequency = 28f;

    private Vector3 defaultLocalPosition;
    private float shakeTimer;
    private float shakeDuration;
    private float currentIntensity;
    private float noiseSeed;

    private void Awake()
    {
        defaultLocalPosition = transform.localPosition;
        noiseSeed = Random.Range(0f, 100f);
    }

    private void LateUpdate()
    {
        if (shakeTimer <= 0f)
        {
            transform.localPosition = defaultLocalPosition;
            return;
        }

        shakeTimer -= Time.deltaTime;
        float damper = Mathf.Clamp01(shakeTimer / shakeDuration);

        float time = Time.time * frequency;
        float offsetX = (Mathf.PerlinNoise(noiseSeed, time) - 0.5f) * 2f;
        float offsetY = (Mathf.PerlinNoise(noiseSeed + 1f, time) - 0.5f) * 2f;

        Vector3 offset = new Vector3(offsetX, offsetY, 0f) * currentIntensity * damper;
        transform.localPosition = defaultLocalPosition + offset;
    }

    public void ShakeFromThrowForce(float force, float forceMin, float forceMax)
    {
        float normalized = Mathf.InverseLerp(forceMin, forceMax, force);

        shakeDuration = Mathf.Lerp(minDuration, maxDuration, normalized);
        shakeTimer = shakeDuration;
        currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalized);
    }

    public void Shake(float intensity, float duration)
    {
        shakeDuration = duration;
        shakeTimer = duration;
        currentIntensity = intensity;
    }
}
