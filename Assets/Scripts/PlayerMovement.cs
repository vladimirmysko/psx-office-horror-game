using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;
    
    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    
    private Vector3 _velocity;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private float _movementSpeedMultiplier;
    
    private void Awake()
    {
        _moveAction = playerInput.actions["move"];
        _jumpAction = playerInput.actions["jump"];
        _sprintAction = playerInput.actions["sprint"];
    }
    
    private void Update()
    {
        ApplyGravity();
        Sprint();
        Move();
    }
    
    private void ApplyGravity()
    {
        var gravity = Physics.gravity * (mass * Time.deltaTime);
        _velocity.y = characterController.isGrounded ? -1f : _velocity.y + gravity.y;
    }

    private void Sprint()
    {
        _movementSpeedMultiplier = 1f;
        
        var sprintInput = _sprintAction.ReadValue<float>();
        if (sprintInput == 0) return;
        
        var forwardMovementFactor = Mathf.Clamp01(Vector3.Dot(transform.forward, _velocity.normalized));
        var multiplier = Mathf.Lerp(1f, sprintSpeedMultiplier, forwardMovementFactor);

        _movementSpeedMultiplier *= multiplier;
    }

    private Vector3 GetMovementInput()
    {
        var moveInput = _moveAction.ReadValue<Vector2>();

        var input = new Vector3();

        input += transform.forward * moveInput.y;
        input += transform.right * moveInput.x;
        input = Vector3.ClampMagnitude(input, 1f);
        
        input *= movementSpeed * _movementSpeedMultiplier;

        return input;
    }
    
    private void Move()
    {
        var input = GetMovementInput();

        var factor = acceleration * Time.deltaTime;
        _velocity.x = Mathf.Lerp(_velocity.x, input.x, factor);
        _velocity.z = Mathf.Lerp(_velocity.z, input.z, factor);
        
        var jumpInput = _jumpAction.ReadValue<float>();
        if (jumpInput > 0 && characterController.isGrounded)
        {
            _velocity.y += jumpSpeed;
        }
        
        characterController.Move(_velocity * Time.deltaTime);
    }
}
