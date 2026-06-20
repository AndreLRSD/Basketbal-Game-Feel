using UnityEngine;
using UnityEngine.UI;

public class BallThrower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera cam;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private Slider forceSlider;

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

    [Header("Throw SFX")]
    [SerializeField] private AudioClip throwSfxWeak;
    [SerializeField] private AudioClip throwSfxMedium;
    [SerializeField] private AudioClip throwSfxStrong;

    private Ball heldBall;
    private bool isCharging;
    private float charge01;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cameraShake == null) cameraShake = GetComponent<CameraShake>();
        if (trajectoryLine != null) trajectoryLine.enabled = false;

        if (holdPoint != null)
            holdPoint.localPosition = holdOffset;

        SetupForceSlider();
    }

    private void SetupForceSlider()
    {
        if (forceSlider == null)
            return;

        forceSlider.minValue = 0f;
        forceSlider.maxValue = 1f;
        forceSlider.interactable = false;
        forceSlider.value = 0f;
        forceSlider.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (CanvasManager.IsPaused)
            return;

        if (Input.GetKeyDown(KeyCode.R))
            RecallBallToHands();

        if (heldBall == null)
        {
            if (Input.GetKeyDown(KeyCode.E))
                TryPickupBall();

            UpdateForceSlider();
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

        UpdateForceSlider();
    }

    private void LateUpdate()
    {
        if (heldBall == null || holdPoint == null)
            return;

        float pullBack = isCharging ? charge01 * pullBackDistance : 0f;
        holdPoint.localPosition = holdOffset + Vector3.back * pullBack;
    }

    public void RecallBallToHands()
    {
        if (heldBall != null)
            return;

        StopCharging();

        Ball ball = FindRecallableBall();
        if (ball == null)
        {
            LogPickup("Recall failed — no ball available");
            return;
        }

        heldBall = ball;
        heldBall.SetHeld(holdPoint);
        LogPickup($"Recalled '{ball.name}' to hands");
    }

    private Ball FindRecallableBall()
    {
        Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        Ball closest = null;
        float closestDist = float.MaxValue;

        foreach (Ball ball in balls)
        {
            if (ball.State == Ball.BallState.Held)
                continue;

            float dist = Vector3.Distance(cam.transform.position, ball.transform.position);
            if (dist >= closestDist)
                continue;

            closest = ball;
            closestDist = dist;
        }

        return closest;
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

        cameraShake?.ShakeFromThrowForce(force, minThrowForce, maxThrowForce);
        PlayThrowSfx(charge01);

        StopCharging();
    }

    private void PlayThrowSfx(float charge)
    {
        AudioClip clip = GetThrowSfxForCharge(charge);
        if (clip == null)
            return;

        Vector3 position = holdPoint != null ? holdPoint.position : cam.transform.position;
        AudioManager.Instance?.PlaySfx(clip, position);
    }

    private AudioClip GetThrowSfxForCharge(float charge)
    {
        if (charge < 0.33f)
            return throwSfxWeak;
        if (charge < 0.66f)
            return throwSfxMedium;
        return throwSfxStrong;
    }

    private void StopCharging()
    {
        isCharging = false;
        charge01 = 0f;

        if (holdPoint != null)
            holdPoint.localPosition = holdOffset;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        UpdateForceSlider();
    }

    private void UpdateForceSlider()
    {
        if (forceSlider == null)
            return;

        bool visible = heldBall != null;
        if (forceSlider.gameObject.activeSelf != visible)
            forceSlider.gameObject.SetActive(visible);

        if (!visible)
        {
            forceSlider.value = 0f;
            return;
        }

        forceSlider.value = charge01;
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
