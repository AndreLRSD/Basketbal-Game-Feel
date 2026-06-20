using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int pointsPerBasket = 1;
    [SerializeField] private Text scoreText;

    public int Score { get; private set; }

    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateScoreUI();
    }

    public void AddScore(int points)
    {
        if (points <= 0)
            return;

        Score += points;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(Score);

        Debug.Log($"[GameManager] Score: {Score} (+{points})");
    }

    public void RegisterBasket()
    {
        AddScore(pointsPerBasket);
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(Score);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = Score.ToString();
    }
}
