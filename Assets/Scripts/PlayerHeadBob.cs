using UnityEngine;

public class PlayerHeadBob : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform cameraHolder;
    
    [Header("Settings")]
    [SerializeField] private float amplitude = 0.03f;
    [SerializeField] private float frequency = 10f;

    private readonly float _toggleSpeed = 3f;
    private Vector3 _startPosition;

    private void Awake()
    {
        _startPosition = playerCamera.localPosition;
        amplitude /= 100;
    }

    private void Update()
    {
        CheckMotion();
        ResetPosition();
        
        playerCamera.LookAt(FocusTarget());
    }

    private void PlayMotion(Vector3 motion)
    {
        playerCamera.localPosition += motion;
    }

    private void CheckMotion()
    {
        var speed = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;

        if (speed < _toggleSpeed) return;
        if (!characterController.isGrounded) return;

        PlayMotion(FootStepMotion());
    }

    private Vector3 FootStepMotion()
    {
        Vector3 position = Vector3.zero;
        
        position.y += Mathf.Sin(Time.time * frequency) * amplitude;
        position.x += Mathf.Cos(Time.time * frequency / 2f) * amplitude * 2f;

        return position;
    }

    private void ResetPosition()
    {
        if (playerCamera.localPosition == _startPosition) return;
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, _startPosition, 5f * Time.deltaTime);
    }

    private Vector3 FocusTarget()
    {
        Vector3 position = new Vector3(transform.position.x, transform.position.y + cameraHolder.localPosition.y, transform.position.z);
        position += cameraHolder.forward * 15f;
        
        return position;
    }
}
