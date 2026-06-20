using UnityEngine;

public class BallThrower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera cam;
    [SerializeField] private LineRenderer trajectoryLine;

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 4f;
    [SerializeField] private float pickupRadius = 0.7f;
    [SerializeField] private LayerMask ballLayer;
    [SerializeField] private bool debugPickup = true;

    [Header("Hold")]
    [SerializeField] private Vector3 holdOffset = new Vector3(0f, -0.25f, 1.2f);
    [SerializeField] private float pullBackDistance = 0.35f;

    [Header("Throw")]
    [SerializeField] private float minThrowForce = 8f;
    [SerializeField] private float maxThrowForce = 22f;
    [SerializeField] private float upwardBoost = 0.15f;
    [SerializeField] private float spinStrength = 8f;
    [SerializeField] private float chargeSpeed = 0.8f;

    private Ball heldBall;
    private bool isCharging;
    private float charge01;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (trajectoryLine != null) trajectoryLine.enabled = false;

        if (holdPoint != null)
            holdPoint.localPosition = holdOffset;
    }

    private void Update()
    {
        if (heldBall == null)
        {
            if (Input.GetKeyDown(KeyCode.E))
                TryPickupBall();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            heldBall.Drop();
            heldBall = null;
            StopCharging();
            return;
        }

        if (Input.GetMouseButtonDown(0))
            StartCharging();

        if (isCharging)
        {
            UpdateCharge();
            UpdateTrajectoryPreview();

            if (Input.GetMouseButtonUp(0))
                ThrowBall();
        }
    }

    private void LateUpdate()
    {
        if (heldBall == null || holdPoint == null)
            return;

        float pullBack = isCharging ? charge01 * pullBackDistance : 0f;
        holdPoint.localPosition = holdOffset + Vector3.back * pullBack;
    }

    private void TryPickupBall()
    {
        if (!TryFindBall(out Ball ball, out string findMethod))
        {
            LogPickup($"No ball found (SphereCast + Overlap fallback, range={pickupRange}, radius={pickupRadius})");
            return;
        }

        if (!ball.TryGetPickupFailureReason(out string reason))
        {
            LogPickup($"Found '{ball.name}' via {findMethod} but blocked: {reason}");
            return;
        }

        heldBall = ball;
        heldBall.SetHeld(holdPoint);
        LogPickup($"Success — picked up '{ball.name}' via {findMethod}");
    }

    private bool TryFindBall(out Ball ball, out string method)
    {
        ball = null;
        method = null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.SphereCast(ray, pickupRadius, out RaycastHit hit, pickupRange, ballLayer, QueryTriggerInteraction.Ignore))
        {
            ball = hit.collider.GetComponentInParent<Ball>();
            if (ball != null)
            {
                method = "SphereCast";
                return true;
            }

            LogPickup($"SphereCast hit '{hit.collider.name}' (layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}) but no Ball component");
        }

        Vector3 overlapCenter = cam.transform.position + cam.transform.forward * (pickupRange * 0.5f);
        Collider[] overlaps = Physics.OverlapSphere(overlapCenter, pickupRadius, ballLayer, QueryTriggerInteraction.Ignore);

        Ball closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider col in overlaps)
        {
            Ball candidate = col.GetComponentInParent<Ball>();
            if (candidate == null)
                continue;

            float dist = Vector3.Distance(cam.transform.position, candidate.transform.position);
            if (dist > pickupRange || dist >= closestDist)
                continue;

            closest = candidate;
            closestDist = dist;
        }

        if (closest != null)
        {
            ball = closest;
            method = "OverlapSphere";
            return true;
        }

        return false;
    }

    private void LogPickup(string message)
    {
        if (debugPickup)
            Debug.Log($"[BallThrower] {message}", this);
    }

    private void StartCharging()
    {
        isCharging = true;
        charge01 = 0f;

        if (trajectoryLine != null)
            trajectoryLine.enabled = true;
    }

    private void UpdateCharge()
    {
        charge01 = Mathf.MoveTowards(charge01, 1f, chargeSpeed * Time.deltaTime);
    }

    private void ThrowBall()
    {
        float force = Mathf.Lerp(minThrowForce, maxThrowForce, charge01);
        Vector3 throwDir = cam.transform.forward + Vector3.up * upwardBoost;
        throwDir.Normalize();

        Vector3 velocity = throwDir * force;
        Vector3 spin = cam.transform.right * -spinStrength;

        heldBall.Release(velocity, spin);
        heldBall = null;

        StopCharging();
    }

    private void StopCharging()
    {
        isCharging = false;
        charge01 = 0f;

        if (holdPoint != null)
            holdPoint.localPosition = holdOffset;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
    }

    private void UpdateTrajectoryPreview()
    {
        if (trajectoryLine == null || heldBall == null) return;

        float force = Mathf.Lerp(minThrowForce, maxThrowForce, charge01);
        Vector3 throwDir = (cam.transform.forward + Vector3.up * upwardBoost).normalized;
        Vector3 velocity = throwDir * force;

        int points = 20;
        trajectoryLine.positionCount = points;

        Vector3 pos = holdPoint.position;
        Vector3 vel = velocity;
        float dt = 0.08f;

        for (int i = 0; i < points; i++)
        {
            trajectoryLine.SetPosition(i, pos);
            vel += Physics.gravity * dt;
            pos += vel * dt;
        }
    }
}
