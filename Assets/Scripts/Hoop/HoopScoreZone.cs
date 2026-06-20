using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HoopScoreZone : MonoBehaviour
{
    [SerializeField] private int points = 1;
    [SerializeField] private float scoreCooldown = 1.5f;
    [SerializeField] private float minDownwardSpeed = 0.5f;
    [SerializeField] private bool debugScore;

    private int lastScoredBallId = -1;
    private float lastScoreTime = -999f;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"[HoopScoreZone] '{name}' collider should be a trigger.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetBall(other, out Ball ball, out Rigidbody rb))
            return;

        if (ball.State == Ball.BallState.Held)
            return;

        if (rb.linearVelocity.y > -minDownwardSpeed)
        {
            if (debugScore)
                Debug.Log($"[HoopScoreZone] Ignored — ball not moving downward ({rb.linearVelocity.y:F2})", this);
            return;
        }

        int ballId = ball.GetInstanceID();
        if (ballId == lastScoredBallId && Time.time - lastScoreTime < scoreCooldown)
            return;

        lastScoredBallId = ballId;
        lastScoreTime = Time.time;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(points);
        else
            Debug.LogWarning("[HoopScoreZone] No GameManager in scene.", this);

        if (debugScore)
            Debug.Log($"[HoopScoreZone] Basket! +{points}", this);
    }

    private static bool TryGetBall(Collider other, out Ball ball, out Rigidbody rb)
    {
        ball = other.GetComponentInParent<Ball>();
        rb = other.attachedRigidbody;

        return ball != null && rb != null && !rb.isKinematic;
    }
}
