using UnityEngine;
using TMPro;

public class HoopDistanceIndicator : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshProUGUI distanceText;

    [Header("Colors")]
    [SerializeField] private Color closeColor = new Color(0.2f, 1f, 0.35f);
    [SerializeField] private Color mediumColor = new Color(1f, 0.9f, 0.2f);
    [SerializeField] private Color longColor = new Color(1f, 0.35f, 0.2f);

    private void Update()
    {
        if (distanceText == null || player == null || GameManager.Instance == null)
            return;

        Transform hoop = GameManager.Instance.HoopTarget;
        if (hoop == null)
        {
            distanceText.text = string.Empty;
            return;
        }

        float distance = GameManager.GetHorizontalDistance(player.position, hoop.position);
        int points = GameManager.Instance.GetPointsForDistance(distance);
        int tier = GameManager.Instance.GetTierForDistance(distance);

        distanceText.text = $"{distance:F0}m · {points} PT";
        distanceText.color = GetColorForTier(tier);
    }

    private Color GetColorForTier(int tier)
    {
        return tier switch
        {
            0 => closeColor,
            1 => mediumColor,
            _ => longColor
        };
    }
}
