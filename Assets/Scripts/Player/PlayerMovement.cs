using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.2f;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        HandleMovement();
    }
    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        Vector2 input = GetMoveInput();
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        float speed = GetCurrentSpeed();
        controller.Move(move * speed * Time.deltaTime);
        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private Vector2 GetMoveInput()
    {
        float x = Input.GetAxisRaw("Horizontal"); 
        float y = Input.GetAxisRaw("Vertical");   
        Vector2 input = new Vector2(x, y);
        return input.sqrMagnitude > 1f ? input.normalized : input;
    }
    private float GetCurrentSpeed()
    {
        return Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
    }
}
