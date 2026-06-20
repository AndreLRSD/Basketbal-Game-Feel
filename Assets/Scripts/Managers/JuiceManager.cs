using UnityEngine;

public class JuiceManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private Transform scoreVfxSpawn;

    [Header("Hitstop")]
    [SerializeField] private float hitstopDurationTier1 = 0.08f;
    [SerializeField] private float hitstopDurationTier2 = 0.12f;
    [SerializeField] private float hitstopDurationTier3 = 0.18f;
    [SerializeField] private float hitstopTimeScale = 0.08f;

    [Header("Camera Shake On Score")]
    [SerializeField] private float scoreShakeIntensityTier1 = 0.12f;
    [SerializeField] private float scoreShakeIntensityTier2 = 0.18f;
    [SerializeField] private float scoreShakeIntensityTier3 = 0.26f;
    [SerializeField] private float scoreShakeDuration = 0.22f;

    [Header("Score VFX (index 0=close, 1=medium, 2=long)")]
    [SerializeField] private GameObject[] scoreVfxPrefabs;
    [SerializeField] private float scoreVfxLifetime = 3f;

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnBasketScored -= HandleBasketScored;
    }

    private void Start()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnBasketScored -= HandleBasketScored;
        GameManager.Instance.OnBasketScored += HandleBasketScored;

        if (cameraShake == null)
            cameraShake = FindFirstObjectByType<CameraShake>();
    }

    private void HandleBasketScored(BasketScoreData data)
    {
        PlayHitstop(data.Tier);
        PlayCameraShake(data.Tier);
        SpawnScoreVfx(data.Tier);
    }

    private void PlayHitstop(int tier)
    {
        if (Hitstop.Instance == null)
            return;

        float duration = tier switch
        {
            0 => hitstopDurationTier1,
            1 => hitstopDurationTier2,
            _ => hitstopDurationTier3
        };

        Hitstop.Instance.Play(duration, hitstopTimeScale);
    }

    private void PlayCameraShake(int tier)
    {
        if (cameraShake == null)
            return;

        float intensity = tier switch
        {
            0 => scoreShakeIntensityTier1,
            1 => scoreShakeIntensityTier2,
            _ => scoreShakeIntensityTier3
        };

        cameraShake.Shake(intensity, scoreShakeDuration);
    }

    private void SpawnScoreVfx(int tier)
    {
        if (scoreVfxPrefabs == null || scoreVfxPrefabs.Length == 0)
            return;

        int index = Mathf.Clamp(tier, 0, scoreVfxPrefabs.Length - 1);
        GameObject prefab = scoreVfxPrefabs[index];
        if (prefab == null)
            return;

        Vector3 position = scoreVfxSpawn != null ? scoreVfxSpawn.position : transform.position;
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        Destroy(instance, scoreVfxLifetime);
    }
}
