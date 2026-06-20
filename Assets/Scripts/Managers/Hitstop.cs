using System.Collections;
using UnityEngine;

public class Hitstop : MonoBehaviour
{
    public static Hitstop Instance { get; private set; }

    private Coroutine activeHitstop;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Play(float duration, float timeScale = 0.08f)
    {
        if (activeHitstop != null)
            StopCoroutine(activeHitstop);

        activeHitstop = StartCoroutine(HitstopRoutine(duration, timeScale));
    }

    private IEnumerator HitstopRoutine(float duration, float timeScale)
    {
        float previousTimeScale = Time.timeScale;
        Time.timeScale = timeScale;

        yield return new WaitForSecondsRealtime(duration);

        if (!CanvasManager.IsPaused)
            Time.timeScale = previousTimeScale;

        activeHitstop = null;
    }
}
