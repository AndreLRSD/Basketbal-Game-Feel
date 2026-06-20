using System;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Hoop")]
    [SerializeField] private Transform hoopTarget;

    [Header("Distance Scoring")]
    [SerializeField] private float closeMaxDistance = 10f;
    [SerializeField] private float mediumMaxDistance = 20f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI basketPopupText;
    [SerializeField] private float basketPopupDuration = 1.2f;

    public int Score { get; private set; }
    public Transform HoopTarget => hoopTarget;

    public event Action<int> OnScoreChanged;
    public event Action<BasketScoreData> OnBasketScored;

    private float basketPopupTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateScoreUI();

        if (basketPopupText != null)
            basketPopupText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (basketPopupText == null || basketPopupTimer <= 0f)
            return;

        basketPopupTimer -= Time.unscaledDeltaTime;
        if (basketPopupTimer <= 0f)
            basketPopupText.gameObject.SetActive(false);
    }

    public void RegisterBasket(Vector3 shotOrigin)
    {
        if (hoopTarget == null)
        {
            Debug.LogWarning("[GameManager] Hoop target not assigned.");
            RegisterBasket(0f, 1, 0);
            return;
        }

        float distance = GetHorizontalDistance(shotOrigin, hoopTarget.position);
        int tier = GetTierForDistance(distance);
        int points = GetPointsForDistance(distance);
        RegisterBasket(distance, points, tier);
    }

    public void RegisterBasket(float distance, int points, int tier)
    {
        BasketScoreData data = new BasketScoreData(points, distance, tier);

        Score += points;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(Score);
        OnBasketScored?.Invoke(data);
        ShowBasketPopup(data);

        Debug.Log($"[GameManager] Basket! {distance:F1}m → +{points} pts (tier {tier}) | Total: {Score}");
    }

    public void AddScore(int points)
    {
        if (points <= 0)
            return;

        Score += points;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(Score);
    }

    public int GetPointsForDistance(float distance)
    {
        if (distance < closeMaxDistance)
            return 1;
        if (distance < mediumMaxDistance)
            return 2;
        return 3;
    }

    public int GetTierForDistance(float distance)
    {
        if (distance < closeMaxDistance)
            return 0;
        if (distance < mediumMaxDistance)
            return 1;
        return 2;
    }

    public static float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        from.y = 0f;
        to.y = 0f;
        return Vector3.Distance(from, to);
    }

    private void ShowBasketPopup(BasketScoreData data)
    {
        if (basketPopupText == null)
            return;

        basketPopupText.gameObject.SetActive(true);
        basketPopupText.text = $"+{data.Points}\n{data.Distance:F0}m";
        basketPopupTimer = basketPopupDuration;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = Score.ToString();
    }
}
