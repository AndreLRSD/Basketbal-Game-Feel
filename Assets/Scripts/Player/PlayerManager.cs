using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    
    [SerializeField] private float pushPower = 2f;
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic)
            return;
        // Não empurra a bola quando está em cima dela (pulo/pisar)
        if (hit.moveDirection.y < -0.3f)
            return;
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        if (pushDir.sqrMagnitude < 0.001f)
            return;
        rb.AddForce(pushDir * pushPower, ForceMode.Impulse);
    }
}
