using UnityEngine;

public class Ball : MonoBehaviour
{
    public enum BallState { Free, Held, Thrown }

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider ballCollider;
    [SerializeField] private float maxPickupSpeed = 2.5f;
    [SerializeField] private bool debugPickup;

    public BallState State { get; private set; } = BallState.Free;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (ballCollider == null) ballCollider = GetComponent<Collider>();
    }

    public bool CanBePickedUp()
    {
        return TryGetPickupFailureReason(out _);
    }

    public bool TryGetPickupFailureReason(out string reason)
    {
        if (State != BallState.Free)
        {
            reason = $"State is {State} (needs Free)";
            return false;
        }

        if (ballCollider == null || !ballCollider.enabled)
        {
            reason = "Collider missing or disabled";
            return false;
        }

        float speed = rb.linearVelocity.magnitude;
        if (speed >= maxPickupSpeed)
        {
            reason = $"Too fast ({speed:F2} m/s, max {maxPickupSpeed})";
            return false;
        }

        reason = null;
        return true;
    }

    public void SetHeld(Transform point)
    {
        State = BallState.Held;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (ballCollider != null)
            ballCollider.enabled = false;

        transform.SetParent(point);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (debugPickup)
            Debug.Log($"[Ball] Picked up — state={State}", this);
    }

    public void Release(Vector3 velocity, Vector3 spin)
    {
        DetachFromHold();

        State = BallState.Free;
        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        rb.angularVelocity = spin;

        if (debugPickup)
            Debug.Log($"[Ball] Released — state={State}, speed={velocity.magnitude:F2}", this);
    }

    public void Drop()
    {
        DetachFromHold();
        State = BallState.Free;
        rb.isKinematic = false;

        if (debugPickup)
            Debug.Log($"[Ball] Dropped — state={State}", this);
    }

    public void ResetToFree()
    {
        DetachFromHold();
        State = BallState.Free;
        rb.isKinematic = false;
    }

    private void DetachFromHold()
    {
        transform.SetParent(null);

        if (ballCollider != null)
            ballCollider.enabled = true;
    }
}
