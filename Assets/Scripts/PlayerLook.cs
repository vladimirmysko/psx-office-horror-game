using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform cameraTransform;
    
    [Header("Settings")]
    [SerializeField] private float lookSensitivity = 3f;
    
    private Vector2 _look;
    private InputAction _lookAction;
    
    private void Awake()
    {
        _lookAction = playerInput.actions["look"];
    }
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        var lookInput = _lookAction.ReadValue<Vector2>();
        _look.x += lookInput.x * lookSensitivity;
        _look.y += lookInput.y * lookSensitivity;

        _look.y = Mathf.Clamp(_look.y, -89f, 89f);
        
        cameraTransform.localRotation = Quaternion.Euler(-_look.y, -0f, -0f);
        transform.localRotation = Quaternion.Euler(0f, _look.x, 0f);
    }
}
