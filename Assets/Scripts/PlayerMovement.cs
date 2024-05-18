using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform cameraTransform;
    
    [Header("Walk and Run")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    
    [Header("Jump")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float jumpSpeed = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    
    private Vector3 _velocity;
    private Vector3 _initialCameraPosition;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _crouchAction;
    private float _movementSpeedMultiplier;
    private float _currentHeight;
    private float _standingHeight;
    
    private bool IsGrounded => Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
    private bool IsCrouching => _standingHeight - _currentHeight > 0.1f;
    
    private void Awake()
    {
        _moveAction = playerInput.actions["move"];
        _jumpAction = playerInput.actions["jump"];
        _sprintAction = playerInput.actions["sprint"];
        _crouchAction = playerInput.actions["crouch"];
    }

    private void Start()
    {
        _initialCameraPosition = cameraTransform.localPosition;
        _standingHeight = _currentHeight = characterController.height;
    }

    private void Update()
    {
        ApplyGravity();
        Sprint();
        Crouch();
        Move();
    }
    
    private void ApplyGravity()
    {
        var gravity = Physics.gravity * (mass * Time.deltaTime);
        _velocity.y = IsGrounded ? -1f : _velocity.y + gravity.y;
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

    private void Crouch()
    {
        var isTryingToCrouch = _crouchAction.ReadValue<float>() > 0;

        var heightTarget = isTryingToCrouch ? crouchHeight : _standingHeight;

        if (IsCrouching && !isTryingToCrouch)
        {
            var castOrigin = transform.position + new Vector3(0f, _currentHeight / 2, 0f);
            if (Physics.Raycast(castOrigin, Vector3.up, out RaycastHit hit, 0.2f))
            {
                var distanceToCeiling = hit.point.y - castOrigin.y;
                heightTarget = Mathf.Max(_currentHeight + distanceToCeiling - 0.1f, crouchHeight);
            }
        }

        if (!Mathf.Approximately(heightTarget, _currentHeight))
        {
            var crouchDelta = Time.deltaTime * crouchTransitionSpeed;
            _currentHeight = Mathf.Lerp(_currentHeight, heightTarget, crouchDelta);

            var halfHeightDifference = new Vector3(0f, (_standingHeight - _currentHeight) / 2, 0f);
            var newCameraPosition = _initialCameraPosition - halfHeightDifference;

            cameraTransform.localPosition = newCameraPosition;
            characterController.height = _currentHeight;
        }

        if (IsCrouching)
        {
            _movementSpeedMultiplier *= crouchSpeedMultiplier;
        }
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
        if (jumpInput > 0 && IsGrounded)
        {
            _velocity.y += jumpSpeed;
        }
        
        characterController.Move(_velocity * Time.deltaTime);
    }
}
